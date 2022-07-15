using Betreten_Verboten.Components.Base;
using Betreten_Verboten.Components.Base.Characters;
using System.Collections.Generic;
using System.Linq;

namespace Betreten_Verboten.Components.BV.Player
{
    /// <summary>
    /// Extends the regular player class to store information specific to the "Betreten Verboten" play mode.
    /// </summary>
    public abstract class BVPlayer : Base.Player
    {
        //ctor
        public BVPlayer(int Nr) : base(Nr) => CharacterConfig = new CharConfig() { Name = "Player " + (Nr + 1) };

        //Const
        public const int SCORE_PER_STEP = 10;
        public const int SCORE_PER_ANGER_BTN = 50;
        public const int SACRIFICE_COOLDOWN = 50;

        //Fields
        private int _sacrificeCount = 0; //Cooldown counter for sacrificing

        //Static fields
        private static List<GlobalPosition> _suicideFields = new List<GlobalPosition>(); //Stores the suicide fields of all players, so there are no duplicates

        /// <summary>
        /// Stores the number of anger buttons the player owns.
        /// </summary>
        public virtual int AngerCount { get; set; } = 5;

        /// <summary>
        /// Keeps track of any points that additionally get added to the final score, like for example Kick bonus or sacrifice bonus.
        /// </summary>
        public int AdditionalPoints { get; set; } = 0;

        /// <summary>
        /// Returns if the player is allowed to sacrifice yet. Set to true if you want to tick the cooldown counter, false if you want to reset it.
        /// </summary>
        public bool Sacrificable { get => _sacrificeCount < 1; set { if (value) _sacrificeCount--; else _sacrificeCount = SACRIFICE_COOLDOWN; } }

        /// <summary>
        /// Notes if the player shall skip one round due to sacrifice reasons.
        /// </summary>
        public bool SkipRound { get; set; } = false;

        /// <summary>
        /// Stores the position(-1 if not yet set) of a field, which kills the player and their team mates upon entering.
        /// </summary>
        public int SuicideField { get; set; } = -1;

        /// <summary>
        /// Returns the total of the player's points(singularly, team members are not counted in)
        /// </summary>
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

        /// <summary>
        /// Returns all figures of the player that can be sacrificed
        /// </summary>
        public IEnumerable<Character> GetSacrificableFigures() => _figures.Where(x => x.Position > -1 && x.Position < Board.FieldCountTotal);

        /// <summary>
        /// Returns all figures of the player that are in the house(lol).
        /// </summary>
        public IEnumerable<Character> GetHouseFigures() => _figures.Where(x => x.Position >= Board.FieldCountTotal && x.Position < Board.DistanceLimit);
    
    }
}
