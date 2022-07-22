#region File Description
//-----------------------------------------------------------------------------
// Helper utility to get keyboard and mouse input.
// It provides easier access to the Input API, and in addition functions to
// measure changes between frames.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Nez.GeonBit.UI
{
    /// <summary>
    /// Supported mouse buttons.
    /// </summary>
    public enum MouseButton
    {
        ///<summary>Left mouse button.</summary>
        Left,

        ///<summary>Right mouse button.</summary>
        Right,

        ///<summary>Middle mouse button (eg scrollwheel when clicked).</summary>
        Middle
    };

    /// <summary>
    /// Supported GamePad Buttons
    /// </summary>
    public enum GamePadButton
    {
        ///<summary>GamePad A button.</summary>
        A_Button,
        ///<summary>GamePad down button.</summary>
        DPadDown,
        ///<summary>GamePad up button.</summary>
        /// <summary>
        /// DPAD-Up button
        /// </summary>
        DPadUp,
        /// <summary>
        /// DPAD-Left button
        /// </summary>
        DPadLeft,
        /// <summary>
        /// DPAD-Right button
        /// </summary>
        DPadRight
    };

    /// <summary>
    /// Some special characters input.
    /// Note: enum values are based on ascii table values for these special characters.
    /// </summary>
    internal enum SpecialChars
    {
        Null = 0,           // no character input
        Delete = 127,       // delete char
        Backspace = 8,      // backspace char
        Space = 32,         // space character input
        ArrowLeft = 1,      // arrow left - moving caret left
        ArrowRight = 2,     // arrow right - moving caret right
    };

    /// <summary>
    /// Provide easier keyboard and mouse access, keyboard text input, and other user input utils.
    /// </summary>
    public class InputHelper
    {
        // locks the mouse cursor position
        private bool _LockMousePosition = false;
        /// <summary>
        /// LockMousePosition
        /// </summary>
        public bool LockMousePosition
        {
            private get => _LockMousePosition;
            set => _LockMousePosition = value;
        }

        /// <summary>
        /// Is the A button on the gamepad currently being pressed?
        /// </summary>
        public bool IsAButtonPressed = false;

        // store current & previous keyboard states so we can detect key release
        private KeyboardState _newKeyboardState;
        private KeyboardState _oldKeyboardState;

        // store current & previous mouse states so we can detect key release and diff
        private MouseState _newMouseState;
        private MouseState _oldMouseState;
        private GamePadState _newGamePadState;
        private GamePadState _oldGamePadState;
        private Vector2 _newMousePos;

        // store current frame gametime
        private GameTime _currTime;

        /// <summary>An artificial "lag" after a key is pressed when typing text input, to prevent mistake duplications.</summary>
        public float KeysTypeCooldown = 0.6f;

        // last character that was pressed down
        private char _currCharacterInput = (char)SpecialChars.Null;

        // last key that provide character input and was pressed
        private Keys _currCharacterInputKey = Keys.Escape;

        // keyboard input cooldown for textual input
        private float _keyboardInputCooldown = 0f;

        // true when a new keyboard key is pressed
        private bool _newKeyIsPressed = false;

        // current capslock state
        private bool _capslock = false;

        /// <summary>
        /// Current mouse wheel value.
        /// </summary>
        public int MouseWheel = 0;

        /// <summary>
        /// Mouse wheel change sign (eg 0, 1 or -1) since last frame.
        /// </summary>
        public int MouseWheelChange = 0;

        /// <summary>
        /// ThumbStickLeftChange. Used for mouse cursor control (after dragging)
        /// </summary>
        public Vector2 ThumbStickLeftChange = Vector2.Zero;
        /// <summary>
        /// Defines if the left thumbstick should be used for dragging (sliders etc.)
        /// </summary>
        private bool _ThumbStickLeftDragging = true;
        /// <summary>
        /// Set this to true if you want to use the left thumbstick of your gamepad for dragging sliders etc.
        /// </summary>
        public bool ThumbStickLeftDragging
        {
            get => _ThumbStickLeftDragging;
            set => _ThumbStickLeftDragging = value;
        }

        /// <summary>
        /// Defines if the left thumbstick can be used.
        /// </summary>
        private bool _ThumbStickLeftCanDrag = false;
        /// <summary>
        /// Defines if the left thumbstick can be used (don't changed it manually! It's automatically set for you).
        /// </summary>
        public bool ThumbStickLeftCanDrag
        {
            get => _ThumbStickLeftCanDrag;
            set => _ThumbStickLeftCanDrag = value;
        }

        private float ThumbStickLeftCoolDown = 12f;
        private float ThumbStickLeftCoolDownMax = 12f;

        /// <summary>
        /// Create the input helper.
        /// </summary>
        public InputHelper()
        {
            // init keyboard states
            _newKeyboardState = _oldKeyboardState;

            // init mouse and gamepad states
            _newGamePadState = _oldGamePadState;
            _newMouseState = _oldMouseState;

            // call first update to get starting positions
            Update(new GameTime());
        }

        /// <summary>
        /// Current frame game time.
        /// </summary>
        public GameTime CurrGameTime => _currTime;

        /// <summary>
        /// Update current states.
        /// If used outside GeonBit.UI, this function should be called first thing inside your game 'Update()' function,
        /// and before you make any use of this class.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            // store game time
            _currTime = gameTime;

            // store previous states
            _oldMouseState = _newMouseState;
            _oldKeyboardState = _newKeyboardState;
            _oldGamePadState = _newGamePadState;

            // get new states
            _newMouseState = Mouse.GetState();
            _newKeyboardState = Keyboard.GetState();
            _newGamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

            // If the mouse was used, set to roaming cursor mode (as opposed to snapping mode for gamepad)
            if (Input.MousePositionDelta.X != 0 && Input.MousePositionDelta.Y != 0)
            {
                UserInterface.GetCursorMode = UserInterface.CursorMode.Roaming;
            }

            if (!_LockMousePosition)
            {
                _newMousePos = Input.MousePosition;
            }

            // get thumbstickleft state
            if (ThumbStickLeftDragging)
            {
                ThumbStickLeftChange = _newGamePadState.ThumbSticks.Left;
                if (ThumbStickLeftCanDrag == false && ThumbStickLeftCoolDown > 0)
                {
                    ThumbStickLeftCoolDown--;
                }
                else
                {
                    ThumbStickLeftCanDrag = true;
                    ThumbStickLeftCoolDown = ThumbStickLeftCoolDownMax;
                }
            }

            // Update mouse state's position if cursor mode is set to roaming (used by gamepad)
            if (_newGamePadState.ThumbSticks.Left != Vector2.Zero && UserInterface.GamePadModeEnabled )
            {
                UserInterface.GetCursorMode = UserInterface.CursorMode.Roaming;
                UpdateCursorPosition(new Vector2(
                Input.RawMousePosition.X + (_newGamePadState.ThumbSticks.Left.X * 10),
                Input.RawMousePosition.Y + (-_newGamePadState.ThumbSticks.Left.Y * 10)));
            }

            // get mouse wheel state
            int prevMouseWheel = MouseWheel;
            MouseWheel = Input.MouseWheel;
            MouseWheelChange = System.Math.Sign(MouseWheel - prevMouseWheel);

            // update capslock state
            if (_newKeyboardState.IsKeyDown(Keys.CapsLock) && !_oldKeyboardState.IsKeyDown(Keys.CapsLock))
            {
                _capslock = !_capslock;
            }

            // decrease keyboard input cooldown time
            if (_keyboardInputCooldown > 0f)
            {
                _newKeyIsPressed = false;
                _keyboardInputCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // if current text input key is no longer down, reset text input character
            if (_currCharacterInput != (char)SpecialChars.Null &&
                !_newKeyboardState.IsKeyDown(_currCharacterInputKey))
            {
                _currCharacterInput = (char)SpecialChars.Null;
            }

            // send key-down events
            foreach (Keys key in System.Enum.GetValues(typeof(Keys)))
            {
                if (_newKeyboardState.IsKeyDown(key) && !_oldKeyboardState.IsKeyDown(key))
                {
                    OnKeyPressed(key);
                }
            }
        }

        /// <summary>
        /// Move the cursor to be at the center of the screen.
        /// </summary>
        /// <param name="pos">New mouse position.</param>
        public void UpdateCursorPosition(Vector2 pos)
        {
            // move mouse position back to center
            Mouse.SetPosition((int)pos.X, (int)pos.Y);
            _newMousePos = pos;
        }

        /// <summary>
        /// Calculate and return current cursor position transformed by a matrix.
        /// </summary>
        /// <param name="transform">Matrix to transform cursor position by.</param>
        /// <returns>Cursor position with optional transform applied.</returns>
        public Vector2 TransformCursorPos(Matrix? transform)
        {
            var newMousePos = _newMousePos;
            if (transform != null)
            {
                return Vector2.Transform(newMousePos, transform.Value) - new Vector2(transform.Value.Translation.X, transform.Value.Translation.Y);
            }
            return newMousePos;
        }

        /// <summary>
        /// Called every time a keyboard key is pressed (called once on the frame key was pressed).
        /// </summary>
        /// <param name="key">Key code that is being pressed on this frame.</param>
        protected void OnKeyPressed(Keys key) => NewKeyTextInput(key);

        /// <summary>
        /// This update the character the user currently type down, for text input.
        /// This function called whenever a new key is pressed down, and becomes the current input
        /// until it is released.
        /// </summary>
        /// <param name="key">Key code that is being pressed down on this frame.</param>
        private void NewKeyTextInput(Keys key)
        {
            // reset cooldown time and set new key pressed = true
            _keyboardInputCooldown = KeysTypeCooldown;
            _newKeyIsPressed = true;

            // get if shift is currently down
            bool isShiftDown = _newKeyboardState.IsKeyDown(Keys.LeftShift) || _newKeyboardState.IsKeyDown(Keys.RightShift);

            // set curr input key, but also keep the previous key in case we need to revert
            var prevKey = _currCharacterInputKey;
            _currCharacterInputKey = key;

            // handle special keys and characters
            switch (key)
            {
                case Keys.Space:
                    _currCharacterInput = (char)SpecialChars.Space;
                    return;

                case Keys.Left:
                    _currCharacterInput = (char)SpecialChars.ArrowLeft;
                    return;

                case Keys.Right:
                    _currCharacterInput = (char)SpecialChars.ArrowRight;
                    return;

                case Keys.Delete:
                    _currCharacterInput = (char)SpecialChars.Delete;
                    return;

                case Keys.Back:
                    _currCharacterInput = (char)SpecialChars.Backspace;
                    return;

                case Keys.CapsLock:
                case Keys.RightShift:
                case Keys.LeftShift:
                    _newKeyIsPressed = false;
                    return;

                // line break
                case Keys.Enter:
                    _currCharacterInput = '\n';
                    return;

                // number 0
                case Keys.D0:
                case Keys.NumPad0:
                    _currCharacterInput = (isShiftDown && key == Keys.D0) ? ')' : '0';
                    return;

                // number 9
                case Keys.D9:
                case Keys.NumPad9:
                    _currCharacterInput = (isShiftDown && key == Keys.D9) ? '(' : '9';
                    return;

                // number 8
                case Keys.D8:
                case Keys.NumPad8:
                    _currCharacterInput = (isShiftDown && key == Keys.D8) ? '*' : '8';
                    return;

                // number 7
                case Keys.D7:
                case Keys.NumPad7:
                    _currCharacterInput = (isShiftDown && key == Keys.D7) ? '&' : '7';
                    return;

                // number 6
                case Keys.D6:
                case Keys.NumPad6:
                    _currCharacterInput = (isShiftDown && key == Keys.D6) ? '^' : '6';
                    return;

                // number 5
                case Keys.D5:
                case Keys.NumPad5:
                    _currCharacterInput = (isShiftDown && key == Keys.D5) ? '%' : '5';
                    return;

                // number 4
                case Keys.D4:
                case Keys.NumPad4:
                    _currCharacterInput = (isShiftDown && key == Keys.D4) ? '$' : '4';
                    return;

                // number 3
                case Keys.D3:
                case Keys.NumPad3:
                    _currCharacterInput = (isShiftDown && key == Keys.D3) ? '#' : '3';
                    return;

                // number 2
                case Keys.D2:
                case Keys.NumPad2:
                    _currCharacterInput = (isShiftDown && key == Keys.D2) ? '@' : '2';
                    return;

                // number 1
                case Keys.D1:
                case Keys.NumPad1:
                    _currCharacterInput = (isShiftDown && key == Keys.D1) ? '!' : '1';
                    return;

                // question mark
                case Keys.OemQuestion:
                    _currCharacterInput = isShiftDown ? '?' : '/';
                    return;

                // quotes
                case Keys.OemQuotes:
                    _currCharacterInput = isShiftDown ? '\"' : '\'';
                    return;

                // semicolon
                case Keys.OemSemicolon:
                    _currCharacterInput = isShiftDown ? ':' : ';';
                    return;

                // tilde
                case Keys.OemTilde:
                    _currCharacterInput = isShiftDown ? '~' : '`';
                    return;

                // open brackets
                case Keys.OemOpenBrackets:
                    _currCharacterInput = isShiftDown ? '{' : '[';
                    return;

                // close brackets
                case Keys.OemCloseBrackets:
                    _currCharacterInput = isShiftDown ? '}' : ']';
                    return;

                // add
                case Keys.OemPlus:
                case Keys.Add:
                    _currCharacterInput = (isShiftDown || key == Keys.Add) ? '+' : '=';
                    return;

                // substract
                case Keys.OemMinus:
                case Keys.Subtract:
                    _currCharacterInput = isShiftDown ? '_' : '-';
                    return;

                // decimal dot
                case Keys.OemPeriod:
                case Keys.Decimal:
                    _currCharacterInput = isShiftDown ? '>' : '.';
                    return;

                // divide
                case Keys.Divide:
                    _currCharacterInput = isShiftDown ? '?' : '/';
                    return;

                // multiply
                case Keys.Multiply:
                    _currCharacterInput = '*';
                    return;

                // backslash
                case Keys.OemBackslash:
                    _currCharacterInput = isShiftDown ? '|' : '\\';
                    return;

                // comma
                case Keys.OemComma:
                    _currCharacterInput = isShiftDown ? '<' : ',';
                    return;

                // tab
                case Keys.Tab:
                    _currCharacterInput = ' ';
                    return;

                // not a special char - revert last character input key and continue processing.
                default:
                    _currCharacterInputKey = prevKey;
                    break;
            };

            // get current key thats getting pressed as a string
            string lastCharPressedStr = key.ToString();

            // if character is not a valid char but a special key we don't want to handle, skip
            // (note: keys that are characters should have length of 1)
            if (lastCharPressedStr.Length > 1)
            {
                return;
            }

            // set current key as the current text input key
            _currCharacterInputKey = key;

            // get current capslock state and invert if shift is down
            bool capsLock = _capslock;
            if (isShiftDown)
            {
                capsLock = !capsLock;
            }

            // fix case and set as current char pressed
            _currCharacterInput = (capsLock ? lastCharPressedStr.ToUpper() : lastCharPressedStr.ToLower())[0];

        }

        /// <summary>
        /// Get textual input from keyboard.
        /// If user enter keys it will push them into string, if delete or backspace will remove chars, etc.
        /// This also handles keyboard cooldown, to make it feel like windows-input.
        /// </summary>
        /// <param name="txt">String to push text input into.</param>
        /// <param name="pos">Position to insert / remove characters. -1 to push at the end of string. After done, will contain actual new caret position.</param>
        /// <returns>String after text input applied on it.</returns>
        public string GetTextInput(string txt, ref int pos)
        {
            // if need to skip due to cooldown time
            if (!_newKeyIsPressed && _keyboardInputCooldown > 0f)
            {
                return txt;
            }

            // if no valid characters are currently input
            if (_currCharacterInput == (char)SpecialChars.Null)
            {
                return txt;
            }

            // get default position
            if (pos == -1)
            {
                pos = txt.Length;
            }

            // handle special chars
            switch (_currCharacterInput)
            {
                case (char)SpecialChars.ArrowLeft:
                    if (--pos < 0) { pos = 0; }
                    return txt;

                case (char)SpecialChars.ArrowRight:
                    if (++pos > txt.Length) { pos = txt.Length; }
                    return txt;

                case (char)SpecialChars.Backspace:
                    pos--;
                    return (pos < txt.Length && pos >= 0 && txt.Length > 0) ? txt.Remove(pos, 1) : txt;

                case (char)SpecialChars.Delete:
                    return (pos < txt.Length && txt.Length > 0) ? txt.Remove(pos, 1) : txt;
            }

            // add current character
            return txt.Insert(pos++, _currCharacterInput.ToString());
        }

        /// <summary>
        /// Get current mouse poisition.
        /// </summary>
        public Vector2 MousePosition => _newMousePos;

        /// <summary>
        /// Get mouse position change since last frame.
        /// </summary>
        /// <return>Mouse position change as a 2d vector.</return>
        public Vector2 MousePositionDiff => Input.MousePositionDelta.ToVector2();

        /// <summary>
        /// Check if a given mouse button is down.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is down.</return>
        public bool MouseButtonHeldDown(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Pressed;

        /// <summary>
        /// Return if any of mouse buttons is down.
        /// </summary>
        /// <returns>True if any mouse button is currently down.</returns>
        public bool AnyMouseButtonDown() => MouseButtonHeldDown(MouseButton.Left) ||
                MouseButtonHeldDown(MouseButton.Right) ||
                MouseButtonHeldDown(MouseButton.Middle);

        /// <summary>
        /// Check if a given mouse button was released in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was released in this frame.</return>
        public bool MouseButtonReleased(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;

        /// <summary>
        /// Check if a given mouse button is held down.
        /// </summary>
        /// <param name="button">Bouse button to check.</param>
        /// <return>True if given button is down.</return>
        public bool GamePadButtonHeldDown(GamePadButton button) => UserInterface.GamePadModeEnabled && GetGamePadButtonState(button) == ButtonState.Pressed;

        /// <summary>
        /// Check if a given gamepad button was released in current frame.
        /// </summary>
        /// <param name="button">GamePad button to check.</param>
        /// <return>True if given gamepad button was released in this frame.</return>
        public bool GamePadButtonReleased(GamePadButton button) => UserInterface.GamePadModeEnabled && GetGamePadButtonState(button) == ButtonState.Released && GetGamePadPreviousButtonState(button) == ButtonState.Pressed;

        /// <summary>
        /// Check if a given gamepad button was pressed in current frame.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool GamePadButtonPressed(GamePadButton button) => UserInterface.GamePadModeEnabled && GetGamePadButtonState(button) == ButtonState.Pressed && GetGamePadPreviousButtonState(button) == ButtonState.Released;

        /// <summary>
        /// Return if any mouse button was released this frame.
        /// </summary>
        /// <returns>True if any mouse button was released.</returns>
        public bool AnyMouseButtonReleased() => MouseButtonReleased(MouseButton.Left) ||
                MouseButtonReleased(MouseButton.Right) ||
                MouseButtonReleased(MouseButton.Middle);

        /// <summary>
        /// Check if a given mouse button was pressed in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was pressed in this frame.</return>
        public bool MouseButtonPressed(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Pressed && GetMousePreviousButtonState(button) == ButtonState.Released;

        /// <summary>
        /// Return if any mouse button was pressed in current frame.
        /// </summary>
        /// <returns>True if any mouse button was pressed in current frame..</returns>
        public bool AnyMouseButtonPressed() => MouseButtonPressed(MouseButton.Left) ||
                MouseButtonPressed(MouseButton.Right) ||
                MouseButtonPressed(MouseButton.Middle);

        /// <summary>
        /// Check if a given mouse button was just clicked (eg released after being pressed down)
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is clicked.</return>
        public bool MouseButtonClick(MouseButton button = MouseButton.Left) => GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;

        /// <summary>
        /// Return if any of mouse buttons was clicked this frame.
        /// </summary>
        /// <returns>True if any mouse button was clicked.</returns>
        public bool AnyMouseButtonClicked() => MouseButtonClick(MouseButton.Left) ||
                MouseButtonClick(MouseButton.Right) ||
                MouseButtonClick(MouseButton.Middle);

        /// <summary>
        /// Return the state of a mouse button (up / down).
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Mouse button state.</returns>
        private ButtonState GetMouseButtonState(MouseButton button = MouseButton.Left)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return _newMouseState.LeftButton;
                case MouseButton.Right:
                    return _newMouseState.RightButton;
                case MouseButton.Middle:
                    return _newMouseState.MiddleButton;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Return the state of a gamepad button (up / down).
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>GamePad button state.</returns>
        private ButtonState GetGamePadButtonState(GamePadButton button)
        {
            switch (button)
            {
                case GamePadButton.DPadUp:
                    return _newGamePadState.DPad.Up;
                case GamePadButton.DPadRight:
                    return _newGamePadState.DPad.Right;
                case GamePadButton.DPadDown:
                    return _newGamePadState.DPad.Down;
                case GamePadButton.DPadLeft:
                    return _newGamePadState.DPad.Left;
                case GamePadButton.A_Button:
                    return _newGamePadState.Buttons.A;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Return the state of a mouse button (up / down), in previous frame.
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Mouse button state.</returns>
        private ButtonState GetMousePreviousButtonState(MouseButton button = MouseButton.Left)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return _oldMouseState.LeftButton;
                case MouseButton.Right:
                    return _oldMouseState.RightButton;
                case MouseButton.Middle:
                    return _oldMouseState.MiddleButton;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Get GamePad state in previous frame.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private ButtonState GetGamePadPreviousButtonState(GamePadButton button = GamePadButton.A_Button)
        {
            switch (button)
            {
                case GamePadButton.DPadUp:
                    return _oldGamePadState.DPad.Up;
                case GamePadButton.DPadRight:
                    return _oldGamePadState.DPad.Right;
                case GamePadButton.DPadDown:
                    return _oldGamePadState.DPad.Down;
                case GamePadButton.DPadLeft:
                    return _oldGamePadState.DPad.Left;
                case GamePadButton.A_Button:
                    return _oldGamePadState.Buttons.A;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Check if a given keyboard key is down.
        /// </summary>
        /// <param name="key">Key button to check.</param>
        /// <return>True if given key button is down.</return>
        public bool IsKeyDown(Keys key) => _newKeyboardState.IsKeyDown(key);

        /// <summary>
        /// Check if a given keyboard key was previously pressed down and now released in this frame.
        /// </summary>
        /// <param name="key">Key button to check.</param>
        /// <return>True if given key button was just released.</return>
        public bool IsKeyReleased(Keys key) => _oldKeyboardState.IsKeyDown(key) &&
                   _newKeyboardState.IsKeyUp(key);
    }
}
