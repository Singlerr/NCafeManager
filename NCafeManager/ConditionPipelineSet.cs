namespace Conditions
{
    internal class ConditionPipelineSet
    {
        public ConditionParentPipeline ConditionAuthorParentPipeline;
        public ConditionParentPipeline ConditionKeywordParentPipeline;
        public ConditionParentPipeline ConditionTimeParentPipeline;

        public void Reset()
        {
            ConditionAuthorParentPipeline?.Reset();
            ConditionTimeParentPipeline?.Reset();
            ConditionKeywordParentPipeline?.Reset();
        }
    }

    internal enum ConditionPipelineState
    {
        Include,
        NotRequisite,
        Exclude
    }
}