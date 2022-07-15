using Betreten_Verboten.Components.Base.Characters;

namespace Betreten_Verboten.Components.Base
{
    public struct GlobalPosition
    {
        public int Position;
        public bool Valid;

        public static bool operator ==(GlobalPosition a, GlobalPosition b) => a.Position == b.Position && a.Valid && b.Valid;
        public static bool operator !=(GlobalPosition a, GlobalPosition b) => !(a == b);
        public override bool Equals(object o) => o is GlobalPosition b && this == b;
        public override int GetHashCode() => Position.GetHashCode();

        public static GlobalPosition FromChar(Character c) => FromLocalPosition(c.Owner, c.Position);

        public static GlobalPosition FromLocalPosition(Player p, int position)
        {
            var ret = new GlobalPosition();
            var board = p.Board;
            ret.Valid = position >= 0 && position <= board.FieldCountTotal;
            ret.Position = ret.Valid ? (position + board.FieldCountPP * p.Nr) % p.Board.FieldCountTotal : -1;
            return ret;
        }

    }
}
