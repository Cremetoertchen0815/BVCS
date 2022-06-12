using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Betreten_Verboten.Components.Base;

namespace Betreten_Verboten.Components.BV.Player
{
    public abstract class BVPlayer : Base.Player
    {
        public virtual int AngerCount { get; set; }
        public int AdditionalPoints { get; set; }
        public override int Points => AdditionalPoints;
    }
}
