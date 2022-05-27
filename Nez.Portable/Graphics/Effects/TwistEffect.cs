using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
    public class TwistEffect : Effect
    {
        [Range(0, 2)]
        public float Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    _radius = value;
                    _radiusParam.SetValue(_radius);
                }
            }
        }

        [Range(-50, 50)]
        public float Angle
        {
            get => _angle;
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    _angleParam.SetValue(_angle);
                }
            }
        }

        public Vector2 Offset
        {
            get => _offset;
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    _offsetParam.SetValue(_offset);
                }
            }
        }

        private float _radius = 0.5f;
        private float _angle = 5f;
        private Vector2 _offset = Vector2Ext.HalfVector();
        private EffectParameter _radiusParam;
        private EffectParameter _angleParam;
        private EffectParameter _offsetParam;


        public TwistEffect() : base(Core.GraphicsDevice, EffectResource.TwistBytes)
        {
            _radiusParam = Parameters["radius"];
            _angleParam = Parameters["angle"];
            _offsetParam = Parameters["offset"];

            _radiusParam.SetValue(_radius);
            _angleParam.SetValue(_angle);
            _offsetParam.SetValue(_offset);
        }
    }
}