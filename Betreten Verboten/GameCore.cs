using Microsoft.Xna.Framework;
using Nez;

namespace Betreten_Verboten
{
    class GameCore : Core
	{
        protected override void Initialize()
        {
            base.Initialize();
            Scene = new FunniTestClass();

        }
    }

    public class FunniTestClass : Scene
    {

    }
}
