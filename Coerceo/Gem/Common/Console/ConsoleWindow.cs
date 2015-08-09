using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gem.Console
{
	public class ConsoleWindowKeyboardHandler : IKeyboardHandler
	{
		public ConsoleWindow Console;

		public ConsoleWindowKeyboardHandler(ConsoleWindow TargetConsole)
		{
			this.Console = TargetConsole;
		}

		public void KeyUp(System.Windows.Forms.KeyEventArgs e)
		{
			Console.KeyUp(e.KeyCode, e.KeyValue);
		}

		public void KeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			Console.KeyDown(e.KeyCode, e.KeyValue);
		}

		public void KeyPress(System.Windows.Forms.KeyPressEventArgs e)
		{
			e.Handled = false;
			if (e.KeyChar == '~') return;
			Console.KeyPress(e.KeyChar);
			e.Handled = true;
		}
	}

    /// <summary>
    /// Creates and encapsulates a console input handler, buffer, and display, and binds them together.
    /// </summary>
    public class ConsoleWindow
    {
        public Action<String> ConsoleCommandHandler = null;
        public Rectangle ScreenPosition { get; private set; }
        public Rectangle ActualDrawSize { get; private set; }

        public String Title
        {
            get { return Buffer.title; }
            set { Buffer.title = value; }
        }

        public String Info
        {
            get { return Buffer.info; }
            set { Buffer.info = value; }
        }

        public bool HideInput
        {
            get { return Buffer.InputHidden; }
            set { Buffer.InputHidden = value; }
        }

        GraphicsDevice Graphics;
        EpisodeContentManager Content;
        
        private Gem.Gui.BitmapFont font;
        private System.Threading.Mutex HandlerMutex = new System.Threading.Mutex();
        private Console.ConsoleInputHandler InputHandler;
        private RenderTarget2D ConsoleRenderSurface;
        private DynamicConsoleBuffer Buffer;
        private ConsoleDisplay Display;
        
        public void Write(String s) { Buffer.Write(s); }
        public void WriteLine(String s) { Buffer.Write(s + "\n"); }

        public ConsoleWindow(
            GraphicsDevice Graphics,
            EpisodeContentManager Content,
            Rectangle ScreenPosition,
            int FontScale)
        {
            this.Graphics = Graphics;
            this.Content = Content;

			var wd = Environment.CurrentDirectory;
            font = new Gui.BitmapFont(Content.Load<Texture2D>("Content/small-font"), 6, 8, 6);

            Resize(ScreenPosition, FontScale);
        }

        public void Resize(Rectangle ScreenPosition, int FontScale)
        {
            int width = ScreenPosition.Width / (6 * FontScale);
            int height = ScreenPosition.Height / (8 * FontScale); 

            var rowSize = ScreenPosition.Height / height;
            var realHeight = height * rowSize;
            var colSize = ScreenPosition.Width / width;
            var realWidth = width * colSize;

            ActualDrawSize = new Rectangle(
                ScreenPosition.X + ((ScreenPosition.Width - realWidth) / 2),
                ScreenPosition.Y + ((ScreenPosition.Height - realHeight) / 2),
                realWidth,
                realHeight);

            Display = new ConsoleDisplay(width, height, font, Graphics, Content);
            Buffer = new DynamicConsoleBuffer(2048, Display);
            InputHandler = new ConsoleInputHandler(ConsoleCommandHandler, Buffer, width);

            this.ScreenPosition = ScreenPosition;
            ConsoleRenderSurface = new RenderTarget2D(Graphics, width * font.glyphWidth, height * font.glyphHeight, false,
                SurfaceFormat.Color, DepthFormat.Depth16);
        }

        public void PrepareImage()
        {
            HandlerMutex.WaitOne();

            Buffer.PopulateDisplay();
            Graphics.SetRenderTarget(ConsoleRenderSurface);
            Display.Draw(Graphics);
            Graphics.SetRenderTarget(null);

            HandlerMutex.ReleaseMutex();
        }

        public void Draw(Gem.Render.RenderContext Immediate2d)
        {
                    Immediate2d.Texture = ConsoleRenderSurface;
                    Immediate2d.Color = new Vector3(1, 1, 1);
                    Immediate2d.ImmediateMode.Quad(ActualDrawSize, 10);
        }

		internal void KeyDown(System.Windows.Forms.Keys Code, int Value)
		{
			HandlerMutex.WaitOne();
			this.InputHandler.KeyDown(Code, Value);
			HandlerMutex.ReleaseMutex();
		}

		internal void KeyUp(System.Windows.Forms.Keys Code, int Value)
		{
			HandlerMutex.WaitOne();
			this.InputHandler.KeyUp(Code, Value);
			HandlerMutex.ReleaseMutex();
		}

		internal void KeyPress(char Key)
		{
            HandlerMutex.WaitOne();
            this.InputHandler.KeyPress(Key);
            HandlerMutex.ReleaseMutex();
        }

    }
}
