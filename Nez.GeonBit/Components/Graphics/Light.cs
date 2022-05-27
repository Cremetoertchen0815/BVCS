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
// A light source component.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace Nez.GeonBit
{
    /// <summary>
    /// This component implements a light source.
    /// </summary>
    public class Light : BaseComponent
    {
        // the core light source.
        private Lights.LightSource _light;
        private GeonNode Node;

        /// <summary>
        /// Light direction, if its a directional light.
        /// </summary>
        public Vector3? Direction
        {
            get => _light.Direction;
            set => _light.Direction = value;
        }

        /// <summary>
        /// Return if this light source is infinite, eg has no range and reach anywhere (like a directional light).
        /// </summary>
        public virtual bool IsInfinite => _light.IsInfinite;

        /// <summary>
        /// Return if this light is a directional light.
        /// </summary>
        public bool IsDirectionalLight => _light.IsDirectionalLight;

        /// <summary>
        /// Light range.
        /// </summary>
        public float Range
        {
            get => _light.Range;
            set => _light.Range = value;
        }

        /// <summary>
        /// Light color and strength (A field = light strength).
        /// </summary>
        public Color Color
        {
            get => _light.Color;
            set => _light.Color = value;
        }

        /// <summary>
        /// Light Intensity (equivilent to Color.A).
        /// </summary>
        public float Intensity
        {
            get => _light.Intensity;
            set => _light.Intensity = value;
        }

        /// <summary>
        /// Specular factor.
        /// </summary>
        public float Specular
        {
            get => _light.Specular;
            set => _light.Specular = value;
        }

        /// <summary>
        /// Create the light component.
        /// </summary>
        public Light() =>
            // create the light source
            _light = new Lights.LightSource();

        /// <summary>
        /// Clone this component.
        /// </summary>
        /// <returns>Cloned copy of this component.</returns>
        public override Component Clone()
        {
            var ret = new Light
            {
                Intensity = Intensity,
                Specular = Specular,
                Color = Color,
                Direction = Direction,
                Range = Range
            };
            
            return ret;
        }

        /// <summary>
        /// Called when GameObject turned disabled.
        /// </summary>
        public override void OnDisabled() => _light.Visible = false;

        /// <summary>
        /// Called when GameObject is enabled.
        /// </summary>
        public override void OnEnabled() => _light.Visible = true;

        /// <summary>
        /// Called every time scene node transformation updates.
        /// Note: this is called only if GameObject is enabled and have Update events enabled.
        /// </summary>
        protected override void OnTransformationUpdate()
        {
            if (!_light.IsInfinite) { _light.Position = Node.WorldPosition; }
        }

        /// <summary>
        /// Called when this component is effectively removed from scene, eg when removed
        /// from a GameObject or when its GameObject is removed from scene.
        /// </summary>
        public override void OnRemovedFromEntity() => _light.Remove();

        /// <summary>
        /// Called when this component is effectively added to scene, eg when added
        /// to a GameObject currently in scene or when its GameObject is added to scene.
        /// </summary>
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            GeonBitRenderer.Lights.AddLight(_light);
        }
    }
}
