using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.UI;
using System;

namespace Nez.GeonBit.ECS
{
    public class FinalUIRender : GlobalManager, IFinalRenderDelegate
    {
        private SpriteBatch _batch = new SpriteBatch(Core.GraphicsDevice);
        private UserInterface _ui;
        private GameTime _gt = new GameTime();

        public FinalUIRender()
        {
            if (UserInterface.Active == null) UserInterface.Initialize(Core.Content);
            _ui = UserInterface.Active;
            _ui.UseRenderTarget = true;
        }

        public void HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source, Rectangle finalRenderDestinationRect, SamplerState samplerState)
        {
            _ui.Draw(_batch);

            _batch.Begin();
            _batch.Draw(source, finalRenderDestinationRect, Color.White);
            _batch.End();

            _ui.DrawMainRenderTarget(_batch, finalRenderDestinationRect);

        }

        public void OnAddedToScene(Scene scene) { }

        public void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
        {
            //Tell UI that it shall change resolution
        }

        public void Unload() { }

        public override void Update()
        {
            _gt.ElapsedGameTime = TimeSpan.FromMilliseconds(Time.DeltaTime);
            _ui.Update(_gt);
        }
    }
}
