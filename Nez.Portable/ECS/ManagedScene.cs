using System;

namespace Nez
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ManagedScene : Attribute
    {
        public int SceneNumber;
        public bool AcceptsArgument;

        public ManagedScene(int sceneNr, bool acceptsArgs = true)
        {
            SceneNumber = sceneNr;
            AcceptsArgument = acceptsArgs;
        }
    }
}
