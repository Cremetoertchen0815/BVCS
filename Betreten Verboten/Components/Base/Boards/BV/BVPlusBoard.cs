using Microsoft.Xna.Framework;

namespace Betreten_Verboten.Components.Base.Boards.BV
{
    public class BVPlusBoard : BVBoard
    {

        //Enthält Transform-Matritzen, welche die SPielfeld-Hitboxen um den Spielfeld-Mittelpunkt rotieren.
        private readonly Matrix[] transmatrices0 = { Matrix.CreateRotationZ(MathHelper.PiOver2 * 3), Matrix.Identity, Matrix.CreateRotationZ(MathHelper.PiOver2), Matrix.CreateRotationZ(MathHelper.Pi) };
        //Enthält Transform-Matritzen, welche die SPielfeld-Hitboxen um den Spielfeld-Mittelpunkt rotieren.
        private readonly float[] rotato0 = { 0, MathHelper.PiOver2, MathHelper.Pi, MathHelper.PiOver2 * 3 };



        protected override int FieldHouseDiameter => 20;
        protected override int FieldHomeDiameter => 20;
        protected override int FieldPlayerDiameter => 28;
        protected override int FieldDistance => 85;
        public override int FieldCountPP => 10;
        public override int FigureCountPP => 4;
        public override int PlayerCount => 4;
        public override float CharScale => 0.105f;
        public override float FigureJumpHeight => 6f;

        public override Vector2 GetFieldPosition(int player, int fieldNr, FieldType fieldType, bool centerOffset = true)
        {
            //Get unrotated position
            Vector2 positionUnrotated;
            switch (fieldType)
            {
                default:
                    positionUnrotated = GetRegularFieldUnrotated(fieldNr);
                    break;
                case FieldType.Home:
                    positionUnrotated = GetHomeFieldUnrotated(fieldNr);
                    break;
                case FieldType.House:
                    positionUnrotated = GetHouseFieldUnrotated(fieldNr);
                    break;
            }

            return Vector2.Transform(positionUnrotated, transmatrices0[player]) + (centerOffset ? _centerOffset : Vector2.Zero);
        }

        private Vector2 GetHouseFieldUnrotated(int fieldNr)
        {
            switch (fieldNr)
            {

                case 0: return new Vector2(-FieldDistance * 4, 0);
                case 1: return new Vector2(-FieldDistance * 3, 0);
                case 2: return new Vector2(-FieldDistance * 2, 0);
                case 3: return new Vector2(-FieldDistance, 0);
                default: return Vector2.Zero;
            }
        }
        private Vector2 GetHomeFieldUnrotated(int fieldNr)
        {
            switch (fieldNr)
            {
                case 0: return new Vector2(-420, -420);
                case 1: return new Vector2(-350, -420);
                case 2: return new Vector2(-420, -350);
                case 3: return new Vector2(-350, -350);
                default: return Vector2.Zero;
            }
        }
        private Vector2 GetRegularFieldUnrotated(int fieldNr)
        {
            switch (fieldNr)
            {
                case 0: return new Vector2(-FieldDistance * 5, -FieldDistance);
                case 1: return new Vector2(-FieldDistance * 4, -FieldDistance);
                case 2: return new Vector2(-FieldDistance * 3, -FieldDistance);
                case 3: return new Vector2(-FieldDistance * 2, -FieldDistance);
                case 4: return new Vector2(-FieldDistance * 1, -FieldDistance);
                case 5: return new Vector2(-FieldDistance, -FieldDistance * 2);
                case 6: return new Vector2(-FieldDistance, -FieldDistance * 3);
                case 7: return new Vector2(-FieldDistance, -FieldDistance * 4);
                case 8: return new Vector2(-FieldDistance, -FieldDistance * 5);
                case 9: return new Vector2(0, -FieldDistance * 5);
                default: return Vector2.Zero;
            }
        }
    }
}
