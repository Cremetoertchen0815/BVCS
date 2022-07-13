using Betreten_Verboten.Components.Base.Characters;
using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using System.Collections.Generic;
using System.Linq;

namespace Betreten_Verboten.Components.Base
{
    public abstract class Player : Component, ITelegramReceiver
    {
        public int Nr { get; set; }
        public string ID { get; set; }
        public bool MissingTurn { get; set; }
        public abstract int Points { get; }
        public string TelegramSender => "player_" + Nr;
        public CharConfig CharacterConfig { get; set; }


        protected Character[] _figures;
        public BVBoard Board { get; private set; }

        //ctor
        public Player(int Nr) => this.Nr = Nr;

        public override void OnAddedToEntity()
        {
            //Fetch GeonScene & board
            var geonScene = (GeonScene)Entity.Scene;
            Board = geonScene.FindEntity("board").GetComponent<BVBoard>();

            //Create figures
            _figures = new Character[Board.FigureCountPP];
            for (int i = 0; i < _figures.Length; i++)
            {
                var ent = geonScene.CreateGeonEntity("char" + Nr + "_" + i, new Vector3(0, Character.CHAR_HITBOX_HEIGHT - 1f, 0), NodeType.BoundingBoxCulling);
                _figures[i] = ent.AddComponent(new Character(this, i, CharacterConfig)).SetPosition(-1);
            }

            //Register in telegram service
            this.TeleRegister("players");
            this.SendPublicTele("player_registered", this);
        }

        public override void OnRemovedFromEntity() => this.TeleDeregister();

        public abstract void MessageReceived(Telegram message);

        public Character[] GetFigures() => _figures;

        /// <summary>
        /// Decides on the next course of action after a successful roll.
        /// </summary>
        /// <param name="nrs">Values of the rolled dices.</param>
        public void DecideAfterDiceroll(List<int> nrs)
        {
            //Calculate conditions & helper variables
            int homebaseNr = _figures.Aggregate(-1, (a, b) => (b.Position < 0 && a < 0) ? b.Nr : a); //Stores the ID of a figure that is still in the homebase, -1 if none are
            int distance = nrs.Sum(), trueDistance;
            bool is6InDicelist = nrs.Contains(6); //Stores whether any 6es were rolled
            bool isBasefieldBlocked = IsFieldBlocked(0, out int baseBlocker);
            bool isTrueDestinationBlocked = IsFieldBlocked(trueDistance = GetTrueLaunchDistance(nrs), out int _);
            Character[] movableChars;

            if (is6InDicelist && homebaseNr > -1 && !isTrueDestinationBlocked && !isBasefieldBlocked)
            {
                //If we rolled a 6, the base and out destination is not blocked and we got a figure in our home, move with it! It has top priority!
                this.SendPrivateTele("base", "show_tutorial", "Move Character out of your homebase and move him " + trueDistance + " spaces!");
                _figures[homebaseNr].AdvanceSteps(trueDistance);
            } else if(is6InDicelist && homebaseNr > -1 && !isTrueDestinationBlocked)
            {
                //Check if we had the potential to move out of the house, but our base field was blocked!
                //The blocking character standing on the base field can & has to be moved out of the way
                this.SendPrivateTele("base", "show_tutorial", "Start field blocked! Move pieces out of the way first!");
                _figures[baseBlocker].AdvanceSteps(trueDistance);
            } else if (!(movableChars = GetMovableFigures(distance)).Any() || !is6InDicelist && GetHomebaseFigCount() == Board.FigureCountPP)
            {
                //If a regular move is not possible or all figures are in the home and no 6 was rolled, no move is possible!
                this.SendPrivateTele("base", "show_tutorial", "No move possible!");
                Core.Schedule(1f, x => this.SendPrivateTele("base", "char_move_done", null));
            } else if (movableChars.Length == 1)
            {
                //If only one regular figure is movable, move it automatically!
                this.SendPrivateTele("base", "show_tutorial", "Auto-moving character " + distance + " spaces!");
                _figures[movableChars[0].Nr].AdvanceSteps(distance);
            }
            else //If arrived here, no special action is necessairy and figure to be moved can be selected.
            {
                foreach (var item in _figures) item.CanBeSelected = movableChars.Contains(item); //Make the right figures selectable
                Entity.AddComponent(new CharPicker(x => distance.SendPrivateObj("char_picker", x.TelegramSender, "char_move"))); //Open the character picker to choose the traveling distance
            }

        }

