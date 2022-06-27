using Betreten_Verboten.Components.Base.Characters;
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
        private Player _owner;
        private Character[] _figures;

        private static MaterialOverrides _materialAvailable;
        private static MaterialOverrides _materialHover;

        public override void OnAddedToEntity()
        {
            //Fetch components
            _owner = Entity.GetComponent<Player>() ?? throw new Exception("Character picker has to be added to a Player Entity!");
            _figures = _owner.GetFigures();
            
            //Set material overrides
            foreach (var item in _figures) item.Renderer.MaterialOverride = _materialAvailable;
        }

        //Clear material overrides for characters
        public override void OnRemovedFromEntity() => Array.ForEach(_figures, x => x.Renderer.MaterialOverride = null);
        
        public void Update()
        {
            var loos = new PhysicsWorld();
            loos.
        }
    }
}
