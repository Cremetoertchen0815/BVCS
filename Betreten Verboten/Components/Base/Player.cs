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
            _figures = new Character[Board.FigureCount];
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
            bool is6InDicelist = nrs.Contains(6); //Stores whether any 6es were rolled
            int homebaseNr = _figures.Aggregate(-1, (a, b) => (b.Position < 0 && a < 0) ? b.Nr : a); //Stores the ID of a figure that is still in the homebase, -1 if none are
            bool isBasefieldBlocked = IsFieldBlocked(0);
            int distance = nrs.Sum();

            Entity.AddComponent(new CharPicker(distance)); //Open the character picker to choose the traveling distance
        }

        /// <summary>
        /// Return true when the player is allowed to roll at least thrice to get a little help leaving his home.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanRollThrice()
        {
            var coveredHouseFields = new List<int>();
            for (int i = 0; i < Board.FigureCount; i++)
            {
                int pos = _figures[i].Position;
                if (pos >= 0 && pos < Board.FieldCount) return false; //If figure is on the track, rolling thrice isn't allowed
                if (pos >= Board.FieldCount) coveredHouseFields.Add(pos); //If figure is in house, remember covered field
            }

            //If the end of the house is not continuously covered, rolling thrice also isn't allowed
            for (int i = -1; i >= -coveredHouseFields.Count; i--) if (!coveredHouseFields.Contains(Board.FieldCount + Board.FigureCount - i)) return false;
            return true;
        }

        /// <summary>
        /// Returns true when on of its own figures is standing of the specified field.
        /// </summary>
        /// <param name="field">The local field to be inspected.</param>
        /// <returns></returns>
        private bool IsFieldBlocked(int field) => _figures.Where(x => x.Position == field).Count() > 0;
    }
}
