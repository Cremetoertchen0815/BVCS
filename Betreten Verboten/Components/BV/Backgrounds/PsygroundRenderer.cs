using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.BV.Backgrounds
{
    public class PsygroundRenderer : Renderer
    {
        //Fields
        private VertexPositionColor[] _vertexList;
        private int[] _indexList;
        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private BasicEffect _effect;
        private bool _loops = true;
        private float _alpha;
        private Matrix _view;
        private Matrix _projection;

        //Corner colors
        private Color _colorA;
        private Color _colorB;
        private Color _colorC;
        private Color _colorD;

        //Properties
        public Vector2 Size { get; set; } = Screen.Size;
        public Color[] Colors { get; set; } = { Color.Blue, Color.DarkCyan, Color.Purple, Color.Orange, Color.Green, Color.Teal, Color.Black, Color.Maroon };
        public float Speed { get; set; } = 10f;

        public PsygroundRenderer(int order, float alpha = 0.3f) : base(order) => _alpha = alpha;

        public override void OnAddedToScene(Scene scene)
        {
            base.OnAddedToScene(scene);

            _view = Matrix.CreateLookAt(new Vector3(0, 0, -1), new Vector3(0, 0, 0), new Vector3(0, -10, 0));
            _projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreatePerspectiveOffCenter(0, Size.X, Size.Y, 0, 1, 999);

            _vertexList = new VertexPositionColor[] {
                          new VertexPositionColor(new Vector3(0, 0, 0), Color.White),
                          new VertexPositionColor(new Vector3(Size.X, 0, 0), Color.White),
                          new VertexPositionColor(new Vector3(Size.X, Size.Y, 0), Color.White),
                          new VertexPositionColor(new Vector3(0, Size.Y, 0), Color.White)};
            _indexList = new int[] { 0, 1, 2, 0, 2, 3 };

            _vertexBuffer = new DynamicVertexBuffer(Core.GraphicsDevice, typeof(VertexPositionColor), _vertexList.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertexList);
            _indexBuffer = new DynamicIndexBuffer(Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, _indexList.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indexList);

            _effect = new BasicEffect(Core.GraphicsDevice);
            _effect.World = Matrix.Identity;
            _effect.VertexColorEnabled = true;

            _loops = true;
            _colorA = GetRndColor();
            _colorB = GetRndColor();
            _colorC = GetRndColor();
            _colorD = GetRndColor();
            LaunchColorFader();
        }

        public override void Unload()
        {
            _loops = false;
            base.Unload();
        }

        public override void Render(Scene scene)
        {
            base.BeginRender(null);

            _effect.View = _view;
            _effect.Projection = _projection;
            _effect.Alpha = _alpha;


            _vertexList = new VertexPositionColor[] {
                          new VertexPositionColor(new Vector3(0, 0, 0), _colorA),
                          new VertexPositionColor(new Vector3(Size.X, 0, 0), _colorB),
                          new VertexPositionColor(new Vector3(Size.X, Size.Y, 0), _colorC),
                          new VertexPositionColor(new Vector3(0, Size.Y, 0), _colorD) };

            _vertexBuffer.SetData(_vertexList);
            Core.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (var item in _effect.CurrentTechnique.Passes)
            {
                Core.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                Core.GraphicsDevice.Indices = _indexBuffer;
                
                item.Apply();
                Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4);
            }
        }

        private void LaunchColorFader()
        {
            if (!_loops) return;
            this.Tween("_colorA", GetRndColor(), Speed).SetCompletionHandler(x => LaunchColorFader()).Start();
            this.Tween("_colorB", GetRndColor(), Speed).Start();
            this.Tween("_colorC", GetRndColor(), Speed).Start();
            this.Tween("_colorD", GetRndColor(), Speed).Start();
        }

        private Color GetRndColor() => Colors[Random.Range(0, Colors.Length)];
    }
}
