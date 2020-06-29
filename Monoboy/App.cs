using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monoboy.Emulator.Utility;

namespace Monoboy
{
    public class App : Game
    {
        readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        Emulator.MonoboyEmulator emulator;

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

            emulator = new Emulator.MonoboyEmulator();

            string romPath = @"Roms\cpu test.gb";

            Debug.Log(Path.GetFileNameWithoutExtension(romPath), ConsoleColor.DarkBlue);

            emulator.LoadRom(romPath);
            Window.Title += " - " + Path.GetFileNameWithoutExtension(romPath);

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts/Font");
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            newKeyboard = Keyboard.GetState();

            if(newKeyboard.IsKeyDown(Keys.Space) == true && oldKeyboard.IsKeyDown(Keys.Space) == false)
            {
                Debug.Enable();
                emulator.Step();
            }



            if(newKeyboard.IsKeyDown(Keys.V) == true)
            {
                Debug.DisableNext();
                emulator.Step();
            }

            if(newKeyboard.IsKeyDown(Keys.D) == true && oldKeyboard.IsKeyDown(Keys.D) == false)
            {
                emulator.Dump();
            }

            if(newKeyboard.IsKeyDown(Keys.S) == true && oldKeyboard.IsKeyDown(Keys.S) == false)
            {
                for(int i = 0; i < 1000000; i++)
                {
                    Debug.Disable();
                    emulator.Step();
                }
            }

            if(newKeyboard.IsKeyDown(Keys.C) == true && oldKeyboard.IsKeyDown(Keys.C) == false)
            {
                Console.Clear();
            }

            oldKeyboard = newKeyboard;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            List<string> lines = new List<string>();
            GraphicsDevice.Clear(Color.White);


            lines.Add("F: ZNHF");
            lines.Add("F: " + (emulator.register.GetFlag(Emulator.Flag.Zero) ? 1 : 0).ToString() + (emulator.register.GetFlag(Emulator.Flag.Negative) ? 1 : 0).ToString() + (emulator.register.GetFlag(Emulator.Flag.HalfCarry) ? 1 : 0).ToString() + (emulator.register.GetFlag(Emulator.Flag.FullCarry) ? 1 : 0).ToString());
            lines.Add("AF: " + emulator.register.AF.ToHex());
            lines.Add("BC: " + emulator.register.BC.ToHex());
            lines.Add("DE: " + emulator.register.DE.ToHex());
            lines.Add("HL: " + emulator.register.HL.ToHex());
            lines.Add("SP: " + emulator.register.SP.ToHex());
            lines.Add("PC: " + emulator.register.PC.ToHex() + " = " + emulator.CurrentOpcode);
            lines.Add("Step: " + emulator.steps);

            spriteBatch.Begin();
            {
                for(int i = 0; i < lines.Count; i++)
                {
                    spriteBatch.DrawString(font, lines[i], new Vector2(6, (6 * (i + 1)) + (16 * i)), Color.Black);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
