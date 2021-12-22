using System.Threading.Tasks;

namespace Pipelines
{
    internal abstract class Pipeline
    {
        private ParentPipeline _pipelineColony;

        protected Pipeline(ParentPipeline colony)
        {
            _pipelineColony = colony;
        }

        public abstract Task<object> DoFilter(int step, object preFiltered);
    }
}