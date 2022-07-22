#region File Description
//-----------------------------------------------------------------------------
// Generate message boxes and other prompts.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;


namespace Nez.GeonBit.UI.Utils
{
    /// <summary>
    /// GeonBit.UI.Utils contain different utilities and helper classes to use GeonBit.UI.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    internal class NamespaceDoc
    {
    }

    /// <summary>
    /// Helper class to generate message boxes and prompts.
    /// </summary>
    public static class MessageBox
    {
        /// <summary>
        /// Default size to use for message boxes.
        /// </summary>
        public static Vector2 DefaultMsgBoxSize = new Vector2(480, 350);

        /// <summary>
        /// Default text for OK button.
        /// </summary>
        public static string DefaultOkButtonText = "OK";

        /// <summary>
        /// Will block and fade background with this color while messages are opened.
        /// </summary>
        public static Color BackgroundFaderColor = new Color(0, 0, 0, 100);

        /// <summary>
        /// Count currently opened message boxes.
        /// </summary>
        public static int OpenedMsgBoxesCount
        {
            get; private set;
        } = 0;

        /// <summary>
        /// Get if there's a message box currently opened.
        /// </summary>
        public static bool IsMsgBoxOpened => OpenedMsgBoxesCount > 0;

        /// <summary>
        /// A button / option for a message box.
        /// </summary>
        public class MsgBoxOption
        {
            /// <summary>
            /// Option title (for the button).
            /// </summary>
            public string Title;

            /// <summary>
            /// Callback to run when clicked. Return false to leave message box opened (true will close it).
            /// </summary>
            public System.Func<bool> Callback;

            /// <summary>
            /// Create the message box option.
            /// </summary>
            /// <param name="title">Text to write on the button.</param>
            /// <param name="callback">Action when clicked. Return false if you want to abort and leave the message opened, return true to close it.</param>
            public MsgBoxOption(string title, System.Func<bool> callback)
            {
                Title = title;
                Callback = callback;
            }
        }

        /// <summary>
        /// A button / option for a message box.
        /// </summary>
        public class InputBoxOption
        {
            /// <summary>
            /// Option title (for the button).
            /// </summary>
            public string Title;

            /// <summary>
            /// Callback to run when clicked. Return false to leave message box opened (true will close it).
            /// </summary>
            public System.Func<string, bool> Callback;

            /// <summary>
            /// Create the message box option.
            /// </summary>
            /// <param name="title">Text to write on the button.</param>
            /// <param name="callback">Action when clicked. Return false if you want to abort and leave the message opened, return true to close it.</param>
            public InputBoxOption(string title, System.Func<string, bool> callback)
            {
                Title = title;
                Callback = callback;
            }
        }

        /// <summary>
        /// Show a message box with custom buttons and callbacks.
        /// </summary>
        /// <param name="header">Messagebox header.</param>
        /// <param name="text">Main text.</param>
        /// <param name="options">Msgbox response options.</param>
        /// <returns>Message box panel.</returns>
        public static Entities.Panel ShowMsgBox(string header, string text, params MsgBoxOption[] options) => ShowMsgBox(header, text, options, null);

        /// <summary>
        /// Show a message box with custom buttons and callbacks.
        /// </summary>
        /// <param name="header">Messagebox header.</param>
        /// <param name="text">Main text.</param>
        /// <param name="options">Msgbox response options.</param>
        /// <param name="append">Optional array of entities to add to msg box under the text and above the buttons.</param>
        /// <param name="size">Alternative size to use.</param>
        /// <param name="onDone">Optional callback to call when this msgbox closes.</param>
        /// <returns>Message box panel.</returns>
        public static Entities.Panel ShowMsgBox(string header, string text, MsgBoxOption[] options, Entities.Entity[] append = null, Vector2? size = null, System.Action onDone = null)
        {
            UserInterface.GamePadModeEnabled = true;

            // create panel for messagebox
            size = size ?? new Vector2(500, 500);
            var panel = new Entities.Panel(size.Value);
            panel.AddChild(new Entities.Header(header));
            panel.AddChild(new Entities.HorizontalLine());
            panel.AddChild(new Entities.Paragraph(text));

            // add to opened boxes counter
            OpenedMsgBoxesCount++;

            // add rectangle to hide and lock background
            Entities.ColoredRectangle fader = null;
            if (BackgroundFaderColor.A != 0)
            {
                fader = new Entities.ColoredRectangle(Vector2.Zero, Entities.Anchor.Center)
                {
                    FillColor = new Color(0, 0, 0, 100),
                    OutlineWidth = 0,
                    ClickThrough = false
                };
                UserInterface.Active.AddEntity(fader);
            }

            // add custom appended entities
            if (append != null)
            {
                foreach (var entity in append)
                {
                    panel.AddChild(entity);
                }
            }

            // add bottom buttons panel
            var buttonsPanel = new Entities.Panel(new Vector2(0, 70), Entities.PanelSkin.None, Entities.Anchor.BottomCenter)
            {
                Padding = Vector2.Zero
            };
            panel.AddChild(buttonsPanel);

            // add all option buttons
            var btnSize = new Vector2(options.Length == 1 ? 0f : (1f / options.Length), 60);
            for (int i = 0; i < options.Length; i++)
            {
                var option = options[i];
                // add button entity
                var button = new Entities.Button(option.Title, anchor: Entities.Anchor.AutoInline, size: btnSize) { Selectable = true, IsFirstSelection = i == 0};

                // set click event
                button.OnClick += (Entities.Entity ent) =>
                {
                    // if need to close message box after clicking this button, close it:
                    if (option.Callback == null || option.Callback())
                    {
                        // remove fader and msg box panel
                        if (fader != null) { fader.RemoveFromParent(); }
                        panel.RemoveFromParent();

                        // decrease msg boxes count
                        OpenedMsgBoxesCount--;

                        // call on-done callback
                        onDone?.Invoke();
                    }
                };

                if (i == 0) button.Select();

                // add button to buttons panel
                buttonsPanel.AddChild(button);
            }

            // add panel to active ui
            UserInterface.Active.AddEntity(panel);
            return panel;
        }

