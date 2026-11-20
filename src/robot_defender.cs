using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace robot_defender
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D Gummy;
        private Texture2D moon;
        private Texture2D Base;

        private Texture2D corazonTexture;
        private int vidasBaseMax = 5;

        private Texture2D enemigoAzul;
        private Texture2D crab;
        private Texture2D laserRojo;
        private Texture2D misil;
        private Texture2D laserAzul;
        private SpriteFont fuenteTexto;

        private enum EstadoJuego { Menu, Jugando, Opciones, Pausa }
        private EstadoJuego estadoActual = EstadoJuego.Menu;

        private enum PuntoSpawn { Arriba, Derecha, Izquierda, Abajo }

        private struct Enemigo
        {
            public Vector2 Posicion;
            public PuntoSpawn Spawn;
            public float TemporizadorDisparo;
            public int IndiceAnimacion;
            public bool atacando;

            public Enemigo(Vector2 posicion, PuntoSpawn spawn)
            {
                Posicion = posicion;
                Spawn = spawn;
                TemporizadorDisparo = 0f;
                IndiceAnimacion = 0;
                atacando = false;
            }
        }

        private struct Bala
        {
            public Vector2 Posicion;
            public Vector2 Direccion;
            public bool EsJugador;
            public bool EsLateralEnemigo;

            public Bala(Vector2 posicion, Vector2 direccion, bool esJugador, bool esLateralEnemigo = false)
            {
                Posicion = posicion;
                Direccion = direccion;
                EsJugador = esJugador;
                EsLateralEnemigo = esLateralEnemigo;
            }
        }

        private const int TamañoEnemigo = 40;
        private const int TamañoBalaJugador = 40;
        private const int TamañoBalaEnemigo = 40;
        private float DistanciaAtaqueBase = 100f;

        private const float IntervaloDisparoEnemigo = 2000f;
        private const int EnemigosActivosIniciales = 4;
        private int ReservasPorOleada = 8;

        private bool dibujarTexto = true;
        private int OleadaActual = 1;

        private readonly PuntoSpawn[] puntosSpawn =
        {
            PuntoSpawn.Arriba,
            PuntoSpawn.Derecha,
            PuntoSpawn.Izquierda,
            PuntoSpawn.Abajo
        };

        private readonly List<Enemigo> enemigos = new();
        private readonly List<Bala> balas = new();
        private readonly List<PuntoSpawn> puntosSpawnPendientes = new();

        private Texture2D menuBackground;
        private Texture2D botonStartTexture;
        private Rectangle botonStartRect;
        private Texture2D botonOpcionesTexture;
        private Rectangle botonOpcionesRect;

        private Texture2D botonVolverTexture;
        private Rectangle botonVolverRect;
        private Texture2D botonMusicaTexture;
        private Rectangle botonMusicaRect;

        private Texture2D pauseBoxTexture;
        private Rectangle pauseBoxRect;
        private Texture2D botonReanudarTexture;
        private Rectangle botonReanudarRect;
        private Texture2D botonSalirMenuTexture;
        private Rectangle botonSalirMenuRect;

        private ConexionBD basededatos = new ConexionBD();

        private float temporizador = 0f;
        private float temporizadorDisparos = 0f;
        private bool cooldown = false;
        private float temporizadorCooldown = 0f;

        private int vidas = 5;
        private int disparosCooldown = 0;
        private int direccion = -1;
        private int actualizacionTiempo = 120;
        private const int totalFrames = 4;
        private Rectangle[] rectangulo = new Rectangle[totalFrames];
        private int indiceActualAnimacion = 0;

        private float velocidadGlobal = 5.0f;
        private Vector2 posicion = new Vector2(350, 250);

        private Texture2D _pixel;
        private Rectangle hitboxAlien;
        private Rectangle hitboxMapa;
        private Rectangle hitboxBase;
        private Rectangle rectanguloMisil;

        private SoundEffect Laser1;
        private Song menuSong;
        private Song gameSong;
        private bool musicaActiva = true;
        private float temporizadorTexto = 0f;

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

            try
            {
                basededatos.RegistrarJugador(1, "Jugador1", 4000);
            }
            catch (Exception)
            {
            }

            hitboxAlien = new Rectangle((int)posicion.X, (int)posicion.Y, 32 * 2, 32 * 2);
            hitboxMapa = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, 260);
            hitboxBase = new Rectangle((GraphicsDevice.Viewport.Width / 2) - 50, (GraphicsDevice.Viewport.Height / 2) - 45, 100, 250);
            IniciarOleada();

            botonStartRect = new Rectangle(50, 450, 200, 60);
            botonOpcionesRect = new Rectangle(550, 450, 200, 60);
            botonMusicaRect = new Rectangle(300, 220, 200, 60);
            botonVolverRect = new Rectangle(300, 380, 200, 60);

            pauseBoxRect = new Rectangle(100, 150, 600, 300);
            botonReanudarRect = new Rectangle(150, 350, 200, 50);
            botonSalirMenuRect = new Rectangle(450, 350, 200, 50);

            rectanguloMisil = new Rectangle(160, 89, 23, 13);
            int frameWidth = 32;
            int frameHeight = 32;
            int offsetX = 32;
            int offsetY = 32;

            for (int i = 0; i < totalFrames; i++)
            {
                rectangulo[i] = new Rectangle(offsetX + (i * frameWidth), offsetY, frameWidth, frameHeight);
            }
        }

        private Vector2 ObtenerPosicionSpawn(PuntoSpawn spawn)
        {
            return spawn switch
            {
                PuntoSpawn.Arriba => new Vector2(GraphicsDevice.Viewport.Width / 2f - TamañoEnemigo / 2f, 0),
                PuntoSpawn.Derecha => new Vector2(GraphicsDevice.Viewport.Width - TamañoEnemigo, GraphicsDevice.Viewport.Height / 2f - TamañoEnemigo / 2f),
                PuntoSpawn.Izquierda => new Vector2(0, GraphicsDevice.Viewport.Height / 2f - TamañoEnemigo / 2f),
                PuntoSpawn.Abajo => new Vector2(GraphicsDevice.Viewport.Width / 2f - TamañoEnemigo / 2f, GraphicsDevice.Viewport.Height - TamañoEnemigo)
            };
        }

        private void IniciarOleada()
        {
            dibujarTexto = true;
            temporizadorTexto = 0f;
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

            menuBackground = Content.Load<Texture2D>("menuBackground");
            botonStartTexture = Content.Load<Texture2D>("botonStart (1)");
            botonOpcionesTexture = Content.Load<Texture2D>("botonOpciones");
            botonVolverTexture = Content.Load<Texture2D>("botonVolver");
            botonMusicaTexture = Content.Load<Texture2D>("botonMusica");

            pauseBoxTexture = Content.Load<Texture2D>("pauseBox");
            botonReanudarTexture = Content.Load<Texture2D>("botonReanudar");
            botonSalirMenuTexture = Content.Load<Texture2D>("botonSalir");
            Gummy = Content.Load<Texture2D>("Gum Bot sprites");
            moon = Content.Load<Texture2D>("FONDO");
            Base = Content.Load<Texture2D>("Catbase");
            corazonTexture = Content.Load<Texture2D>("corazones");

            enemigoAzul = Content.Load<Texture2D>("enemigoAzul");
            crab = Content.Load<Texture2D>("crab");
            laserRojo = Content.Load<Texture2D>("laserRojo");
            misil = Content.Load<Texture2D>("misiles");
            laserAzul = Content.Load<Texture2D>("laserAzul");

            fuenteTexto = Content.Load<SpriteFont>("Minecraft");

            menuSong = Content.Load<Song>("MenuBG");
            gameSong = Content.Load<Song>("BG");
            Laser1 = Content.Load<SoundEffect>("BubblePop");

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            MediaPlayer.Play(menuSong);
            MediaPlayer.IsRepeating = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouseState = Mouse.GetState();
            Rectangle cursorRect = new Rectangle(mouseState.X, mouseState.Y, 1, 1);
            bool mouseClicked = mouseState.LeftButton == ButtonState.Pressed;

            if (estadoActual == EstadoJuego.Menu)
            {
                dibujarTexto = false;
                if (MediaPlayer.Queue.ActiveSong != menuSong && musicaActiva)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(menuSong);
                    MediaPlayer.IsRepeating = true;
                }

                if (cursorRect.Intersects(botonStartRect) && mouseClicked)
                {
                    estadoActual = EstadoJuego.Jugando;
                    dibujarTexto = true;
                    temporizadorTexto = 0f;
                }
                else if (cursorRect.Intersects(botonOpcionesRect) && mouseClicked)
                {
                    estadoActual = EstadoJuego.Opciones;
                }
            }
            else if (estadoActual == EstadoJuego.Opciones)
            {
                dibujarTexto = false;
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
                if (dibujarTexto)
                {
                    temporizadorTexto += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (temporizadorTexto >= 3000)
                    {
                        dibujarTexto = false;
                        temporizadorTexto = 0f;
                    }
                }

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

                        if (disparosCooldown > 0 && cooldown == false)
                        {
                            temporizadorDisparos += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                            if (temporizadorDisparos >= 5000)
                            {
                                disparosCooldown = 0;
                                temporizadorDisparos = 0f;
                            }
                        }

                        dispararJugador();

                        if (hitboxAlien.Intersects(hitboxMapa))
                            posicion.Y++;

                        ActualizarEnemigos(gameTime);
                        ActualizarBalas();

                        temporizador += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (temporizador >= actualizacionTiempo)
                        {
                            indiceActualAnimacion = (indiceActualAnimacion + 1) % totalFrames;

                            for (int i = 0; i < enemigos.Count; i++)
                            {
                                Enemigo en = enemigos[i];
                                int maxFrames = (en.Spawn == PuntoSpawn.Izquierda || en.Spawn == PuntoSpawn.Derecha) ? 9 : 4;
                                en.IndiceAnimacion = (en.IndiceAnimacion + 1) % maxFrames;
                                enemigos[i] = en;
                            }

                            temporizador = 0;
                        }

                        if (cooldown)
                        {
                            temporizadorCooldown += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                            if (temporizadorCooldown >= 500)
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (estadoActual == EstadoJuego.Menu)
            {
                _spriteBatch.Draw(menuBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                _spriteBatch.Draw(botonStartTexture, botonStartRect, Color.White);
                _spriteBatch.Draw(botonOpcionesTexture, botonOpcionesRect, Color.White);
            }
            else if (estadoActual == EstadoJuego.Opciones)
            {
                _spriteBatch.Draw(menuBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.DarkGray);
                _spriteBatch.Draw(botonMusicaTexture, botonMusicaRect, Color.White);
                _spriteBatch.Draw(botonVolverTexture, botonVolverRect, Color.White);
            }
            else if (estadoActual == EstadoJuego.Jugando || estadoActual == EstadoJuego.Pausa)
            {
                _spriteBatch.Draw(moon, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

                int safeIndex = Math.Clamp(indiceActualAnimacion, 0, totalFrames - 1);
                Rectangle sourceRectangle = rectangulo[safeIndex];

                _spriteBatch.Draw(
                    Gummy,
                    destinationRectangle: new Rectangle((int)posicion.X, (int)posicion.Y, 32 * 3, 32 * 3),
                    sourceRectangle: sourceRectangle,
                    color: Color.White
                );

                _spriteBatch.Draw(_pixel, hitboxAlien, Color.Red * 0.4f);
                _spriteBatch.Draw(_pixel, hitboxMapa, Color.Yellow * 0.7f);

                foreach (Enemigo enemigo in enemigos)
                {
                    Rectangle sourceRectEnemigo;

                    if (enemigo.Spawn == PuntoSpawn.Izquierda || enemigo.Spawn == PuntoSpawn.Derecha)
                    {
                        int ancho = enemigoAzul.Width / 17;
                        int alto = enemigoAzul.Height / 7;

                        int indice = enemigo.IndiceAnimacion % 9;

                        sourceRectEnemigo = new Rectangle(indice * ancho - 10, 1 * alto, ancho - 20, alto);

                        _spriteBatch.Draw(enemigoAzul, ObtenerHitbox(enemigo), sourceRectEnemigo, Color.White);
                    }
                    else
                    {
                        int ancho = crab.Width / 12;
                        int alto = crab.Height / 11;

                        if (enemigo.atacando == true)
                        {
                            sourceRectEnemigo = new Rectangle(20, 10 * alto, ancho - 10, alto - 15);
                        }
                        else
                        {
                            int frameIndex = enemigo.IndiceAnimacion % 4;
                            sourceRectEnemigo = new Rectangle(frameIndex * ancho - 10, 4 * alto, ancho - 10, alto - 15);
                        }

                        _spriteBatch.Draw(crab, ObtenerHitbox(enemigo), sourceRectEnemigo, Color.White);
                    }
                }

                _spriteBatch.Draw(Base, hitboxBase, Color.White);

                int corazonAncho = 24;
                int corazonAlto = 24;
                int espacioEntreCorazones = 4;
                int anchoTotalCorazones = (vidasBaseMax * corazonAncho) + ((vidasBaseMax - 1) * espacioEntreCorazones);
                int startX = hitboxBase.X + (hitboxBase.Width / 2) - (anchoTotalCorazones / 2);
                int startY = hitboxBase.Y - 35;

                for (int i = 0; i < vidasBaseMax; i++)
                {
                    Vector2 posicionCorazon = new Vector2(startX + (i * (corazonAncho + espacioEntreCorazones)), startY);
                    Color colorCorazon = i < vidas ? Color.White : Color.Gray * 0.4f;

                    _spriteBatch.Draw(
                        corazonTexture,
                        destinationRectangle: new Rectangle((int)posicionCorazon.X, (int)posicionCorazon.Y, corazonAncho, corazonAlto),
                        sourceRectangle: new Rectangle(131,158,corazonAncho / 2,corazonAlto - 5),
                        //                       offsetX,offsetY,/2 pq aparecian 2, - 5 pq quedaba muy alto
                        color: colorCorazon
                    );
                }

                foreach (Bala bala in balas)
                {
                    Texture2D texturaBala;
                    Rectangle sourceRectBala;
                    float rotacionBala = MathF.Atan2(bala.Direccion.Y, bala.Direccion.X);

                    if (bala.EsJugador)
                    {
                        texturaBala = laserRojo;
                        sourceRectBala = new Rectangle(0, 0, laserRojo.Width, laserRojo.Height);
                    }
                    else if (bala.EsLateralEnemigo)
                    {
                        texturaBala = laserAzul;
                        sourceRectBala = new Rectangle(0, 0, laserAzul.Width, laserAzul.Height);
                        rotacionBala += MathF.PI * 180f;
                    }
                    else
                    {
                        texturaBala = misil;
                        sourceRectBala = rectanguloMisil;
                        rotacionBala += MathF.PI;
                    }

                    Rectangle hitboxBala = ObtenerHitbox(bala);
                    Rectangle destRectBala = hitboxBala;

                    if (!bala.EsJugador && bala.EsLateralEnemigo)
                    {
                        destRectBala = new Rectangle(
                            hitboxBala.Center.X - 20,
                            hitboxBala.Center.Y - 11,
                            40,
                            23);
                    }

                    Vector2 origenBala = new Vector2(
                        sourceRectBala.Width / 2f,
                        sourceRectBala.Height / 2f);

                    _spriteBatch.Draw(
                        texturaBala,
                        destinationRectangle: destRectBala,
                        sourceRectangle: sourceRectBala,
                        color: Color.White,
                        rotation: rotacionBala,
                        origin: origenBala,
                        effects: SpriteEffects.None,
                        layerDepth: 0f
                    );
                }

                string textoOleada = "Oleada " + OleadaActual;
                if (dibujarTexto)
                {
                    _spriteBatch.DrawString(
                        fuenteTexto,
                        textoOleada,
                        new Vector2(300, 300),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        3f,
                        SpriteEffects.None,
                        0f);
                }

                if (estadoActual == EstadoJuego.Pausa)
                {
                    _spriteBatch.Draw(pauseBoxTexture, pauseBoxRect, Color.White);
                    _spriteBatch.Draw(botonReanudarTexture, botonReanudarRect, Color.White);
                    _spriteBatch.Draw(botonSalirMenuTexture, botonSalirMenuRect, Color.White);
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
            posicion.X = 350;
            posicion.Y = 250;
            vidas = 5;
            estadoActual = EstadoJuego.Menu;
            IniciarOleada();
            dibujarTexto = false;
        }

        private Rectangle ObtenerHitbox(Enemigo enemigo)
        {
            return new Rectangle(
                (int)enemigo.Posicion.X,
                (int)enemigo.Posicion.Y,
                TamañoEnemigo,
                TamañoEnemigo);
        }

        private Rectangle ObtenerHitbox(Bala bala)
        {
            int tamaño = bala.EsJugador ? TamañoBalaJugador : TamañoBalaEnemigo;
            return new Rectangle(
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
                Rectangle hitboxEnemigo = ObtenerHitbox(enemigo);

                float distanciaX = 0f;
                float distanciaY = 0f;

                if (hitboxEnemigo.Right < hitboxBase.Left)
                    distanciaX = hitboxBase.Left - hitboxEnemigo.Right;
                else if (hitboxEnemigo.Left > hitboxBase.Right)
                    distanciaX = hitboxEnemigo.Left - hitboxBase.Right;

                if (hitboxEnemigo.Bottom < hitboxBase.Top)
                    distanciaY = hitboxBase.Top - hitboxEnemigo.Bottom;
                else if (hitboxEnemigo.Top > hitboxBase.Bottom)
                    distanciaY = hitboxEnemigo.Top - hitboxBase.Bottom;

                float distanciaBase = new Vector2(distanciaX, distanciaY).Length();
                float distanciaCentros = new Vector2(diferenciaX, diferenciaY).Length();

                if (distanciaBase > DistanciaAtaqueBase)
                {
                    enemigo.atacando = false;
                    float distanciaMovimiento = velocidadGlobal * 0.10f;
                    if (distanciaMovimiento > distanciaBase - DistanciaAtaqueBase)
                        distanciaMovimiento = distanciaBase - DistanciaAtaqueBase;

                    if (distanciaCentros > 0f)
                    {
                        enemigo.Posicion.X += diferenciaX / distanciaCentros * distanciaMovimiento;
                        enemigo.Posicion.Y += diferenciaY / distanciaCentros * distanciaMovimiento;
                    }
                    enemigo.TemporizadorDisparo = 0f;
                }
                else
                {
                    enemigo.atacando = true;
                    enemigo.TemporizadorDisparo += tiempoTranscurrido;
                    if (enemigo.TemporizadorDisparo >= (enemigo.Spawn == PuntoSpawn.Abajo ? 4000f : IntervaloDisparoEnemigo))
                    {
                        dispararEnemigo(new Vector2(centroEnemigoX, centroEnemigoY), enemigo.Spawn);
                        enemigo.TemporizadorDisparo = 0f;
                    }
                }

                enemigos[i] = enemigo;
            }
        }

        private void dispararEnemigo(Vector2 origen, PuntoSpawn spawn)
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

            bool esLateral = (spawn == PuntoSpawn.Izquierda || spawn == PuntoSpawn.Derecha);

            Vector2 posicionBala = new Vector2(
                origen.X - TamañoBalaEnemigo / 2f,
                origen.Y - TamañoBalaEnemigo / 2f);

            balas.Add(new Bala(posicionBala, new Vector2(direccionX, direccionY), false, esLateral));
        }

        private void ActualizarBalas()
        {
            for (int i = balas.Count - 1; i >= 0; i--)
            {
                Bala bala = balas[i];
                bala.Posicion += bala.Direccion * velocidadGlobal;
                Rectangle hitboxBala = ObtenerHitbox(bala);
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

            if (enemigos.Count == 0 && puntosSpawnPendientes.Count == 0)
            {
                OleadaActual++;
                ReservasPorOleada++;
                IniciarOleada();
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

                if (disparosCooldown >= 6)
                {
                    cooldown = true;
                    temporizadorCooldown = 0f;
                    disparosCooldown = 0;
                    temporizadorDisparos = 0f;
                }
            }
        }
    }
}
