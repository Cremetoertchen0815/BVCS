using Betreten_Verboten.Components.Base.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components
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
            var fcount = c.Owner.Board.FieldCount;
            var plcount = c.Owner.Board.PlayerCount;
            ret.Valid = c.Position >= 0 && c.Position <= fcount;
            ret.Position = ret.Valid ? (c.Position + fcount * c.Owner.Nr) % fcount * plcount : -1;
            return ret;
        }

    }
}
