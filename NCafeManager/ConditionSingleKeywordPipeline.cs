using Pipelines;

namespace Conditions
{
    internal class ConditionSingleKeywordPipeline : ConditionPipeline
    {
        private readonly string _keyword;
        private readonly ConditionOperationType _operationType;

        public ConditionSingleKeywordPipeline(ParentPipeline pipeline, string keyword,
            ConditionOperationType operationType) : base(pipeline)
        {
            _keyword = keyword;
            _operationType = operationType;
        }


        public override bool DoFilter(int step, Condition cond, bool stacked, ConditionBox box)
        {
            var b = cond.Title.Contains(_keyword);
            if (_operationType == ConditionOperationType.And)
            {
                //전 단계까지 OR 연산이 이루어졌고, 다음연산이 AND 일경우 지금까지의 OR 연산이 무시되는 경우가 있으므로, AND 연산은 별도로 연산한다. 그리고 마지막에 합친다.
                //마지막에 합치는거는 ConditionParentPipeline 에서 처리
                if (box.ConditionObject is ConditionOperationType.Or)
                {
                    box.ConditionObject = b;
                    //위의 과정이 전 단계에서 있었으므로 계속 이어서 AND 연산을 계속해야됨
                    return stacked;
                }

                if (box.ConditionObject is bool pre)
                {
                    box.ConditionObject = pre && b;
                    return stacked;
                }

                return stacked && b;
            }

            if (_operationType == ConditionOperationType.Or)
            {
                //OR 일경우 그냥 연산
                //그리고 ConditionObject를 OR 타입으로 정해주면서 OR 연산이 행해졌음을 전달
                box.ConditionObject = ConditionOperationType.Or;
                return stacked || b;
            }

            //OperartionType이 None임. 즉 첫번째 키워드  연산
            return b;
        }
    }
}