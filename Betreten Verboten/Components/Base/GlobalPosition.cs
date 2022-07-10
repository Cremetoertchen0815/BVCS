using Betreten_Verboten.Components.Base.Characters;

namespace Betreten_Verboten.Components.Base
{
    public struct GlobalPosition
    {
        public int Position;
        public bool Valid;

        public static bool operator ==(GlobalPosition a, GlobalPosition b) => a.Position == b.Position;
        public static bool operator !=(GlobalPosition a, GlobalPosition b) => !(a == b);
        public override bool Equals(object o) => o is GlobalPosition b && this == b;
        public override int GetHashCode() => Position.GetHashCode();

        public static GlobalPosition FromChar(Character c)
        {
            var ret = new GlobalPosition();
            var board = c.Owner.Board;
            ret.Valid = c.Position >= 0 && c.Position <= board.FieldCountPP;
            ret.Position = ret.Valid ? (c.Position + board.FieldCountPP * c.Owner.Nr) % c.Owner.Board.FieldCountTotal : -1;
            return ret;
        }

    }
}
