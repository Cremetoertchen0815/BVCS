using Nez;

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
