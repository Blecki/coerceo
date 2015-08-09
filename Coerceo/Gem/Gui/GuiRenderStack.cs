using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gem.Render;

namespace Gem.Gui
{
    public class GuiRenderStack
    {
        GraphicsDevice Device;
        RenderContext Context;
        OrthographicCamera Camera = new OrthographicCamera(new Viewport(0, 0, 1, 1));
        MatrixStack MatrixStack;
        ImmediateMode2D ImmediateMode;

        public GuiRenderStack(RenderContext Context, GraphicsDevice Device)
        {
            this.Device = Device;
            this.Context = Context;

            ImmediateMode = new ImmediateMode2D(Device);
            MatrixStack = new MatrixStack();
        }
               
        public void BeginGUI(RenderTarget2D target)
        {
            Device.SetRenderTarget(target);
            Device.Clear(Microsoft.Xna.Framework.Color.Transparent);
        }
    }
}
