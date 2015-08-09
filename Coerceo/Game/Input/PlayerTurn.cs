using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game.Input
{
    public class PlayerTurn : InputState
    {
        private byte CurrentPlayer;
        private MovePiece MovePieceState;

        public PlayerTurn(byte CurrentPlayer)
        {
            this.CurrentPlayer = CurrentPlayer;
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
            var hoverMesh = Game.HoverNode as Gem.Render.NormalMapMeshNode;
            if (hoverMesh == null) return;

            var coordinate = hoverMesh.Tag as Coordinate?;
            if (coordinate == null || !coordinate.HasValue) return;

            var legalMoves = Coerceo.EnumerateLegalPieceMoves(coordinate.Value, Game.CurrentBoard).Count();
            if (legalMoves != 0)
            { 
                hoverMesh.Hilite = true;
                if (Game.Main.Input.Check("CLICK"))
                {
                    MovePieceState = new MovePiece(coordinate.Value);
                    Game.PushInputState(MovePieceState);
                }
            }
        }

        public override void Exposed(WorldScreen Game)
        {
            Game.DisplayBoard(Game.CurrentBoard);
            if (MovePieceState != null && !MovePieceState.Cancelled) Game.PopInputState();
        }

        public override void LeaveState(WorldScreen Game)
        {
        }

    }
}
