using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
    public class PolygonLightEffect : Effect
    {
        public Matrix ViewProjectionMatrix { set => _viewProjectionMatrixParam.SetValue(value); }
        public Vector2 LightSource { set => _lightSourceParam.SetValue(value); }
        public Vector3 LightColor { set => _lightColorParam.SetValue(value); }
        public float LightRadius { set => _lightRadius.SetValue(value); }

        private EffectParameter _viewProjectionMatrixParam;
        private EffectParameter _lightSourceParam;
        private EffectParameter _lightColorParam;
        private EffectParameter _lightRadius;

        public PolygonLightEffect() : base(Core.GraphicsDevice, EffectResource.PolygonLightBytes)
        {
            _viewProjectionMatrixParam = Parameters["viewProjectionMatrix"];
            _lightSourceParam = Parameters["lightSource"];
            _lightColorParam = Parameters["lightColor"];
            _lightRadius = Parameters["lightRadius"];
        }
    }
}