        /// <summary>
        /// Show an input box with custom buttons and callbacks.
        /// </summary>
        /// <param name="header">Inputbox header.</param>
        /// <param name="text">Main text.</param>
        /// <param name="options">Input response options.</param>
        /// <returns>Input box panel.</returns>
        public static Entities.Panel ShowInputBox(string header, string text, params InputBoxOption[] options) => ShowInputBox(header, text, options, null);

        /// <summary>
        /// Show an input box with custom buttons and callbacks.
        /// </summary>
        /// <param name="header">Inputbox header.</param>
        /// <param name="text">Main text.</param>
        /// <param name="options">Input response options.</param>
        /// <param name="append">Optional array of entities to add to msg box under the text and above the buttons.</param>
        /// <param name="size">Alternative size to use.</param>
        /// <param name="onDone">Optional callback to call when this inputbox closes.</param>
        /// <returns>Input box panel.</returns>
        public static Entities.Panel ShowInputBox(string header, string text, InputBoxOption[] options, Entities.Entity[] append = null, Vector2? size = null, System.Action onDone = null)
        {
            UserInterface.GamePadModeEnabled = true;

            // create panel for messagebox
            size = size ?? new Vector2(500, 500);
            var panel = new Entities.Panel(size.Value);
            panel.AddChild(new Entities.Header(header));
            panel.AddChild(new Entities.HorizontalLine());
            panel.AddChild(new Entities.Paragraph(text));
            var txt = panel.AddChild(new Entities.TextInput(false) { IsFocused = true });

            // add to opened boxes counter
            OpenedMsgBoxesCount++;

            // add rectangle to hide and lock background
            Entities.ColoredRectangle fader = null;
            if (BackgroundFaderColor.A != 0)
            {
                fader = new Entities.ColoredRectangle(Vector2.Zero, Entities.Anchor.Center)
                {
                    FillColor = new Color(0, 0, 0, 100),
                    OutlineWidth = 0,
                    ClickThrough = false
                };
                UserInterface.Active.AddEntity(fader);
            }

            // add custom appended entities
            if (append != null)
            {
                foreach (var entity in append)
                {
                    panel.AddChild(entity);
                }
            }

            // add bottom buttons panel
            var buttonsPanel = new Entities.Panel(new Vector2(0, 70), Entities.PanelSkin.None, Entities.Anchor.BottomCenter)
            {
                Padding = Vector2.Zero
            };
            panel.AddChild(buttonsPanel);

            // add all option buttons
            var btnSize = new Vector2(options.Length == 1 ? 0f : (1f / options.Length), 60);
            for (int i = 0; i < options.Length; i++)
            {
                var option = options[i];
                // add button entity
                var button = new Entities.Button(option.Title, anchor: Entities.Anchor.AutoInline, size: btnSize) { Selectable = true, IsFirstSelection = i == 0};

                // set click event
                button.OnClick += (Entities.Entity ent) =>
                {
                    // if need to close message box after clicking this button, close it:
                    if (option.Callback == null || option.Callback(txt.Value))
                    {
                        // remove fader and msg box panel
                        if (fader != null) { fader.RemoveFromParent(); }
                        panel.RemoveFromParent();

                        // decrease msg boxes count
                        OpenedMsgBoxesCount--;

                        // call on-done callback
                        onDone?.Invoke();
                    }
                };

                // add button to buttons panel
                buttonsPanel.AddChild(button);

                if (i == 0) button.Select();
            }

            // add panel to active ui
            UserInterface.Active.AddEntity(panel);
            return panel;
        }

        /// <summary>
        /// Show a message box with just "OK".
        /// </summary>
        /// <param name="header">Message box title.</param>
        /// <param name="text">Main text to write on the message box.</param>
        /// <param name="closeButtonTxt">Text for the closing button (if not provided will use default).</param>
        /// <param name="size">Message box size (if not provided will use default).</param>
        /// <returns>Message box panel.</returns>
        public static Entities.Panel ShowMsgBox(string header, string text, string closeButtonTxt = null, Vector2? size = null)
        {
            UserInterface.GetCursorMode = UserInterface.CursorMode.Roaming;
            return ShowMsgBox(header, text, new MsgBoxOption[]
            {
                new MsgBoxOption(closeButtonTxt ?? DefaultOkButtonText, null)
            }, size: size ?? DefaultMsgBoxSize);
        }
    }
}
