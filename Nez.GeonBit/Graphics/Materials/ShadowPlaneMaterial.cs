using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit.Materials
{
    public class ShadowPlaneMaterial : MaterialAPI
    {
		// effect path
		private static readonly string _effectPath = EffectsPath + "ShadowPlaneEffect";

		// the effect instance of this material.
		private readonly Effect _effect;

		/// <summary>
		/// Get the effect instance.
		/// </summary>
		public override Effect Effect => _effect;

		/// <summary>
		/// If true, will use the currently set lights manager in `Graphics.GeonBitRenderer.LightsManager` and call ApplyLights() with the lights from manager.
		/// </summary>
		protected override bool UseDefaultLightsManager => false;

		// effect parameters
		private EffectParameterCollection _effectParams;

		/// <summary>
		/// Normal map texture.
		/// </summary>
		public virtual Texture2D ShadowMap
		{
			get => _shadowMap;
			set { _shadowMap = value; SetAsDirty(MaterialDirtyFlags.TextureParams); }
		}

		private Texture2D _shadowMap;

		/// <summary>
		/// Get how many samplers this material uses.
		/// </summary>
		protected override int SamplersCount => 2;

		/// <summary>
		/// Return if this material support dynamic lighting.
		/// </summary>
		public override bool LightingEnabled => false;

		/// <summary>
		/// Create new lit effect instance.
		/// </summary>
		/// <returns>New lit effect instance.</returns>
		public virtual Effect CreateEffect() => Core.Content.Load<Effect>(_effectPath).Clone();

		/// <summary>
		/// Create the lit material from an empty effect.
		/// </summary>
		public ShadowPlaneMaterial()
		{
			_effect = CreateEffect();
			SetDefaults();
			InitLightParams();
		}

		/// <summary>
		/// Create the material from another material instance.
		/// </summary>
		/// <param name="other">Other material to clone.</param>
		public ShadowPlaneMaterial(ShadowPlaneMaterial other)
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
		public ShadowPlaneMaterial(Effect fromEffect)
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
		public ShadowPlaneMaterial(BasicEffect fromEffect, bool copyEffectProperties = true)
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
		}

		/// <summary>
		/// Apply this material.
		/// </summary>
		protected override void MaterialSpecificApply(bool wasLastMaterial)
		{
			// set world matrix
			_effectParams["WorldViewProjection"].SetValue(World * ViewProjection);

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

				_effectParams["ShadowMap"].SetValue(ShadowMap);
			}
			if (IsDirty(MaterialDirtyFlags.Alpha))
			{
				_effectParams["Alpha"].SetValue(Alpha);
			}
			if (IsDirty(MaterialDirtyFlags.MaterialColors))
			{
				_effectParams["DiffuseColor"].SetValue(DiffuseColor.ToVector3());
			}

			// set global light params
			if (IsDirty(MaterialDirtyFlags.EmissiveLight))
			{
				_effectParams["EmissiveColor"].SetValue(EmissiveLight.ToVector3());
			}
			if (IsDirty(MaterialDirtyFlags.AmbientLight))
			{
				_effectParams["AmbientColor"].SetValue(AmbientLight.ToVector3());
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
		/// Clone this material.
		/// </summary>
		/// <returns>Copy of this material.</returns>
		public override MaterialAPI Clone() => new ShadowPlaneMaterial(this);
	}
}
