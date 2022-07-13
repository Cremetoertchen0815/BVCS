using Betreten_Verboten.Components.Base.Characters;
using System.Collections.Generic;
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
        public const int SACRIFICE_COOLDOWN = 50;

        private int _sacrificeCount = 0;

        public virtual int AngerCount { get; set; } = 1;
        public int AdditionalPoints { get; set; } = 0;
        public bool Sacrificable { get => _sacrificeCount < 1; set { if (value) _sacrificeCount--; else _sacrificeCount = SACRIFICE_COOLDOWN; } }
        public override int Points => AdditionalPoints + AngerCount * SCORE_PER_ANGER_BTN + _figures.Sum(x => System.Math.Max(x.Position, 0)) * SCORE_PER_STEP;


        public IEnumerable<Character> GetSacrificableFigures() => _figures.Where(x => x.Position > -1 && x.Position < Board.DistanceLimit);
    
    }
}
