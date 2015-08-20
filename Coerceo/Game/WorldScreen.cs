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
    public enum PlayerType
    {
        Player,
        AI
    }

    public class WorldScreen : IScreen
    {
        public Gem.Input Input { get; set; }
        public Main Main { get; set; }

        EpisodeContentManager Content;
        float CameraDistance = -12;
        Vector3 CameraFocus = new Vector3(0.0f, 0.0f, 0.0f);
        public RenderContext RenderContext { get; private set; }
        public Gem.Render.FreeCamera Camera { get; private set; }
        public Gem.Render.BranchNode SceneGraph { get; private set; }
        public ISceneNode HoverNode { get; private set; }

        public List<NormalMapMeshNode> HexNodes = new List<NormalMapMeshNode>();
        public List<NormalMapMeshNode> PieceNodes = new List<NormalMapMeshNode>();
        public List<NormalMapMeshNode> GhostPieceNodes = new List<NormalMapMeshNode>();
        public Texture2D[] PieceTextures;

        public int TotalMoves = 0;

        public Board CurrentBoard;
        public bool GameOver = false;

        Gem.Gui.GuiSceneNode Gui;

        private List<InputState> InputStack = new List<InputState>();
        private PlayerType[] PlayerTypes;

        public float ElapsedSeconds { get; private set; }

        public void PushInputState(InputState NextState)
        {
            if (InputStack.Count > 0) InputStack.Last().Covered(this);
            InputStack.Add(NextState);
            NextState.EnterState(this);
        }

        public void PopInputState()
        {
            InputStack.Last().LeaveState(this);
            InputStack.RemoveAt(InputStack.Count - 1);
            if (InputStack.Count > 0) InputStack.Last().Exposed(this);
        }

        public WorldScreen(PlayerType White, PlayerType Black)
        {
            PlayerTypes = new PlayerType[] { White, Black };
        }

        public void Begin()
        {
            Tables.Initialize();
            CurrentBoard = Tables.InitialBoard;

            Content = new EpisodeContentManager(Main.EpisodeContent.ServiceProvider, "Content");

            PieceTextures = new Texture2D[] {
                Content.Load<Texture2D>("blue-piece"),
                Content.Load<Texture2D>("white-piece")
            };
            
            RenderContext = new RenderContext(Content.Load<Effect>("draw"), Main.GraphicsDevice);
            Camera = new FreeCamera(new Vector3(0.0f, -8.0f, 8.0f), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport);
            Camera.Viewport = Main.GraphicsDevice.Viewport;
            Camera.LookAt(Vector3.Zero);
            RenderContext.Camera = Camera;

            SceneGraph = new Gem.Render.BranchNode();

            Input.ClearBindings();
            Input.AddAxis("MAIN", new MouseAxisBinding());
            Main.Input.AddBinding("RIGHT", new KeyboardBinding(Keys.Right, KeyBindingType.Held));
            Main.Input.AddBinding("LEFT", new KeyboardBinding(Keys.Left, KeyBindingType.Held));
            Main.Input.AddBinding("UP", new KeyboardBinding(Keys.Up, KeyBindingType.Held));
            Main.Input.AddBinding("DOWN", new KeyboardBinding(Keys.Down, KeyBindingType.Held));
            Main.Input.AddBinding("CLICK", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            
            Main.Input.AddBinding("CAMERA-DISTANCE-TOGGLE", new KeyboardBinding(Keys.R, KeyBindingType.Held));

            Main.Input.AddBinding("EXIT", new KeyboardBinding(Keys.Escape, KeyBindingType.Pressed));

            var hexMesh = Gem.Geo.Gen.CreateUnitPolygon(6);
            
            hexMesh = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(hexMesh, Matrix.CreateRotationZ((float)System.Math.PI / 2.0f)));
            Gem.Geo.Gen.ProjectTexture(hexMesh, new Vector3(1.0f, 1.0f, 0.0f), Vector3.UnitX * 2, Vector3.UnitY * 2);
            Gem.Geo.Gen.CalculateTangentsAndBiNormals(hexMesh);
            
            for (var x = 0; x < 19; ++x)
            {
                var hexNode = new NormalMapMeshNode()
                {
                    Mesh = hexMesh,
                    Texture = Content.Load<Texture2D>("hex"),
                    NormalMap = Content.Load<Texture2D>("hex-normal"),
                };
                hexNode.Orientation.Position = new Vector3(Tables.HexWorldPositions[x], 0.0f);
                SceneGraph.Add(hexNode);
                HexNodes.Add(hexNode);
            }

            var tetraMesh = Gem.Geo.Gen.CreateUnitPolygon(3);
            tetraMesh.verticies[0].Position = new Vector3(0.0f, 0.0f, 1.0f);
            Gem.Geo.Gen.Transform(tetraMesh, Matrix.CreateRotationZ((float)Math.PI));
            tetraMesh = Gem.Geo.Gen.FacetCopy(tetraMesh);
            for (var i = 0; i < tetraMesh.indicies.Length; i += 3)
            {
                tetraMesh.verticies[tetraMesh.indicies[i]].TextureCoordinate = new Vector2(0.5f, 0);
                tetraMesh.verticies[tetraMesh.indicies[i + 1]].TextureCoordinate = new Vector2(1, 1);
                tetraMesh.verticies[tetraMesh.indicies[i + 2]].TextureCoordinate = new Vector2(0, 1);
            }

            Gem.Geo.Gen.CalculateTangentsAndBiNormals(tetraMesh);

            for (var x = 0; x < 36; ++x)
            {
                var tetraNode = new NormalMapMeshNode()
                {
                    Mesh = tetraMesh,
                    HiliteColor = new Vector3(1,0,0),
                    HiliteMesh = tetraMesh,
                    Hidden = true
                };
                tetraNode.Orientation.Scale = new Vector3(0.4f, 0.4f, 0.4f);

                SceneGraph.Add(tetraNode);
                PieceNodes.Add(tetraNode);
            }

            for (var x = 0; x < 6; ++x)
            {
                var ghostNode = new NormalMapMeshNode()
                {
                    Mesh = tetraMesh,
                    HiliteColor = new Vector3(1, 0, 0),
                    HiliteMesh = tetraMesh,
                    Hidden = true,
                    Alpha = 0.75f,
                    Color = new Vector3(0.8f, 0.8f, 0.0f),
                };
                ghostNode.Orientation.Scale = new Vector3(0.4f, 0.4f, 0.4f);
                SceneGraph.Add(ghostNode);
                GhostPieceNodes.Add(ghostNode);
            }

            var guiQuad = Gem.Geo.Gen.CreateQuad();
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateRotationX(Gem.Math.Angle.PI / 2.0f));
            //Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateRotationY(Gem.Math.Angle.PI));
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateScale(10.0f, 1.0f, 5.0f));
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateTranslation(0.0f, 0.0f, 2.0f));
            Gui = new Gem.Gui.GuiSceneNode(guiQuad, Main.GraphicsDevice, 1024, 512);
            Gui.uiRoot.AddPropertySet(null, new Gem.Gui.GuiProperties
            {
                BackgroundColor = new Vector3(0, 0, 1),
                Transparent = true
            });
            Gui.uiRoot.AddChild(new Gem.Gui.UIItem(
                "TURN-INDICATOR",
                new Gem.Gui.QuadShape(0, 0, 1, 1),
                new Gem.Gui.GuiProperties
                {
                    Font = new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6),
                    TextOrigin = new Vector2(32, 32),
                    TextColor = new Vector3(1, 0, 0),
                    Label = "HELLO COERCEO",
                    FontScale = 4.0f,
                    Transparent = true
                }));
            Gui.uiRoot.AddChild(new Gem.Gui.UIItem(
                "TILE-COUNT",
                new Gem.Gui.QuadShape(0, 0, 1, 1),
                new Gem.Gui.GuiProperties
                {
                    Font = new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6),
                    TextOrigin = new Vector2(32, 56),
                    TextColor = new Vector3(1, 0, 0),
                    Label = "TILE-COUNT",
                    FontScale = 4.0f,
                    Transparent = true
                }));
            Gui.uiRoot.AddChild(new Gem.Gui.UIItem(
                "STATS",
                new Gem.Gui.QuadShape(0, 0, 1, 1),
                new Gem.Gui.GuiProperties
                {
                    Font = new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6),
                    TextOrigin = new Vector2(768, 32),
                    TextColor = new Vector3(1, 0, 0),
                    Label = "STATS",
                    FontScale = 4.0f,
                    Transparent = true
                }));
            Gui.RenderOnTop = true;
            Gui.DistanceBias = float.NegativeInfinity;
            SceneGraph.Add(Gui);

            PushInputState(new Input.TurnScheduler(PlayerTypes));
        }

        public void DisplayBoard(Board Board)
        {
            var moveCount = Coerceo.EnumerateLegalMoves(Board).Count();
            if (moveCount == 0) GameOver = true;

            if (GameOver)
                Gui.Find("TURN-INDICATOR").Properties[0].Values.Upsert("label", "GAME OVER");
            else if (Board.Header.WhoseTurnNext == 0)
                Gui.Find("TURN-INDICATOR").Properties[0].Values.Upsert("label", "White to move");
            else
                Gui.Find("TURN-INDICATOR").Properties[0].Values.Upsert("label", "Black to move");

            Gui.Find("TILE-COUNT").Properties[0].Values.Upsert("label", String.Format("WHITE TILES: {0}\nBLACK TILES: {1}\nWHITE PIECES: {2}\nBLACK PIECES: {3}\nMOVES: {4}",
                CurrentBoard.Tiles.Count(t => t.IsHeldBy(0)),
                CurrentBoard.Tiles.Count(t => t.IsHeldBy(1)),
                CurrentBoard.CountOfPieces(0),
                CurrentBoard.CountOfPieces(1),
                TotalMoves));

            foreach (var piece in PieceNodes) 
                piece.Hidden = true;
            foreach (var ghost in GhostPieceNodes)
                ghost.Hidden = true;

            var nextPiece = 0;

            foreach (var tile in Board.Tiles)
            {
                HexNodes[tile.ID].Hidden = tile.IsOutOfPlay();
                for (byte t = 0; t < 6; ++t)
                    if (tile.GetTriangle(t) == 0x01)
                        PositionPiece(nextPiece++, t % 2, tile.ID, t);
            }
        }

        public void PositionPiece(int PieceID, int Player, byte HexID, byte Triangle)
        {
            ShowPiece(PieceNodes[PieceID], Player, HexID, Triangle);
        }

        public void ShowPiece(NormalMapMeshNode Node, int Player, byte HexID, byte Triangle)
        {
            var angle = Triangle * (Gem.Math.Angle.PI2 / 6.0f);
            Node.Orientation.Orientation.Z = angle;
            Node.Orientation.Position = new Vector3(Tables.HexWorldPositions[HexID], 0.0f) +
                Vector3.Transform(new Vector3(0.0f, 0.56f, 0.0f), Matrix.CreateRotationZ(angle));
            Node.Texture = PieceTextures[Player];
            Node.Hidden = false;
            Node.Tag = new Coordinate(HexID, Triangle);
        }

        public void ShowGhost(int ID, int Player, byte HexID, byte Triangle, Object Tag)
        {
            ShowPiece(GhostPieceNodes[ID], Player, HexID, Triangle);
            GhostPieceNodes[ID].Tag = Tag;
        }

        public void End()
        {
        }

        public void Update(float elapsedSeconds)
        {
            if (Main.Input.Check("EXIT"))
            {
                Main.Game = new MainMenu();
                return;
            }

            ElapsedSeconds = elapsedSeconds;

            if (Main.Input.Check("RIGHT")) Camera.Yaw(elapsedSeconds);
            if (Main.Input.Check("LEFT")) Camera.Yaw(-elapsedSeconds);
            if (Main.Input.Check("UP")) Camera.Pitch(elapsedSeconds);
            if (Main.Input.Check("DOWN")) Camera.Pitch(-elapsedSeconds);

            if (Main.Input.Check("CAMERA-DISTANCE-TOGGLE")) CameraDistance = 16.0f;
            else CameraDistance = 10.0f;
            Camera.Position = CameraFocus - (Camera.GetEyeVector() * CameraDistance);
            Camera.AlignUp(Vector3.UnitZ);
            
            var pitchAngle = Gem.Math.Vector.AngleBetweenVectors(Vector3.UnitZ, Camera.GetEyeVector());
            

            if (Camera.Position.Z < 2.0f) Camera.Position = new Vector3(Camera.Position.X, Camera.Position.Y, 2.0f);
            if (Camera.Position.Z > 9.0f) Camera.Position = new Vector3(Camera.Position.X, Camera.Position.Y, 9.0f);
            Camera.LookAt(CameraFocus);

            //Camera.Distance = CameraDistance;
            //Camera.OrbitDistance = CameraDistance;

            HoverNode = null;

            var pickVector = Camera.Unproject(new Vector3(Main.Input.QueryAxis("MAIN"), 0.0f));
            var pickRay = new Ray(Camera.GetPosition(), pickVector - Camera.GetPosition());
            var hoverItems = new List<HoverItem>();
            SceneGraph.CalculateLocalMouse(pickRay, (node, distance) => hoverItems.Add(new HoverItem { Node = node, Distance = distance }));

            if (hoverItems.Count > 0)
            {
                var nearestDistance = float.PositiveInfinity;
                foreach (var hoverItem in hoverItems)
                    if (hoverItem.Distance < nearestDistance) nearestDistance = hoverItem.Distance;
                HoverNode = hoverItems.First(item => item.Distance <= nearestDistance).Node;
            }

            if (!GameOver && InputStack.Count > 0) InputStack.Last().Update(this);

            var eyeVector = Camera.GetEyeVector();
            Gui.Orientation.Orientation.Z = Gem.Math.Vector.AngleBetweenVectors(Vector2.UnitY, Vector2.Normalize(new Vector2(eyeVector.X, eyeVector.Y)));

            Gui.Find("STATS").Properties[0].Values.Upsert("label", String.Format("AI STATS\nCS:{0}\nD:{1}",
                AIV2.CountOfConfigurationsScored, AIV2.DepthReached));
        }

        private struct HoverItem
        {
            public ISceneNode Node;
            public float Distance;
        }

        public void Draw(float elapsedSeconds)
        {
            var viewport = Main.GraphicsDevice.Viewport;

            SceneGraph.UpdateWorldTransform(Matrix.Identity);
            SceneGraph.PreDraw(elapsedSeconds, RenderContext);
                       
            Main.GraphicsDevice.SetRenderTarget(null);
            Main.GraphicsDevice.Viewport = viewport;
            Main.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Main.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Main.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            RenderContext.Camera = Camera;
            RenderContext.Color = Vector3.One;
            RenderContext.Alpha = 1.0f;
            RenderContext.LightingEnabled = true;
            RenderContext.UVTransform = Matrix.Identity;
            RenderContext.World = Matrix.Identity;
            RenderContext.SetLight(0, new Vector3(0.0f, 10.0f, 4.0f), 20, new Vector3(1, 1, 1));
            RenderContext.SetLight(1, new Vector3(10.0f, 10.0f, 7.5f), 20, new Vector3(1, 1, 1));
            RenderContext.SetLight(2, new Vector3(-10.0f, -10.5f, 3.5f), 20, new Vector3(1, 1, 1));
            RenderContext.ActiveLightCount = 3;
            RenderContext.Texture = RenderContext.White;
            RenderContext.NormalMap = RenderContext.NeutralNormals;
            RenderContext.ApplyChanges();

            Main.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0xFFFFFF, 0);
            SceneGraph.Draw(RenderContext);
            RenderContext.LightingEnabled = true;
            

            RenderContext.World = Matrix.Identity;
            RenderContext.Texture = RenderContext.White;
            
            //World.NavMesh.DebugRender(RenderContext);
            //if (HitFace != null) 
            //    World.NavMesh.DebugRenderFace(RenderContext, HitFace);
        }
    }
}
