namespace Nez.GeonBit
{
	public class GeonEntity : Entity
	{
		public new GeonScene Scene;
		public Node Node;

		public GeonEntity(string Name) : base(Name) { }

		/// <summary>
		/// Adds a Component to the components list. Returns the Component.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <param name="component">Component.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T AddGeonComponent<T>(T component, bool registerAsChildNode) where T : GeonComponent
		{
			component.Entity = this;
			component.Node = registerAsChildNode ? Node.AddChildNode(new Node()) : Node;
			Components.Add(component);
			component.Initialize();

			return component;
		}


		/// <summary>
		/// Gets the first component of type T and returns it. If no components are found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetGeonComponent<T>() where T : GeonComponent => Components.GetComponent<T>(false);

		public T GetGeonComponent<T>(string name) where T : GeonComponent => Components.GetComponent<T>(name, false);
	}
}
