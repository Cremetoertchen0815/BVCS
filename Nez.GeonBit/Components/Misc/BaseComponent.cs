namespace Nez.GeonBit
{
    public class BaseComponent : Component
    {
        internal GeonNode Node;

        public override void OnAddedToEntity() => Node = Entity.GetComponent<GeonNode>() ?? throw new System.Exception("Entity must have a GeonNode component!");

        public override void OnRemovedFromEntity() => Node?.RemoveComponent(this);

		public virtual BaseComponent CopyBasics(BaseComponent c) => c;

        internal void Destroy()
        {
            Entity.RemoveComponent(this);
        }

        public virtual void OnParentChange(GeonNode from, GeonNode to) { }
        public virtual void OnTransformationUpdate() { }
    }
}
