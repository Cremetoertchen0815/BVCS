using Betreten_Verboten.Components.Base.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.GeonBit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base
{
    public abstract class Player : Component, ITelegramReceiver
    {
        public int Nr { get; set; }
        public string ID { get; set; }
        public bool MissingTurn { get; set; }
        public abstract int Points { get; }
        public CharConfig CharacterConfig { get; set; }


        private Component[] _figures;
        private Player[] _otherPlayers;
        private BVBoard _board;

        public override void OnAddedToEntity()
        {
            //Fetch GeonScene & board
            var geonScene = (GeonScene)Entity.Scene;
            _board = geonScene.FindEntity("board").GetComponent<BVBoard>();

            //Create figures
            _figures = new Component[_board.FigureCount];
            for (int i = 0; i < _figures.Length; i++)
            {
                var pos = _board.GetFieldPosition(Nr, i, FieldType.Regular);
                var ent = geonScene.CreateGeonEntity("figure" + Nr + "-" + i, new Vector3(pos.X, 0, pos.Y), NodeType.BoundingBoxCulling);
                _figures[i] = ent.AddComponent(new Component());
            }

        }

        public abstract void MessageReceived(Telegram message);
    }
}
