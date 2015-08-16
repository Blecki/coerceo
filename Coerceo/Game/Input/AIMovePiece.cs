using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game.Input
{
    public class AIMovePiece : InputState
    {
        private Move ChosenMove;
        private float Timer = 0;

        public AIMovePiece(Move ChosenMove)
        {
            this.ChosenMove = ChosenMove;
        }

        public override void EnterState(WorldScreen Game)
        {
            Game.DisplayBoard(Game.CurrentBoard);
            var ghost = 0;

            foreach (var move in Coerceo.EnumerateLegalPieceMoves(ChosenMove.Coordinate, Game.CurrentBoard))
            {
                var neighbor = Coerceo.FindMoveNeighbor(ChosenMove.Coordinate, move.Direction);
                Game.ShowGhost(ghost++, ChosenMove.Coordinate.Triangle % 2, neighbor.Tile, neighbor.Triangle, neighbor);
            }
        }

        public override void Covered(WorldScreen Game)
        {

        }

        public override void Update(WorldScreen Game)
        {
            Timer += Game.ElapsedSeconds;

                foreach (var piece in Game.GhostPieceNodes)
                {
                    var coordinate = piece.Tag as Coordinate?;
                    if (coordinate == null || !coordinate.HasValue) continue;
                    if (coordinate.Value == Coerceo.FindMoveNeighbor(ChosenMove.Coordinate, ChosenMove.Direction))
                        piece.Hilite = true;
                }

            if (Timer > 1)
            {
                var newBoard = Coerceo.ApplyMove(Game.CurrentBoard, ChosenMove);
                Game.CurrentBoard = newBoard;
                Game.PopInputState();
            }
        }

        public override void Exposed(WorldScreen Game)
        {
        }


        public override void LeaveState(WorldScreen Game)
        {
        }

    }
}
