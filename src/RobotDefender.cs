using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;



namespace robot_defender
{



    public class Game1 : Game
    {

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D Gummy;
          private Texture2D moon;

        private Texture2D Base;


        private enum EstadoJuego { Menu, Jugando, Opciones, Pausa }
        private EstadoJuego estadoActual = EstadoJuego.Menu;

        private enum PuntoSpawn { Arriba, Derecha, Izquierda, Abajo }

        private struct Enemigo //struct de enemigos para poder copiarlos
        {
            public Vector2 Posicion;
            public PuntoSpawn Spawn;
            public float TemporizadorDisparo; //Cuanto tarda en disparar

            public Enemigo(Vector2 posicion, PuntoSpawn spawn)
            {
                Posicion = posicion;
                Spawn = spawn;
                TemporizadorDisparo = 0f;
            }
        }

        private struct Bala
        {
            public Vector2 Posicion;
            public Vector2 Direccion;
            public bool EsJugador; //Quien disparó la bala (jugador o enemigo)

            public Bala(Vector2 posicion, Vector2 direccion, bool esJugador)
            {
                Posicion = posicion;
                Direccion = direccion;
                EsJugador = esJugador;
            }
        }

        private const int TamañoEnemigo = 40;
        private const int TamañoBalaJugador = 20;
        private const int TamañoBalaEnemigo = 12;
        private const float DistanciaAtaqueBase = 250f; //Cuanto se tienen que acercar para poder disparar

        private const float DistanciaSpawnBase = 400f; //Que tan lejos aparecen de la base
        private const float IntervaloDisparoEnemigo = 1500f; //Cada cuanto disparan
        private const int EnemigosActivosIniciales = 4; //Los primeros 4
        private const int ReservasPorOleada = 8; //Cantidad que enemigos que reaparecen

        private readonly PuntoSpawn[] puntosSpawn =
        {
            PuntoSpawn.Arriba,
            PuntoSpawn.Derecha,
            PuntoSpawn.Izquierda,
            PuntoSpawn.Abajo
        };

        private readonly List<Enemigo> enemigos = new();
        private readonly List<Bala> balas = new();
        private readonly List<PuntoSpawn> puntosSpawnPendientes = new(); //Donde murio uno para reemplazarlo

        // Menú Principal
        private Texture2D menuBackground;
        private Texture2D botonStartTexture;
        private Microsoft.Xna.Framework.Rectangle botonStartRect;
        private Texture2D botonOpcionesTexture;
        private Microsoft.Xna.Framework.Rectangle botonOpcionesRect;
        // Opciones
        private Texture2D botonVolverTexture;
        private Microsoft.Xna.Framework.Rectangle botonVolverRect;
        private Texture2D botonMusicaTexture; // Nuevo botón para opciones de música
        private Microsoft.Xna.Framework.Rectangle botonMusicaRect;
        // Pausa
        private Texture2D pauseBoxTexture;
        private Microsoft.Xna.Framework.Rectangle pauseBoxRect;
        private Texture2D botonReanudarTexture;
        private Microsoft.Xna.Framework.Rectangle botonReanudarRect;
        private Texture2D botonSalirMenuTexture;
        private Microsoft.Xna.Framework.Rectangle botonSalirMenuRect;


        private ConexionBD basededatos = new ConexionBD();



        private float temporizador = 0f;

        private float temporizadorDisparos = 0f; //Para ver cuantas veces disparo el jugador
        private bool cooldown = false;

        private float temporizadorCooldown = 0f;

        private int vidas = 5; //cantidad de vidas de la base 

        private int disparosCooldown = 0; //timer del cooldown

        private int direccion = -1; //0 = arriba, 1 = derecha, 2 = abajo, 3 = izquierda -1= no se movio
        private int actualizacionTiempo = 120;
        private const int totalFrames = 4;
        private Microsoft.Xna.Framework.Rectangle[] rectangulo = new Microsoft.Xna.Framework.Rectangle[totalFrames];
        private int indiceActualAnimacion = 0;

        private float velocidadGlobal = 5.0f;
        private Microsoft.Xna.Framework.Vector2 posicion = new Microsoft.Xna.Framework.Vector2(350, 250);

        private Texture2D _pixel; //Para dibujar las hitboxes
        private Microsoft.Xna.Framework.Rectangle hitboxAlien;
        private Microsoft.Xna.Framework.Rectangle hitboxMapa;
        private Microsoft.Xna.Framework.Rectangle hitboxBase;


