using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using System.Diagnostics;
namespace Taipu
{
    public class Mainloop : Game
    {

        public Mainloop()
        {
            Debug.WriteLine("Welcome to Taipu <3");
                
            Global.graphicsDeviceManager = new GraphicsDeviceManager(this);
            Global.contentManager = Content;
            Global.game = this;
            Global.contentManager.RootDirectory = "Content";
            IsMouseVisible = true;
            Global.graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        }

        protected override void Initialize()
        {
            Bass.Init();
            MyraEnvironment.Game = this;
            Global.graphicsDevice = GraphicsDevice;
            Global.window = Window;
            base.Initialize();
            Debug.WriteLine(ExtContent.gameFolder);
            Global.graphicsDeviceManager.PreferMultiSampling = true;
            Global.graphicsDevice.PresentationParameters.MultiSampleCount = 4;


        }

        protected override void LoadContent()
        {
            Global.spriteBatch = new SpriteBatch(GraphicsDevice);
            
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            MatrixUpscaler.SetVRes(1280, 720);
            WindowManager.SetResolution(1600, 900);
            MatrixUpscaler.Update(Global.graphicsDevice.Viewport);
            SceneManager.LoadScene(new Scenes.Disclaimer());

        }

        protected override void Update(GameTime gameTime)
        {
            Global.gameTime = gameTime;
            MouseMan.Update();
            KeyboardMan.Update();
            base.Update(gameTime);
            SceneManager.currentScene.Update();
            
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Global.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, transformMatrix: MatrixUpscaler.transformationMatrix);
            SceneManager.currentScene.Draw();
            Global.spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
