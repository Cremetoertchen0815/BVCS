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
// A component that renders a 3D model.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit
{
    /// <summary>
    /// This component renders a 3D model.
    /// </summary>
    public class ModelRenderer : BaseRendererWithOverrideMaterial
    {
        /// <summary>
        /// The entity from the core layer used to draw the model.
        /// </summary>
        protected ModelEntity _entity;

        /// <summary>
        /// Get renderer model.
        /// </summary>
        public Model Model => _entity.Model;

        /// <summary>
        /// Override material default settings for this specific model instance.
        /// </summary>
        public override MaterialOverrides MaterialOverride
        {
            get => _entity.MaterialOverride;
            set => _entity.MaterialOverride = value;
        }

        /// <summary>
        /// Set alternative material for a specific mesh id.
        /// </summary>
        /// <param name="material">Material to set.</param>
        /// <param name="meshId">Mesh name. If empty string is provided, this material will be used for all meshes.</param>
        public void SetMaterial(Materials.MaterialAPI material, string meshId = "") => _entity.SetMaterial(material, meshId);

        /// <summary>
        /// Set alternative materials for a specific mesh id.
        /// </summary>
        /// <param name="material">Materials to set.</param>
        /// <param name="meshId">Mesh name. If empty string is provided, this material will be used for all meshes.</param>
        public void SetMaterials(Materials.MaterialAPI[] material, string meshId = "") => _entity.SetMaterials(material, meshId);

        /// <summary>
        /// Get material for a given mesh id and part index.
        /// </summary>
        /// <param name="meshId">Mesh id to get material for.</param>
        /// <param name="meshPartIndex">MeshPart index to get material for.</param>
        public Materials.MaterialAPI GetMaterial(string meshId, int meshPartIndex = 0) => _entity.GetMaterial(meshId, meshPartIndex);

        /// <summary>
        /// Get the first material used in this renderer.
        /// </summary>
        public Materials.MaterialAPI GetFirstMaterial() => _entity.GetFirstMaterial();

        /// <summary>
        /// Return a list with all materials in model.
        /// Note: if alternative materials are set, will return them.
        /// Note2: prevent duplications, eg if even if more than one part uses the same material it will only return it once.
        /// </summary>
        /// <returns>List of materials.</returns>
        public System.Collections.Generic.List<Materials.MaterialAPI> GetMaterials() => _entity.GetMaterials();

        /// <summary>
        /// Get the main entity instance of this renderer.
        /// </summary>
        protected override BaseRenderableEntity RenderableEntity => _entity;

        /// <summary>
        /// Protected constructor without params to use without creating entity, for inheriting classes.
        /// </summary>
        protected ModelRenderer()
        {
        }


        /// <summary>
        /// Create the model renderer component.
        /// </summary>
        /// <param name="model">Model to draw.</param>
        public ModelRenderer(Model model) => _entity = new ModelEntity(model);

        /// <summary>
        /// Create the model renderer component.
        /// </summary>
        /// <param name="model">Path of the model asset to draw.</param>
        public ModelRenderer(string model) => _entity = new ModelEntity(GeonRenderer.CurrentContentManager.LoadModel(model, x => Materials.DefaultMaterialsFactory.GetDefaultMaterial(x)));

        /// <summary>
        /// Copy basic properties to another component (helper function to help with Cloning).
        /// </summary>
        /// <param name="copyTo">Other component to copy values to.</param>
        /// <returns>The object we are copying properties to.</returns>
        public override Component CopyBasics(Component copyTo)
        {
            var other = copyTo as ModelRenderer;
            other.MaterialOverride = MaterialOverride.Clone();
            other._entity.CopyMaterials(_entity.OverrideMaterialsDictionary);
            return base.CopyBasics(other);
        }



        /// <summary>
        /// Clone this component.
        /// </summary>
        /// <returns>Cloned copy of this component.</returns>
        public override Component Clone()
        {
            var ret = new ModelRenderer(_entity.Model);
            CopyBasics(ret);
            return ret;
        }
    }
}
