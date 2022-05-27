using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;
using System.Collections;


namespace Nez
{
    public class CinematicLetterboxPostProcessor : PostProcessor
    {
        /// <summary>
        /// color of the letterbox
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;

                    if (Effect != null)
                        _colorParam.SetValue(_color.ToVector4());
                }
            }
        }

        /// <summary>
        /// size in pixels of the letterbox
        /// </summary>
        /// <value>The size of the letterbox.</value>
        public float LetterboxSize
        {
            get => _letterboxSize;
            set
            {
                if (_letterboxSize != value)
                {
                    _letterboxSize = value;

                    if (Effect != null)
                        _letterboxSizeParam.SetValue(_letterboxSize);
                }
            }
        }

        private Color _color = Color.Black;
        private float _letterboxSize = 0f;
        private EffectParameter _colorParam;
        private EffectParameter _letterboxSizeParam;
        private bool _isAnimating;


        public CinematicLetterboxPostProcessor(int executionOrder) : base(executionOrder)
        {
        }

        public override void OnAddedToScene(Scene scene)
        {
            base.OnAddedToScene(scene);
            Effect = _scene.Content.LoadEffect<Effect>("vignette", EffectResource.LetterboxBytes);

            _colorParam = Effect.Parameters["_color"];
            _letterboxSizeParam = Effect.Parameters["_letterboxSize"];
            _colorParam.SetValue(_color.ToVector4());
            _letterboxSizeParam.SetValue(_letterboxSize);
        }

        public override void Unload()
        {
            _scene.Content.UnloadEffect(Effect);
            base.Unload();
        }

        /// <summary>
        /// animates the letterbox in
        /// </summary>
        /// <returns>The in.</returns>
        /// <param name="letterboxSize">Letterbox size.</param>
        /// <param name="duration">Duration.</param>
        /// <param name="easeType">Ease type.</param>
        public IEnumerator AnimateIn(float letterboxSize, float duration = 2, EaseType easeType = EaseType.ExpoOut)
        {
            // wait for any current animations to complete
            while (_isAnimating)
                yield return null;

            _isAnimating = true;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.DeltaTime;
                LetterboxSize = Lerps.Ease(easeType, 0, letterboxSize, elapsedTime, duration);
                yield return null;
            }

            _isAnimating = false;
        }

        /// <summary>
        /// animates the letterbox out
        /// </summary>
        /// <returns>The out.</returns>
        /// <param name="duration">Duration.</param>
        /// <param name="easeType">Ease type.</param>
        public IEnumerator AnimateOut(float duration = 2, EaseType easeType = EaseType.ExpoIn)
        {
            // wait for any current animations to complete
            while (_isAnimating)
                yield return null;

            _isAnimating = true;
            float startSize = LetterboxSize;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.DeltaTime;
                LetterboxSize = Lerps.Ease(easeType, startSize, 0, elapsedTime, duration);
                yield return null;
            }

            _isAnimating = false;
        }
    }
}