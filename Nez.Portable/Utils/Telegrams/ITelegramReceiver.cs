namespace Nez
{

    public interface ITelegramReceiver
    {
        void MessageReceived(Telegram message);
    }
}
