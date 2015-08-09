using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// http://romsteady.blogspot.com/2011/06/key-events-in-xna-40.html

namespace Gem
{

    public class KeyboardMessageFilter : IMessageFilter
    {
        [DllImport("user32.dll")]
        static extern bool TranslateMessage(ref Message lpMsg);

        const int WM_CHAR = 0x0102;
        const int WM_KEYUP = 0x101;
        const int WM_KEYDOWN = 0x0100;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYUP)
                TranslateMessage(ref m);
                
            if (m.Msg == WM_KEYDOWN || m.Msg == WM_KEYUP || m.Msg == WM_CHAR)
            {
                if (KeyboardEvent != null)
                    KeyboardEvent(m);
            }
            return false;
        }

        private Action<System.Windows.Forms.Message> KeyboardEvent;
        
        public static void AddKeyboardMessageFilter(Action<System.Windows.Forms.Message> KeyboardEvent)
        {
            var filter = new KeyboardMessageFilter();
            filter.KeyboardEvent = KeyboardEvent;
            System.Windows.Forms.Application.AddMessageFilter(filter);
        }
    }
}