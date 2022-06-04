namespace Nez.GeonBit
{
	public class GeonComponent : Component
	{

		public new GeonEntity Entity;

		[Inspectable]
		public Node Node;

		public virtual GeonComponent CopyBasics(GeonComponent c) => c;

		internal void Destroy() => Entity.RemoveComponent(this);

		public virtual void OnParentChange(Node from, Node to) { }
		public virtual void OnTransformationUpdate() { }
	}
}