        /// <summary>
        /// Returns the traveling distance from the dice array, ignoring the magic launching number 6 and any number that predates it.
        /// </summary>
        /// <param name="nrs">The dice array.</param>
        public int GetTrueLaunchDistance(List<int> nrs)
        {
            int sum = 0;
            bool was6AlreadyPresent = false;
            for (int i = 0; i < nrs.Count; i++)
            {
                int nr = nrs[i];
                if (was6AlreadyPresent) sum += nr;
                if (nr == 6) was6AlreadyPresent = true;
            }

            return sum + 1;
        }

        /// <summary>
        /// Return true when the player is allowed to roll at least thrice to get a little help leaving his home.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanRollThrice()
        {
            var coveredHouseFields = new List<int>();
            for (int i = 0; i < Board.FigureCountPP; i++)
            {
                if (IsFigureOnTrack(i)) return false; //If figure is on the track, rolling thrice isn't allowed
                if (_figures[i].Position >= Board.FieldCountTotal) coveredHouseFields.Add(_figures[i].Position); //If figure is in house, remember covered field
            }

            //If the end of the house is not continuously covered, rolling thrice also isn't allowed
            for (int i = -1; i >= -coveredHouseFields.Count; i--) if (!coveredHouseFields.Contains(Board.FieldCountTotal + Board.FigureCountPP - i)) return false;
            return true;
        }

        /// <summary>
        /// Returns true if the potential figure would be allowed to move.
        /// </summary>
        /// <param name="pos">The current position of the potential figure.</param>
        /// <param name="distance">The distance the potential figure would travel next move.</param>
        private bool CanMove(int pos, int distance) => pos > -1 && pos + distance < Board.DistanceLimit && !IsFieldBlocked(pos + distance, out var _) && !IsOvertakingInHouse(pos, distance);

        /// <summary>
        /// Returns the figures the player could possibly move with.
        /// </summary>
        /// <param name="distance">The distance the potential figure would travel next move.</param>
        private Character[] GetMovableFigures(int distance) => _figures.Where(x => CanMove(x.Position, distance)).ToArray();
        /// <summary>
        /// Returns true if the specified figure is on the track, that means moving between home and house.
        /// </summary>
        /// <param name="figure">The ID of the figure to be checked.</param>
        private bool IsFigureOnTrack(int figure) => _figures[figure].Position >= 0 && _figures[figure].Position < Board.FieldCountTotal;
       
        /// <summary>
        /// Checks if a potential figure would overtake one of his siblings in the player's house, which is illegal.
        /// </summary>
        /// <param name="pos">The current position of the potential figure.</param>
        /// <param name="distance">The distance the potential figure would travel next move.</param>
        private bool IsOvertakingInHouse(int pos, int distance)
        {
            if (distance < Board.DistanceLimit) return false;
            for (int i = 0; i < distance; i++) if (IsFieldBlocked(pos + distance + 1, out var _)) return true;
            return false;
        }

        /// <summary>
        /// Returns the number of figures still present in the homebase.
        /// </summary>
        private int GetHomebaseFigCount() => _figures.Where(x => x.Position < 0).Count();

        /// <summary>
        /// Returns true when one of its own figures is standing of the specified field.
        /// </summary>
        /// <param name="field">The local field to be inspected.</param>
        /// <returns></returns>
        private bool IsFieldBlocked(int field, out int who, int ignoredFigure = -1)
        {
            var res = _figures.Where(x => x.Position == field && x.Nr != ignoredFigure);
            var any = res.Any();
            who = any ? res.ElementAt(0).Nr : -1;
            return any;
        }
    }
}
