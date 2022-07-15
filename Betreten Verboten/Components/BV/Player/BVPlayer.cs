using Betreten_Verboten.Components.Base;
using Betreten_Verboten.Components.Base.Characters;
using System.Collections.Generic;
using System.Linq;

namespace Betreten_Verboten.Components.BV.Player
{
    public abstract class BVPlayer : Base.Player
    {
        //ctor
        public BVPlayer(int Nr) : base(Nr) => CharacterConfig = new CharConfig() { Name = "Player " + (Nr + 1) };

        //Const
        public const int SCORE_PER_STEP = 10;
        public const int SCORE_PER_ANGER_BTN = 50;
        public const int SACRIFICE_COOLDOWN = 50;

        //Fields
        private int _sacrificeCount = 0;

        //Static fields
        private static List<GlobalPosition> _suicideFields = new List<GlobalPosition>(); //Stores the suicide fields of all players, so there are no duplicates

        /// <summary>
        /// Stores the number of anger buttons the player owns.
        /// </summary>
        public virtual int AngerCount { get; set; } = 5;
        public int AdditionalPoints { get; set; } = 0;
        public bool Sacrificable { get => _sacrificeCount < 1; set { if (value) _sacrificeCount--; else _sacrificeCount = SACRIFICE_COOLDOWN; } }
        public bool SkipRound { get; set; } = false;
        public int SuicideField { get; set; } = -1;
        public override int Points => AdditionalPoints + AngerCount * SCORE_PER_ANGER_BTN + _figures.Sum(x => System.Math.Max(x.Position, 0)) * SCORE_PER_STEP;

        /// <summary>
        /// Checks if a suicide field needs to be created & does so if necessary
        /// </summary>
        public void CheckForSuicideField()
        {
            IEnumerable<Character> houseFigs;
            if (SuicideField > -1 || (houseFigs = GetHouseFigures()).Count() < Board.FigureCountPP - 1) return;
            while (true)
            {
                SuicideField = Nez.Random.Range(0, Board.FieldCountTotal);
                var glob = GlobalPosition.FromLocalPosition(this, SuicideField);
                if (!glob.Valid || _suicideFields.Contains(glob)) return;
                _suicideFields.Add(glob);
                break;
            }
        }

        public IEnumerable<Character> GetSacrificableFigures() => _figures.Where(x => x.Position > -1 && x.Position < Board.FieldCountTotal);
        public IEnumerable<Character> GetHouseFigures() => _figures.Where(x => x.Position >= Board.FieldCountTotal && x.Position < Board.DistanceLimit);
    
    }
}
