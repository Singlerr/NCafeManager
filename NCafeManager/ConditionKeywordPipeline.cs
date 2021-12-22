using System.Collections.Generic;
using Pipelines;

namespace Conditions
{
    internal class ConditionKeywordPipeline : ConditionPipeline
    {
        private readonly List<string> _keywords;
        private readonly ConditionOperationType _operationType;

        public ConditionKeywordPipeline(ParentPipeline pipeline, IEnumerable<string> keywords,
            ConditionOperationType operationType) : base(pipeline)
        {
            _keywords = new List<string>();
            _keywords.AddRange(keywords);
            _operationType = operationType;
        }


        public override bool DoFilter(int step, Condition cond, bool stacked, ConditionBox box)
        {
            var b = false;
            if (_operationType == ConditionOperationType.And)
            {
                b = true;
                foreach (var keyword in _keywords) b = b && cond.Title.Contains(keyword);
            }
            else
            {
                foreach (var keyword in _keywords) b = b || cond.Title.Contains(keyword);
            }

            return b;
        }
    }
}