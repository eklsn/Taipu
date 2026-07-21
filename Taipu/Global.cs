using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;

namespace Taipu
{
    public static class Global
    {
        public static SpriteBatch spriteBatch;
        public static GraphicsDeviceManager graphicsDeviceManager;
        public static GraphicsDevice graphicsDevice;
        public static ContentManager contentManager;
        public static GameWindow window;
        public static GameTime gameTime;
        public static InputListener inputListener;
        public static Mainloop game;
        public static double deltaTime => gameTime.ElapsedGameTime.TotalSeconds;
    }
}
