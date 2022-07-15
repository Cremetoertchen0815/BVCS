using Nez;

namespace Betreten_Verboten.Components.BV.Player
{
    /// <summary>
    /// Represents a BV player that plays actively on the current machine.
    /// </summary>
    public class LocalPlayer : BVPlayer
    {
        //ctor
        public LocalPlayer(int Nr) : base(Nr) { }

        //Currently, no special telegram handling necessary
        public override void MessageReceived(Telegram message) { }
    }
}
