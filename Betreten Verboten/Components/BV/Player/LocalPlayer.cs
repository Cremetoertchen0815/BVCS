using Nez;

namespace Betreten_Verboten.Components.BV.Player
{
	public class LocalPlayer : BVPlayer
	{
		public LocalPlayer(int Nr) : base(Nr)
		{
			//Register UI fields
		}

		public void RegisterUI()
		{

		}

		public override void MessageReceived(Telegram message)
		{
			switch (message.Head)
			{
				case "player_active":
					break;
				default:
					break;
			}
		}
	}
}
