using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Betreten_Verboten.Scenes.Main;
using Nez.GeonBit;
using Microsoft.Xna.Framework;
using Nez.Tweens;

namespace Betreten_Verboten.Components.BV
{
    public class Saucer : GeonComponent, ITelegramReceiver
    {

        private const float SPEEN_SPEED = 3f;

        private BVGame _ownerScene;
        private int _moveCounter;

        public string TelegramSender => "saucer";

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "player_change":
                    if (++_moveCounter >= _ownerScene.Board.SaucerSpawnRate)
                    {
                        _moveCounter = 0;
                        SpawnField();
                    }

                    break;
                case "char_move_done":
                    break;
            }
        }

        private void SpawnField()
        {
            int tryCount = 0;
            int nr = Nez.Random.Range(0, _ownerScene.Board.FieldCountTotal);
            var lst = _ownerScene.Board.SaucerFields;
            while (lst.Contains(nr))
            {
                if (++tryCount > _ownerScene.Board.FieldCountTotal) return;
                nr = Nez.Random.Range(0, _ownerScene.Board.FieldCountTotal);
            }
            lst.Add(nr);
        }

        public override void OnAddedToEntity()
        {
            _ownerScene = Entity.Scene as BVGame ?? throw new Exception("Can only be added to a BVGame scene!");

            //Create renderer
            var renderer = Entity.AddComponentAsChild(new ModelRenderer("mesh/saucer"));

            //Scale him and make him speen
            renderer.Node.Scale = 0.05f * Vector3.One;
            renderer.Node.RotationX = -MathHelper.PiOver2;
            renderer.Node.Tween("RotationY", MathHelper.TwoPi, SPEEN_SPEED).SetFrom(0f).SetLoops(LoopType.RestartFromBeginning, -1).SetEaseType(EaseType.Linear).Start();
            renderer.Enabled = false;

            this.TeleRegister();
        }
    }
}
