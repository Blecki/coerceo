using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gem
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        private IScreen activeGame = null;
        private IScreen nextGame = null;
        public IScreen Game { get { return activeGame; } set { nextGame = value; } }
		public EpisodeContentManager EpisodeContent;
        public ScriptBuilder ScriptBuilder;
        private Render.RenderContext RenderContext;
        private Render.OrthographicCamera ConsoleCamera;

        private List<Console.ConsoleWindow> Consoles = new List<Console.ConsoleWindow>();
        
        public Console.ConsoleWindow MainConsole
        {
            get
            {
                if (Consoles.Count == 0) return null;
                return Consoles[0];
            }
        }

        GraphicsDeviceManager graphics;

        public Input Input { get; private set; }

        public bool ConsoleOpen { get; private set; }
        private string startupCommand;

        public void ReportException(Exception e)
        {
            ConsoleOpen = true;
            Consoles[0].WriteLine(e.Message);
            Consoles[0].WriteLine(e.StackTrace);
        }

        public void ReportError(String msg)
        {
            ConsoleOpen = true;
            Consoles[0].WriteLine(msg);
        }

        public void Write(String msg)
        {
            Consoles[0].Write(msg);
        }

        public Main(String startupCommand)
        {
            ConsoleOpen = false;

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Input = new Input(Window.Handle);
            Input.AddAxis("MAIN", new MouseAxisBinding());

            this.startupCommand = startupCommand;
        }

        public Console.ConsoleWindow AllocateConsole(Rectangle at, int FontScale = 2)
        {
            Consoles.Add(new Console.ConsoleWindow(GraphicsDevice, EpisodeContent, at, FontScale));
            return Consoles[Consoles.Count - 1];
        }

        public void ClearConsoles()
        {
            Consoles.Clear();
        }

        public void OpenConsole()
        {
            if (ConsoleOpen) return;

               if (MainConsole != null)
                        Input.PushKeyboardHandler(new Console.ConsoleWindowKeyboardHandler(MainConsole));
                    ConsoleOpen = true;
        }

        public void CloseConsole()
        {
            if (!ConsoleOpen) return;
            Input.PopKeyboardHandler();
            ConsoleOpen = false;
        }

        protected override void LoadContent()
        {
			EpisodeContent = new EpisodeContentManager(Content.ServiceProvider, "");
			var mainConsole = AllocateConsole(GraphicsDevice.Viewport.Bounds);
            ScriptBuilder = new Gem.ScriptBuilder(mainConsole.WriteLine);
            mainConsole.ConsoleCommandHandler += s =>
            {
                var action = ScriptBuilder.CompileScript(s);
                if (action != null) action();
            };
            mainConsole.Resize(GraphicsDevice.Viewport.Bounds, 2);

            RenderContext = new Render.RenderContext(EpisodeContent.Load<Effect>("Content/draw"), GraphicsDevice);
            ConsoleCamera = new Render.OrthographicCamera(GraphicsDevice.Viewport);
            ConsoleCamera.focus = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			Input.PushKeyboardHandler(new MainKeyboardHandler(this));
        }

        protected override void UnloadContent()
        {
            if (activeGame != null)
                activeGame.End();
            activeGame = null;
        }

        private int ticks = 0;
        protected override void Update(GameTime gameTime)
        {
            if (ticks != 0)
            {
                if (nextGame != null)
                {
                    var saveActive = activeGame;
                    if (activeGame != null) activeGame.End();
                    activeGame = nextGame;
                    activeGame.Main = this;
                    activeGame.Input = Input;
                    try
                    {
                        activeGame.Begin();
                    }
                    catch (Exception e)
                    {
                        activeGame = saveActive;
                        if (activeGame != null) activeGame.Begin();
                        ReportException(e);
                    }
                    nextGame = null;
                }

                //try
                {
                    Input.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    if (activeGame != null) activeGame.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                //catch (Exception e)
                {
                //    ReportException(e);
                }
            }
            else
                ticks = 1;
            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (ConsoleOpen)
                foreach (var consoleWindow in Consoles)
                    consoleWindow.PrepareImage();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            if (activeGame != null) activeGame.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (ConsoleOpen)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
                RenderContext.Camera = ConsoleCamera;
                RenderContext.LightingEnabled = false;
                RenderContext.World = Matrix.Identity;
                RenderContext.UVTransform = Matrix.Identity;
                RenderContext.NormalMap = RenderContext.NeutralNormals;
                RenderContext.Alpha = 0.75f;
                RenderContext.ApplyChanges();

                foreach (var consoleWindow in Consoles)
                    consoleWindow.Draw(RenderContext);
            }
            
            base.Draw(gameTime);
        }
    }
}
