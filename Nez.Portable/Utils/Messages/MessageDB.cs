using System.Collections.Generic;

namespace Nez
{
    public class MessageDB
    {
        internal List<Message>[] _setList;
        internal List<string> _setNames;

        public List<Message> GetSetByName(string name) => _setNames.Contains(name) ? _setList[_setNames.IndexOf(name)] : null;
    }
}
