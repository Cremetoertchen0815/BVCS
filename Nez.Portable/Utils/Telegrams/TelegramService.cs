using Nez.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nez
{
    internal static class TelegramService
    {
        private static Dictionary<string, List<ITelegramReceiver>> receivers = new Dictionary<string, List<ITelegramReceiver>>();
        internal static bool LogToConsole = false;
        private static string internalTelegramReceiverName = "teleman";

        public static void SendPublic(Telegram message)
        {
            foreach (var a in receivers) foreach (var b in a.Value) b.MessageReceived(message);
            if (receivers.Count > 0) DebugConsole.Instance.Log("Telegram transmitted: " + message.Sender + " -> all; " + message.Head + "|" + message.Body);
        }

        public static void Register(ITelegramReceiver reg, params string[] IDs)
        {
            foreach (string ID in IDs)
            {

                if (receivers.ContainsKey(ID))
                    receivers[ID].Add(reg);
                else
                    receivers.Add(ID, new List<ITelegramReceiver>() { reg });
            }
        }
        public static void Deregister(ITelegramReceiver reg)
        {
            var IDs = new List<string>();

            foreach (var item in receivers)
                if (item.Value.Contains(reg)) IDs.Add(item.Key);

            foreach (var ID in IDs)
                if (receivers[ID].Contains(reg)) receivers[ID].Remove(reg);
        }

        public static void DeregisterAll() => receivers.Clear();

        public static bool SendPrivate(Telegram message)
        {
            if (message == Telegram.Empty) return false; //Ignore empty message

            if (receivers.ContainsKey(message.Receiver))
            {
                foreach (var cp in receivers[message.Receiver]) cp.MessageReceived(message);
                DebugConsole.Instance.Log("Telegram transmitted: " + message.Sender + " -> " + message.Receiver + "; " + message.Head + message.Body != string.Empty ? " | " + message.Body : string.Empty);
                return true;
            }
            else if (message.Receiver == internalTelegramReceiverName)
            {
                switch (message.Head)
                {
                    case "execute_double":
                        DebugConsole.Instance.Log("Telegram transmitted: " + message.Sender + " -> " + message.Receiver + "; " + message.Head + message.Body != string.Empty ? " | " + message.Body : string.Empty);
                        var resultingtwo = Newtonsoft.Json.JsonConvert.DeserializeObject<(Telegram, Telegram)>((string)message.Body);
                        SendPrivate(resultingtwo.Item1);
                        SendPrivate(resultingtwo.Item2);
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        [Command("telegram", "Allows control of the telegram service.")]
        private static void ManageScene(string command = "", string param = "")
        {

            var builder = new StringBuilder();

            switch (command)
            {

                case "list":

                    builder.AppendLine("#id: name");
                    builder.AppendLine();
                    builder.AppendLine("#teleman: TelegramService");
                    foreach (var el in receivers)
                    {
                        if (el.Value.Count == 1)
                        {
                            builder.AppendLine("#" + el.Key + ": " + el.Value[0].ToString());
                        }
                        else if (el.Value.Count > 1)
                        {
                            builder.AppendLine("#" + el.Key + ":");
                            foreach (var l in el.Value) builder.AppendLine(l.ToString());
                        }
                    }
                    break;

                case "log":
                    bool r;
                    if (bool.TryParse(param, out r)) LogToConsole = r; else LogToConsole = true;
                    break;
                case "post":
                    try
                    {
                        string[] data = param.Split('|');
                        var tele = new Telegram
                        {
                            Sender = "debug",
                            Head = data[0],
                            Body = data.Length > 1 ? data[1] : string.Empty
                        };
                        SendPublic(tele);
                    }
                    catch (Exception)
                    {
                        builder.AppendLine("Invalid telegram!");
                    }
                    break;

                case "send":
                    try
                    {
                        string[] data = param.Split('|');
                        var tele = new Telegram
                        {
                            Sender = "debug",
                            Receiver = data[0],
                            Head = data[1],
                            Body = data.Length > 2 ? data[2] : string.Empty
                        };
                        if (!SendPrivate(tele)) builder.AppendLine("Invalid receiver!");
                    }
                    catch (Exception)
                    {
                        builder.AppendLine("Invalid telegram!");
                    }
                    break;
                default:
                    builder.AppendLine("Allows control of the telegram service.");
                    builder.AppendLine("Usage: telegram [command] {parameter}");
                    builder.AppendLine();
                    builder.AppendLine("---Commands---");
                    builder.AppendLine("telegram list");
                    builder.AppendLine("Lists all registered Telegram receivers.");
                    builder.AppendLine();
                    builder.AppendLine("telegram log {true/false}");
                    builder.AppendLine("Enables/Disables logging telegram traffic in the debug console.");
                    builder.AppendLine();
                    builder.AppendLine("telegram post \"head|body\"");
                    builder.AppendLine("Sends a public telegram to all receivers registered.");
                    builder.AppendLine();
                    builder.AppendLine("telegram send \"receiver|head|body\"");
                    builder.AppendLine("Sends a private telegram to a specific group ID.");
                    break;
            }


            DebugConsole.Instance.Log(builder.ToString());

        }
    }
}