         private SoundEffect Laser1;
          private SoundEffect Laser2;
        private SoundEffect Walk;
        private Song menuSong;
        private Song gameSong;
        private bool musicaActiva = true; // Control para el botón de música


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();


            basededatos.RegistrarJugador(1, "Jugador1", 4000);

            hitboxAlien = new Microsoft.Xna.Framework.Rectangle((int)posicion.X, (int)posicion.Y, 32 * 2, 32 * 2);
            hitboxMapa = new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.Viewport.Width, 200);
            hitboxBase = new Microsoft.Xna.Framework.Rectangle((GraphicsDevice.Viewport.Width / 2) - 100 , (GraphicsDevice.Viewport.Height / 2) - 100 , 200, 250);
            IniciarOleada();

            // Posiciones de botones del Menú
            botonStartRect = new Microsoft.Xna.Framework.Rectangle(50, 450, 200, 60);
            botonOpcionesRect = new Microsoft.Xna.Framework.Rectangle(550, 450, 200, 60);

            // Posiciones de botones de Opciones (separados para que se vean)
            botonMusicaRect = new Microsoft.Xna.Framework.Rectangle(300, 220, 200, 60);
            botonVolverRect = new Microsoft.Xna.Framework.Rectangle(300, 380, 200, 60);

            // Posición del cuadro de pausa
            pauseBoxRect = new Microsoft.Xna.Framework.Rectangle(100, 150, 600, 300);
            botonReanudarRect = new Microsoft.Xna.Framework.Rectangle(150, 350, 200, 50);
            botonSalirMenuRect = new Microsoft.Xna.Framework.Rectangle(450, 350, 200, 50);

            int frameWidth = 32;
            int frameHeight = 32;
            int offsetX = 32;
            int offsetY = 32;

