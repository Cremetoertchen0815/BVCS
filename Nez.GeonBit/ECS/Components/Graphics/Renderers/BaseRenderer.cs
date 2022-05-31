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
// Implement basic functionality for components that render stuff.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit
{
	/// <summary>
	/// Base implementation for most graphics-related components.
	/// </summary>
	public abstract class BaseRendererComponent : GeonComponent
	{
		/// <summary>
		/// Get the main entity instance of this renderer.
		/// </summary>
		protected abstract BaseRenderableEntity RenderableEntity { get; }

		public override void OnAddedToEntity()
		{
			base.OnAddedToEntity();
			Node.AddEntity(RenderableEntity);
		}

		/// <summary>
		/// Set / get Entity blending state.
		/// </summary>
		public BlendState BlendingState
		{
			set => RenderableEntity.BlendingState = value;
			get => RenderableEntity.BlendingState;
		}

		/// <summary>
		/// Set / get the rendering queue of this entity.
		/// </summary>
		public virtual RenderingQueue RenderingQueue
		{
			get => RenderableEntity.RenderingQueue;
			set => RenderableEntity.RenderingQueue = value;
		}

		/// <summary>
		/// Copy basic properties to another component (helper function to help with Cloning).
		/// </summary>
		/// <param name="copyTo">Other component to copy values to.</param>
		/// <returns>The object we are copying properties to.</returns>
		public virtual Component CopyBasics(Component copyTo)
		{
			var otherRenderer = copyTo as BaseRendererComponent;
			otherRenderer.RenderingQueue = RenderingQueue;
			otherRenderer.BlendingState = BlendingState;
			return copyTo;
		}

		/// <summary>
		/// Called when GameObject turned disabled.
		/// </summary>
		public override void OnDisabled() => RenderableEntity.Visible = false;

		/// <summary>
		/// Called when GameObject is enabled.
		/// </summary>
		public override void OnEnabled() => RenderableEntity.Visible = true;
	}
}
