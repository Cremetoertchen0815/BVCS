using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez
{
    public class MosaicPostProcessor : PostProcessor
    {
        private EffectParameter _horDivide;
        private EffectParameter _verDivide;
        private Vector2 _Resolution = new Vector2(50, 50);
        private Vector2 _SceneResolution;

        public MosaicPostProcessor(int execOrder) : base(execOrder) => _SceneResolution = Screen.Size;

        public override void OnAddedToScene(Scene scene)
        {
            base.OnAddedToScene(scene);

            Effect = scene.Content.LoadEffect<Effect>("Mosaic", EffectResource.Mosaic);
            _horDivide = Effect.Parameters["horDivide"];
            _verDivide = Effect.Parameters["verDivide"];

            SamplerState = SamplerState.PointClamp;
            Resolution = _Resolution;
        }

        public Vector2 Resolution
        {
            get => _Resolution;
            set
            {
                _horDivide?.SetValue((float)System.Math.Floor(value.X));
                _verDivide?.SetValue((float)System.Math.Floor(value.Y));
                _Resolution = value;
            }
        }

        public float Divide
        {
            get => _SceneResolution.X / Resolution.X;
            set => Resolution = _SceneResolution / value;
        }

        public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
        {
            base.OnSceneBackBufferSizeChanged(newWidth, newHeight);
            _SceneResolution = new Vector2(newWidth, newHeight);
        }
    }
}
