using Nez.Tiled;

namespace Nez
{
    public class TiledGroupRenderer : RenderableComponent
    {
        public TmxMap TiledMap;

        public override float Width => TiledMap.Width * TiledMap.TileWidth;
        public override float Height => TiledMap.Height * TiledMap.TileHeight;

        public TmxLayer CollisionLayer;
        private TmxGroup renderGroup;
        public bool DebugOnly;


        public TiledGroupRenderer(TmxMap tiledMap, string groupName, bool debugOnly = false)
        {
            TiledMap = tiledMap;
            renderGroup = TiledMap.Groups[groupName];
            DebugOnly = debugOnly;

        }

        //public override void OnAddedToEntity()
        //{
        //	//Create background sprites
        //	foreach (var l in renderGroup.ImageLayers) 
        //	{
        //		var scale = l.Properties.ContainsKey("scale") ? float.Parse(l.Properties["scale"]) : 1F;
        //		Entity.AddComponent(new BackgroundSprite(l.Image.Texture, new Rectangle((int)l.OffsetX, (int)l.OffsetY, (int)(scale * l.Image.Texture.Width), (int)(scale * l.Image.Texture.Height)), new Vector2(l.ParallaxFactorX, l.ParallaxFactorY))); 
        //	}
        //}

        /// <summary>
        /// sets this components group to render
        /// </summary>
        /// <param name="layerName">Layer name.</param>
        public void SetGroupToRender(string groupName) => renderGroup = TiledMap.Groups[groupName];


        #region Component overrides

        public override void Render(Batcher batcher, Camera camera)
        {
            if (DebugOnly) return;

            for (int i = 0; i < renderGroup.Layers.Count; i++)
            {
                if (renderGroup.Layers[i].Visible)
                    TiledRendering.RenderLayer(renderGroup.Layers[i], batcher, Entity.Transform.Position + _localOffset, Transform.Scale, LayerDepth, new RectangleF(camera.Bounds.Center - Screen.Center, Screen.Size));
            }
        }

        public override void DebugRender(Batcher batcher)
        {
            if (!DebugOnly) return;
            foreach (var group in renderGroup.ObjectGroups)
                TiledRendering.RenderObjectGroup(group, batcher, Entity.Transform.Position + _localOffset, Transform.Scale, LayerDepth);

        }

        #endregion
    }
}