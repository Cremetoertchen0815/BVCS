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
		public T AddComponentAsChild<T>(T component) where T : GeonComponent
		{
			component.Entity = this;
			component.Node = Node.AddChildNode(new Node());
			Components.Add(component);
			component.Initialize();

			return component;
		}



		/// <summary>
		/// Adds a Component to the components list. Returns the Component.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <param name="component">Component.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public override T AddComponent<T>(T component)
		{
			component.Entity = this;
			if (component is GeonComponent g) g.Node = Node;
			Components.Add(component);
			component.Initialize();
			return component;
		}

		/// <summary>
		/// Adds a Component to the components list. Returns the Component.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public override T AddComponent<T>() => AddComponent(new T { Entity = this });
	}
}
