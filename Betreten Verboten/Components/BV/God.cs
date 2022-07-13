using Betreten_Verboten.Components.Base.Characters;
using Betreten_Verboten.Components.BV.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.BV
{
    public static class God
    {
        public static void Sacrifice(Character character)
        {
            if (!(character.Owner is BVPlayer p && p.Sacrificable)) throw new Exception("Player has to be a BV player!");
            p.Sacrificable = false;
            character.Kick(null);
        }
    }
}
