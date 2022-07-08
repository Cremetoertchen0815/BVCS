using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;
using Nez.Textures;


namespace Nez
{
    /// <summary>
    /// wrapper class that holds in instance of a Batcher and helpers so that it can be passed around and draw anything.
    /// </summary>
    public class Graphics
    {
        public static Graphics Instance;

        /// <summary>
        /// All 2D rendering is done through this Batcher instance
        /// </summary>
        public Batcher Batcher;

        /// <summary>
        /// default font is loaded up and stored here for easy access. Nez uses it for the DebugConsole
        /// </summary>
        public BitmapFont BitmapFont;

        public Sprite DebugSprite;

        /// <summary>
        /// A sprite used to draw rectangles, lines, circles, etc. 
        /// Will be generated at startup, but you can replace this with a sprite from your atlas to reduce texture swaps.
        /// Should be a 1x1 white pixel
        /// </summary>
        public Sprite PixelTexture { get; set; }

        public Sprite CircleTexture;

        public Graphics()
        {
        }


        public Graphics(BitmapFont font)
        {
            Batcher = new Batcher(Core.GraphicsDevice);
            BitmapFont = font;

            PixelTexture = new Sprite(CreateSingleColorTexture(1, 1, Color.White), 0, 0, 1, 1);

            DebugSprite = new Sprite(Texture2D.FromFile(Core.GraphicsDevice, Core.Content.RootDirectory + "/nez/textures/placeholder.png"));
            CircleTexture = new Sprite(Texture2D.FromFile(Core.GraphicsDevice, Core.Content.RootDirectory + "/nez/textures/circle.png"));
        }


        /// <summary>
        /// helper method that generates a single color texture of the given dimensions
        /// </summary>
        /// <returns>The single color texture.</returns>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="color">Color.</param>
        public static Texture2D CreateSingleColorTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(Core.GraphicsDevice, width, height);
            var data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
                data[i] = color;

            texture.SetData<Color>(data);
            return texture;
        }


        public void Unload()
        {
            if (PixelTexture != null)
                PixelTexture.Texture2D.Dispose();
            PixelTexture = null;

            Batcher.Dispose();
            Batcher = null;
        }
    }
}