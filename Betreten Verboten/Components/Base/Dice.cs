using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using System.Collections.Generic;
using System.Linq;

namespace Betreten_Verboten.Components.Base
{
    internal class Dice : GeonComponent, IUpdatable
    {
        private const float MEASURE_SPEED = 0.5f;
        public const int ENTITY_TAG = 15;
        public const float ANGLE_REST_LIMIT = 0.95f;


        private RigidBody _rigidBody;
        //Surface normals of dice faces, beginning with 1 and ending with 6
        private readonly Vector3[] _sideNormals = { Vector3.Forward, Vector3.Down, Vector3.Backward, Vector3.Up, Vector3.Right, Vector3.Left };
        private bool _isDoneRolling = false;



        public override void OnAddedToEntity()
        {
            var renderer = Entity.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Cube));
            renderer.SetMaterial(new BasicMaterial() { Texture = Entity.Scene.Content.LoadTexture("mesh/dice_cubemap"), TextureEnabled = true });
            renderer.Node.Scale = new Vector3(30f);
            Entity.Node.Rotation = new Vector3(Random.NextAngle(), Random.NextAngle(), Random.NextAngle());
            _rigidBody = Entity.AddComponent(new RigidBody(new BoxInfo(new Vector3(60f)), 8f, 0.1f));
            _rigidBody.CopyNodeWorldMatrix(true);
            _rigidBody.SetDamping(0.7f, 0.65f);
            _rigidBody.Restitution = 0.2f;
            _rigidBody.Scale = new Vector3(0.07f);
            _rigidBody.Position = Entity.Node.Position;
            _rigidBody.Enabled = true;
            _rigidBody.AngularVelocity = new Vector3(Random.MinusOneToOne(), Random.MinusOneToOne(), Random.MinusOneToOne()) * Random.Range(50f, 80f);
            _rigidBody.LinearVelocity = new Vector3(Random.MinusOneToOne(), 0, Random.MinusOneToOne()) * Random.Range(20f, 50f);

        }

        public void Update()
        {
            if (_isDoneRolling) return;

            var norm = Vector3.TransformNormal(Vector3.Up, Matrix.Invert(_rigidBody.Entity.Node.WorldTransformations)); norm.Normalize();
            float maxDot = _sideNormals.Select(x => Vector3.Dot(norm, x)).Max(); //Determins if the dice lays flat(is still when over rest limit)

            //Check if the dice stopped rolling and if yes, calculate the number
            if (_rigidBody.LinearVelocity.Length() < MEASURE_SPEED && maxDot > ANGLE_REST_LIMIT)
            {
                _isDoneRolling = true;
                Core.Schedule(0.2f, x => GetDiceTopNumber().SendPrivateObj("dice", "base", "dice_value_set"));
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


        public static void Throw(GeonScene scene) => scene.CreateGeonEntity("dice", new Vector3(-500 + Random.MinusOneToOne() * 5, Random.Range(25, 40), -500 + Random.MinusOneToOne() * 5), NodeType.BoundingBoxCulling).SetTag(ENTITY_TAG).AddComponent(new Dice());

        /// <summary>
        /// Returns if another dice roll is necessary, depending on if the player can roll thrice.
        /// The dice should reroll if none of the following conditions is satisfied:
        /// 1. You can't roll thrice & you didn't roll a 6. (regular on-board roll)
        /// 2. You can roll thrice, your last roll was indeed a 6 and this one is not. (Positive roll-thrice exit condition)
        /// 3. You can roll thrice and already did, but even your last one isn't a 6. (Negative roll-thrice exit condition)
        /// </summary>
        /// <param name="nrs"></param>
        /// <param name="RollThrice"></param>
        /// <returns></returns>
        public static bool ShouldReroll(List<int> nrs, bool RollThrice) => !(!RollThrice && nrs[nrs.Count - 1] < 6 || RollThrice && nrs.Count > 1 && nrs[nrs.Count - 1] < 6 && nrs[nrs.Count - 2] >= 6 || RollThrice && nrs.Count >= 3 && nrs[nrs.Count - 1] < 6);
    }
}
