using System.Collections.Generic;


namespace Nez.AI.UtilityAI
{
    /// <summary>
    /// Scores by summing child Appraisals until a child scores below the threshold
    /// </summary>
    public class ThresholdConsideration<T> : IConsideration<T>
    {
        public float Threshold;

        public IAction<T> Action { get; set; }

        private List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();


        public ThresholdConsideration(float threshold) => Threshold = threshold;


        float IConsideration<T>.GetScore(T context)
        {
            float sum = 0f;
            for (int i = 0; i < _appraisals.Count; i++)
            {
                float score = _appraisals[i].GetScore(context);
                if (score < Threshold)
                    return sum;

                sum += score;
            }

            return sum;
        }
    }
}