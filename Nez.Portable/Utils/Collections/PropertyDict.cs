using System;
using System.Collections.Generic;

namespace Nez
{
    public class PropertyDict : Dictionary<string, string>
    {
        public new string this[string index]
        {
            get => ContainsKey(index) ? base[index] : null;
            set { if (ContainsKey(index)) base[index] = value; else Add(index, value); }
        }

        public T FetchType<T>(string index, Func<string, T> conversionFunc, T errVal = default) => ContainsKey(index) ? conversionFunc(base[index]) : errVal;
        public int FetchInteger(string index, int errVal = default) => ContainsKey(index) && int.TryParse(base[index], out int i) ? i : errVal;
        public bool FetchBoolean(string index, bool errVal = default) => ContainsKey(index) && bool.TryParse(base[index], out bool b) ? b : errVal;
        public float FetchFloat(string index, float errVal = default) => ContainsKey(index) && float.TryParse(base[index], out float f) ? f : errVal;
        public Telegram FetchTelegram(string index, Telegram errVal = default) => ContainsKey(index) ? (Telegram.Deserialize(base[index]) ?? errVal) : errVal;


        public int? FetchIntegerNullable(string index) => ContainsKey(index) && int.TryParse(base[index], out int i) ? (int?)i : null;
        public bool? FetchBooleanNullable(string index) => ContainsKey(index) && bool.TryParse(base[index], out bool b) ? (bool?)b : null;
        public float? FetchFloatNullable(string index) => ContainsKey(index) && float.TryParse(base[index], out float f) ? (float?)f : null;
        public Telegram? FetchTelegramNullable(string index) => ContainsKey(index) ? Telegram.Deserialize(base[index]) : null;
    }
}
