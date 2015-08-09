using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Input
{
    public class TurnScheduler : InputState
    {
        byte CurrentTurn = 0;

        public TurnScheduler()
        {

        }

        public override void EnterState(WorldScreen Game)
        {
            Game.PushInputState(new PlayerTurn(CurrentTurn));
        }

        public override void Exposed(WorldScreen Game)
        {
            CurrentTurn += 1;
            if (CurrentTurn == 2) CurrentTurn = 0;
            Game.PushInputState(new PlayerTurn(CurrentTurn));
        }
    }
}
