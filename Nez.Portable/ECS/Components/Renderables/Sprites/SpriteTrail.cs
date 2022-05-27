using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez.Sprites
{
    /// <summary>
    /// renders and fades a series of copies of the Sprite on the same Entity. minDistanceBetweenInstances determines how often a trail
    /// sprite is added.
    /// </summary>
    public class SpriteTrail : RenderableComponent, IUpdatable
    {
        /// <summary>
        /// helper class that houses the data required for the individual trail instances
        /// </summary>
        private class SpriteTrailInstance
        {
            public Rectangle DestRect;
            public Vector2 Position;
            private Sprite _sprite;
            private float _fadeDuration;
            private float _fadeDelay;
            private float _elapsedTime;
            private Color _initialColor;
            private Color _targetColor;
            private Color _renderColor;
            private float _rotation;
            private Vector2 _origin;
            private SpriteEffects _spriteEffects;
            private float _layerDepth;


            public void Spawn(Vector2 position, Vector2 size, Sprite sprite, float fadeDuration, float fadeDelay,
                              Color initialColor, Color targetColor)
            {
                DestRect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
                Position = position;
                _sprite = sprite;

                _initialColor = initialColor;
                _elapsedTime = 0f;

                _fadeDuration = fadeDuration;
                _fadeDelay = fadeDelay;
                _initialColor = initialColor;
                _targetColor = targetColor;
            }


            public void SetSpriteRenderOptions(float rotation, Vector2 origin,
                                               SpriteEffects spriteEffects, float layerDepth)
            {
                _rotation = rotation;
                _origin = origin;
                _spriteEffects = spriteEffects;
                _layerDepth = layerDepth;
            }


            /// <summary>
            /// returns true when the fade out is complete
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Update()
            {
                _elapsedTime += Time.DeltaTime;

                // fading block
                if (_elapsedTime > _fadeDelay && _elapsedTime < _fadeDuration + _fadeDelay)
                {
                    float t = Mathf.Map01(_elapsedTime, 0f, _fadeDelay + _fadeDuration);
                    ColorExt.Lerp(ref _initialColor, ref _targetColor, out _renderColor, t);
                }
                else if (_elapsedTime >= _fadeDuration + _fadeDelay)
                {
                    return true;
                }

                return false;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Render(Batcher batcher, Camera camera) => batcher.Draw(_sprite, DestRect, _sprite.SourceRect, _renderColor, _rotation, _origin, _spriteEffects,
                    _layerDepth);
        }


        public override RectangleF Bounds => _bounds;

        public int MaxSpriteInstances
        {
            get => _maxSpriteInstances;
            set => SetMaxSpriteInstances(value);
        }

        /// <summary>
        /// how far does the Sprite have to move before a new instance is spawned
        /// </summary>
        public float MinDistanceBetweenInstances = 30f;

        /// <summary>
        /// total duration of the fade from initialColor to fadeToColor
        /// </summary>
        public float FadeDuration = 0.8f;

        /// <summary>
        /// delay before starting the color fade
        /// </summary>
        public float FadeDelay = 0.1f;

        /// <summary>
        /// initial color of the trail instances
        /// </summary>
        public Color InitialColor = Color.White;

        /// <summary>
        /// final color that will be lerped to over the course of fadeDuration
        /// </summary>
        public Color FadeToColor = Color.Transparent;

        public Vector2 Size = Vector2.One;
        private int _maxSpriteInstances = 15;
        private Stack<SpriteTrailInstance> _availableSpriteTrailInstances = new Stack<SpriteTrailInstance>();
        private List<SpriteTrailInstance> _liveSpriteTrailInstances = new List<SpriteTrailInstance>(5);
        private Vector2 _lastPosition;
        private SpriteRenderer _sprite;

        /// <summary>
        /// flag when true it will always add a new instance regardless of the distance check
        /// </summary>
        private bool _isFirstInstance;

        /// <summary>
        /// if awaitingDisable all instances are allowed to fade out before the component is disabled
        /// </summary>
        private bool _awaitingDisable;


        public SpriteTrail()
        {
        }

        public SpriteTrail(SpriteRenderer sprite) => _sprite = sprite;


        #region Fluent setters

        public SpriteTrail SetMaxSpriteInstances(int maxSpriteInstances)
        {
            // if our new value is greater than our previous count instantiate the required SpriteTrailInstances
            if (_availableSpriteTrailInstances.Count < maxSpriteInstances)
            {
                int newInstances = _availableSpriteTrailInstances.Count - maxSpriteInstances;
                for (int i = 0; i < newInstances; i++)
                    _availableSpriteTrailInstances.Push(new SpriteTrailInstance());
            }

            // if our new value is less than our previous count trim the List
            if (_availableSpriteTrailInstances.Count > maxSpriteInstances)
            {
                int excessInstances = maxSpriteInstances - _availableSpriteTrailInstances.Count;
                for (int i = 0; i < excessInstances; i++)
                    _availableSpriteTrailInstances.Pop();
            }

            _maxSpriteInstances = maxSpriteInstances;


            return this;
        }


        public SpriteTrail SetMinDistanceBetweenInstances(float minDistanceBetweenInstances)
        {
            MinDistanceBetweenInstances = minDistanceBetweenInstances;
            return this;
        }


        public SpriteTrail SetFadeDuration(float fadeDuration)
        {
            FadeDuration = fadeDuration;
            return this;
        }


        public SpriteTrail SetFadeDelay(float fadeDelay)
        {
            FadeDelay = fadeDelay;
            return this;
        }


        public SpriteTrail SetInitialColor(Color initialColor)
        {
            InitialColor = initialColor;
            return this;
        }


        public SpriteTrail SetFadeToColor(Color fadeToColor)
        {
            FadeToColor = fadeToColor;
            return this;
        }

        #endregion


        /// <summary>
        /// enables the SpriteTrail
        /// </summary>
        /// <returns>The sprite trail.</returns>
        public SpriteTrail EnableSpriteTrail()
        {
            _awaitingDisable = false;
            _isFirstInstance = true;
            Enabled = true;
            return this;
        }

        /// <summary>
        /// disables the SpriteTrail optionally waiting for the current trail to fade out first
        /// </summary>
        /// <param name="completeCurrentTrail">If set to <c>true</c> complete current trail.</param>
        public void DisableSpriteTrail(bool completeCurrentTrail = true)
        {
            if (completeCurrentTrail)
            {
                _awaitingDisable = true;
            }
            else
            {
                Enabled = false;

                for (int i = 0; i < _liveSpriteTrailInstances.Count; i++)
                    _availableSpriteTrailInstances.Push(_liveSpriteTrailInstances[i]);
                _liveSpriteTrailInstances.Clear();
            }
        }

        public override void OnAddedToEntity()
        {
            if (_sprite == null)
                _sprite = this.GetComponent<SpriteRenderer>();

            if (_sprite == null)
            {
                Enabled = false;
                return;
            }

            // move the trail behind the Sprite
            LayerDepth = _sprite.LayerDepth + 0.001f;

            // if setMaxSpriteInstances is called it will handle initializing the SpriteTrailInstances so make sure we dont do it twice
            if (_availableSpriteTrailInstances.Count == 0)
            {
                for (int i = 0; i < _maxSpriteInstances; i++)
                    _availableSpriteTrailInstances.Push(new SpriteTrailInstance());
            }
        }

        public virtual void Update()
        {
            if (_isFirstInstance)
            {
                _isFirstInstance = false;
                SpawnInstance();
            }
            else
            {
                float distanceMoved = Math.Abs(Vector2.Distance(Entity.Transform.Position + _localOffset, _lastPosition));
                if (distanceMoved >= MinDistanceBetweenInstances)
                    SpawnInstance();
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            // update any live instances
            for (int i = _liveSpriteTrailInstances.Count - 1; i >= 0; i--)
            {
                if (_liveSpriteTrailInstances[i].Update())
                {
                    _availableSpriteTrailInstances.Push(_liveSpriteTrailInstances[i]);
                    _liveSpriteTrailInstances.RemoveAt(i);
                }
                else
                {
                    // calculate our min/max for the bounds
                    Vector2.Min(ref min, ref _liveSpriteTrailInstances[i].Position, out min);
                    Vector2.Max(ref max, ref _liveSpriteTrailInstances[i].Position, out max);
                }
            }

            _bounds.Location = min;
            _bounds.Width = max.X - min.X;
            _bounds.Height = max.Y - min.Y;
            _bounds.Inflate(Size.X, Size.Y);

            // nothing left to render. disable ourself
            if (_awaitingDisable && _liveSpriteTrailInstances.Count == 0)
                Enabled = false;
        }

        public override bool IsVisibleFromCamera(Camera camera) => base.IsVisibleFromCamera(camera);

        /// <summary>
        /// stores the last position for distance calculations and spawns a new trail instance if there is one available in the stack
        /// </summary>
        private void SpawnInstance()
        {
            _lastPosition = _sprite.Entity.Transform.Position + _sprite.LocalOffset;

            if (_awaitingDisable || _availableSpriteTrailInstances.Count == 0)
                return;

            var instance = _availableSpriteTrailInstances.Pop();
            instance.Spawn(_lastPosition, Size, _sprite.Sprite, FadeDuration, FadeDelay, InitialColor, FadeToColor);
            instance.SetSpriteRenderOptions(_sprite.Entity.Transform.Rotation, _sprite.Sprite.Origin, _sprite.SpriteEffects, LayerDepth);
            _liveSpriteTrailInstances.Add(instance);
        }

        protected override void Render(Batcher batcher, Camera camera)
        {
            for (int i = 0; i < _liveSpriteTrailInstances.Count; i++)
                _liveSpriteTrailInstances[i].Render(batcher, camera);
        }
    }
}