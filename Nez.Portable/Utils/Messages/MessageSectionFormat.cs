using Microsoft.Xna.Framework;

namespace Nez
{
    public struct MessageSectionFormat
    {
        public bool Bold;
        public float Speed;
        public Color Color;

        public MessageSectionFormat(bool bold, float speed, Color color)
        {
            Bold = bold;
            Speed = speed;
            Color = color;
        }
    }
}
