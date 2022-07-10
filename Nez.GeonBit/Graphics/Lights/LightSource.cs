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
// Basic light source entity.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;


namespace Nez.GeonBit.Lights
{
    /// <summary>
    /// Basic light source entity.
    /// </summary>
    public class LightSource
    {
        /// <summary>
        /// Is this light source currently visible?
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// Parent lights manager.
        /// </summary>
        internal ILightsManager LightsManager = null;

        /// <summary>
        /// So we can cache lights and identify when they were changed.
        /// </summary>
        public uint ParamsVersion { get; private set; } = 1;

        /// <summary>
        /// Light bounding sphere.
        /// </summary>
        public BoundingSphere BoundingSphere { get; protected set; }

        /// <summary>
        /// Return if this light source is infinite, eg has no range and reach anywhere (like a directional light).
        /// </summary>
        public virtual bool IsInfinite => Direction != null || _range == 0f;

        /// <summary>
        /// Return if this light is a directional light.
        /// </summary>
        public virtual bool IsDirectionalLight => Direction != null;

        /// <summary>
        /// Light direction, if its a directional light.
        /// </summary>
        public Vector3? Direction
        {
            get => _direction;
            set { if (_direction == value) { return; } _direction = value; ParamsVersion++; RecalcBoundingSphere(); }
        }
        private Vector3? _direction = null;

        /// <summary>
        /// Light range.
        /// </summary>
        public float Range
        {
            get => _range;
            set { if (_range == value) { return; } _range = value; ParamsVersion++; RecalcBoundingSphere(); }
        }
        private float _range = 100f;

        /// <summary>
        /// Light position in world space.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set { if (_position == value) { return; } _position = value; ParamsVersion++; RecalcBoundingSphere(); }
        }

        private Vector3 _position = Vector3.Zero;

        /// <summary>
        /// Light color and strength (A field = light strength).
        /// </summary>
        public Color Color
        {
            get => _color;
            set { if (_color == value) { return; } _color = value; ParamsVersion++; }
        }

        private Color _color = Color.White;

        /// <summary>
        /// Light Intensity (equivilent to Color.A).
        /// </summary>
        public float Intensity
        {
            get => _intensity;
            set { if (_intensity == value) { return; } _intensity = value; ParamsVersion++; }
        }

        private float _intensity = 1f;

        /// <summary>
        /// Specular factor.
        /// </summary>
        public float Specular
        {
            get => _specular;
            set { if (_specular == value) { return; } _specular = value; ParamsVersion++; }
        }

        private float _specular = 1f;

        /// <summary>
        /// Last light known transform.
        /// </summary>
        private Matrix _transform;

        /// <summary>
        /// Remove self from parent lights manager.
        /// </summary>
        public void Remove()
        {
            if (LightsManager != null)
            {
                LightsManager.RemoveLight(this);
            }
        }

        /// <summary>
        /// Create the light source.
        /// </summary>
        public LightSource() =>
            // count the object creation
            CountAndAlert.Count(CountAndAlert.PredefAlertTypes.AddedOrCreated);

        /// <summary>
        /// Update light transformations.
        /// </summary>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        public virtual void UpdateTransforms(ref Matrix worldTransformations)
        {
            // if didn't really change skip
            if (_transform == worldTransformations) { return; }

            // break transformation into components
            _transform.Decompose(out var scale, out var rotation, out var position);

            // set world position. this will also recalc bounding sphere and update lights manager, if needed.
            Position = position;
        }

        /// <summary>
        /// Recalculate light bounding sphere after transformations or radius change.
        /// </summary>
        /// <param name="updateInLightsManager">If true, will also update light position in lights manager.</param>
        public virtual void RecalcBoundingSphere(bool updateInLightsManager = true)
        {
            // calc light bounding sphere
            BoundingSphere = new BoundingSphere(Position, _range);

            // notify manager on update
            if (updateInLightsManager && LightsManager != null)
            {
                LightsManager.UpdateLightTransform(this);
            }
        }
    }
}
