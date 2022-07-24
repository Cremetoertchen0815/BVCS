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

        internal static byte[] QualityBloom => GetFileResourceBytes("engine/fx/ppfx/bloom_hq");

        internal static byte[] Mosaic => GetFileResourceBytes("engine/fx/ppfx/mosaic");

        internal static byte[] LUTColorGrade => GetFileResourceBytes("engine/fx/ppfx/color_grade");

        internal static byte[] GaussianBlurBytes => GetFileResourceBytes("engine/fx/ppfx/gauss_blur");

        internal static byte[] VignetteBytes => GetFileResourceBytes("engine/fx/ppfx/vignette");

        internal static byte[] LetterboxBytes => GetFileResourceBytes("engine/fx/ppfx/letterbox");

        internal static byte[] HeatDistortionBytes => GetFileResourceBytes("engine/fx/ppfx/heat_distortion");

        internal static byte[] SpriteLightMultiplyBytes => GetFileResourceBytes("engine/fx/ppfx/sprite_light_multiply");

        internal static byte[] PixelGlitchBytes => GetFileResourceBytes("engine/fx/ppfx/pixel_glitch");

        internal static byte[] StencilLightBytes => GetFileResourceBytes("engine/fx/ppfx/stencil_light");

        // deferred lighting
        internal static byte[] DeferredSpriteBytes => GetFileResourceBytes("engine/fx/light/deferred_sprite");

        internal static byte[] DeferredLightBytes => GetFileResourceBytes("engine/fx/light/deferred");

        // forward lighting
        internal static byte[] ForwardLightingBytes => GetFileResourceBytes("engine/fx/light/forward");

        internal static byte[] PolygonLightBytes => GetFileResourceBytes("engine/fx/light/polygon");

        // scene transitions
        internal static byte[] SquaresTransitionBytes => GetFileResourceBytes("engine/fx/transition/square");

        // sprite or post processor effects
        internal static byte[] SpriteEffectBytes => GetMonoGameEmbeddedResourceBytes("Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo");

        internal static byte[] MultiTextureOverlayBytes => GetFileResourceBytes("engine/fx/ppfx/multi_tex_overlay");

        internal static byte[] ScanlinesBytes => GetFileResourceBytes("engine/fx/ppfx/scanline");

        internal static byte[] ReflectionBytes => GetFileResourceBytes("engine/fx/ppfx/reflection");

        internal static byte[] GrayscaleBytes => GetFileResourceBytes("engine/fx/ppfx/grayscale");

        internal static byte[] SepiaBytes => GetFileResourceBytes("engine/fx/ppfx/sepia");

        internal static byte[] PaletteCyclerBytes => GetFileResourceBytes("engine/fx/ppfx/palette_cycle");


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