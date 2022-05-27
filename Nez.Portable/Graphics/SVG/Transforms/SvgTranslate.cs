using System.Globalization;


namespace Nez.Svg
{
    public class SvgTranslate : SvgTransform
    {
        private float _x;
        private float _y;


        public SvgTranslate(float x, float y = 0)
        {
            _x = x;
            _y = y;

            Matrix = Matrix2D.CreateTranslation(_x, _y);
        }


        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "translate({0}, {1})", _x, _y);
    }
}