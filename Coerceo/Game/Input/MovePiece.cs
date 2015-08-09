using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game.Input
{
    public class MovePiece : InputState
    {
        private Coordinate SelectedPiece;
        public bool Cancelled = false;

        public MovePiece(Coordinate SelectedPiece)
        {
            this.SelectedPiece = SelectedPiece;
        }

        public override void EnterState(WorldScreen Game)
        {
            Game.DisplayBoard(Game.CurrentBoard);
            var ghost = 0;

            foreach (var move in Coerceo.EnumerateLegalPieceMoves(SelectedPiece, Game.CurrentBoard))
            {
                var neighbor = Coerceo.FindMoveNeighbor(SelectedPiece, move.Direction);
                Game.ShowGhost(ghost++, SelectedPiece.Triangle % 2, neighbor.Tile, neighbor.Triangle, move);
            }

        }

        public override void Covered(WorldScreen Game)
        {

        }

        public override void Update(WorldScreen Game)
        {
            var hoverMesh = Game.HoverNode as Gem.Render.NormalMapMeshNode;
            if (hoverMesh == null) return;

            var move = hoverMesh.Tag as Move?;
            if (move == null || !move.HasValue)
            {
                var coordinate = hoverMesh.Tag as Coordinate?;
                if (coordinate == null || !coordinate.HasValue) return;
                if (coordinate.Value == SelectedPiece)
                {
                    hoverMesh.Hilite = true;
                    if (Game.Main.Input.Check("CLICK"))
                    {
                        Cancelled = true;
                        Game.PopInputState();
                    }
                }
            }
            else
            { 
                hoverMesh.Hilite = true;
                if (Game.Main.Input.Check("CLICK"))
                {
                    var newBoard = Coerceo.ApplyMove(Game.CurrentBoard, move.Value);
                    Game.CurrentBoard = newBoard;
                    Game.PopInputState();
                }
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
