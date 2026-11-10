
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

        private float temporizadorDisparos = 0f;
        private bool cooldown = false;

        private float temporizadorCooldown = 0f;

        private int vidas = 3;

        private int disparosCooldown = 0;

        private int direccion = -1; //0 = arriba, 1 = derecha, 2 = abajo, 3 = izquierda -1= no se movio
        private int direccionBalaJugador = -1;
        private int actualizacionTiempo = 120;
        private const int totalFrames = 4;
        private Microsoft.Xna.Framework.Rectangle[] rectangulo = new Microsoft.Xna.Framework.Rectangle[totalFrames];
        private int indiceActualAnimacion = 0;

        private bool enemigoVivo = true;

        private bool balaActiva = false;
        private bool balaActivaJugador = false;
        private float velocidadGlobal = 5.0f;
        private Microsoft.Xna.Framework.Vector2 posicion = new Microsoft.Xna.Framework.Vector2(350, 250);
        private Microsoft.Xna.Framework.Vector2 PosicionEnemigo = new Microsoft.Xna.Framework.Vector2(700, 300);

        private Texture2D _pixel;
        private Microsoft.Xna.Framework.Rectangle hitboxAlien;
        private Microsoft.Xna.Framework.Rectangle hitboxMapa;
        private Microsoft.Xna.Framework.Rectangle hitboxEnemigo;
        private Microsoft.Xna.Framework.Rectangle hitboxBase;

        private Microsoft.Xna.Framework.Rectangle hitboxBalasJugador;

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
            hitboxBase = new Microsoft.Xna.Framework.Rectangle(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2, 50, 50);
            hitboxBalas = new Microsoft.Xna.Framework.Rectangle(hitboxEnemigo.X + 5, hitboxEnemigo.Y + 5, 50, 20);
            hitboxBalasJugador = new Microsoft.Xna.Framework.Rectangle(hitboxAlien.X + 15, hitboxAlien.Y + 15, 20, 20);

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
                dispararJugador();

                if (hitboxAlien.Intersects(hitboxMapa)) posicion.Y++;


                //Siempre se mueve hacia el jugador (dsp lo cambiamos a la posicion de la base) pero al 70% de velocidad,mas despacito
                //if ((int)posicion.X < (int)PosicionEnemigo.X) PosicionEnemigo.X -= (velocidadGlobal * 0.7f);
                //if ((int)posicion.Y < (int)PosicionEnemigo.Y) PosicionEnemigo.Y -= (velocidadGlobal * 0.7f);
                //if ((int)posicion.X > (int)PosicionEnemigo.X) PosicionEnemigo.X += (velocidadGlobal * 0.7f);
                //if ((int)posicion.Y > (int)PosicionEnemigo.Y) PosicionEnemigo.Y += (velocidadGlobal * 0.7f);

                hitboxEnemigo.X = (int)PosicionEnemigo.X; //cambiar si hace falta como la del alien
                hitboxEnemigo.Y = (int)PosicionEnemigo.Y;

                if (enemigoVivo == true) dispararEnemigo();

            }

            temporizador += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (temporizador >= actualizacionTiempo)
            {
                indiceActualAnimacion = (indiceActualAnimacion + 1) % totalFrames;
                temporizador = 0;
            }

            if (cooldown == true) //Si esta en cooldown,espera 2 segundos
            {
                temporizadorCooldown += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (temporizadorCooldown >= 3000)
                {
                    cooldown = false;
                    temporizadorCooldown = 0f;
                    temporizadorDisparos = 0f;
                    disparosCooldown = 0;
                }
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
            if (enemigoVivo == true) _spriteBatch.Draw(_pixel, hitboxEnemigo, Microsoft.Xna.Framework.Color.Blue * 0.7f); //hitbox de enemigo
            _spriteBatch.Draw(_pixel, hitboxBase, Microsoft.Xna.Framework.Color.Pink * 0.7f); //hitbox de base

            if (balaActiva == true && enemigoVivo == true) _spriteBatch.Draw(_pixel, hitboxBalas, Microsoft.Xna.Framework.Color.Green * 0.7f); //hitbox de balas
            _spriteBatch.Draw(_pixel, hitboxBalasJugador, Microsoft.Xna.Framework.Color.Orange * 0.7f); //hitbox de balas jugador

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void movimientoTeclado()
        {
            KeyboardState teclaPresionada = Keyboard.GetState();



            if (teclaPresionada.IsKeyDown(Keys.W) || teclaPresionada.IsKeyDown(Keys.Up))
            {
                posicion.Y -= velocidadGlobal;
                direccion = 0;
            }

            if (teclaPresionada.IsKeyDown(Keys.S) || teclaPresionada.IsKeyDown(Keys.Down))
            {
                posicion.Y += velocidadGlobal;
                direccion = 2;
            }

            if (teclaPresionada.IsKeyDown(Keys.A) || teclaPresionada.IsKeyDown(Keys.Left))
            {
                posicion.X -= velocidadGlobal;
                direccion = 3;
            }

            if (teclaPresionada.IsKeyDown(Keys.D) || teclaPresionada.IsKeyDown(Keys.Right))
            {
                posicion.X += velocidadGlobal;
                direccion = 1;
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

        void dispararEnemigo()
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
                hitboxBalas.X = hitboxEnemigo.X + 5;
                hitboxBalas.Y = hitboxEnemigo.Y + 5;
            }


        }

        void dispararJugador() //Al presionar espacio,dispara desde la hitboxAlien hacia la direccion de movimiento,matando a hitboxEnemigo
        {
            if (balaActivaJugador == false)
            {
                hitboxBalasJugador.X = hitboxAlien.X + 15;
                hitboxBalasJugador.Y = hitboxAlien.Y + 15;

                if (Keyboard.GetState().IsKeyDown(Keys.Space) && direccion != -1 && cooldown == false) //Lo puse aca y la f(X) en el update para que no haga falta mantener espacio
                {
                    balaActivaJugador = true;
                    direccionBalaJugador = direccion; //Guarda la direccion en la que se disparo para que no se pueda cambiar

                    if (disparosCooldown == 0)
                    {
                        temporizadorDisparos = 0f;
                    }

                    disparosCooldown++;

                    if (disparosCooldown >= 3)
                    {
                        if (temporizadorDisparos <= 4000)
                        {
                            cooldown = true;
                            temporizadorCooldown = 0f;
                        }

                        disparosCooldown = 0;
                        temporizadorDisparos = 0f;
                    }
                }
            }
            else
            {


                if (direccionBalaJugador == 0) hitboxBalasJugador.Y -= (int)velocidadGlobal + 5;

                if (direccionBalaJugador == 1) hitboxBalasJugador.X += (int)velocidadGlobal + 5;

                if (direccionBalaJugador == 2) hitboxBalasJugador.Y += (int)velocidadGlobal + 5;

                if (direccionBalaJugador == 3) hitboxBalasJugador.X -= (int)velocidadGlobal + 5;

                if (enemigoVivo && hitboxBalasJugador.Intersects(hitboxEnemigo))
                {
                    enemigoVivo = false;
                    balaActivaJugador = false;
                }

                if (hitboxBalasJugador.X < 0 || hitboxBalasJugador.Y < 0 || hitboxBalasJugador.X > GraphicsDevice.Viewport.Width || hitboxBalasJugador.Y > GraphicsDevice.Viewport.Height)
                //Si se sale de la pantalla,desaparece
                {
                    balaActivaJugador = false;
                }

                if (balaActivaJugador == false) //Mientras no dispares,sigue al jugador y no elige direccion
                {
                    hitboxBalasJugador.X = hitboxAlien.X + 15;
                    hitboxBalasJugador.Y = hitboxAlien.Y + 15;
                    direccionBalaJugador = -1;
                }
            }
        }
    }

}
