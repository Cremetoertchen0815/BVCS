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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nez.GeonBit.Materials
{
	/// <summary>
	/// A material that support ambient + several point / directional lights.
	/// </summary>
	public class LitMaterial : MaterialAPI
	{
		// effect path
		private static readonly string _effectPath = EffectsPath + "LitEffect";

		// the effect instance of this material.
		private readonly Effect _effect;

		/// <summary>
		/// Get the effect instance.
		/// </summary>
		public override Effect Effect => _effect;

		/// <summary>
		/// If true, will use the currently set lights manager in `Graphics.GeonBitRenderer.LightsManager` and call ApplyLights() with the lights from manager.
		/// </summary>
		protected override bool UseDefaultLightsManager => true;

		// caching of lights-related params from shader
		private EffectParameter _lightsCol;
		private EffectParameter _lightsPos;
		private EffectParameter _lightsIntens;
		private EffectParameter _lightsRange;
		private EffectParameter _lightsSpec;

		// effect parameters
		private EffectParameterCollection _effectParams;

		// current active lights counter
		private int _activeLightsCount = 0;

		// how many of the active lights are directional
		private int _directionalLightsCount = 0;

		/// <summary>
		/// Max light intensity from regular light sources (before specular).
		/// </summary>
		public virtual float MaxLightIntensity
		{
			get => _maxLightIntens;
			set { _maxLightIntens = value; SetAsDirty(MaterialDirtyFlags.MaterialColors); }
		}

		private float _maxLightIntens = 1.0f;

		/// <summary>
		/// Normal map texture.
		/// </summary>
		public virtual Texture2D NormalTexture
		{
			get => _normalTexture;
			set { _normalTexture = value; SetAsDirty(MaterialDirtyFlags.TextureParams); }
		}

		private Texture2D _normalTexture;

		/// <summary>
		/// Get how many samplers this material uses.
		/// </summary>
		protected override int SamplersCount => _normalTexture == null ? 1 : 2;

		// caching lights data in arrays ready to be sent to shader.
		private readonly Vector3[] _lightsColArr = new Vector3[MaxLightsCount];
		private readonly Vector3[] _lightsPosArr = new Vector3[MaxLightsCount];
		private readonly float[] _lightsIntensArr = new float[MaxLightsCount];
		private readonly float[] _lightsRangeArr = new float[MaxLightsCount];
		private readonly float[] _lightsSpecArr = new float[MaxLightsCount];

		// caching world and transpose params
		private EffectParameter _worldParam;
		private EffectParameter _transposeParam;

		// How many lights we can support at the same time. based on effect definition.
		private static readonly int MaxLightsCount = 7;

		// cache of lights we applied
		private readonly Lights.LightSource[] _lastLights = new Lights.LightSource[MaxLightsCount];

		// cache of lights last known params version
		private readonly uint[] _lastLightVersions = new uint[MaxLightsCount];

		/// <summary>
		/// Return if this material support dynamic lighting.
		/// </summary>
		public override bool LightingEnabled => true;

		/// <summary>
		/// Create new lit effect instance.
		/// </summary>
		/// <returns>New lit effect instance.</returns>
		public virtual Effect CreateEffect() => Core.Content.Load<Effect>(_effectPath).Clone();

		/// <summary>
		/// Create the lit material from an empty effect.
		/// </summary>
		public LitMaterial()
		{
			_effect = CreateEffect();
			SetDefaults();
			InitLightParams();
		}

		/// <summary>
		/// Create the material from another material instance.
		/// </summary>
		/// <param name="other">Other material to clone.</param>
		public LitMaterial(LitMaterial other)
		{
			// clone effect and set defaults
			_effect = other._effect.Clone();
			MaterialAPI asBase = this;
			other.CloneBasics(ref asBase);

			// init light params
			InitLightParams();
		}

		/// <summary>
		/// Create the lit material.
		/// </summary>
		/// <param name="fromEffect">Effect to create material from.</param>
		public LitMaterial(Effect fromEffect)
		{
			// clone effect and set defaults
			_effect = fromEffect.Clone();
			SetDefaults();

			// init light params
			InitLightParams();
		}

		/// <summary>
		/// Create the lit material.
		/// </summary>
		/// <param name="fromEffect">Effect to create material from.</param>
		/// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
		public LitMaterial(BasicEffect fromEffect, bool copyEffectProperties = true)
		{
			// store effect and set default properties
			_effect = CreateEffect();
			SetDefaults();

			// copy properties from effect itself
			if (copyEffectProperties)
			{
				// set effect defaults
				Texture = fromEffect.Texture;
				TextureEnabled = fromEffect.TextureEnabled;
				Alpha = fromEffect.Alpha;
				AmbientLight = new Color(fromEffect.AmbientLightColor.X, fromEffect.AmbientLightColor.Y, fromEffect.AmbientLightColor.Z);
				DiffuseColor = new Color(fromEffect.DiffuseColor.X, fromEffect.DiffuseColor.Y, fromEffect.DiffuseColor.Z);
				SpecularColor = new Color(fromEffect.SpecularColor.X, fromEffect.SpecularColor.Y, fromEffect.SpecularColor.Z);
				SpecularPower = fromEffect.SpecularPower;
			}

			// init light params
			InitLightParams();
		}

		/// <summary>
		/// Init light-related params from shader.
		/// </summary>
		private void InitLightParams()
		{
			_effectParams = _effect.Parameters;
			_lightsCol = _effectParams["LightColor"];
			_lightsPos = _effectParams["LightPosition"];
			_lightsIntens = _effectParams["LightIntensity"];
			_lightsRange = _effectParams["LightRange"];
			_lightsSpec = _effectParams["LightSpecular"];
			_transposeParam = _effectParams["WorldInverseTranspose"];
			_worldParam = _effectParams["World"];
		}

		/// <summary>
		/// Apply this material.
		/// </summary>
		protected override void MaterialSpecificApply(bool wasLastMaterial)
		{
			// set world matrix
			_effectParams["WorldViewProjection"].SetValue(World * ViewProjection);

			// set world matrix
			if (IsDirty(MaterialDirtyFlags.World))
			{
				_worldParam.SetValue(World);
				if (_transposeParam != null)
				{
					_transposeParam.SetValue(Matrix.Invert(Matrix.Transpose(World)));
				}
			}

			// set all effect params
			if (IsDirty(MaterialDirtyFlags.TextureParams))
			{
				// set main texture
				var textureParam = _effectParams["MainTexture"];
				if (textureParam != null)
				{
					_effectParams["TextureEnabled"].SetValue(TextureEnabled && Texture != null);
					textureParam.SetValue(Texture);
				}

				// set normal texture
				var normalTextureParam = _effectParams["NormalTexture"];
				if (normalTextureParam != null)
				{
					var normalMapEnabledParam = _effectParams["NormalTextureEnabled"];
					if (normalMapEnabledParam != null)
					{
						normalMapEnabledParam.SetValue(TextureEnabled && NormalTexture != null);
					}

					normalTextureParam.SetValue(NormalTexture);
				}
			}
			if (IsDirty(MaterialDirtyFlags.Alpha))
			{
				_effectParams["Alpha"].SetValue(Alpha);
			}
			if (IsDirty(MaterialDirtyFlags.MaterialColors))
			{
				_effectParams["DiffuseColor"].SetValue(DiffuseColor.ToVector3());
				_effectParams["MaxLightIntensity"].SetValue(MaxLightIntensity);
			}
		}

		/// <summary>
		/// Update material view matrix.
		/// </summary>
		/// <param name="view">New view to set.</param>
		protected override void UpdateView(ref Matrix view)
		{
		}

		/// <summary>
		/// Update material projection matrix.
		/// </summary>
		/// <param name="projection">New projection to set.</param>
		protected override void UpdateProjection(ref Matrix projection)
		{
		}

		/// <summary>
		/// Apply light sources on this material.
		/// </summary>
		/// <param name="lights">Array of light sources to apply.</param>
		/// <param name="worldMatrix">World transforms of the rendering object.</param>
		/// <param name="boundingSphere">Bounding sphere (after world transformation applied) of the rendering object.</param>
		protected override void ApplyLights(Lights.LightSource[] lights, ref Matrix worldMatrix, ref BoundingSphere boundingSphere)
		{
			// set global light params
			if (IsDirty(MaterialDirtyFlags.EmissiveLight))
			{
				_effectParams["EmissiveColor"].SetValue(EmissiveLight.ToVector3());
			}
			if (IsDirty(MaterialDirtyFlags.AmbientLight))
			{
				_effectParams["AmbientColor"].SetValue(AmbientLight.ToVector3());
			}
			if (IsDirty(MaterialDirtyFlags.LightSources))
			{
				_effectParams["MaxLightIntensity"].SetValue(1.0f);
			}

			// do we need to update light sources data?
			bool needUpdate = false;

			// iterate on lights and apply only the changed ones
			int lightsCount = Math.Min(MaxLightsCount, lights.Length);
			for (int i = 0; i < lightsCount; ++i)
			{
				// only if light changed
				if (_lastLights[i] != lights[i] || _lastLightVersions[i] != lights[i].ParamsVersion)
				{
					// mark that an update is required
					needUpdate = true;

					// get current light
					var light = lights[i];

					// set lights data
					_lightsColArr[i] = light.Color.ToVector3();
					_lightsPosArr[i] = light.IsDirectionalLight ? Vector3.Normalize(light.Direction.Value) : light.Position;
					_lightsIntensArr[i] = light.Intensity;
					_lightsRangeArr[i] = light.IsInfinite ? 0f : light.Range;
					_lightsSpecArr[i] = light.Specular;

					// store light in cache so we won't copy it next time if it haven't changed
					_lastLights[i] = lights[i];
					_lastLightVersions[i] = lights[i].ParamsVersion;
				}
			}

			// update active lights count
			if (_activeLightsCount != lightsCount)
			{
				_activeLightsCount = lightsCount;
				_effect.Parameters["ActiveLightsCount"].SetValue(_activeLightsCount);
			}

			// count directional lights
			int directionalLightsCount = 0;
			foreach (var light in lights)
			{
				if (!light.IsDirectionalLight)
				{
					break;
				}

				directionalLightsCount++;
			}

			// update directional lights count
			if (_directionalLightsCount != directionalLightsCount)
			{
				_directionalLightsCount = directionalLightsCount;
				var dirCount = _effect.Parameters["DirectionalLightsCount"];
				if (dirCount != null) { dirCount.SetValue(_directionalLightsCount); }
			}

			// if we need to update lights, write their arrays
			if (needUpdate)
			{
				if (_lightsCol != null)
				{
					_lightsCol.SetValue(_lightsColArr);
				}

				if (_lightsPos != null)
				{
					_lightsPos.SetValue(_lightsPosArr);
				}

				if (_lightsIntens != null)
				{
					_lightsIntens.SetValue(_lightsIntensArr);
				}

				if (_lightsRange != null)
				{
					_lightsRange.SetValue(_lightsRangeArr);
				}

				if (_lightsSpec != null)
				{
					_lightsSpec.SetValue(_lightsSpecArr);
				}
			}
		}

		/// <summary>
		/// Clone this material.
		/// </summary>
		/// <returns>Copy of this material.</returns>
		public override MaterialAPI Clone() => new LitMaterial(this);
	}
}
