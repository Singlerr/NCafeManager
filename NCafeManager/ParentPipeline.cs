using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipelines
{
    internal class ParentPipeline
    {
        private int _currentStep;

        private List<Pipeline> _pipelines;

        public ParentPipeline()
        {
            _currentStep = 0;
            _pipelines = new List<Pipeline>();
        }

        public void AddLast(Pipeline pipeline)
        {
            _pipelines.Add(pipeline);
        }

        public void AddFirst(Pipeline pipeline)
        {
            var list = new List<Pipeline>();
            list.Add(pipeline);
            list.AddRange(_pipelines);

            _pipelines = new List<Pipeline>(list);
            list = null;
        }

        public Task<object> Filter(object initialObj)
        {
            return new Task<object>(delegate
            {
                var current = initialObj;
                while (HasNext())
                {
                    var preCurrent = _pipelines[_currentStep].DoFilter(_currentStep, current);
                    preCurrent.Start();
                    preCurrent.Wait();
                    current = preCurrent.Result;
                    _currentStep++;
                }

                return current;
            });
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