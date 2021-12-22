using System;

namespace Conditions
{
    internal class ConditionTimePipeline : ConditionPipeline
    {
        private readonly ConditionOperationType _operationType;
        private DateTime _time;

        public ConditionTimePipeline(ConditionParentPipeline pipeline, DateTime time,
            ConditionOperationType operationType) : base(pipeline)
        {
            _time = time;
            _operationType = operationType;
        }

        public override bool DoFilter(int step, Condition cond, bool stacked, ConditionBox box)
        {
            if (_operationType == ConditionOperationType.After)
                return stacked && _time.CompareTo(cond.Time) <= 0;
            if (_operationType == ConditionOperationType.Before) return stacked && _time.CompareTo(cond.Time) >= 0;

            return false;
        }
    }
}