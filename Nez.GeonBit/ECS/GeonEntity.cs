using Microsoft.Xna.Framework;
using System;

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
			component.Node.Entity = this;
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
			if (component is GeonComponent g)
			{
				g.Entity = this;
				g.Node = Node;
			}
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



		/// <summary>
		/// creates a deep clone of this Entity. Subclasses can override this method to copy any custom fields. When overriding,
		/// the CopyFrom method should be called which will clone all Components, Colliders and Transform children for you. Note
		/// that the cloned Entity will not be added to any Scene! You must add them yourself!
		/// </summary>
		public GeonEntity Clone(Vector3 position = default)
		{
			var entity = Activator.CreateInstance(GetType()) as GeonEntity;
			entity.Name = Name + "(clone)";
			entity.CopyFrom(this);
			entity.Node.Position = position;
			entity.Node.Entity = this;

			return entity;
		}
	}
}
