using Microsoft.Xna.Framework;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base
{
    internal class Dice : GeonComponent
    {
        public override void OnAddedToEntity()
        {
            var renderer = Entity.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Cube));
            renderer.SetMaterial(new ReflectiveMaterial() { DiffuseColor = Color.DarkOrchid,  });
            renderer.Node.Scale = new Vector3(25f);
            var rb = Entity.AddComponentAsChild(new RigidBody(new BoxInfo(new Vector3(50f)), 20f, 0.20f));
            rb.Gravity = new Vector3(0, -350, 0);
            rb.SetDamping(0.20f, 0.5f);
            rb.Restitution = 0.1f;
            rb.Scale = new Vector3(0.1f);
            rb.Position = Entity.Node.Position;
            rb.LinearVelocity = new Vector3(Nez.Random.NextFloat(), 0, Nez.Random.NextFloat()) * 25;
            rb.AngularVelocity = new Vector3(Nez.Random.NextFloat(), 0, Nez.Random.NextFloat()) * 10;
        }
    }
}
