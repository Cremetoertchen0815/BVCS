using Newtonsoft.Json;

namespace Nez
{
    public struct Telegram
    {
        public string Sender;
        public string Receiver;
        public string Head;
        public string Body;

        public Telegram(string Sender, string Receiver, string Head, string Body)
        {
            this.Sender = Sender;
            this.Receiver = Receiver;
            this.Head = Head;
            this.Body = Body;
        }

        public static Telegram Empty => new Telegram();

        public string Serialize(Telegram t) => JsonConvert.SerializeObject(t);
        public static Telegram? Deserialize(string s) => JsonConvert.DeserializeObject<Telegram?>(s);


        public static bool operator ==(Telegram a, Telegram b) => a.Sender == b.Sender && a.Receiver == b.Receiver && a.Head == b.Head && a.Body == b.Body;
        public static bool operator !=(Telegram a, Telegram b) => !(a == b);
        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}
