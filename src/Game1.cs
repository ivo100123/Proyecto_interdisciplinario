using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Drawing;
using System.Reflection.Metadata;



namespace robot_defender
{



    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        //  private Texture2D enemigo;

        //   private Texture2D Gummy;
        //  private Texture2D moon;

        //private Texture2d Base;

        private ConexionBD basededatos = new ConexionBD();
        


        private float temporizador = 0f;
        private int vidas = 3;
        private int actualizacionTiempo = 120;
        private const int totalFrames = 4;
        private Microsoft.Xna.Framework.Rectangle[] rectangulo = new Microsoft.Xna.Framework.Rectangle[totalFrames];
        private int indiceActualAnimacion = 0;

        private bool balaActiva = false;

        private float velocidadGlobal = 5.0f;
        private Microsoft.Xna.Framework.Vector2 posicion = new Microsoft.Xna.Framework.Vector2(350, 250);
        private Microsoft.Xna.Framework.Vector2 PosicionEnemigo = new Microsoft.Xna.Framework.Vector2(700, 300);

        private Texture2D _pixel;
        private Microsoft.Xna.Framework.Rectangle hitboxAlien;
        private Microsoft.Xna.Framework.Rectangle hitboxMapa;
        private Microsoft.Xna.Framework.Rectangle hitboxEnemigo;
        private Microsoft.Xna.Framework.Rectangle hitboxBase;

        private Microsoft.Xna.Framework.Rectangle hitboxBalas;


        // private SoundEffect Laser1;
        //  private SoundEffect Laser2;
        //private SoundEffect Walk;
        // private Song BG;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();


            basededatos.RegistrarJugador(1, "Jugador1", 4000);

            hitboxAlien = new Microsoft.Xna.Framework.Rectangle((int)posicion.X, (int)posicion.Y, 32 * 2, 32 * 2);
            hitboxMapa = new Microsoft.Xna.Framework.Rectangle(0, 0, 1500, 200);
            hitboxEnemigo = new Microsoft.Xna.Framework.Rectangle((int)PosicionEnemigo.X, (int)PosicionEnemigo.Y, 40, 40); //Cambiar como haga falta
            hitboxBase = new Microsoft.Xna.Framework.Rectangle(300, 300, 50, 50);
            hitboxBalas = new Microsoft.Xna.Framework.Rectangle(hitboxEnemigo.X, hitboxEnemigo.Y, 50, 20);


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

            // Gummy = Content.Load<Texture2D>("Descargas/Gum Bot sprites"); //NO ME ANDA EL CONTENTTTA AAAAAA
            // moon = Content.Load<Texture2D>("Descargas/moon");
            // enemigo = Content.Load<Texture2D>("Descargas/enemigo");

            _pixel = new Texture2D(GraphicsDevice, 1, 1); //Para el color de las hitboxes
            _pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });



            /*if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(BG);

            MediaPlayer.IsRepeating = true;*/

        }

        protected override void Update(GameTime gameTime)
        {

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();



            if (vidas == 0) muere(); //Abajo de todo esta la funcion

            if (vidas > 0) //todo se apaga cuando se muere
            {
                hitboxAlien.X = (int)posicion.X + 15;
                hitboxAlien.Y = (int)posicion.Y + 35;

                movimientoTeclado();

                if (hitboxAlien.Intersects(hitboxMapa)) posicion.Y++;


                //Siempre se mueve hacia el jugador (dsp lo cambiamos a la posicion de la base) pero al 70% de velocidad,mas despacito
                //if ((int)posicion.X < (int)PosicionEnemigo.X) PosicionEnemigo.X -= (velocidadGlobal * 0.7f);
                //if ((int)posicion.Y < (int)PosicionEnemigo.Y) PosicionEnemigo.Y -= (velocidadGlobal * 0.7f);
                //if ((int)posicion.X > (int)PosicionEnemigo.X) PosicionEnemigo.X += (velocidadGlobal * 0.7f);
                //if ((int)posicion.Y > (int)PosicionEnemigo.Y) PosicionEnemigo.Y += (velocidadGlobal * 0.7f);

                hitboxEnemigo.X = (int)PosicionEnemigo.X; //cambiar si hace falta como la del alien
                hitboxEnemigo.Y = (int)PosicionEnemigo.Y;

                disparar();

            }

            temporizador += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (temporizador >= actualizacionTiempo)
            {
                indiceActualAnimacion = (indiceActualAnimacion + 1) % totalFrames;
                temporizador = 0;
            }





            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            _spriteBatch.Begin();

            /*_spriteBatch.Draw(
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

            _spriteBatch.Draw(enemigo, destinationRectangle: new Microsoft.Xna.Framework.Rectangle((int)PosicionEnemigo.X, (int)PosicionEnemigo.Y, 32 * 3, 32 * 3),
                sourceRectangle: sourceRectangle,
                color: Microsoft.Xna.Framework.Color.White);*/


            _spriteBatch.Draw(_pixel, hitboxAlien, Microsoft.Xna.Framework.Color.Red * 0.4f);
            _spriteBatch.Draw(_pixel, hitboxMapa, Microsoft.Xna.Framework.Color.Yellow * 0.7f);
            _spriteBatch.Draw(_pixel, hitboxEnemigo, Microsoft.Xna.Framework.Color.Blue * 0.7f); //hitbox de enemigo
            _spriteBatch.Draw(_pixel, hitboxBase, Microsoft.Xna.Framework.Color.Pink * 0.7f); //hitbox de base

            if (balaActiva == true) _spriteBatch.Draw(_pixel, hitboxBalas, Microsoft.Xna.Framework.Color.Green * 0.7f); //hitbox de balas


            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void movimientoTeclado()
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

        private void muere()
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            //Mensaje de has muerto y reintentar. En teoria deberia resetearse solo ahora
            posicion.X = 350;
            posicion.Y = 250;
            PosicionEnemigo.X = 700;
            PosicionEnemigo.Y = 300;
            vidas = 3;

        }

        void disparar()
        {
            if (balaActiva == false)
            {
                hitboxBalas.X = hitboxEnemigo.X;
                hitboxBalas.Y = hitboxEnemigo.Y;
                balaActiva = true;
            }

            Vector2 destino = new Vector2(hitboxBase.X, hitboxBase.Y);


            if (destino.X < hitboxBalas.X) hitboxBalas.X -= (int)velocidadGlobal + 1; //+1 para que vaya más rapido
            if (destino.Y < hitboxBalas.Y) hitboxBalas.Y -= (int)velocidadGlobal + 1;
            if (destino.X > hitboxBalas.X) hitboxBalas.X += (int)velocidadGlobal + 1;
            if (destino.Y > hitboxBalas.Y) hitboxBalas.Y += (int)velocidadGlobal + 1;

            if (hitboxBalas.Intersects(hitboxBase))
            {
                vidas--;
                balaActiva = false;
                hitboxBalas.X = hitboxEnemigo.X;
                hitboxBalas.Y = hitboxEnemigo.Y;
            }


        }
    }

}
