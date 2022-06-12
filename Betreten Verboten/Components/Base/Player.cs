using Betreten_Verboten.Components.Base.Characters;
using Microsoft.Xna.Framework.Audio;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base
{
    public abstract class Player : Component, ITelegramReceiver
    {
        public string ID { get; set; }
        public bool MissingTurn { get; set; }
        public abstract int Points { get; }
        public CharConfig CharacterConfig { get; set; }


        private Component[] _figures;
        private Player[] _otherPlayers;

        public abstract void MessageReceived(Telegram message);
    }
}
