using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NEmu
{
    public static class Input
    {
        public static KeyboardState CurrentKeyboardState { get; set; } = Keyboard.GetState();
        public static MouseState CurrentMouseState { get; set; } = Mouse.GetState();

        public static Vector2 MouseVector { get; set; }

        private static KeyboardState lastKeyboardState;
        private static MouseState lastMouseState;

        public static void Update()
        {
            lastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            lastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            MouseVector = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);
        }

        public static bool IsKeyDown(Keys input)
	        => CurrentKeyboardState.IsKeyDown(input);

        public static bool IsKeyUp(Keys input)
	        => CurrentKeyboardState.IsKeyUp(input);

        public static bool KeyPressed(Keys input)
	        => EmulatorGame.Instance.IsActive && CurrentKeyboardState.IsKeyDown(input) && !lastKeyboardState.IsKeyDown(input);

        public static bool MouseLeftDown
	        => EmulatorGame.Instance.IsActive && CurrentMouseState.LeftButton == ButtonState.Pressed;

        public static bool MouseRightDown
	        => EmulatorGame.Instance.IsActive && CurrentMouseState.RightButton == ButtonState.Pressed;

        public static bool MouseLeftClicked
	        => EmulatorGame.Instance.IsActive && CurrentMouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released;

        public static bool MouseRightClicked
	        => EmulatorGame.Instance.IsActive && CurrentMouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released;
    }
}