#region LICENSE
//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------
#endregion
#region File Description
//-----------------------------------------------------------------------------
// A component that renders a 3D bounding-box around this GameObject.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion

namespace Nez.GeonBit
{
	/// <summary>
	/// This component renders a bounding-box around this GameObject.
	/// </summary>
	public class BoundingBoxRenderer : BaseRendererComponent, IUpdatable
	{
		// the entity used to draw the model
		private readonly BoundingBoxEntity _entity;

		/// <summary>
		/// Get the main entity instance of this renderer.
		/// </summary>
		protected override BaseRenderableEntity RenderableEntity => _entity;

		/// <summary>
		/// Create the bounding-box renderer component.
		/// </summary>
		public BoundingBoxRenderer() => _entity = new BoundingBoxEntity();

		/// <summary>
		/// Clone this component.
		/// </summary>
		/// <returns>Cloned copy of this component.</returns>
		public override Component CopyBasics(Component copyTo) => CopyBasics(new BoundingBoxRenderer());

		/// <summary>
		/// Called every frame to do the component events.
		/// </summary>
		public void Update()
		{
			// update bounding box
			if (Node != null)
			{
				_entity.Box = Node.GetBoundingBox();
			}
		}
	}
}
