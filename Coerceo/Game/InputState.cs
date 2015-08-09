using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public class InputState
    {
        public virtual void EnterState(WorldScreen Game) { }
        public virtual void Covered(WorldScreen Game) { }
        public virtual void Update(WorldScreen Game) { }
        public virtual void Exposed(WorldScreen Game) { }
        public virtual void LeaveState(WorldScreen Game) { }

    }
}