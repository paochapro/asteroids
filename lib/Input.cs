using Microsoft.Xna.Framework.Input;

namespace Asteroids;

static class Input
{
    public static MouseState Mouse => Microsoft.Xna.Framework.Input.Mouse.GetState();
    public static KeyboardState Keys => Keyboard.GetState();
    private static KeyboardState PreviousKeys { get; set; }

    public static void CycleEnd() => PreviousKeys = Keys;
    public static bool Pressed(Keys key) => Keys.IsKeyDown(key) && !PreviousKeys.IsKeyDown(key);
    public static bool NotPressed(Keys key) => !Keys.IsKeyDown(key) && PreviousKeys.IsKeyDown(key);
    public static bool IsKeyDown(Keys key) => Keys.IsKeyDown(key);
    public static bool IsKeyUp(Keys key) => Keys.IsKeyUp(key);
}