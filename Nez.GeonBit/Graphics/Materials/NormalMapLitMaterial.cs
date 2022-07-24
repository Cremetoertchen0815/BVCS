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
// A basic one-pass lit material.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Materials
{
    /// <summary>
    /// A material that support ambient + several point / directional lights.
    /// </summary>
    public class NormalMapLitMaterial : LitMaterial
    {
        // effect path
        private static readonly string _effectPath = EffectsPath + "lit_normal";

        /// <summary>
        /// Create new lit effect instance.
        /// </summary>
        /// <returns>New lit effect instance.</returns>
        public override Effect CreateEffect() => GeonDefaultRenderer.CurrentContentManager.Load<Effect>(_effectPath).Clone();

        /// <summary>
        /// Create the lit material from an empty effect.
        /// </summary>
        public NormalMapLitMaterial() : base()
        {
        }

        /// <summary>
        /// Create the material from another material instance.
        /// </summary>
        /// <param name="other">Other material to clone.</param>
        public NormalMapLitMaterial(LitMaterial other) : base(other)
        {
        }

        /// <summary>
        /// Create the lit material.
        /// </summary>
        /// <param name="fromEffect">Effect to create material from.</param>
        public NormalMapLitMaterial(Effect fromEffect) : base(fromEffect)
        {
        }

        /// <summary>
        /// Create the lit material.
        /// </summary>
        /// <param name="fromEffect">Effect to create material from.</param>
        /// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
        public NormalMapLitMaterial(BasicEffect fromEffect, bool copyEffectProperties = true) : base(fromEffect, copyEffectProperties)
        {
        }

        /// <summary>
        /// Clone this material.
        /// </summary>
        /// <returns>Copy of this material.</returns>
        public override MaterialAPI Clone() => new NormalMapLitMaterial(this);
    }
}
