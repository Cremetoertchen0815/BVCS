using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Betreten_Verboten.GlobalFields;

namespace Betreten_Verboten.Components.Base
{
    public class Board : RenderableComponent
    {

        private const int TEX_RES = 500;

        private RenderTexture _boardTexture;
        protected ShapeRenderer _shapeRenderer;
        protected KinematicBody _kinematicBody;

        public override void OnAddedToEntity()
        {
            //Check for correct Entity type
            Insist.IsNotNull(Entity as GeonEntity, "Board has to be added to GeonEntity!");

            //Add board texture renderer
            Entity.Scene.AddRenderer(new RenderLayerRenderer(0, RENDER_LAYER_BOARD) { RenderTexture = _boardTexture = new RenderTexture(TEX_RES, TEX_RES) { ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None }, RenderTargetClearColor = Color.Black });

            //Configure 3D renderer
            var geonEntity = Entity as GeonEntity;
            _shapeRenderer = geonEntity.AddComponent(new ShapeRenderer(ShapeMeshes.Plane), null);
            _shapeRenderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            _shapeRenderer.Node.RotationType = RotationType.Euler;
            _shapeRenderer.Node.Position = new Vector3(0, 0, 0);
            _shapeRenderer.Node.Scale = new Vector3(100);
            _shapeRenderer.RenderingQueue = RenderingQueue.Terrain;
            _shapeRenderer.SetMaterial(new BasicMaterial() { Texture = _boardTexture, TextureEnabled = true, DiffuseColor = Color.White });

            //Configure physics
            _kinematicBody = geonEntity.AddComponent(new KinematicBody(new EndlessPlaneInfo(Vector3.Up)));
            _kinematicBody.CollisionGroup = Nez.GeonBit.Physics.CollisionGroups.Terrain;


            //Config renderable 
            SetRenderLayer(RENDER_LAYER_BOARD);


        }

        protected override void Render(Batcher batcher, Camera camera)
        {

        }
    }
}
