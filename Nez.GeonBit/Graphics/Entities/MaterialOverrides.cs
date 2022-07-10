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
// A special class to hold per-entity material properties.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nez.GeonBit
{
    /// <summary>
    /// Helper class to manage materials override properties.
    /// You can use this class to set color, material, specular, etc for a specific model renderers, without having to create a
    /// new material for them.
    /// 
    /// This allow you to easily override material's defaults without having to define lots of different materials, but note that this method is
    /// slightly slower so don't use it on too many entities at the same time.
    /// </summary>
    public class MaterialOverrides
    {
        // optional per-entity properties that override material's defaults
        private Color? _diffuseColor = null;
        private Color? _specularColor = null;
        private Color? _emissiveLight = null;
        private float? _alpha = null;
        private Texture2D _texture = null;

        /// <summary>
        /// Override diffuse color for this specific entity.
        /// Note: will affect all parts of the model.
        /// </summary>
        public Color? DiffuseColor
        {
            get => _diffuseColor;
            set { _diffuseColor = value; UpdateOverridePropertiesState(); }
        }

        /// <summary>
        /// Override specular color for this specific entity.
        /// Note: will affect all parts of the model.
        /// </summary>
        public Color? SpecularColor
        {
            get => _specularColor;
            set { _specularColor = value; UpdateOverridePropertiesState(); }
        }

        /// <summary>
        /// Override emissive color for this specific entity.
        /// Note: will affect all parts of the model.
        /// </summary>
        public Color? EmissiveLight
        {
            get => _emissiveLight;
            set { _emissiveLight = value; UpdateOverridePropertiesState(); }
        }

        /// <summary>
        /// Override Alpha value for this specific entity.
        /// Note: will affect all parts of the model.
        /// </summary>
        public float? Alpha
        {
            get => _alpha;
            set { _alpha = value; UpdateOverridePropertiesState(); }
        }

        /// <summary>
        /// Override material texture for this specific entity.
        /// Note: will affect all parts of the model.
        /// </summary>
        public Texture2D Texture
        {
            get => _texture;
            set { _texture = value; UpdateOverridePropertiesState(); }
        }

        /// <summary>
        /// If true will use the override per-entity properties.
        /// </summary>
        public bool UsingOverrideProperties { get; protected set; }

        /// <summary>
        /// Return if this entity should use material override properties (properties like texture, color, etc
        /// which override the material defaults).
        /// </summary>
        private bool HaveOverrideProperties => Alpha != null || DiffuseColor != null || Texture != null || SpecularColor != null || EmissiveLight != null;

        /// <summary>
        /// Update if currently using override properties or not.
        /// </summary>
        private void UpdateOverridePropertiesState() => UsingOverrideProperties = HaveOverrideProperties;

        /// <summary>
        /// Clone custom render settings.
        /// </summary>
        /// <returns>Cloned settings.</returns>
        public MaterialOverrides Clone()
        {
            var ret = new MaterialOverrides
            {
                _diffuseColor = _diffuseColor,
                _specularColor = _specularColor,
                _emissiveLight = _emissiveLight,
                _alpha = _alpha,
                _texture = _texture
            };
            ret.UpdateOverridePropertiesState();
            return ret;
        }

        // dictionary of cached material clones for original materials replacement
        private readonly Dictionary<Materials.MaterialAPI, Materials.MaterialAPI> _materialsCahce = new Dictionary<Materials.MaterialAPI, Materials.MaterialAPI>();

        /// <summary>
        /// Apply all custom render properties on a given material, and return either the given material or a clone of it, if needed.
        /// This will not do anything if there are no custom properties currently used.
        /// </summary>
        /// <param name="material">Effect to set properties.</param>
        /// <returns>Either the input material or a clone of it with applied properties.</returns>
        public Materials.MaterialAPI Apply(Materials.MaterialAPI material)
        {
            // if there's nothing to do just return the original material
            if (!UsingOverrideProperties)
            {
                _materialsCahce.Clear();
                return material;
            }

            // we need to apply custom properties. get the cached material with properties or create a new one
            var original = material;
            if (!_materialsCahce.TryGetValue(material, out material))
            {
                material = original.Clone();
                _materialsCahce[original] = material;
            }

            // if got override diffuse color, set it
            if (DiffuseColor != null)
            {
                material.DiffuseColor = DiffuseColor.Value;
            }

            // if got override specular color, set it
            if (SpecularColor != null)
            {
                material.SpecularColor = SpecularColor.Value;
            }

            // if got override emissive color, set it
            if (EmissiveLight != null)
            {
                material.EmissiveLight = EmissiveLight.Value;
            }

            // if got override alpha, set it
            if (Alpha != null)
            {
                material.Alpha = Alpha.Value;
            }

            // if got override texture, set it
            if (Texture != null)
            {
                material.Texture = Texture;
                material.TextureEnabled = true;
            }

            // return the cloned material
            return material;
        }
    }
}
