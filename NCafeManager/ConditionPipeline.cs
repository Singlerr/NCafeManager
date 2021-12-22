using System.Threading.Tasks;
using Pipelines;

namespace Conditions
{
    internal abstract class ConditionPipeline : Pipeline
    {
        protected ConditionPipeline(ParentPipeline pipeline) : base(pipeline)
        {
        }

        public override Task<object> DoFilter(int step, object preFiltered)
        {
            return new Task<object>(() => DoFilter(step, preFiltered is bool stacked ? stacked : false));
        }

        public abstract bool DoFilter(int step, Condition cond, bool stacked, ConditionBox box);
    }
}