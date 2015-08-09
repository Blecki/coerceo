using System;
using System.Collections.Generic;
using System.Linq;

namespace Gem
{
    internal class MainKeyboardHandler : IKeyboardHandler
    {
        Main Main;

        internal MainKeyboardHandler(Main Main)
        {
            this.Main = Main;
        }

        public void KeyUp(System.Windows.Forms.KeyEventArgs e)
        {
        }

        public void KeyDown(System.Windows.Forms.KeyEventArgs e)
        {
        }

        public void KeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == '~')
            {
                if (Main.ConsoleOpen) Main.CloseConsole();
                else Main.OpenConsole();
                e.Handled = true;
            }
        }
    }
}