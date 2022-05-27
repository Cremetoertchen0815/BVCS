using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Sprites
{
    /// <summary>
    /// the most basic and common Renderable. Renders a Sprite/Texture.
    /// </summary>
    public class SpriteRenderer : RenderableComponent
    {
        public override RectangleF Bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    if (_sprite != null)
                    {
                        _bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin,
                            Entity.Transform.Scale, Entity.Transform.Rotation, _sprite.SourceRect.Width,
                            _sprite.SourceRect.Height);
                    }

                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }

        /// <summary>
        /// the origin of the Sprite. This is set automatically when setting a Sprite.
        /// </summary>
        /// <value>The origin.</value>
        public Vector2 Origin
        {
            get => _origin;
            set => SetOrigin(value);
        }

        public Vector2 Size = Vector2.Zero;

        /// <summary>
        /// helper property for setting the origin in normalized fashion (0-1 for x and y)
        /// </summary>
        /// <value>The origin normalized.</value>
        public Vector2 OriginNormalized
        {
            get => new Vector2(_origin.X / Width * Entity.Transform.Scale.X,
                _origin.Y / Height * Entity.Transform.Scale.Y);
            set => SetOrigin(new Vector2(value.X * Width / Entity.Transform.Scale.X,
                value.Y * Height / Entity.Transform.Scale.Y));
        }

        /// <summary>
        /// determines if the sprite should be rendered normally or flipped horizontally
        /// </summary>
        /// <value><c>true</c> if flip x; otherwise, <c>false</c>.</value>
        public bool FlipX
        {
            get => (SpriteEffects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
            set => SpriteEffects = value
                ? (SpriteEffects | SpriteEffects.FlipHorizontally)
                : (SpriteEffects & ~SpriteEffects.FlipHorizontally);
        }

        /// <summary>
        /// determines if the sprite should be rendered normally or flipped vertically
        /// </summary>
        /// <value><c>true</c> if flip y; otherwise, <c>false</c>.</value>
        public bool FlipY
        {
            get => (SpriteEffects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            set => SpriteEffects = value
                ? (SpriteEffects | SpriteEffects.FlipVertically)
                : (SpriteEffects & ~SpriteEffects.FlipVertically);
        }

        /// <summary>
        /// Batchers passed along to the Batcher when rendering. flipX/flipY are helpers for setting this.
        /// </summary>
        public SpriteEffects SpriteEffects = SpriteEffects.None;

        /// <summary>
        /// the Sprite that should be displayed by this Sprite. When set, the origin of the Sprite is also set to match Sprite.origin.
        /// </summary>
        /// <value>The sprite.</value>
        public Sprite Sprite
        {
            get => _sprite;
            set => SetSprite(value);
        }

        protected Vector2 _origin;
        protected Sprite _sprite;


        public SpriteRenderer()
        { }

        public SpriteRenderer(Texture2D texture) : this(new Sprite(texture))
        { }

        public SpriteRenderer(Sprite sprite) => SetSprite(sprite);

        #region fluent setters

        /// <summary>
        /// sets the Sprite and updates the origin of the Sprite to match Sprite.origin. If for whatever reason you need
        /// an origin different from the Sprite either clone it or set the origin AFTER setting the Sprite here.
        /// </summary>
        public SpriteRenderer SetSprite(Sprite sprite, SizingMode sMode = SizingMode.KeepSize)
        {
            _sprite = sprite;
            if (Size == Vector2.Zero || sMode == SizingMode.Resize) Size = new Vector2(sprite.Texture2D.Width, sprite.Texture2D.Height);
            if (_sprite != null)
                _origin = _sprite.Origin;
            return this;
        }

        /// <summary>
        /// sets the origin for the Renderable
        /// </summary>
        public SpriteRenderer SetOrigin(Vector2 origin)
        {
            if (_origin != origin)
            {
                _origin = origin;
                _areBoundsDirty = true;
            }

            return this;
        }

        /// <summary>
        /// helper for setting the origin in normalized fashion (0-1 for x and y)
        /// </summary>
        public SpriteRenderer SetOriginNormalized(Vector2 value)
        {
            SetOrigin(new Vector2(value.X * Width / Entity.Transform.Scale.X,
                value.Y * Height / Entity.Transform.Scale.Y));
            return this;
        }

        #endregion

        public enum SizingMode { KeepSize, Resize }


        /// <summary>
        /// Draws the Renderable with an outline. Note that this should be called on disabled Renderables since they shouldnt take part in default
        /// rendering if they need an ouline.
        /// </summary>
        public void DrawOutline(Batcher batcher, Camera camera, bool useStencil, int offset = 1) => DrawOutline(batcher, camera, Color.Black, useStencil, offset);

        public void DrawOutline(Batcher batcher, Camera camera, Color outlineColor, bool useStencil, int offset = 1)
        {
            // save the stuff we are going to modify so we can restore it later
            var originalPosition = _localOffset;
            var originalColor = Color;
            float originalLayerDepth = _layerDepth;

            // set our new values
            Color = outlineColor;
            _layerDepth += 0.01f;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        _localOffset = originalPosition + new Vector2(i * offset, j * offset);

                        var txt = useStencil ? Sprite.StencilTexture : Sprite.Texture2D;
                        batcher.Draw(txt, new Rectangle((int)(Entity.Transform.Position.X + LocalOffset.X), (int)(Entity.Transform.Position.Y + LocalOffset.Y),
                                                                       (int)(Entity.Transform.Scale.X * Size.X), (int)(Entity.Transform.Scale.Y * Size.Y)), Sprite.SourceRect, Color, Entity.Transform.Rotation, Origin,
                                                                       SpriteEffects, _layerDepth);
                    }
                }
            }

            // restore changed state
            _localOffset = originalPosition;
            Color = originalColor;
            _layerDepth = originalLayerDepth;
        }

        protected override void Render(Batcher batcher, Camera camera) => batcher.Draw(Sprite, new Rectangle((int)(Entity.Transform.Position.X + LocalOffset.X), (int)(Entity.Transform.Position.Y + LocalOffset.Y),
                                                                       (int)(Entity.Transform.Scale.X * Size.X), (int)(Entity.Transform.Scale.Y * Size.Y)), Sprite.SourceRect, Color, Entity.Transform.Rotation, Origin,
                                                                       SpriteEffects, _layerDepth);
    }
}