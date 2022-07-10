using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.GeonBit
{
    public class ShadowPlaneRenderer : Renderer
    {

        private Matrix _projection;
        private Texture2D _filler;
        private Lights.ILightsManager lightsManager => GeonDefaultRenderer.ActiveLightsManager;
        private SpriteBatch _batch;
        public ShadowPlaneRenderer(int renderOrder) : base(renderOrder)
        {
            RenderTexture = GeonDefaultRenderer.ActiveLightsManager.ShadowMap = new RenderTexture(1000, 1000, SurfaceFormat.Color, DepthFormat.Depth24Stencil8) { ResizeBehavior = Textures.RenderTexture.RenderTextureResizeBehavior.None };
            _batch = new SpriteBatch(Core.GraphicsDevice);
            _filler = new Texture2D(Core.GraphicsDevice, 1, 1);
            _filler.SetData(new Color[] { Color.White });
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1, 1, 999);//Matrix.CreateOrthographic(1200, 1200, 1, 50);
        }

        public override void OnAddedToScene(Scene scene) => lightsManager.ShadowsEnabed = true;

        public override void Render(Scene scene)
        {
            Core.GraphicsDevice.SetRenderTarget(RenderTexture);
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            NodesManager.StartFrame();

            // update culling nodes camera frustum
            CullingNode.CurrentCameraFrustum = GeonDefaultRenderer.ActiveCamera != null ? GeonDefaultRenderer.ActiveCamera.ViewFrustum : null;

            var lst = scene.EntitiesOfType<GeonEntity>();
            foreach (var item in lst)
            {
                item.Node?.Draw();
            }
            ListPool<GeonEntity>.Free(lst);

            Materials.MaterialAPI.SetViewProjection(lightsManager.ShadowViewMatrix, _projection);
            RenderingQueues.DrawQueues(false);

            Core.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);

            _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
            _batch.Draw(_filler, new Rectangle(0, 0, RenderTexture.RenderTarget.Width, RenderTexture.RenderTarget.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1f);
            _batch.End();
        }
    }
}
