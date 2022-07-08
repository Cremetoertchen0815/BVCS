using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Nez.Console
{
    internal class FramerateGraph : Renderable3D, IUpdatable
    {
        public enum GraphType
        {
            Line,
            Fill
        }

        public List<(float, Color)> values = new List<(float, Color)>();

        /// <summary>
        /// Determines whether the drawn graph will be line only, or filled
        /// </summary>
        public GraphType Type { get; set; }

        public override float Width => Size.X;
        public override float Height => Size.Y;

        public Vector2 Size = new Vector2(1, 1);
        private Vector2 _pos;

        /// <summary>
        /// Determines the vertical scaling of the graph.
        /// The value that is equal to MaxValue will be displayed at the top of the graph (at point Size.Y)
        /// </summary>
        public float MaxValue { get; set; }

        private BasicEffect _effect;
        private short[] lineListIndices;
        private short[] triangleStripIndices;

        public FramerateGraph(Vector2 size, Vector2 position)
        {
            Size = size;
            MaxValue = 1;
            Type = FramerateGraph.GraphType.Line;
            _pos = position;
        }

        public override void OnAddedToEntity()
        {
            _effect = new BasicEffect(Core.GraphicsDevice)
            {
                View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreateOrthographicOffCenter(0, Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height, 0, 1.0f, 1000.0f),
                World = Matrix.Identity,

                VertexColorEnabled = true
            };
            Rotation = new Vector3(MathHelper.Pi, 0, 0);
            Position = new Vector3(_pos.X, -_pos.Y, 0);
        }

        /// <summary>
        /// Draws the values in given order, with specific color for each value
        /// </summary>
        /// <param name="values">Value/color pairs to draw, in order from left to right</param>
        public override void Render(Batcher batcher, Camera camera)
        {
            if (values.Count < 2)
                return;



            //creates scaling (for the transformation) based on the number of points to draw
            Scale = new Vector3(Width / values.Count, Height / MaxValue, 1);
            //_effect.World = Matrix.CreateRotationX(MathHelper.Pi) * WorldMatrix; //flips the graph so that the higher values are above. Makes bottom left the graph origin.
            //_effect.View = camera.ViewMatrix3D;
            //_effect.Projection = camera.ProjectionMatrix;

            _effect.World = WorldMatrix;

            //different point lists for different types of graphs
            if (Type == GraphType.Line)
            {
                var pointList = new VertexPositionColor[values.Count];
                for (int i = 0; i < values.Count; i++)
                {
                    pointList[i] = new VertexPositionColor(new Vector3(i, values[i].Item1 < MaxValue ? values[i].Item1 : MaxValue, 0), values[i].Item2);
                }

                DrawLineList(pointList);
            }
            else if (Type == GraphType.Fill)
            {
                var pointList = new VertexPositionColor[values.Count * 2];
                for (int i = 0; i < values.Count; i++)
                {
                    //The vertices are created so that the triangles are inverted (back facing). When rotated they will become front facing.
                    //This is done to avoid changing rasterizer state to CullMode.CullClockwiseFace.
                    pointList[i * 2 + 1] = new VertexPositionColor(new Vector3(i, values[i].Item1 < MaxValue ? values[i].Item1 : MaxValue, 0), values[i].Item2);
                    pointList[i * 2] = new VertexPositionColor(new Vector3(i, 0, 0), values[i].Item2);
                }

                DrawTriangleStrip(pointList);
            }
        }

        private void DrawLineList(VertexPositionColor[] pointList)
        {
            //indices updated only need to be updated when the number of points has changed
            if (lineListIndices == null || lineListIndices.Length != ((pointList.Length * 2) - 2))
            {
                lineListIndices = new short[(pointList.Length * 2) - 2];
                for (int i = 0; i < pointList.Length - 1; i++)
                {
                    lineListIndices[i * 2] = (short)(i);
                    lineListIndices[(i * 2) + 1] = (short)(i + 1);
                }
            }

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList,
                    pointList,
                    0,
                    pointList.Length,
                    lineListIndices,
                    0,
                    pointList.Length - 1
                );
            }
        }

        private void DrawTriangleStrip(VertexPositionColor[] pointList)
        {
            if (triangleStripIndices == null || triangleStripIndices.Length != pointList.Length)
            {
                triangleStripIndices = new short[pointList.Length];
                for (int i = 0; i < pointList.Length; i++)
                    triangleStripIndices[i] = (short)i;
            }

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleStrip,
                    pointList,
                    0,
                    pointList.Length,
                    triangleStripIndices,
                    0,
                    pointList.Length - 2
                );
            }
        }

        public override bool IsVisibleFromCamera(Camera camera) => true;

        private const int SampleCount = 600;
        private const int bestFrameRate = 120;
        private float fr;

        public void Update()
        {
            float newFr = (float)Math.Round(1 / Time.UnscaledDeltaTime);
            float lerpFactor = MathHelper.Clamp(Math.Abs(newFr - fr) / 1000, 0.01F, 0.5F);
            fr = MathHelper.Lerp(fr, newFr, lerpFactor);
            values.Add((fr, Color.Lerp(Color.Red, Color.Lime, fr / bestFrameRate)));
            if (values.Count > SampleCount) values.RemoveRange(0, values.Count - SampleCount); //Remove access
            MaxValue = Math.Max(MaxValue, fr);
        }

        public static bool isGraphInScene
        {
            get => Core.Scene.FindEntity("frmGraph") != null;
            set
            {
                if (value && Core.Scene.FindEntity("frmGraph") == null)
                {
                    Core.Scene.CreateEntity("frmGraph").AddComponent(new FramerateGraph(new Vector2(200, 100), new Vector2(100, 200)) { Type = GraphType.Fill }).SetRenderLayer(-10);
                }
                else if (!value)
                {
                    Core.Scene.FindEntity("frmGraph")?.Destroy();

                }
            }
        }
    }
}