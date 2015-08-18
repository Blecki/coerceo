using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game.Input
{
    public class AIThink : InputState
    {
        private byte CurrentPlayer;
        private Task<Move> AsyncTask = null;
        
        public AIThink(byte CurrentPlayer)
        {
            this.CurrentPlayer = CurrentPlayer;
        }

        public override void EnterState(WorldScreen Game)
        {
            Game.DisplayBoard(Game.CurrentBoard);

            AsyncTask = AI.PickBestMove(Game.CurrentBoard, 4);
        }

        public override void Covered(WorldScreen Game)
        {

        }

        public override void Update(WorldScreen Game)
        {
            if (AsyncTask.IsCompleted)
            {
                Game.PushInputState(new AITurn(CurrentPlayer, AsyncTask.Result));
            }
        }

        public override void Exposed(WorldScreen Game)
        {
            Game.PopInputState();
        }

        public override void LeaveState(WorldScreen Game)
        {
        }

    }
}
