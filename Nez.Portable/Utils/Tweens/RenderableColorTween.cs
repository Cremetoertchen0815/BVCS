using Microsoft.Xna.Framework;


namespace Nez.Tweens
{
    public class RenderableColorTween : ColorTween, ITweenTarget<Color>
    {
        private RenderableComponent _renderable;


        public void SetTweenedValue(Color value) => _renderable.Color = value;


        public Color GetTweenedValue() => _renderable.Color;


        public new object GetTargetObject() => _renderable;


        protected override void UpdateValue() => SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));


        public void SetTarget(RenderableComponent renderable) => _renderable = renderable;
    }
}