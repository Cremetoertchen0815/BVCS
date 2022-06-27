using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez
{
    public static class TelegramExtension
    {
        public static void SendPrivateObj(this object Body, string sender, string Receiver, string Head) => TelegramService.SendPrivate(new Telegram(sender, Receiver, Head, Body));
        public static void SendPrivateTele(this ITelegramReceiver sender, string Receiver, string Head, object Body) => TelegramService.SendPrivate(new Telegram(sender.TelegramSender, Receiver, Head, Body));
        public static void SendPublicObj(this object Body, string sender, string Head) => TelegramService.SendPublic(new Telegram(sender, null, Head, Body));
        public static void SendPublicTele(this ITelegramReceiver sender, string Head, object Body) => TelegramService.SendPublic(new Telegram(sender.TelegramSender, null, Head, Body));
        public static void TeleRegister(this ITelegramReceiver reg, params string[] AdditionalIDs) => TelegramService.Register(reg, AdditionalIDs.Concat(new string[] { reg.TelegramSender }).ToArray());
        public static void TeleDeregister(this ITelegramReceiver reg) => TelegramService.Deregister(reg);
    }
}
