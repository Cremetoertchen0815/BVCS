using System.Collections.Generic;


namespace Nez.AI.UtilityAI
{
    /// <summary>
    /// Scores by summing the score of all child Appraisals
    /// </summary>
    public class SumOfChildrenConsideration<T> : IConsideration<T>
    {
        public IAction<T> Action { get; set; }

        private List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();


        float IConsideration<T>.GetScore(T context)
        {
            float score = 0f;
            for (int i = 0; i < _appraisals.Count; i++)
                score += _appraisals[i].GetScore(context);

            return score;
        }
    }
}