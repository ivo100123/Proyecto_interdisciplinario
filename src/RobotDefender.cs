using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;

namespace Logica_de_colisiones
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D Gummy;
        private Texture2D moon;

        private float temporizador = 0f;
        private int actualizacionTiempo = 120;
        private const int totalFrames = 4;
        private Microsoft.Xna.Framework.Rectangle[] rectangulo = new Microsoft.Xna.Framework.Rectangle[totalFrames];
        private int indiceActualAnimacion = 0;

        private float velocidadGlobal = 5.0f;
        private Microsoft.Xna.Framework.Vector2 posicion = new Microsoft.Xna.Framework.Vector2(350, 250);

        private Texture2D _pixel;
        private Microsoft.Xna.Framework.Rectangle hitboxAlien;
        private Microsoft.Xna.Framework.Rectangle hitboxMapa;

        private SoundEffect Laser1;
        private SoundEffect Laser2;
        private SoundEffect Walk;
        private Song BG;
       
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            hitboxAlien = new Microsoft.Xna.Framework.Rectangle((int)posicion.X, (int)posicion.Y, 32 * 2, 32 * 2);
            hitboxMapa = new Microsoft.Xna.Framework.Rectangle(0, 0, 1500, 200);

            int frameWidth = 32;
            int frameHeight = 32;
            int offsetX = 32;
            int offsetY = 32;

            for (int i = 0; i < totalFrames; i++)
            {
                rectangulo[i] = new Microsoft.Xna.Framework.Rectangle(offsetX + (i * frameWidth), offsetY, frameWidth, frameHeight);
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Gummy = Content.Load<Texture2D>("sprites/GumBotsprites");
            moon = Content.Load<Texture2D>("sprites/moon");

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });

            Laser1 = Content.Load<SoundEffect>("Sounds/Snare Drum 3");
            Laser2 = Content.Load<SoundEffect>("Sounds/Snare Drum 2");
            Walk = Content.Load<SoundEffect>("Sounds/BubblePop");
            BG = Content.Load<Song>("Sounds/BG");

            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(BG);

            MediaPlayer.IsRepeating = true;

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            movimientoTeclado();

            hitboxAlien.X = (int)posicion.X + 15;
            hitboxAlien.Y = (int)posicion.Y + 35;

            temporizador += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (temporizador >= actualizacionTiempo)
            {
                indiceActualAnimacion = (indiceActualAnimacion + 1) % totalFrames;
                temporizador = 0;
            }

            if (hitboxAlien.Intersects(hitboxMapa))
            {
                Console.WriteLine("Interseccion :)");
                posicion.Y++;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(
                moon,
                new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Microsoft.Xna.Framework.Color.White
            );

            int safeIndex = Math.Clamp(indiceActualAnimacion, 0, totalFrames - 1);
            Microsoft.Xna.Framework.Rectangle sourceRectangle = rectangulo[safeIndex];

            _spriteBatch.Draw(
                Gummy,
                destinationRectangle: new Microsoft.Xna.Framework.Rectangle((int)posicion.X, (int)posicion.Y, 32 * 3, 32 * 3),
                sourceRectangle: sourceRectangle,
                color: Microsoft.Xna.Framework.Color.White
            );

            _spriteBatch.Draw(_pixel, hitboxAlien, Microsoft.Xna.Framework.Color.Red * 0.4f);
            _spriteBatch.Draw(_pixel, hitboxMapa, Microsoft.Xna.Framework.Color.Yellow * 0.7f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void movimientoTeclado()
        {
            KeyboardState teclaPresionada = Keyboard.GetState();

            if (teclaPresionada.IsKeyDown(Keys.W) || teclaPresionada.IsKeyDown(Keys.Up))
            {
                posicion.Y -= velocidadGlobal;
            }

            if (teclaPresionada.IsKeyDown(Keys.S) || teclaPresionada.IsKeyDown(Keys.Down))
            {
                posicion.Y += velocidadGlobal;
            }

            if (teclaPresionada.IsKeyDown(Keys.A) || teclaPresionada.IsKeyDown(Keys.Left))
            {
                posicion.X -= velocidadGlobal;
            }

            if (teclaPresionada.IsKeyDown(Keys.D) || teclaPresionada.IsKeyDown(Keys.Right))
            {
                posicion.X += velocidadGlobal;
            }
        }
    }
}
