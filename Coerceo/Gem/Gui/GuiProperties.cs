using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Gem.Gui
{
    public class GuiProperties : PropertyBag
    {
        public Vector3 BackgroundColor
        {
            get { return GetPropertyAs<Vector3>("bg-color"); }
            set { Upsert("bg-color", value); }
        }

        public Vector3 TextColor
        {
            get { return GetPropertyAs<Vector3>("text-color"); }
            set { Upsert("text-color", value); }
        }

        public BitmapFont Font
        {
            get { return GetPropertyAs<BitmapFont>("font"); }
            set { Upsert("font", value); }
        }

        public bool Transparent
        {
            set { Upsert("transparent", value); }
        }

        public String Label
        {
            set { Upsert("label", value); }
        }

        public Vector2 TextOrigin
        {
            set { Upsert("text-origin", value); }
        }

        public Action ClickAction
        {
            set { Upsert("click-action", value); }
        }

        public Microsoft.Xna.Framework.Graphics.Texture2D Image { set { Upsert("image", value); } }
        public Matrix ImageTransform { set { Upsert("image-transform", value); } }
        public float FontScale { set { Upsert("font-scale", value); } }
    }
}
