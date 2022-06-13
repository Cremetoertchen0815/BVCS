using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.BV.Player
{
	public class LocalPlayer : BVPlayer
	{
		public LocalPlayer(int Nr) : base(Nr)
		{

		}

		public override void MessageReceived(Telegram message) { return; }
	}
}
