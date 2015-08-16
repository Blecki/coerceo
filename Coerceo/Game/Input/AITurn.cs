using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game.Input
{
    public class AITurn : InputState
    {
        private byte CurrentPlayer;
        private float Timer = 0;
        private Move ChosenMove;
        
        public AITurn(byte CurrentPlayer, Move ChosenMove)
        {
            this.CurrentPlayer = CurrentPlayer;
            this.ChosenMove = ChosenMove;
        }

        public override void EnterState(WorldScreen Game)
        {
            Game.DisplayBoard(Game.CurrentBoard);
        }

        public override void Covered(WorldScreen Game)
        {

        }

        public override void Update(WorldScreen Game)
        {
            Timer += Game.ElapsedSeconds;

            foreach (var piece in Game.PieceNodes)
            {
                var coordinate = piece.Tag as Coordinate?;
                if (coordinate == null || !coordinate.HasValue) continue;
                if (coordinate.Value == ChosenMove.Coordinate)
                    piece.Hilite = true;
            }

            if (Timer > 1)
            {
                if (ChosenMove.Type == (byte)MoveType.MovePiece)
                    Game.PushInputState(new AIMovePiece(ChosenMove));
                else
                {
                    var newBoard = Coerceo.ApplyMove(Game.CurrentBoard, ChosenMove);
                    Game.CurrentBoard = newBoard;
                    Game.PopInputState();
                }
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
