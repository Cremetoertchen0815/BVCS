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
// A component that renders a composite 3D model.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nez.GeonBit
{
    /// <summary>
    /// This component renders a 3D model but also provide a fine control over its internal meshes.
    /// Its slightly slower than SimpleModelRenderer and ModelRenderer, but provide more control and better distance-from-camera sorting for transparency.
    /// </summary>
    public class CompositeModelRenderer : BaseRendererComponent
    {
        /// <summary>
        /// The entity from the core layer used to draw the model.
        /// </summary>
        protected CompositeModelEntity _entity;

        /// <summary>
        /// Get the main entity instance of this renderer.
        /// </summary>
        protected override BaseRenderableEntity RenderableEntity => _entity;

        /// <summary>
        /// Set the rendering queue of for all meshes in the composite model.
        /// </summary>
        public override RenderingQueue RenderingQueue
        {
            set
            {
                foreach (var mesh in GetMeshes())
                {
                    mesh.RenderingQueue = value;
                }
            }
        }

        /// <summary>
        /// Return meshes count.
        /// </summary>
        public int MeshesCount => _entity.MeshesCount;

        /// <summary>
        /// Get mesh entity by index.
        /// </summary>
        /// <param name="index">Mesh index to get.</param>
        /// <returns>MeshEntity instance for this mesh.</returns>
        public MeshEntity GetMesh(int index) => _entity.GetMesh(index);

        /// <summary>
        /// Get mesh entity by name.
        /// </summary>
        /// <param name="name">Mesh name to get.</param>
        /// <returns>MeshEntity instance for this mesh.</returns>
        public MeshEntity GetMesh(string name) => _entity.GetMesh(name);

        /// <summary>
        /// Get all meshes as a list.
        /// </summary>
        /// <returns>List of MeshEntity instances in this composite model.</returns>
        public List<MeshEntity> GetMeshes() => _entity.GetMeshes();

        /// <summary>
        /// Return a list with all materials in model.
        /// Note: if alternative materials are set, will return them.
        /// Note2: prevent duplications, eg if even if more than one part uses the same material it will only return it once.
        /// </summary>
        /// <returns>List of materials.</returns>
        public System.Collections.Generic.List<Materials.MaterialAPI> GetMaterials() => _entity.GetMaterials();

        /// <summary>
        /// Protected constructor without params to use without creating entity, for inheriting classes.
        /// </summary>
        protected CompositeModelRenderer()
        {
        }

        /// <summary>
        /// Create the model renderer component.
        /// </summary>
        /// <param name="model">Model to draw.</param>
        public CompositeModelRenderer(Model model) => _entity = new CompositeModelEntity(model);

        /// <summary>
        /// Copy basic properties to another component (helper function to help with Cloning).
        /// </summary>
        /// <param name="copyTo">Other component to copy values to.</param>
        /// <returns>The object we are copying properties to.</returns>
        public override Component CopyBasics(Component copyTo)
        {
            var other = copyTo as CompositeModelRenderer;
            return base.CopyBasics(other);
        }

        /// <summary>
        /// Clone this component.
        /// </summary>
        /// <returns>Cloned copy of this component.</returns>
        public override Component Clone()
        {
            var ret = new CompositeModelRenderer(_entity.Model);
            CopyBasics(ret);
            for (int i = 0; i < _entity.MeshesCount; ++i)
            {
                var other = ret.GetMesh(i);
                var self = GetMesh(i);
                other.MaterialOverride = self.MaterialOverride.Clone();
                other.BlendingState = self.BlendingState;
                other.SetMaterials(self.OverrideMaterials);
                other.RenderingQueue = self.RenderingQueue;
            }
            return ret;
        }
    }
}
