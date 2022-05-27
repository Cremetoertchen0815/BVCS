using System.Globalization;


namespace Nez.Svg
{
    public class SvgShear : SvgTransform
    {
        private float _shearX;
        private float _shearY;


        public SvgShear(float shearX, float shearY)
        {
            _shearX = shearX;
            _shearY = shearY;
            Debug.Warn("SvgSkew shear is not implemented");
        }


        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "shear({0}, {1})", _shearX, _shearY);
    }
}