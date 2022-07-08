using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nez
{
#if DEBUG
    public class DeltaAnalyzer : RenderableComponent
    {
        //Component functions
        public override RectangleF Bounds => new RectangleF(0, 0, 1, 1);
        public override bool IsVisibleFromCamera(Camera camera) => true;
        public override void OnAddedToEntity()
        {
            Active = true;
            SetRenderLayer(-3);
            _s.Restart();
        }


        private Dictionary<string, Color> _colors = new Dictionary<string, Color>();

        public override void Render(Batcher batcher, Camera camera)
        {

            //Draw background
            int offsetX = 1920 - 500;
            int offsetY = 50;
            int offsetY2 = 360;
            batcher.DrawRect(new Rectangle(offsetX, 0, 600, 650), Color.Black * 0.9F);

            //Draw text
            batcher.DrawString(Graphics.Instance.BitmapFont, "DeltaAnalyzer", new Vector2(offsetX + 20, 0), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "Update", new Vector2(offsetX + 20, offsetY - 20), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "Draw", new Vector2(offsetX + 20, offsetY2 - 20), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "Update vs. Draw", new Vector2(offsetX + 20 + 250, offsetY2 - 20), Color.White);

            //Draw headings
            batcher.DrawString(Graphics.Instance.BitmapFont, "Entity Rank", new Vector2(offsetX + 20, offsetY), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "Name", new Vector2(offsetX + 20, offsetY + 20), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "abs.(MS)", new Vector2(offsetX + 120, offsetY + 20), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "rel.(%)", new Vector2(offsetX + 160, offsetY + 20), Color.White);

            batcher.DrawString(Graphics.Instance.BitmapFont, "Component Rank", new Vector2(offsetX + 20 + 250, offsetY), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "Name", new Vector2(offsetX + 20 + 250, offsetY + 20), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "abs.(MS)", new Vector2(offsetX + 120 + 250, offsetY + 20), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "rel.(%)", new Vector2(offsetX + 160 + 250, offsetY + 20), Color.White);


            batcher.DrawString(Graphics.Instance.BitmapFont, "Name", new Vector2(offsetX + 20, offsetY2), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "abs.(MS)", new Vector2(offsetX + 120, offsetY2), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "rel.(%)", new Vector2(offsetX + 160, offsetY2), Color.White);

            batcher.DrawString(Graphics.Instance.BitmapFont, "Name", new Vector2(offsetX + 20 + 250, offsetY2), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "abs.(MS)", new Vector2(offsetX + 120 + 250, offsetY2), Color.White);
            batcher.DrawString(Graphics.Instance.BitmapFont, "rel.(%)", new Vector2(offsetX + 160 + 250, offsetY2), Color.White);

            //Draw data
            for (int i = 0; i < Math.Min(EntitiesUpdatePercentage.Length, 25); i++)
            {
                var el = EntitiesUpdatePercentage[i];
                bool contains = _colors.ContainsKey(el.Item1);
                var chlor = contains ? _colors[el.Item1] : Random.NextColor();
                if (!contains) _colors.Add(el.Item1, chlor);

                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item1.ToString(), new Vector2(offsetX + 20, offsetY + (i + 3) * 10), chlor);
                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item2.ToString(), new Vector2(offsetX + 120, offsetY + (i + 3) * 10), chlor);
                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item3.ToString(), new Vector2(offsetX + 160, offsetY + (i + 3) * 10), chlor);
            }
            for (int i = 0; i < Math.Min(ComponentUpdatePercentage.Length, 25); i++)
            {
                var el = ComponentUpdatePercentage[i];
                bool contains = _colors.ContainsKey(el.Item1);
                var chlor = contains ? _colors[el.Item1] : Random.NextColor();
                if (!contains) _colors.Add(el.Item1, chlor);

                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item1.ToString(), new Vector2(offsetX + 20 + 250, offsetY + (i + 3) * 10), chlor);
                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item2.ToString(), new Vector2(offsetX + 120 + 250, offsetY + (i + 3) * 10), chlor);
                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item3.ToString(), new Vector2(offsetX + 160 + 250, offsetY + (i + 3) * 10), chlor);
            }

            for (int i = 0; i < Math.Min(ObjectsDrawPercentage.Length, 25); i++)
            {
                var el = ObjectsDrawPercentage[i];
                bool contains = _colors.ContainsKey(el.Item1);
                var chlor = contains ? _colors[el.Item1] : Random.NextColor();
                if (!contains) _colors.Add(el.Item1, chlor);

                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item1.ToString(), new Vector2(offsetX + 20, offsetY2 + (i + 2) * 10), chlor);
                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item2.ToString(), new Vector2(offsetX + 120, offsetY2 + (i + 2) * 10), chlor);
                batcher.DrawString(Graphics.Instance.BitmapFont, el.Item3.ToString(), new Vector2(offsetX + 160, offsetY2 + (i + 2) * 10), chlor);
            }

            float add = _oldUpdateTotalTime + _oldDrawTotalTime;

            batcher.DrawString(Graphics.Instance.BitmapFont, "Update", new Vector2(offsetX + 20 + 250, offsetY2 + 20), Color.Orange);
            batcher.DrawString(Graphics.Instance.BitmapFont, Math.Round(_oldUpdateTotalTime, 3).ToString(), new Vector2(offsetX + 120 + 250, offsetY2 + 20), Color.Orange);
            batcher.DrawString(Graphics.Instance.BitmapFont, $"{Math.Round(_oldUpdateTotalTime / add * 100, 2)}%", new Vector2(offsetX + 160 + 250, offsetY2 + 20), Color.Orange);

            batcher.DrawString(Graphics.Instance.BitmapFont, "Draw", new Vector2(offsetX + 20 + 250, offsetY2 + 30), Color.Lime);
            batcher.DrawString(Graphics.Instance.BitmapFont, Math.Round(_oldDrawTotalTime, 3).ToString(), new Vector2(offsetX + 120 + 250, offsetY2 + 30), Color.Lime);
            batcher.DrawString(Graphics.Instance.BitmapFont, $"{Math.Round(_oldDrawTotalTime / add * 100, 2)}%", new Vector2(offsetX + 160 + 250, offsetY2 + 30), Color.Lime);
        }

        //Static functions
        private static Stopwatch _s = new Stopwatch();
        private static float _oldUpdateTotalTime;
        private static float _oldDrawTotalTime;
        private static DeltaDict _oldEntitiesUpdateList = new DeltaDict();
        private static DeltaDict _oldCompnentUpdateList = new DeltaDict();
        private static DeltaDict _oldObjctDrawDeltaList = new DeltaDict();


        public static (string, float, float)[] EntitiesUpdatePercentage = new (string, float, float)[] { };
        public static (string, float, float)[] ComponentUpdatePercentage = new (string, float, float)[] { };
        public static (string, float, float)[] ObjectsDrawPercentage = new (string, float, float)[] { };
        public static bool Active { get; set; } = false;

        public static void RestartMeasure()
        {
            //Inactive shit
            if (!Active)
            {
                foreach (var item in UpdateDeltas) item.Value.Clear();
                DrawDeltas.Clear();
                return;
            }

            //Grab new lists
            double UpdateTotalTime = 0d;
            double DrawTotalTime = 0d;
            var entitiesUpdateList = Pool<DeltaDict>.Obtain();
            var compnentUpdateList = Pool<DeltaDict>.Obtain();

            //Fill lists with new data
            foreach (var item in UpdateDeltas)
            {
                if (!entitiesUpdateList.ContainsKey(item.Key)) entitiesUpdateList.Add(item.Key, 0);
                foreach (var element in item.Value)
                {
                    entitiesUpdateList[item.Key] += element.Item2;
                    UpdateTotalTime += element.Item2;

                    if (!compnentUpdateList.ContainsKey(element.Item1)) compnentUpdateList.Add(element.Item1, 0);
                    compnentUpdateList[element.Item1] += element.Item2;
                }
                item.Value.Clear();
            }
            foreach (var item in DrawDeltas) DrawTotalTime += item.Value;

            //Lerp the new results with the old ones to smooth out the calculation
            string[] entitiesUpdateNames = entitiesUpdateList.Select((x) => x.Key).ToArray();
            string[] compnentUpdateNames = compnentUpdateList.Select((x) => x.Key).ToArray();
            string[] objectsDrawingNames = DrawDeltas.Select((x) => x.Key).ToArray();
            foreach (string element in entitiesUpdateNames) if (_oldEntitiesUpdateList.ContainsKey(element)) entitiesUpdateList[element] = MathHelper.LerpPrecise((float)entitiesUpdateList[element], (float)_oldEntitiesUpdateList[element], 0.92F);
            foreach (string element in compnentUpdateNames) if (_oldCompnentUpdateList.ContainsKey(element)) compnentUpdateList[element] = MathHelper.LerpPrecise((float)compnentUpdateList[element], (float)_oldCompnentUpdateList[element], 0.92F);
            foreach (string element in objectsDrawingNames) if (_oldObjctDrawDeltaList.ContainsKey(element)) DrawDeltas[element] = MathHelper.LerpPrecise((float)DrawDeltas[element], (float)_oldObjctDrawDeltaList[element], 0.92F);
            UpdateTotalTime = MathHelper.Lerp((float)UpdateTotalTime, _oldUpdateTotalTime, 0.92F);
            DrawTotalTime = MathHelper.Lerp((float)DrawTotalTime, _oldDrawTotalTime, 0.92F);


            EntitiesUpdatePercentage = entitiesUpdateList.Select((x) => (x.Key, (float)Math.Round(x.Value * 1000, 3), (float)Math.Round(x.Value / UpdateTotalTime * 100, 2))).OrderBy((x) => -x.Item3).ToArray();
            ComponentUpdatePercentage = compnentUpdateList.Select((x) => (x.Key, (float)Math.Round(x.Value * 1000, 3), (float)Math.Round(x.Value / UpdateTotalTime * 100, 2))).OrderBy((x) => -x.Item3).ToArray();
            ObjectsDrawPercentage = DrawDeltas.Select((x) => (x.Key, (float)Math.Round(x.Value * 1000, 2), (float)Math.Round(x.Value / DrawTotalTime * 100, 2))).OrderBy((x) => -x.Item3).ToArray();

            //Recycle and store old update lists
            Pool<DeltaDict>.Free(_oldEntitiesUpdateList);
            Pool<DeltaDict>.Free(_oldCompnentUpdateList);
            _oldEntitiesUpdateList = entitiesUpdateList;
            _oldCompnentUpdateList = compnentUpdateList;
            //Recycle and store old draw lists
            Pool<DeltaDict>.Free(_oldObjctDrawDeltaList);
            _oldObjctDrawDeltaList = DrawDeltas;
            DrawDeltas = Pool<DeltaDict>.Obtain();
            //Store old total delta values
            _oldUpdateTotalTime = (float)UpdateTotalTime;
            _oldDrawTotalTime = (float)DrawTotalTime;
        }
        public static DeltaSegment MeasureSegment(string entity, string component, DeltaSegmentType type) => DeltaSegment.Start(entity, component, type, _s);

        private static Dictionary<string, List<(string, double)>> UpdateDeltas = new Dictionary<string, List<(string, double)>>();
        private static DeltaDict DrawDeltas = new DeltaDict();

        //Sub Classes
        private class DeltaDict : Dictionary<string, double>, IPoolable
        {
            public void Reset() => Clear();
        }


        public enum DeltaSegmentState { Idle, HasStarted, HasEnded }
        public enum DeltaSegmentType { Draw, Update }

        public class DeltaSegment : IPoolable
        {
            public string Entity;
            public string Component;
            public DeltaSegmentType Type;
            public DeltaSegmentState Status;
            public TimeSpan SpanStart;
            public TimeSpan SpanEnd;
            public double Delta;
            private Stopwatch _s;
            public void Reset()
            {
                Entity = string.Empty;
                Component = string.Empty;
                Status = DeltaSegmentState.Idle;
                Delta = 0F;
                _s = null;
            }

            public static DeltaSegment Start(string entity, string component, DeltaSegmentType type, Stopwatch s)
            {
                var sg = Pool<DeltaSegment>.Obtain();
                sg._s = s;
                sg.Entity = entity;
                sg.Component = component;
                sg.Type = type;
                sg.SpanStart = s.Elapsed;
                sg.Status = DeltaSegmentState.HasStarted;
                return sg;
            }

            public void Stop()
            {
                Status = DeltaSegmentState.HasEnded;
                SpanEnd = _s.Elapsed;
                Delta = (float)(SpanEnd - SpanStart).TotalSeconds;

                if (Type == DeltaSegmentType.Update)
                {
                    var ComponLst = DeltaAnalyzer.UpdateDeltas.ContainsKey(Entity) ? DeltaAnalyzer.UpdateDeltas[Entity] : null;
                    if (ComponLst == null) { ComponLst = new List<(string, double)>(); DeltaAnalyzer.UpdateDeltas.Add(Entity, ComponLst); }
                    ComponLst.Add((Component, Delta));
                }
                else
                {
                    if (DrawDeltas.ContainsKey(Entity)) DrawDeltas[Entity] += Delta; else DrawDeltas.Add(Entity, Delta);
                }

                Pool<DeltaSegment>.Free(this);
            }
        }
    }

#endif
}
