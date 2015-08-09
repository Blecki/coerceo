using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Gem.Gui
{
	public class UIItemProperties
	{
		public PropertyBag Values;
		public Func<UIItem, bool> Condition;

		public UIItemProperties(Func<UIItem, bool> Condition, PropertyBag Values)
		{
			this.Values = Values;
			this.Condition = Condition;
		}
	}

    public class UIItem
    {
        public Shape Shape;
        public List<UIItem> children = new List<UIItem>();
        public UIItem parent;
        public bool Visible = true;
		public List<UIItemProperties> Properties = new List<UIItemProperties>();
        public bool Hover { get; set; }
		public UIItem FocusItem { get; set; }
        public String Name;
        
        public UIItem root { 
            get { 
				if (parent == null) return this;
                return parent.root; 
			}
		}

        public UIItem(String Name, Shape Shape, PropertyBag settings)
        {
            this.Name = Name;
			if (settings != null) Properties.Add(new UIItemProperties(null, settings));
            this.Shape = Shape;
            Hover = false;
        }

        public UIItem Find(String Name)
        {
            if (this.Name == Name) return this;
            else 
                foreach(var child in children)
                {
                    var found = child.Find(Name);
                    if (found != null) return found;
                }
            return null;
        }

		public void AddPropertySet(Func<UIItem, bool> Condition, PropertyBag Values)
		{
			Properties.Insert(0, new UIItemProperties(Condition, Values));
		}

		public static bool IsHover(UIItem item) { return item.Hover; }

        public UIItem FindHoverItem(int x, int y)
        {
            foreach (var child in children)
            {
                var item = child.FindHoverItem(x, y);
                if (item != null) return item;
            }

            if ((GetSetting("transparent", false) as bool?).Value) return null;
            if (Shape.PointInside(new Vector2(x, y))) return this;
            return null;
        }

        public virtual void ClearHover()
        {
            Hover = false;
            foreach (var child in children) child.ClearHover();
        }

		public virtual void AddChild(UIItem child)
		{
			children.Add(child);
			child.parent = this;
		}
		
		public virtual void RemoveChild(UIItem child)	
        {
            children.Remove(child);
        }
        		
		public PropertyBag Settings
		{
			get
			{
				return Properties[0].Values;
			}
		}

		public Object GetSetting(String name, Object _default)
		{
           foreach (var propertySet in Properties)
			{
				if (propertySet.Values.HasProperty(name))
				{
					bool conditionPassed = true;
					if (propertySet.Condition != null) conditionPassed = propertySet.Condition(this);
					if (conditionPassed) return propertySet.Values.GetProperty(name);
				}
			}
			return _default;
		}

        public int GetIntegerSetting(String name, int _default)
        {
            var setting = GetSetting(name, null);
            if (setting == null) return _default;
            try
            {
                return Convert.ToInt32(setting);
            }
            catch (Exception) { return _default; }
        }
		
		public virtual void Render(Gem.Render.RenderContext Context) 
		{
            if (Visible)
            {
                if (!((GetSetting("transparent", false) as bool?).Value))
                {
                    Context.Texture = Context.White;
                    Context.Color = (GetSetting("bg-color", Vector3.One) as Vector3?).Value;

                    var bgImage = GetSetting("image", null) as Microsoft.Xna.Framework.Graphics.Texture2D;
                    if (bgImage != null)
                    {
                        Context.UVTransform = (GetSetting("image-transform", Matrix.Identity) as Matrix?).Value;
                        Context.Texture = bgImage;
                        Shape.Render(Context);
                    }
                    else
                    {
                        Shape.Render(Context);
                    }

                    Context.UVTransform = Matrix.Identity;
                }

                var label = GetSetting("label", null) as String;
                if (label != null)
                {
                    var font = GetSetting("font", null) as BitmapFont;
                    if (font != null)
                    {
                        Context.Color = (GetSetting("text-color", Vector3.Zero) as Vector3?).Value;
                        var textOrigin = (GetSetting("text-origin", Vector2.Zero) as Vector2?).Value;
                        BitmapFont.RenderText(label, textOrigin.X, textOrigin.Y, float.PositiveInfinity, (GetSetting("font-scale", 2.0f) as float?).Value, Context, font);
                    }
                }

                foreach (var child in children)
                    child.Render(Context);
            }
        }

	}

}