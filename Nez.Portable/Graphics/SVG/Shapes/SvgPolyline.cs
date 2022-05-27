using Microsoft.Xna.Framework;
using System.Xml.Serialization;


namespace Nez.Svg
{
    public class SvgPolyline : SvgElement
    {
        [XmlAttribute("points")]
        public string PointsAttribute
        {
            get => null;
            set => ParsePoints(value);
        }

        public Vector2[] Points;

        private void ParsePoints(string str)
        {
            // normalize commas and spaces since some programs use comma separate points and others use spaces
            str = str.Replace(',', ' ');
            string[] pairs = str.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            Points = new Vector2[pairs.Length / 2];

            int pointIndex = 0;
            for (int i = 0; i < pairs.Length; i += 2)
                Points[pointIndex++] = new Vector2(float.Parse(pairs[i]), float.Parse(pairs[i + 1]));
        }


        public Vector2[] GetTransformedPoints()
        {
            var pts = new Vector2[Points.Length];
            var mat = GetCombinedMatrix();
            Vector2Ext.Transform(Points, ref mat, pts);

            return pts;
        }
    }
}