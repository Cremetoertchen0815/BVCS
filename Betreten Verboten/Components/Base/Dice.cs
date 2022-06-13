using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Betreten_Verboten.GlobalFields;

namespace Betreten_Verboten.Components.Base
{
    internal class Dice : GeonComponent, IUpdatable
    {
        private const float cMeasureSpeed = 0.02f;


        private RigidBody _rigidBody;
        //Surface normals of dice faces, beginning with 1 and ending with 6
        private Vector3[] _sideNormals = { Vector3.Forward, Vector3.Down, Vector3.Backward, Vector3.Up, Vector3.Right, Vector3.Left };
        private bool _isDoneRolling = false;


        public override void OnAddedToEntity()
        {
            var renderer = Entity.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Cube));
            renderer.SetMaterial(new BasicMaterial() { Texture = Entity.Scene.Content.LoadTexture("mesh/dice_cubemap"), TextureEnabled = true  });
            renderer.Node.Scale = new Vector3(30f);
            _rigidBody = Entity.AddComponent(new RigidBody(new BoxInfo(new Vector3(60f)), 20f, 0.1f));
            _rigidBody.Gravity = new Vector3(0, -350, 0);
            _rigidBody.SetDamping(0.20f, 0.5f);
            _rigidBody.Restitution = 0f;
            _rigidBody.Scale = new Vector3(0.07f);
            _rigidBody.Position = Entity.Node.Position;
            _rigidBody.LinearVelocity = new Vector3(Nez.Random.NextFloat(), 0, Nez.Random.NextFloat()) * 45 * 0.5f;
            _rigidBody.AngularVelocity = new Vector3(Nez.Random.NextFloat(), 0, Nez.Random.NextFloat()) * 15 * 0.5f;

        }

        public void Update()
        {
            //Check if the dice stopped rolling and if yes, calculate the number
            if (_rigidBody.LinearVelocity.Length() < cMeasureSpeed && !_isDoneRolling)
            {
                _isDoneRolling = true;
                var nr = GetDiceTopNumber();
                Console.WriteLine(nr);
            }
        }

        /// <summary>
        /// Calculates the face index of the face matching the target normal closest, with the target normal pointing upwards.
        /// </summary>
        /// <returns>The number on the top face of the dice.</returns>
        private int GetDiceTopNumber()
        {
            var floatDots = new (float dot, int index)[_sideNormals.Length]; //Store dice face dot products with their indices
            var norm = Vector3.TransformNormal(Vector3.Up, Matrix.Invert(_rigidBody.Entity.Node.WorldTransformations)); //Translate target normal into object space
            //Calculate dot product between translated target normal and surface normal. Bigger means closer to the target normal.
            for (int i = 0; i < _sideNormals.Length; i++) floatDots[i] = (Vector3.Dot(norm, _sideNormals[i]), i);
            //Effeciently calculate the face that is closest to matching the target normal and return the face index.
            return floatDots.Aggregate((x, y) => x.dot > y.dot ? x : y).index + 1;
        }
    }
}
