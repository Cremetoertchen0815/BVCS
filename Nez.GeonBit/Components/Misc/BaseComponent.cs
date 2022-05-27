using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit
{
    public class BaseComponent : Component
    {
        internal GeonNode Node;

        public override void OnAddedToEntity()
        {
            Node = Entity.GetComponent<GeonNode>();
        }

        public virtual BaseComponent  CopyBasics(BaseComponent c)
        {
            return c;
        }

        internal void Destroy()
        {

        }

        public virtual void OnParentChange(GeonNode from, GeonNode to) { }
        public virtual void OnTransformationUpdate() { }
    }
}
