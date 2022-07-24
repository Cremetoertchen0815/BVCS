using Microsoft.Xna.Framework;
using System;
using System.IO;


namespace Nez
{
    public static class EffectResource
    {
        // sprite effects
        internal static byte[] SpriteBlinkEffectBytes => GetFileResourceBytes("engine/fx/sprite/blink");

        internal static byte[] SpriteLinesEffectBytes => GetFileResourceBytes("engine/fx/sprite/lines");

        internal static byte[] SpriteAlphaTestBytes => GetFileResourceBytes("engine/fx/sprite/alpha_test");

        internal static byte[] CrosshatchBytes => GetFileResourceBytes("engine/fx/sprite/crosshatch");

        internal static byte[] NoiseBytes => GetFileResourceBytes("engine/fx/sprite/noise");

        internal static byte[] TwistBytes => GetFileResourceBytes("engine/fx/sprite/twist");

        internal static byte[] DotsBytes => GetFileResourceBytes("engine/fx/sprite/dots");

        internal static byte[] DissolveBytes => GetFileResourceBytes("engine/fx/sprite/dissolve");

        // post processor effects
        internal static byte[] BloomCombineBytes => GetFileResourceBytes("engine/fx/ppfx/bloom_combine");

        internal static byte[] BloomExtractBytes => GetFileResourceBytes("engine/fx/ppfx/bloom_extract");

        internal static byte[] QualityBloom => GetFileResourceBytes("/nez/effects/QualityBloom.mgfxo");

        internal static byte[] Mosaic => GetFileResourceBytes("/nez/effects/Mosaic.mgfxo");

        internal static byte[] LUTColorGrade => GetFileResourceBytes("/nez/effects/LUTColorGrade.mgfxo");

        internal static byte[] GaussianBlurBytes => GetFileResourceBytes("/nez/effects/GaussianBlur.mgfxo");

        internal static byte[] VignetteBytes => GetFileResourceBytes("/nez/effects/Vignette.mgfxo");

        internal static byte[] LetterboxBytes => GetFileResourceBytes("/nez/effects/Letterbox.mgfxo");

        internal static byte[] HeatDistortionBytes => GetFileResourceBytes("/nez/effects/HeatDistortion.mgfxo");

        internal static byte[] SpriteLightMultiplyBytes => GetFileResourceBytes("/nez/effects/SpriteLightMultiply.mgfxo");

        internal static byte[] PixelGlitchBytes => GetFileResourceBytes("/nez/effects/PixelGlitch.mgfxo");

        internal static byte[] StencilLightBytes => GetFileResourceBytes("/nez/effects/StencilLight.mgfxo");

        // deferred lighting
        internal static byte[] DeferredSpriteBytes => GetFileResourceBytes("/nez/effects/DeferredSprite.mgfxo");

        internal static byte[] DeferredLightBytes => GetFileResourceBytes("/nez/effects/DeferredLighting.mgfxo");

        // forward lighting
        internal static byte[] ForwardLightingBytes => GetFileResourceBytes("/nez/effects/ForwardLighting.mgfxo");

        internal static byte[] PolygonLightBytes => GetFileResourceBytes("/nez/effects/PolygonLight.mgfxo");

        // scene transitions
        internal static byte[] SquaresTransitionBytes => GetFileResourceBytes("/nez/effects/transitions/Squares.mgfxo");

        // sprite or post processor effects
        internal static byte[] SpriteEffectBytes => GetMonoGameEmbeddedResourceBytes("Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo");

        internal static byte[] MultiTextureOverlayBytes => GetFileResourceBytes("/nez/effects/MultiTextureOverlay.mgfxo");

        internal static byte[] ScanlinesBytes => GetFileResourceBytes("/nez/effects/Scanlines.mgfxo");

        internal static byte[] ReflectionBytes => GetFileResourceBytes("/nez/effects/Reflection.mgfxo");

        internal static byte[] GrayscaleBytes => GetFileResourceBytes("/nez/effects/Grayscale.mgfxo");

        internal static byte[] SepiaBytes => GetFileResourceBytes("/nez/effects/Sepia.mgfxo");

        internal static byte[] PaletteCyclerBytes => GetFileResourceBytes("/nez/effects/PaletteCycler.mgfxo");


        /// <summary>
        /// gets the raw byte[] from an EmbeddedResource
        /// </summary>
        /// <returns>The embedded resource bytes.</returns>
        /// <param name="name">Name.</param>
        private static byte[] GetEmbeddedResourceBytes(string name)
        {
            var assembly = typeof(EffectResource).Assembly;
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }


        internal static byte[] GetMonoGameEmbeddedResourceBytes(string name)
        {
            var assembly = typeof(MathHelper).Assembly;
#if FNA
			name = name.Replace( ".ogl.mgfxo", ".fxb" );
#else
            // MG 3.8 decided to change the location of Effecs...sigh.
            if (!assembly.GetManifestResourceNames().Contains(name))
                name = name.Replace(".Framework", ".Framework.Platform");
#endif

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] GetFileResourceBytes(string path) => Core.Content.Load<byte[]>(path);
    }
}