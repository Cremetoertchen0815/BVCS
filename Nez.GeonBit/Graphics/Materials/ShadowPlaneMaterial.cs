using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit.Materials
{
    internal class ShadowPlaneMaterial : MaterialAPI
    {
		// the effect instance of this material.
		private readonly Effect _effect;

		private Matrix _effectWorld { get => _effect.Parameters["World"].GetValueMatrix(); set => _effect.Parameters["World"].SetValue(value); }
		private Matrix _effectView { get => _effect.Parameters["World"].GetValueMatrix(); set => _effect.Parameters["World"].SetValue(value); }
		private Matrix _effectProjection { get => _effect.Parameters["World"].GetValueMatrix(); set => _effect.Parameters["World"].SetValue(value); }
		private Texture2D _effectTexture { get => _effect.Parameters["Texture"].GetValueTexture2D(); set => _effect.Parameters["Texture"].SetValue(value); }
		private Texture2D _effectShadowStencil { get => _effect.Parameters["ShadowStencil"].GetValueTexture2D(); set => _effect.Parameters["ShadowStencil"].SetValue(value); }
		private bool _effectTextureEnabled { get => _effect.Parameters["TextureEnabled"].GetValueBoolean(); set => _effect.Parameters["TextureEnabled"].SetValue(value); }
		private Vector3 _effectAmbient { get => _effect.Parameters["Ambient"].GetValueVector3(); set => _effect.Parameters["Ambient"].SetValue(value); }
		private Vector3 _effectDiffuse { get => _effect.Parameters["Diffuse"].GetValueVector3(); set => _effect.Parameters["Diffuse"].SetValue(value); }
		private Vector3 _effectSpecular { get => _effect.Parameters["Specular"].GetValueVector3(); set => _effect.Parameters["Specular"].SetValue(value); }
		private float _effectAlpha { get => _effect.Parameters["Alpha"].GetValueSingle(); set => _effect.Parameters["Alpha"].SetValue(value); }
		private float _effectBlurRadius { get => _effect.Parameters["BlurRadius"].GetValueSingle(); set => _effect.Parameters["BlurRadius"].SetValue(value); }
		private float _effectSpecularPower { get => _effect.Parameters["SpecularPower"].GetValueSingle(); set => _effect.Parameters["SpecularPower"].SetValue(value); }

		/// <summary>
		/// Get the effect instance.
		/// </summary>
		public override Effect Effect => _effect;

		public virtual Texture2D ShadowStencilTexture
		{
			get => _shadowStencilTexture;
			set { _shadowStencilTexture = value; SetAsDirty(MaterialDirtyFlags.TextureParams); }
		}

		private Texture2D _shadowStencilTexture;

		public virtual float BlurRadius
		{
			get => _blurRadius;
			set { _blurRadius = value; SetAsDirty(MaterialDirtyFlags.TextureParams); }
		}

		private float _blurRadius;

		// empty effect instance to clone when creating new material
		private static readonly Effect _emptyEffect = Core.Content.Load<Effect>("");

		/// <summary>
		/// Create the default material from empty effect.
		/// </summary>
		public ShadowPlaneMaterial() : this(_emptyEffect, true)
		{
		}

		/// <summary>
		/// Create the material from another material instance.
		/// </summary>
		/// <param name="other">Other material to clone.</param>
		public ShadowPlaneMaterial(ShadowPlaneMaterial other)
		{
			_effect = other._effect.Clone() as BasicEffect;
			MaterialAPI asBase = this;
			other.CloneBasics(ref asBase);
		}

		/// <summary>
		/// Create the default material.
		/// </summary>
		/// <param name="fromEffect">Effect to create material from.</param>
		/// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
		public ShadowPlaneMaterial(Effect fromEffect, bool copyEffectProperties = true)
		{
			// store effect and set default properties
			_effect = fromEffect.Clone() as Effect;
			SetDefaults();

			// copy properties from effect itself
			if (copyEffectProperties)
			{
				// set effect defaults
				Texture = fromEffect.Parameters["Texture"].GetValueTexture2D();
				Texture = fromEffect.Parameters["ShadowStencil"].GetValueTexture2D();
				TextureEnabled = fromEffect.Parameters["TextureEnabled"].GetValueBoolean();
				AmbientLight = new Color(fromEffect.Parameters["Ambient"].GetValueVector3());
				DiffuseColor = new Color(fromEffect.Parameters["Diffuse"].GetValueVector3());
				SpecularColor = new Color(fromEffect.Parameters["Specular"].GetValueVector3());
				SpecularPower = fromEffect.Parameters["SpecularPower"].GetValueSingle();
				Alpha = fromEffect.Parameters["Alpha"].GetValueSingle();
				BlurRadius = fromEffect.Parameters["BlurRadius"].GetValueSingle();

			}
		}

		/// <summary>
		/// Apply this material.
		/// </summary>
		protected override void MaterialSpecificApply(bool wasLastMaterial)
		{
			// set world matrix
			if (IsDirty(MaterialDirtyFlags.World))
			{
				_effectWorld = World;
			}

			// if it was last material used, stop here - no need for the following settings
			if (wasLastMaterial) { return; }

			// set all effect params
			if (IsDirty(MaterialDirtyFlags.TextureParams))
			{
				_effectTexture = Texture;
				_effectTextureEnabled = TextureEnabled;
				_effectShadowStencil = ShadowStencilTexture;
				_effectBlurRadius = BlurRadius;
			}
			if (IsDirty(MaterialDirtyFlags.Alpha))
			{
				_effectAlpha = Alpha;
			}
			if (IsDirty(MaterialDirtyFlags.AmbientLight))
			{
				_effectAmbient = AmbientLight.ToVector3();
			}
			if (IsDirty(MaterialDirtyFlags.MaterialColors))
			{
				_effectDiffuse = DiffuseColor.ToVector3();
				_effectSpecular = SpecularColor.ToVector3();
				_effectSpecularPower = SpecularPower;
			}
		}

		/// <summary>
		/// Update material view matrix.
		/// </summary>
		/// <param name="view">New view to set.</param>
		protected override void UpdateView(ref Matrix view) => _effectView = View;

		/// <summary>
		/// Update material projection matrix.
		/// </summary>
		/// <param name="projection">New projection to set.</param>
		protected override void UpdateProjection(ref Matrix projection) => _effectProjection = Projection;

		/// <summary>
		/// Clone this material.
		/// </summary>
		/// <returns>Copy of this material.</returns>
		public override MaterialAPI Clone() => new ShadowPlaneMaterial(this);
	}
}
