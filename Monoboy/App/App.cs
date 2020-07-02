using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monoboy.Emulator;
using Monoboy.Properties;
using Monogame.UI;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Myra.Graphics2D.UI.Styles;
using XNAssets;

namespace Monoboy
{
    public class App : Game
    {
        readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        MonoboyEmulator emulator;

        static readonly int scale = 3;

        KeyboardState newKeyboard;
        KeyboardState oldKeyboard;

        bool stepping = false;

        SpriteFont font;
        Desktop desktop;
        GUI gui;
        bool guiOn = true;

        Texture2D screen;

        public App()
        {
            graphics = new GraphicsDeviceManager(this);
            ResourceContentManager resxContent;
            resxContent = new ResourceContentManager(Services, Resources.ResourceManager);
            Content = resxContent;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = scale * 160;
            graphics.PreferredBackBufferHeight = (scale * 144) + 14;
            graphics.ApplyChanges();

            emulator = new Emulator.MonoboyEmulator();
            //emulator.SkipBootRom();
            emulator.LoadRom("Roms/Tetris.gb");

            screen = new Texture2D(GraphicsDevice, 160, 144);

            emulator.LinkTexture(screen);

            while(emulator.CurrentAddress != 0x0086)
            {
                emulator.Step();
            }

            stepping = true;

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");

            MyraEnvironment.Game = this;

            var assetManager = new AssetManager(GraphicsDevice, new ResourceAssetResolver(typeof(App).Assembly, "Resources"));

            Stylesheet.Current.HorizontalMenuStyle.LabelStyle.Font = font;
            Stylesheet.Current.HorizontalMenuStyle.SpecialCharColor = Color.SlateGray;

            gui = new GUI();

            gui.FileOpen.Selected += (a, b) => OpenRom();
            gui.FileQuit.Selected += (a, b) => Exit();
            gui.ViewToolbarToggle.Selected += (a, b) => ToggleGUI();

            desktop = new Desktop
            {
                Root = gui
            };

            base.LoadContent();
        }

        private void ToggleGUI()
        {
            guiOn = !guiOn;
        }

        private void OpenRom()
        {
            FileDialog fileDialog = new FileDialog(FileDialogMode.OpenFile);
            //fileDialog.Height = ;
            //fileDialog.Width = desktop.Width;
            gui.AddChild(fileDialog);
            Debug.WriteLine(fileDialog.FilePath);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            desktop.UpdateSystems();
            desktop.UpdateInput();

            newKeyboard = Keyboard.GetState();

            if(stepping == false)
            {
                for(int i = 0; i < 69905; i++)
                {
                    emulator.Step();
                }
            }

            if(newKeyboard.IsKeyDown(Keys.S) == true && oldKeyboard.IsKeyDown(Keys.S) == false)
            {
                stepping = !stepping;
            }

            if(newKeyboard.IsKeyDown(Keys.Space) == true && oldKeyboard.IsKeyDown(Keys.Space) == false)
            {
                emulator.Step();
            }

            if(newKeyboard.IsKeyDown(Keys.V) == true)
            {
                emulator.Step();
            }

            if(newKeyboard.IsKeyDown(Keys.D) == true && oldKeyboard.IsKeyDown(Keys.D) == false)
            {
                emulator.Dump();
            }

            if(newKeyboard.IsKeyDown(Keys.J) == true && oldKeyboard.IsKeyDown(Keys.J) == false)
            {
                while(emulator.register.A != 0x8F)
                {
                    emulator.Step();
                }
            }

            if(newKeyboard.IsKeyDown(Keys.F) == true && oldKeyboard.IsKeyDown(Keys.F) == false)
            {
                emulator.DumpAsImage(GraphicsDevice);
            }

            //UI Toggle
            if(newKeyboard.IsKeyDown(Keys.LeftControl) == true && newKeyboard.IsKeyDown(Keys.T) == true && oldKeyboard.IsKeyDown(Keys.T) == false)
            {
                ToggleGUI();
            }

            oldKeyboard = newKeyboard;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            {
                spriteBatch.Draw(screen, Vector2.Zero, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

                string[] lines = emulator.DebugInfo();
                for(int i = 0; i < lines.Length; i++)
                {
                    spriteBatch.DrawString(font, lines[i], new Vector2(6, 16 + (6 * (i + 1)) + (16 * (i))), Color.Black);
                }
            }
            spriteBatch.End();

            if(guiOn == true)
                desktop.Render();

            base.Draw(gameTime);
        }
    }
}
