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
    public class MainMenu : IScreen
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
                
        Gem.Gui.GuiSceneNode Gui;

        public void Begin()
        {
            Content = new EpisodeContentManager(Main.EpisodeContent.ServiceProvider, "Content");

            RenderContext = new RenderContext(Content.Load<Effect>("draw"), Main.GraphicsDevice);
            Camera = new FreeCamera(new Vector3(0.0f, -8.0f, 8.0f), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport);
            Camera.Viewport = Main.GraphicsDevice.Viewport;
            Camera.LookAt(Vector3.Zero);
            RenderContext.Camera = Camera;

            SceneGraph = new Gem.Render.BranchNode();

            Main.Input.ClearBindings();
            Input.AddAxis("MAIN", new MouseAxisBinding());
            Main.Input.AddBinding("CLICK", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            Main.Input.AddBinding("EXIT", new KeyboardBinding(Keys.Escape, KeyBindingType.Pressed));

            var guiQuad = Gem.Geo.Gen.CreateQuad();
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateRotationX(Gem.Math.Angle.PI / 2.0f));
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateScale(10.0f, 1.0f, 5.0f));
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateTranslation(0.0f, 0.0f, 2.0f));
            Gui = new Gem.Gui.GuiSceneNode(guiQuad, Main.GraphicsDevice, 1024, 512);
            Gui.uiRoot.AddPropertySet(null, new Gem.Gui.GuiProperties
            {
                BackgroundColor = new Vector3(0, 0, 1),
                Transparent = true
            });

            var button1 = new Gem.Gui.UIItem(
                "BUTTON",
                new Gem.Gui.QuadShape(32, 32, 512, 32),
                new Gem.Gui.GuiProperties
                {
                    BackgroundColor = new Vector3(0.9f, 0.4f, 0.4f),
                    Font = new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6),
                    TextOrigin = new Vector2(32, 32),
                    TextColor = new Vector3(0, 0, 0),
                    Label = "PLAYER VS PLAYER",
                    FontScale = 4.0f,
                    Transparent = false,
                    ClickAction = () => { Main.Game = new WorldScreen(PlayerType.Player, PlayerType.Player); }
                });
            button1.AddPropertySet(i => i.Hover, new Gem.Gui.GuiProperties { BackgroundColor = new Vector3(1, 0, 0) });
            Gui.uiRoot.AddChild(button1);

            var button2 = new Gem.Gui.UIItem(
                "BUTTON",
                new Gem.Gui.QuadShape(32, 32 + 32 + 16, 512, 32),
                new Gem.Gui.GuiProperties
                {
                    BackgroundColor = new Vector3(0.9f, 0.4f, 0.4f),
                    Font = new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6),
                    TextOrigin = new Vector2(32, 32 + 32 + 16),
                    TextColor = new Vector3(0, 0, 0),
                    Label = "PLAYER VS AI",
                    FontScale = 4.0f,
                    Transparent = false,
                    ClickAction = () => { Main.Game = new WorldScreen(PlayerType.Player, PlayerType.AI); }
                });
            button2.AddPropertySet(i => i.Hover, new Gem.Gui.GuiProperties { BackgroundColor = new Vector3(1, 0, 0) });
            Gui.uiRoot.AddChild(button2);

            var button3 = new Gem.Gui.UIItem(
                "BUTTON",
                new Gem.Gui.QuadShape(32, 128, 512, 32),
                new Gem.Gui.GuiProperties
                {
                    BackgroundColor = new Vector3(0.9f, 0.4f, 0.4f),
                    Font = new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6),
                    TextOrigin = new Vector2(32, 128),
                    TextColor = new Vector3(0, 0, 0),
                    Label = "AI VS AI",
                    FontScale = 4.0f,
                    Transparent = false,
                    ClickAction = () => { Main.Game = new WorldScreen(PlayerType.AI, PlayerType.AI); }
                });
            button3.AddPropertySet(i => i.Hover, new Gem.Gui.GuiProperties { BackgroundColor = new Vector3(1, 0, 0) });
            Gui.uiRoot.AddChild(button3);

            Gui.RenderOnTop = true;
            Gui.DistanceBias = float.NegativeInfinity;
            SceneGraph.Add(Gui);
        }

        public void Update(float elapsedSeconds)
        {
            if (Main.Input.Check("EXIT")) Main.Exit();

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

            if (HoverNode != null) HoverNode.GetClickAction();

            var eyeVector = Camera.GetEyeVector();
            Gui.Orientation.Orientation.Z = Gem.Math.Vector.AngleBetweenVectors(Vector2.UnitY, Vector2.Normalize(new Vector2(eyeVector.X, eyeVector.Y)));

            if (Main.Input.Check("CLICK") && HoverNode != null && HoverNode.GetClickAction() != null)
                HoverNode.GetClickAction()();
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


        public void End()
        {
        }
    }
}
