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

        private float tiempoTranscurrido = 0f;

        private enum EstadoJuego { Menu, Jugando, Opciones, Pausa }
        private EstadoJuego estadoActual = EstadoJuego.Menu;

        private enum PuntoSpawn { Arriba, Derecha, Izquierda, Abajo }

        private struct Enemigo
        {
            public Vector2 Posicion;
            public PuntoSpawn Spawn;
            public int IdBD;
            public float TemporizadorDisparo;
            public int IndiceAnimacion;
            public bool atacando;

            public Enemigo(Vector2 posicion, PuntoSpawn spawn, int idBD)
            {
                Posicion = posicion;
                Spawn = spawn;
                IdBD = idBD;
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
            public Vector2 OrigenEnemigo;

            public Bala(Vector2 posicion, Vector2 direccion, bool esJugador, bool esLateralEnemigo = false, Vector2 origenEnemigo = default)
            {
                Posicion = posicion;
                Direccion = direccion;
                EsJugador = esJugador;
                EsLateralEnemigo = esLateralEnemigo;
                OrigenEnemigo = origenEnemigo;
            }
        }

        private const int TamañoEnemigo = 40;
        private const int TamañoBalaJugador = 40;
        private const int TamañoBalaEnemigo = 40;
        private float DistanciaAtaqueBase = 100f;

        private int enemigosDestruidos = 0;

        private const float IntervaloDisparoEnemigo = 2000f;
        private const int EnemigosActivosIniciales = 4;
        private int ReservasPorOleada = 8;

        private string ultimoatacante;

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
        private const float DuracionCooldown = 1000f;
        private bool espacioContado = false;

        private int vidas = 5;
        private int disparosCooldown = 0;
        private int direccion = -1;
        private int actualizacionTiempo = 120;
        private const int totalFrames = 9;
        private Rectangle[] rectangulo = new Rectangle[totalFrames];
        private int indiceActualAnimacion = 0;

        private float velocidadGlobal = 5.0f;

        private float tiempoGlobal = 0f;
        private int siguienteIdEnemigo = 0;
        private bool limpiezaBaseDatosEjecutada = false;

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

            // También limpia la base de datos si se cierra la ventana con la X.
            Exiting += (sender, args) => LimpiarBaseDatos();
        }

        protected override void Initialize()
        {
            base.Initialize();

             basededatos.CrearPlayer(basededatos.jugadorActual, null, null);
             basededatos.CrearSessions(basededatos.sesionActual, null, 1, 0);
             basededatos.CrearTablaClasificacion(1, basededatos.jugadorActual, basededatos.sesionActual);
             basededatos.CrearCatalogoEnemigos(1, basededatos.sesionActual);
             basededatos.CrearEnemyType(1, velocidadGlobal * 0.10f, "crab", 1);
             basededatos.CrearEnemyType(2, velocidadGlobal * 0.30f, "electro", 1);
            basededatos.CrearSessionResults(basededatos.sesionActual, 0f, "Partida en curso", 0, basededatos.sesionActual);





            hitboxAlien = new Rectangle((int)posicion.X, (int)posicion.Y, 32 * 2, 32 * 2);
            hitboxMapa = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, 450);
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
            for (int i = 0; i < totalFrames; i++)
            {
                int columna = i < 6 ? i : i - 6;
                int fila = i < 6 ? 1 : 2;
                rectangulo[i] = new Rectangle(32 + (columna * frameWidth), fila * frameHeight, frameWidth, frameHeight);
            }
        }

        private Vector2 ObtenerPosicionSpawn(PuntoSpawn spawn)
        {
            return spawn switch
            {
                PuntoSpawn.Arriba => new Vector2(GraphicsDevice.Viewport.Width / 2f - TamañoEnemigo / 2f, 0),
                PuntoSpawn.Derecha => new Vector2(GraphicsDevice.Viewport.Width - TamañoEnemigo, GraphicsDevice.Viewport.Height / 2f - TamañoEnemigo / 2f),
                PuntoSpawn.Izquierda => new Vector2(0, GraphicsDevice.Viewport.Height / 2f - TamañoEnemigo / 2f),
                _ => new Vector2(GraphicsDevice.Viewport.Width / 2f - TamañoEnemigo / 2f, GraphicsDevice.Viewport.Height - TamañoEnemigo)
            };
        }

        private void IniciarOleada()
        {


            dibujarTexto = true;
            temporizadorTexto = 0f;

            //antes de empezar se limpia la BD por si quedan enemigos vivos al morirse
           foreach (Enemigo enemigo in enemigos)
                basededatos.EliminarEnemyInstance(enemigo.IdBD);

            enemigos.Clear();
            balas.Clear();
            puntosSpawnPendientes.Clear();

            for (int i = 0; i < EnemigosActivosIniciales; i++)
            {
                PuntoSpawn spawn = puntosSpawn[i];
                Enemigo nuevoEnemigo = new Enemigo(ObtenerPosicionSpawn(spawn), spawn, siguienteIdEnemigo++);
                enemigos.Add(nuevoEnemigo);

               if (nuevoEnemigo.Spawn == PuntoSpawn.Abajo || nuevoEnemigo.Spawn == PuntoSpawn.Arriba)
                     basededatos.CrearEnemyInstance(nuevoEnemigo.IdBD, IntervaloDisparoEnemigo, nuevoEnemigo.Posicion.X, nuevoEnemigo.Posicion.Y, 1);
                else
                     basededatos.CrearEnemyInstance(nuevoEnemigo.IdBD, IntervaloDisparoEnemigo, nuevoEnemigo.Posicion.X, nuevoEnemigo.Posicion.Y, 2);

                }

            for (int i = 0; i < ReservasPorOleada; i++)
                puntosSpawnPendientes.Add(puntosSpawn[i % puntosSpawn.Length]);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            menuBackground = Content.Load<Texture2D>("sprites/menuBackground");
            botonStartTexture = Content.Load<Texture2D>("sprites/botonStart (1)");
            botonOpcionesTexture = Content.Load<Texture2D>("sprites/botonOpciones");
            botonVolverTexture = Content.Load<Texture2D>("sprites/botonVolver");
            botonMusicaTexture = Content.Load<Texture2D>("sprites/botonMusica");

            pauseBoxTexture = Content.Load<Texture2D>("sprites/pauseBox");
            botonReanudarTexture = Content.Load<Texture2D>("sprites/botonReanudar");
            botonSalirMenuTexture = Content.Load<Texture2D>("sprites/botonSalir");
            Gummy = Content.Load<Texture2D>("sprites/Gum Bot sprites");
            moon = Content.Load<Texture2D>("sprites/FONDO");
            Base = Content.Load<Texture2D>("sprites/Catbase");
            corazonTexture = Content.Load<Texture2D>("sprites/corazones");

            enemigoAzul = Content.Load<Texture2D>("sprites/enemigoAzul");
            crab = Content.Load<Texture2D>("sprites/crab");
            laserRojo = Content.Load<Texture2D>("sprites/laserRojo");
            misil = Content.Load<Texture2D>("sprites/misiles");
            laserAzul = Content.Load<Texture2D>("sprites/laserAzul");

            fuenteTexto = Content.Load<SpriteFont>("Minecraft");

            menuSong = Content.Load<Song>("Sounds/menuBG");
            gameSong = Content.Load<Song>("Sounds/BG");
            Laser1 = Content.Load<SoundEffect>("Sounds/BubblePop");

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

           MediaPlayer.Play(menuSong);
          MediaPlayer.IsRepeating = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                LimpiarBaseDatos();
                Exit();
                return;
            }

            if (estadoActual == EstadoJuego.Jugando)
                tiempoGlobal += (float)gameTime.ElapsedGameTime.TotalSeconds;

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

                            if (temporizadorDisparos >= 2000)
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
                            if (temporizadorCooldown >= DuracionCooldown)
                            {
                                cooldown = false;
                                temporizadorCooldown = 0f;
                                temporizadorDisparos = 0f;
                                disparosCooldown = 0;
                            }
                        }

                        if (cooldown == false && disparosCooldown == 0 && Keyboard.GetState().IsKeyUp(Keys.Space))
                            espacioContado = false;
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

                int anchoBarraCooldown = 96;
                int altoBarraCooldown = 8;
                int xBarraCooldown = (int)posicion.X;
                int yBarraCooldown = (int)posicion.Y - altoBarraCooldown - 6;
                float progresoBarraCooldown = cooldown
                    ? 1f - temporizadorCooldown / DuracionCooldown
                    : disparosCooldown / 6f;
                progresoBarraCooldown = Math.Clamp(progresoBarraCooldown, 0f, 1f);
                if (progresoBarraCooldown > 0f)
                {
                    int nivelBarra = (int)Math.Ceiling(progresoBarraCooldown * 6f);
                    int ySourceBarra = 33 + (nivelBarra - 1) * 17;
                    Rectangle sourceRectBarra = new Rectangle(12, ySourceBarra, 102, 12);
                    Rectangle destinoBarra = new Rectangle(xBarraCooldown, yBarraCooldown, anchoBarraCooldown, altoBarraCooldown);

                    _spriteBatch.Draw(corazonTexture, destinoBarra, sourceRectBarra, Color.White);
                }

                _spriteBatch.Draw(_pixel, hitboxAlien, Color.Red * 0.4f);
                _spriteBatch.Draw(_pixel, hitboxMapa, Color.Yellow * 0.2f);

                foreach (Enemigo enemigo in enemigos)
                {
                    Rectangle sourceRectEnemigo;

                    if (enemigo.Spawn == PuntoSpawn.Izquierda || enemigo.Spawn == PuntoSpawn.Derecha)
                    {
                        int alto = enemigoAzul.Height / 7;
                        if (enemigo.atacando)
                        {
                            sourceRectEnemigo = new Rectangle(0, alto, 30, alto);
                        }
                        else
                        {
                            int inicioFrameX = 12;
                            int separacionFrames = 31;
                            int anchoFrame = 26;
                            int indice = enemigo.IndiceAnimacion % 9;

                            sourceRectEnemigo = new Rectangle(inicioFrameX + indice * separacionFrames, 2 * alto, anchoFrame, alto);
                        }

                        if (enemigo.Spawn == PuntoSpawn.Derecha) _spriteBatch.Draw(enemigoAzul, ObtenerHitbox(enemigo), sourceRectEnemigo, Color.White);
                        else _spriteBatch.Draw(enemigoAzul, ObtenerHitbox(enemigo), sourceRectEnemigo, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                    }
                    else
                    {
                        int ancho = crab.Width / 12;
                        int alto = crab.Height / 11;
                        int anchoFrame = ancho - 20;

                        if (enemigo.atacando == true)
                        {
                            sourceRectEnemigo = new Rectangle(22, 10 * alto, anchoFrame, alto);
                        }
                        else
                        {
                            int frameIndex = enemigo.IndiceAnimacion % 4;
                            int inicioFrameX = 12 + frameIndex * 50;

                            sourceRectEnemigo = new Rectangle(inicioFrameX, 4 * alto, anchoFrame, alto);
                        }

                        Rectangle destinoCrab = ObtenerHitbox(enemigo);
                        destinoCrab.X -= 8;
                        _spriteBatch.Draw(crab, destinoCrab, sourceRectEnemigo, Color.White);
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
                        sourceRectangle: new Rectangle(131, 158, corazonAncho / 2, corazonAlto - 5),
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

                _spriteBatch.Draw(
                    Gummy,
                    destinationRectangle: new Rectangle((int)posicion.X, (int)posicion.Y, 32 * 3, 32 * 3),
                    sourceRectangle: sourceRectangle,
                    color: Color.White
                );

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

        private void LimpiarBaseDatos()
        {
            if (limpiezaBaseDatosEjecutada)
                return;

            // Estos procedimientos eliminan primero los registros dependientes.
              basededatos.EliminarSessions(basededatos.sesionActual);
              basededatos.EliminarPlayer(basededatos.jugadorActual);

            limpiezaBaseDatosEjecutada = true;
        }

        private void muere()
        {
             basededatos.ActualizarSessions(basededatos.sesionActual, DateTime.Now, OleadaActual, (int)tiempoGlobal, 1);
             basededatos.ActualizarSessionResults(basededatos.sesionActual, (int)tiempoGlobal, ultimoatacante, enemigosDestruidos, basededatos.sesionActual);
             basededatos.ActualizarPlayer(basededatos.jugadorActual, OleadaActual, (int)tiempoGlobal);
             basededatos.ActualizarTablaClasificacion(1, basededatos.jugadorActual, basededatos.sesionActual);
           
            posicion.X = 350;
            posicion.Y = 250;
            vidas = 5;
            estadoActual = EstadoJuego.Menu;

            IniciarOleada();
            dibujarTexto = false;
            basededatos.jugadorActual++;
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
            tiempoTranscurrido = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

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
                    //Si los enemigos son de arriba o abajo (los crabs) van mas despacio,los de los costados (los azules) son mas rapidos
                    float distanciaMovimiento = (float)((enemigo.Spawn == PuntoSpawn.Arriba || enemigo.Spawn == PuntoSpawn.Abajo) ? velocidadGlobal * 0.10f : velocidadGlobal * 0.30);
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

             basededatos.CrearCombatLogs(basededatos.log, "Disparo enemigo", origen, new Vector2(destinoX, destinoY),tiempoGlobal, basededatos.sesionActual);
             basededatos.log++;

            balas.Add(new Bala(posicionBala, new Vector2(direccionX, direccionY), false, esLateral, origen));
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
                              basededatos.CrearCombatLogs(basededatos.log, "Disparo jugador", posicion, new Vector2(enemigos[j].Posicion.X, enemigos[j].Posicion.Y),tiempoGlobal, basededatos.sesionActual);
                              basededatos.log++;
                            indiceEnemigo = j;
                            enemigosDestruidos++;


                            break;
                        }
                    }

                    if (indiceEnemigo >= 0)
                    {
                        basededatos.EliminarEnemyInstance(enemigos[indiceEnemigo].IdBD);
                        enemigos.RemoveAt(indiceEnemigo);
                        balas.RemoveAt(i);
                        ReponerEnemigo();
                        balaSigueActiva = false;
                    }
                }
                else if (hitboxBala.Intersects(hitboxBase))
                {
                    vidas--;
                    ultimoatacante = (bala.EsLateralEnemigo) ? "Laser de electro" : "Misil de un crab";
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

                Enemigo nuevoEnemigo = new Enemigo(ObtenerPosicionSpawn(spawn), spawn, siguienteIdEnemigo++);
                enemigos.Add(nuevoEnemigo);

                if (nuevoEnemigo.Spawn == PuntoSpawn.Abajo || nuevoEnemigo.Spawn == PuntoSpawn.Arriba)
                    basededatos.CrearEnemyInstance(nuevoEnemigo.IdBD, IntervaloDisparoEnemigo, nuevoEnemigo.Posicion.X, nuevoEnemigo.Posicion.Y, 1);
                else
                    basededatos.CrearEnemyInstance(nuevoEnemigo.IdBD, IntervaloDisparoEnemigo, nuevoEnemigo.Posicion.X, nuevoEnemigo.Posicion.Y, 2);
            
                }
        }

        private void dispararJugador()
        {
            bool espacioPresionado = Keyboard.GetState().IsKeyDown(Keys.Space);

            bool yaHayBalaJugador = false;

            for (int i = 0; i < balas.Count; i++)
            {
                if (balas[i].EsJugador)
                    yaHayBalaJugador = true;
            }

            if (yaHayBalaJugador == false &&
                espacioPresionado && direccion != -1 && cooldown == false)
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
            }

            if (espacioPresionado && espacioContado == false && direccion != -1 && cooldown == false)
            {
                espacioContado = true;

                if (disparosCooldown == 0)
                    temporizadorDisparos = 0f;

                disparosCooldown++;

                if (disparosCooldown >= 5)
                {
                    cooldown = true;
                    temporizadorCooldown = 0f;
                    disparosCooldown = 0;
                    temporizadorDisparos = 0f;
                }
            }

            if (espacioPresionado == false)
                espacioContado = false;
        }
    }
}
