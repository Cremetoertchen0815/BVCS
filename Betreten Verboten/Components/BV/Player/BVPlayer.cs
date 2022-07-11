using System.Linq;

namespace Betreten_Verboten.Components.BV.Player
{
    public abstract class BVPlayer : Base.Player
    {
        //ctor
        public BVPlayer(int Nr) : base(Nr) { }

        //Const
        public const int SCORE_PER_STEP = 10;
        public const int SCORE_PER_ANGER_BTN = 50;

        public virtual int AngerCount { get; set; } = 1;
        public int AdditionalPoints { get; set; } = 0;
        public override int Points => AdditionalPoints + AngerCount * SCORE_PER_ANGER_BTN + _figures.Sum(x => System.Math.Max(x.Position, 0)) * SCORE_PER_STEP;
    }
}
