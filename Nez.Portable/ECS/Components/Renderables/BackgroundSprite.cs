using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nez.Sprites
{
    internal class BackgroundSprite : RenderableComponent
    {
        public BackgroundSprite(Texture2D texture, Rectangle destination, Vector2 parallax)
        {
            Texture = texture;
            Position = destination.Location;
            Size = destination.Size;
            Parallax = parallax;
        }

        //Properties
        public Point Position { get; set; }
        public Point Size { get; set; }
        public Vector2 Origin { get; set; }
        public Texture2D Texture { get; set; }
        public Vector2 Parallax { get; set; }
        public LoopMode LoopHorizontal { get; set; }
        public LoopMode LoopVertical { get; set; }

        //Fields
        private Rectangle rectangleOriginal = new Rectangle(0, 0, 1, 1);
        protected BlendState Blend = BlendState.AlphaBlend;
        private static Batcher NeutralBatcher = new Batcher(Core.GraphicsDevice); //A batcher not influenced by the camera's transformation matrix

        public override void Render(Batcher batcher, Camera camera)
        {
            var renderSize = Entity.Scene.SceneRenderTargetSize.ToVector2();
            rectangleOriginal = new Rectangle(Position + ((camera.TransformPosition + Origin) * Parallax).ToPoint(), Size);
            int minX, minY, maxX, maxY;


            //Calculate screen wrap
            if (LoopHorizontal == LoopMode.ScreenWrap) rectangleOriginal.X = (int)Mathf.Repeat(rectangleOriginal.X + Size.X, renderSize.X) - Size.X;
            if (LoopVertical == LoopMode.ScreenWrap) rectangleOriginal.Y = (int)Mathf.Repeat(rectangleOriginal.Y + Size.Y, renderSize.Y) - Size.Y;

            //Start rendering
            NeutralBatcher.Begin(Blend);

            //Draw base
            NeutralBatcher.Draw(Texture, rectangleOriginal);

            //Draw fill loops
            if (LoopHorizontal == LoopMode.FillJump || LoopHorizontal == LoopMode.FillReverse)
            {
                minX = Math.Min((int)Math.Floor((float)-rectangleOriginal.Left / rectangleOriginal.Width), 0);
                minY = Math.Min((int)Math.Floor((float)-rectangleOriginal.Top / rectangleOriginal.Height), 0);
                maxX = Math.Max((int)Math.Ceiling((renderSize.X - rectangleOriginal.Right) / rectangleOriginal.Width), 0);
                maxY = Math.Max((int)Math.Ceiling((renderSize.Y - rectangleOriginal.Bottom) / rectangleOriginal.Height), 0);

                for (int x = minX; x < maxX; x++)
                {
                    var fxX = LoopHorizontal == LoopMode.FillReverse && Mathf.IsOdd(x) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    if (LoopVertical == LoopMode.FillJump || LoopVertical == LoopMode.FillReverse)
                    {
                        for (int y = minY; y < maxY; y++)
                        {
                            //Draw mixed horizontal and vertical loop
                            if (x == 0 && y == 0) continue; //Skip if image is the base
                            var fxY = LoopVertical == LoopMode.FillReverse && Mathf.IsOdd(y) ? SpriteEffects.FlipVertically : SpriteEffects.None;
                            NeutralBatcher.Draw(Texture, new Rectangle(rectangleOriginal.Location + new Point(x * rectangleOriginal.Width, y * rectangleOriginal.Height), Size), null, Color.White, effects: fxX | fxY);
                        }
                    }
                    else
                    {
                        //Draw horizontal loop
                        if (x == 0) continue; //Skip if image is the base
                        NeutralBatcher.Draw(Texture, new Rectangle(rectangleOriginal.Location + new Point(x * rectangleOriginal.Width, 0), Size), null, Color.White, effects: fxX);
                    }
                }
            }
            else if (LoopVertical == LoopMode.FillJump || LoopVertical == LoopMode.FillReverse)
            {
                minY = Math.Min((int)Math.Floor((float)-rectangleOriginal.Top / rectangleOriginal.Height), 0);
                maxY = Math.Max((int)Math.Ceiling((renderSize.Y - rectangleOriginal.Bottom) / rectangleOriginal.Height), 0);

                //Draw vertical loop
                for (int y = minY; y < maxY; y++)
                {
                    if (y == 0) continue; //Skip if image is the base
                    var fxY = LoopVertical == LoopMode.FillReverse && Mathf.IsOdd(y) ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    NeutralBatcher.Draw(Texture, new Rectangle(rectangleOriginal.Location + new Point(0, y * rectangleOriginal.Height), Size), null, Color.White, effects: fxY);
                }
            }

            //Draw screen repeat
            if (LoopHorizontal == LoopMode.ScreenWrap) NeutralBatcher.Draw(Texture, new Rectangle(rectangleOriginal.Location + new Point((int)renderSize.X, 0), Size));
            if (LoopVertical == LoopMode.ScreenWrap) NeutralBatcher.Draw(Texture, new Rectangle(rectangleOriginal.Location + new Point(0, (int)renderSize.Y), Size));
            if (LoopHorizontal == LoopMode.ScreenWrap && LoopVertical == LoopMode.ScreenWrap) NeutralBatcher.Draw(Texture, new Rectangle(rectangleOriginal.Location + renderSize.ToPoint(), Size));

            NeutralBatcher.End();
        }

        public override bool IsVisibleFromCamera(Camera camera) => true;
        public override RectangleF Bounds => new RectangleF(Position.ToVector2(), Size.ToVector2());

        public enum LoopMode
        {
            None, FillJump, FillReverse, ScreenWrap
        }
    }
}
