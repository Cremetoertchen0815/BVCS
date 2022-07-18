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
    public class Saucer : GeonComponent
    {

        private const float SPEEN_SPEED = 3f;

        private BVGame _ownerScene;

        public override void OnAddedToEntity()
        {
            _ownerScene = Entity.Scene as BVGame ?? throw new Exception("Can only be added to a BVGame scene!");

            //Create renderer
            var renderer = Entity.AddComponentAsChild(new ModelRenderer("mesh/saucer"));
            renderer.Node.Scale = 0.05f * Vector3.One;
            renderer.Node.RotationX = -MathHelper.PiOver2;
            renderer.Node.Tween("RotationY", MathHelper.TwoPi, SPEEN_SPEED).SetFrom(0f).SetLoops(LoopType.RestartFromBeginning, -1).SetEaseType(EaseType.Linear).Start();

            Node.Position = new Vector3(0, 20, 0);
        }
    }
}
