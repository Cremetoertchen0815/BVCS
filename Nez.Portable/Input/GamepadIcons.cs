using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nez
{
    public class GamepadIcons
    {
        public static GamepadIcons Instance { get; private set; } = new GamepadIcons();
        private Dictionary<GamepadButton, Texture2D> icons = new Dictionary<GamepadButton, Texture2D>();
        private GamepadIcons() { }

        public Texture2D GetIcon(GamepadButton g) => icons[g];
        public void LoadXboxIcons()
        {
            icons.Clear();
            icons.Add(GamepadButton.A, Core.Content.LoadTexture("engine/tex/gamepad/xbox/a"));
            icons.Add(GamepadButton.B, Core.Content.LoadTexture("engine/tex/gamepad/xbox/b"));
            icons.Add(GamepadButton.X, Core.Content.LoadTexture("engine/tex/gamepad/xbox/x"));
            icons.Add(GamepadButton.Y, Core.Content.LoadTexture("engine/tex/gamepad/xbox/y"));
            icons.Add(GamepadButton.RB, Core.Content.LoadTexture("engine/tex/gamepad/xbox/bumper_right"));
            icons.Add(GamepadButton.LB, Core.Content.LoadTexture("engine/tex/gamepad/xbox/bumper_right"));
            icons.Add(GamepadButton.LT, Core.Content.LoadTexture("engine/tex/gamepad/xbox/trigger_left"));
            icons.Add(GamepadButton.RT, Core.Content.LoadTexture("engine/tex/gamepad/xbox/trigger_right"));
        }

        public enum GamepadButton
        {
            A,
            B,
            X,
            Y,
            LS,
            RS,
            RB,
            LB,
            RT,
            LT,

        }
    }
}
