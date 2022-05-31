namespace Nez.GeonBit
{
    public class GeonComponent : Component
    {
        public Node Node { get; internal set; }

        public override void OnAddedToEntity() => Node = Node ?? Entity.GetComponent<Node>() ?? throw new System.Exception("Entity must have a GeonNode component!");

        public override void OnRemovedFromEntity() => Node?.RemoveComponent(this);

		public virtual GeonComponent CopyBasics(GeonComponent c) => c;

        internal void Destroy() => Entity.RemoveComponent(this);

        public virtual void OnParentChange(Node from, Node to) { }
        public virtual void OnTransformationUpdate() { }
    }
}
