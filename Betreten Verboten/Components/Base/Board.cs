using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using Nez.Textures;
using static Betreten_Verboten.GlobalFields;

namespace Betreten_Verboten.Components.Base
{
    public class Board : RenderableComponent
    {

        private const int TEX_RES = 500;

        private RenderTexture _boardTexture;
        protected ShapeRenderer _shapeRenderer;
        protected RenderTexture _shadowProjection;
        protected StaticBody _kinematicBody;

        public override void OnAddedToEntity()
        {
            //Check for correct Entity type
            Insist.IsNotNull(Entity as GeonEntity, "Board has to be added to GeonEntity!");

            //Add board texture renderer
            _shadowProjection = Entity.Scene.AddRenderer(new ShadowPlaneRenderer(-2)).RenderTexture;
            Entity.Scene.AddRenderer(new RenderLayerRenderer(-1, RENDER_LAYER_BOARD) { RenderTexture = _boardTexture = new RenderTexture(TEX_RES, TEX_RES) { ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None }, RenderTargetClearColor = Color.Black });

            var texx = Entity.Scene.Content.LoadTexture("tex");
            //Configure 3D renderer
            var geonEntity = Entity as GeonEntity;
            _shapeRenderer = geonEntity.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Plane));
            _shapeRenderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            _shapeRenderer.Node.RotationType = RotationType.Euler;
            _shapeRenderer.Node.Scale = new Vector3(20);
            _shapeRenderer.Node.Position = new Vector3(0, 1, 0);
            _shapeRenderer.RenderingQueue = RenderingQueue.BackgroundNoCull;
            _shapeRenderer.SetMaterial(new ShadowPlaneMaterial() { Texture = texx, ShadowMap = _shadowProjection, TextureEnabled = true, DiffuseColor = Color.DarkGray, AmbientLight = Color.White * 0.25f });

            //Configure physics
            _kinematicBody = geonEntity.AddComponent(new StaticBody(new EndlessPlaneInfo(Vector3.Up)));
            _kinematicBody.CollisionGroup = Nez.GeonBit.Physics.CollisionGroups.Terrain;
            _kinematicBody.Restitution = 6f;

            //Config renderable 
            SetRenderLayer(RENDER_LAYER_BOARD);



        }

        public override bool IsVisibleFromCamera(Camera camera) => true;
        public override RectangleF Bounds => new RectangleF(0, 0, TEX_RES, TEX_RES);

        protected override void Render(Batcher batcher, Camera camera)
        {
            batcher.DrawRect(new Rectangle(100, 100, 700, 700), Color.Lime);
        }
    }
}
