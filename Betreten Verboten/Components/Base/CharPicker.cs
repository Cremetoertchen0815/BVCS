using Betreten_Verboten.Components.Base.Characters;
using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using Nez.GeonBit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base
{
    public class CharPicker : Component, IUpdatable
    {
        private const float MAX_OBJ_DISTANCE = 1000f;

        //Cached components
        private int _distance;
        private Player _owner;
        private Character[] _figures;
        private Camera3D _camera;
        private PhysicsWorld _world;

        //ctor
        public CharPicker(int distance) => _distance = distance;

        private static MaterialOverrides _materialSelectable = new MaterialOverrides() { DiffuseColor = Color.Red};
        private static MaterialOverrides _materialNeutral = new MaterialOverrides();

        public override void OnAddedToEntity()
        {
            //Fetch components
            _world = Entity.Scene.GetSceneComponent<PhysicsWorld>();
            _camera = (Entity.Scene as GeonScene)?.Camera ?? throw new Exception("Character picker has to be added to a Geon Scene!");
            _owner = Entity.GetComponent<Player>() ?? throw new Exception("Character picker has to be added to a Player Entity!");
            _figures = _owner.GetFigures();
            
            //Set material overrides
            foreach (var item in _figures) if (item.CanBeSelected) item.Renderer.MaterialOverride = _materialSelectable;
        }

        //Clear material overrides for characters
        public override void OnRemovedFromEntity() => Array.ForEach(_figures, x => x.Renderer.MaterialOverride = _materialNeutral);
        
        public void Update()
        {
            if (Input.LeftMouseButtonPressed)
            {
                var ray = _camera.RayFromMouse();
                var result = _world.Raycast(ray.Position, ray.Position + ray.Direction * MAX_OBJ_DISTANCE);
                if (result.HasHit)
                {
                    //Fetch clicked on character and check if valid
                    var character = result.Collision.CollisionBody.Entity.GetComponent<Character>();
                    if (character == null || !character.CanBeSelected || character.Owner != _owner) return;
                    //If yes, transmit traveling distance to selected character via telegram
                    _distance.SendPrivateObj("char_picker", character.TelegramSender, "char_move");
                    Entity.RemoveComponent(this); //Remove char picker as it fullfilled its purpose and is obsolete now
                }
            }
        }
    }
}
