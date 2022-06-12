namespace Nez
{

    public interface ITelegramReceiver
    {
        string TelegramSender { get; }
        void MessageReceived(Telegram message);
    }
}
