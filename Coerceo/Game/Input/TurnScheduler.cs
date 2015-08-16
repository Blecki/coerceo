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
        PlayerType[] PlayerTypes;

        public TurnScheduler(PlayerType[] PlayerTypes)
        {
            this.PlayerTypes = PlayerTypes;
        }

        public override void EnterState(WorldScreen Game)
        {
            PushTurnHandler(Game);
            Game.TotalMoves = 0;
        }

        public override void Exposed(WorldScreen Game)
        {
            Game.TotalMoves += 1;

            CurrentTurn += 1;
            if (CurrentTurn == 2) CurrentTurn = 0;

            PushTurnHandler(Game);
        }

        private void PushTurnHandler(WorldScreen Game)
        {
            switch (PlayerTypes[CurrentTurn])
            {
                case PlayerType.Player:
                    Game.PushInputState(new PlayerTurn(CurrentTurn));
                    break;
                case PlayerType.AI:
                    Game.PushInputState(new AIThink(CurrentTurn));
                    break;
            }
        }
    }
}
