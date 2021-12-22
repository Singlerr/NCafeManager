using System.Collections.Generic;
using Pipelines;

namespace Conditions
{
    internal class ConditionParentPipeline : ParentPipeline
    {
        private int _currentStep;

        private List<ConditionPipeline> _pipelines;

        public Condition Condition;

        public ConditionPipelineState PipelineState;

        public ConditionParentPipeline()
        {
            _currentStep = 0;
            _pipelines = new List<ConditionPipeline>();
        }

        public void AddLast(ConditionPipeline pipeline)
        {
            _pipelines.Add(pipeline);
        }

        public void AddFirst(ConditionPipeline pipeline)
        {
            var list = new List<ConditionPipeline>();
            list.Add(pipeline);
            list.AddRange(_pipelines);

            _pipelines = new List<ConditionPipeline>(list);
            list = null;
        }

        public bool CheckConditions(bool stacked = false)
        {
            var result = stacked;
            //Allow for wildcard conditions
            if (_pipelines.Count == 0) return true;
            var conditionBox = new ConditionBox();
            while (HasNext())
            {
                result = _pipelines[_currentStep].DoFilter(_currentStep, Condition, result, conditionBox);
                _currentStep++;
            }

            if (conditionBox.ConditionObject is bool b) result = result || b;
            return result;
        }

        private bool HasNext()
        {
            return _pipelines.Count > _currentStep;
        }

        public void Reset()
        {
            _currentStep = 0;
        }
    }
}