            for (int i = 0; i < totalFrames; i++)
            {
                rectangulo[i] = new Microsoft.Xna.Framework.Rectangle(offsetX + (i * frameWidth), offsetY, frameWidth, frameHeight);
            }
        }

        private Vector2 ObtenerPosicionSpawn(PuntoSpawn spawn)
        {
            Vector2 centroBase = new Vector2(
                hitboxBase.X + hitboxBase.Width / 2f,
                hitboxBase.Y + hitboxBase.Height / 2f);

            Vector2 desplazamiento = spawn switch
            {
                PuntoSpawn.Arriba => new Vector2(0, -DistanciaSpawnBase),
                PuntoSpawn.Derecha => new Vector2(DistanciaSpawnBase, 0),
                PuntoSpawn.Izquierda => new Vector2(-DistanciaSpawnBase, 0),
                _ => new Vector2(0, DistanciaSpawnBase)
            };

            return centroBase + desplazamiento - new Vector2(TamañoEnemigo / 2f);
        }

        private void IniciarOleada()
        {
            enemigos.Clear();
            balas.Clear();
            puntosSpawnPendientes.Clear();

            for (int i = 0; i < EnemigosActivosIniciales; i++)
            {
                PuntoSpawn spawn = puntosSpawn[i];
                enemigos.Add(new Enemigo(ObtenerPosicionSpawn(spawn), spawn));
            }

            for (int i = 0; i < ReservasPorOleada; i++)
                puntosSpawnPendientes.Add(puntosSpawn[i % puntosSpawn.Length]);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Menú
            menuBackground = Content.Load<Texture2D>("sprites/menuBackground");
            botonStartTexture = Content.Load<Texture2D>("sprites/botonStart (1)");
            botonOpcionesTexture = Content.Load<Texture2D>("sprites/botonOpciones");

            // Opciones (Asegúrate de tener este sprite o usa botonStartTexture temporalmente)
            botonVolverTexture = Content.Load<Texture2D>("sprites/botonVolver");
            botonMusicaTexture = Content.Load<Texture2D>("sprites/botonMusica");

            // Pausa
            pauseBoxTexture = Content.Load<Texture2D>("sprites/pauseBox");
            botonReanudarTexture = Content.Load<Texture2D>("sprites/botonReanudar");
            botonSalirMenuTexture = Content.Load<Texture2D>("sprites/botonSalir");
            Gummy = Content.Load<Texture2D>("sprites/Gum Bot sprites");
            moon = Content.Load<Texture2D>("sprites/moon");
            Base = Content.Load<Texture2D>("sprites/Catbase");

            //  enemigo = Content.Load<Texture2D>("sprites/enemigo");
            menuSong = Content.Load<Song>("Sounds/MenuBG");
            gameSong = Content.Load<Song>("Sounds/BG");

            Laser1 = Content.Load<SoundEffect>("Sounds/BubblePop"); // Se carga el efecto de sonido para el disparo
            _pixel = new Texture2D(GraphicsDevice, 1, 1); //Para el color de las hitboxes
            _pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });



            MediaPlayer.Play(menuSong);
            MediaPlayer.IsRepeating = true;
        }




        protected override void Update(GameTime gameTime)
        {

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouseState = Mouse.GetState();
            Microsoft.Xna.Framework.Rectangle cursorRect = new Microsoft.Xna.Framework.Rectangle(mouseState.X, mouseState.Y, 1, 1);
            bool mouseClicked = mouseState.LeftButton == ButtonState.Pressed;

            if (estadoActual == EstadoJuego.Menu)
            {
                if (MediaPlayer.Queue.ActiveSong != menuSong && musicaActiva)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(menuSong);
                    MediaPlayer.IsRepeating = true;
                }

                if (cursorRect.Intersects(botonStartRect) && mouseClicked)
                    estadoActual = EstadoJuego.Jugando;
                else if (cursorRect.Intersects(botonOpcionesRect) && mouseClicked)
                    estadoActual = EstadoJuego.Opciones;
            }
            else if (estadoActual == EstadoJuego.Opciones)
            {
                if (cursorRect.Intersects(botonMusicaRect) && mouseClicked)
                {
                    musicaActiva = !musicaActiva;
                    if (musicaActiva)
                        MediaPlayer.Resume();
                    else
                        MediaPlayer.Pause();
                }
                else if (cursorRect.Intersects(botonVolverRect) && mouseClicked)
                {
                    estadoActual = EstadoJuego.Menu;
                }
            }
            else if (estadoActual == EstadoJuego.Jugando)
            {
                if (MediaPlayer.Queue.ActiveSong != gameSong && musicaActiva)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(gameSong);
                    MediaPlayer.IsRepeating = true;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.P))
                {
                    estadoActual = EstadoJuego.Pausa;
                }
                else
                {
                    if (vidas <= 0)
                    {
                        muere();
                    }
                    else
                    {
                        hitboxAlien.X = (int)posicion.X + 15;
                        hitboxAlien.Y = (int)posicion.Y + 35;

                        movimientoTeclado();
                        dispararJugador();

                        if (hitboxAlien.Intersects(hitboxMapa))
                            posicion.Y++;

                        ActualizarEnemigos(gameTime);
                        ActualizarBalas();

                        temporizador += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (temporizador >= actualizacionTiempo)
                        {
                            indiceActualAnimacion = (indiceActualAnimacion + 1) % totalFrames;
                            temporizador = 0;
                        }

                        if (cooldown)
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
                    }
                }
            }
            else if (estadoActual == EstadoJuego.Pausa)
            {
                if (cursorRect.Intersects(botonReanudarRect) && mouseClicked)
                {
                    estadoActual = EstadoJuego.Jugando;
                }
                else if (cursorRect.Intersects(botonSalirMenuRect) && mouseClicked)
                {
                    estadoActual = EstadoJuego.Menu;
                }
            }

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (estadoActual == EstadoJuego.Menu)
            {
                _spriteBatch.Draw(menuBackground, new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Microsoft.Xna.Framework.Color.White);
                _spriteBatch.Draw(botonStartTexture, botonStartRect, Microsoft.Xna.Framework.Color.White);
                _spriteBatch.Draw(botonOpcionesTexture, botonOpcionesRect, Microsoft.Xna.Framework.Color.White);

            }
            else if (estadoActual == EstadoJuego.Opciones)
            {
                _spriteBatch.Draw(menuBackground,
                    new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    Microsoft.Xna.Framework.Color.DarkGray);

                _spriteBatch.Draw(botonMusicaTexture, botonMusicaRect, Microsoft.Xna.Framework.Color.White);
                _spriteBatch.Draw(botonVolverTexture, botonVolverRect, Microsoft.Xna.Framework.Color.White);
            }
            else if (estadoActual == EstadoJuego.Jugando || estadoActual == EstadoJuego.Pausa)
            {
                _spriteBatch.Draw(
                    moon,
                    new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    Microsoft.Xna.Framework.Color.White
                );

                int safeIndex = Math.Clamp(indiceActualAnimacion, 0, totalFrames - 1);
                Microsoft.Xna.Framework.Rectangle sourceRectangle = rectangulo[safeIndex];

                _spriteBatch.Draw(
                    Gummy,
                    destinationRectangle: new Microsoft.Xna.Framework.Rectangle(
                        (int)posicion.X, (int)posicion.Y, 32 * 3, 32 * 3),
                    sourceRectangle: sourceRectangle,
                    color: Microsoft.Xna.Framework.Color.White
                );

                _spriteBatch.Draw(_pixel, hitboxAlien, Microsoft.Xna.Framework.Color.Red * 0.4f);
                _spriteBatch.Draw(_pixel, hitboxMapa, Microsoft.Xna.Framework.Color.Yellow * 0.7f);

                foreach (Enemigo enemigo in enemigos)
                    _spriteBatch.Draw(_pixel, ObtenerHitbox(enemigo), Microsoft.Xna.Framework.Color.Blue * 0.7f);

                    _spriteBatch.Draw(Base, hitboxBase, Microsoft.Xna.Framework.Color.White);

                foreach (Bala bala in balas)
                {
                    Microsoft.Xna.Framework.Color colorBala = bala.EsJugador
                        ? Microsoft.Xna.Framework.Color.Orange
                        : Microsoft.Xna.Framework.Color.Green;
                    _spriteBatch.Draw(_pixel, ObtenerHitbox(bala), colorBala * 0.7f);
                }

                // Dibujamos el menú de pausa encima del juego
                if (estadoActual == EstadoJuego.Pausa)
                {
                    _spriteBatch.Draw(pauseBoxTexture, pauseBoxRect, Microsoft.Xna.Framework.Color.White);
                    _spriteBatch.Draw(botonReanudarTexture, botonReanudarRect, Microsoft.Xna.Framework.Color.White);
                    _spriteBatch.Draw(botonSalirMenuTexture, botonSalirMenuRect, Microsoft.Xna.Framework.Color.White);
                }
            }

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
            //Mensaje de has muerto y reintentar. En teoria deberia resetearse solo ahora
            posicion.X = 350;
            posicion.Y = 250;
            vidas = 5;
            estadoActual = EstadoJuego.Menu;
            IniciarOleada();

        }

        private Microsoft.Xna.Framework.Rectangle ObtenerHitbox(Enemigo enemigo)
        {
            return new Microsoft.Xna.Framework.Rectangle(
                (int)enemigo.Posicion.X,
                (int)enemigo.Posicion.Y,
                TamañoEnemigo,
                TamañoEnemigo);
        }

        private Microsoft.Xna.Framework.Rectangle ObtenerHitbox(Bala bala)
        {
            int tamaño = bala.EsJugador ? TamañoBalaJugador : TamañoBalaEnemigo;
            return new Microsoft.Xna.Framework.Rectangle(
                (int)bala.Posicion.X,
                (int)bala.Posicion.Y,
                tamaño,
                tamaño);
        }

        private void ActualizarEnemigos(GameTime gameTime)
        {
            float centroBaseX = hitboxBase.X + hitboxBase.Width / 2f;
            float centroBaseY = hitboxBase.Y + hitboxBase.Height / 2f;
            float tiempoTranscurrido = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            for (int i = 0; i < enemigos.Count; i++)
            {
                Enemigo enemigo = enemigos[i];
                float centroEnemigoX = enemigo.Posicion.X + TamañoEnemigo / 2f;
                float centroEnemigoY = enemigo.Posicion.Y + TamañoEnemigo / 2f;
                float diferenciaX = centroBaseX - centroEnemigoX;
                float diferenciaY = centroBaseY - centroEnemigoY;
                float distanciaBase = new Vector2(diferenciaX, diferenciaY).Length();

                if (distanciaBase > DistanciaAtaqueBase)
                {
                    float distanciaMovimiento = velocidadGlobal * 0.10f; //Velocidad enemigos
                    if (distanciaMovimiento > distanciaBase - DistanciaAtaqueBase)
                        distanciaMovimiento = distanciaBase - DistanciaAtaqueBase;

                    enemigo.Posicion.X += diferenciaX / distanciaBase * distanciaMovimiento;
                    enemigo.Posicion.Y += diferenciaY / distanciaBase * distanciaMovimiento;
                    enemigo.TemporizadorDisparo = 0f;
                }
                else
                {
                    enemigo.TemporizadorDisparo += tiempoTranscurrido;
                    if (enemigo.TemporizadorDisparo >= IntervaloDisparoEnemigo)
                    {
                        dispararEnemigo(new Vector2(centroEnemigoX, centroEnemigoY));
                        enemigo.TemporizadorDisparo = 0f;
                    }
                }

                enemigos[i] = enemigo;
            }
        }

        private void dispararEnemigo(Vector2 origen)
        {
            float destinoX = hitboxBase.X + hitboxBase.Width / 2f;
            float destinoY = hitboxBase.Y + hitboxBase.Height / 2f;
            float diferenciaX = destinoX - origen.X;
            float diferenciaY = destinoY - origen.Y;
            float distancia = new Vector2(diferenciaX, diferenciaY).Length();
            float direccionX = 0f;
            float direccionY = 0f;

            if (distancia > 0f)
            {
                direccionX = diferenciaX / distancia;
                direccionY = diferenciaY / distancia;
            }

            Vector2 posicionBala = new Vector2(
                origen.X - TamañoBalaEnemigo / 2f,
                origen.Y - TamañoBalaEnemigo / 2f);
            balas.Add(new Bala(posicionBala, new Vector2(direccionX, direccionY), false));
        }

        private void ActualizarBalas()
        {
            for (int i = balas.Count - 1; i >= 0; i--)
            {
                Bala bala = balas[i];
                bala.Posicion += bala.Direccion * velocidadGlobal;
                Microsoft.Xna.Framework.Rectangle hitboxBala = ObtenerHitbox(bala);
                bool balaSigueActiva = true;

                if (bala.EsJugador)
                {
                    int indiceEnemigo = -1;
                    for (int j = 0; j < enemigos.Count; j++)
                    {
                        if (hitboxBala.Intersects(ObtenerHitbox(enemigos[j])))
                        {
                            indiceEnemigo = j;
                            break;
                        }
                    }

                    if (indiceEnemigo >= 0)
                    {
                        enemigos.RemoveAt(indiceEnemigo);
                        balas.RemoveAt(i);
                        ReponerEnemigo();
                        balaSigueActiva = false;
                    }
                }
                else if (hitboxBala.Intersects(hitboxBase))
                {
                    vidas--;
                    balas.RemoveAt(i);
                    balaSigueActiva = false;
                }

                if (balaSigueActiva)
                {
                    if (hitboxBala.Right < 0 || hitboxBala.Left > GraphicsDevice.Viewport.Width ||
                        hitboxBala.Bottom < 0 || hitboxBala.Top > GraphicsDevice.Viewport.Height)
                        balas.RemoveAt(i);
                    else
                        balas[i] = bala;
                }
            }
        }

        private void ReponerEnemigo()
        {
            if (puntosSpawnPendientes.Count > 0)
            {
                PuntoSpawn spawn = puntosSpawnPendientes[0];
                puntosSpawnPendientes.RemoveAt(0);
                enemigos.Add(new Enemigo(ObtenerPosicionSpawn(spawn), spawn));
            }
            else if (enemigos.Count == 0)
            {
                IniciarOleada();
            }
        }

        private void dispararJugador()
        {
            bool yaHayBalaJugador = false;
            for (int i = 0; i < balas.Count; i++)
            {
                if (balas[i].EsJugador)
                    yaHayBalaJugador = true;
            }

            if (yaHayBalaJugador == false &&
                Keyboard.GetState().IsKeyDown(Keys.Space) && direccion != -1 && cooldown == false)
            {
                Vector2 direccionBala = new Vector2(0, 0);

                if (direccion == 0)
                    direccionBala.Y = -1;
                else if (direccion == 1)
                    direccionBala.X = 1;
                else if (direccion == 2)
                    direccionBala.Y = 1;
                else if (direccion == 3)
                    direccionBala.X = -1;

                Vector2 posicionBala = new Vector2(
                    hitboxAlien.X + hitboxAlien.Width / 2f - TamañoBalaJugador / 2f,
                    hitboxAlien.Y + hitboxAlien.Height / 2f - TamañoBalaJugador / 2f);
                balas.Add(new Bala(posicionBala, direccionBala, true));
                Laser1.Play();

                if (disparosCooldown == 0)
                    temporizadorDisparos = 0f;

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
    }

}
