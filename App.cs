using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monoboy.Emulator.Utility;

namespace Monoboy
{
    public class App : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Emulator.Monoboy emulator;

        static readonly int scale = 3;

        KeyboardState newKeyboard;
        KeyboardState oldKeyboard;

        SpriteFont font;

        public App()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = scale * 160;
            graphics.PreferredBackBufferHeight = scale * 144;
            graphics.ApplyChanges();

            emulator = new Emulator.Monoboy();

            string romPath = @"Roms\Dr. Mario.gb";
            Console.Write(Path.GetFileNameWithoutExtension(romPath));

            emulator.cpu.memory.LoadRom(romPath);
            Window.Title += " - " + Path.GetFileNameWithoutExtension(romPath);

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/Arial");
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            newKeyboard = Keyboard.GetState();

            if (newKeyboard.IsKeyDown(Keys.Space) == true && oldKeyboard.IsKeyDown(Keys.Space) == false)
            {
                emulator.cpu.Step();
            }

            if (newKeyboard.IsKeyDown(Keys.V) == true)
            {
                emulator.cpu.Step();
            }

            oldKeyboard = newKeyboard;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            {
                spriteBatch.DrawString(font, "A: " + emulator.cpu.register.A.ToHex(), new Vector2(0, (6 * 0) + (16 * 0)), Color.Black);
                spriteBatch.DrawString(font, "B: " + emulator.cpu.register.B.ToHex(), new Vector2(0, (6 * 1) + (16 * 1)), Color.Black);
                spriteBatch.DrawString(font, "C: " + emulator.cpu.register.C.ToHex(), new Vector2(0, (6 * 2) + (16 * 2)), Color.Black);
                spriteBatch.DrawString(font, "D: " + emulator.cpu.register.D.ToHex(), new Vector2(0, (6 * 3) + (16 * 3)), Color.Black);
                spriteBatch.DrawString(font, "E: " + emulator.cpu.register.E.ToHex(), new Vector2(0, (6 * 4) + (16 * 4)), Color.Black);
                spriteBatch.DrawString(font, "F: " + emulator.cpu.register.F.ToBin(), new Vector2(0, (6 * 5) + (16 * 5)), Color.Black);
                spriteBatch.DrawString(font, "H: " + emulator.cpu.register.H.ToHex(), new Vector2(0, (6 * 6) + (16 * 6)), Color.Black);
                spriteBatch.DrawString(font, "L: " + emulator.cpu.register.L.ToHex(), new Vector2(0, (6 * 7) + (16 * 7)), Color.Black);
                spriteBatch.DrawString(font, "Stack Pointer: " + emulator.cpu.register.SP.ToHex(), new Vector2(0, (6 * 8) + (16 * 8)), Color.Black);
                spriteBatch.DrawString(font, "Program Counter: " + emulator.cpu.register.PC.ToHex() + " : " + emulator.cpu.register.PC, new Vector2(0, (6 * 9) + (16 * 9)), Color.Black);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
