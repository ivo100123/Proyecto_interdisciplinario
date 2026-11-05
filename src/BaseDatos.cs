
using MySqlConnector;

namespace robot_defender
{

    public class ConexionBD
    {
        private string stringConexion = "Server=localhost;Port=3306;Database=robot_defender;User=root;Pwd=";


        public void RegistrarJugador(int idPlayer, string username, int highscore)
        {
            string query = "INSERT INTO players (id_player, username, highscore) VALUES (@id_player, @username, @highscore)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_player", idPlayer);
                        comando.Parameters.AddWithValue("@username", username);
                        comando.Parameters.AddWithValue("@highscore", highscore);
                        comando.ExecuteNonQuery(); //NonQuery porque no devuelve nada solo inserta y ya
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) //Si el id es duplicado
                    System.Diagnostics.Debug.WriteLine("El jugador ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarSesion(int idSessions, int idPlayer, string dificultadInicial, int tiempoDefendido)
        {
            string query = "INSERT INTO Sessions (id_sessions, idPlayer, fecha_inicio, dificultad_inicial, tiempo_defendido) " +
                           "VALUES (@id_sessions, @idPlayer, NOW(), @dificultad_inicial, @tiempo_defendido)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_sessions", idSessions);
                        comando.Parameters.AddWithValue("@idPlayer", idPlayer);
                        comando.Parameters.AddWithValue("@dificultad_inicial", dificultadInicial);
                        comando.Parameters.AddWithValue("@tiempo_defendido", tiempoDefendido);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarResultadosSesion(int idSessionResults, int idSessions, int integridadBaseFinal, int enemigosEliminados, int defensasDestruidas, string motivoFin)
        {
            string query = "INSERT INTO sessionResults (id_sessionResults, id_sessions, integridad_base_final, enemigos_eliminados, defensas_destruidas, motivo_fin) " +
                           "VALUES (@id_sessionResults, @id_sessions, @integridad, @enemigos, @defensas, @motivo)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_sessionResults", idSessionResults);
                        comando.Parameters.AddWithValue("@id_sessions", idSessions);
                        comando.Parameters.AddWithValue("@integridad", integridadBaseFinal);
                        comando.Parameters.AddWithValue("@enemigos", enemigosEliminados);
                        comando.Parameters.AddWithValue("@defensas", defensasDestruidas);
                        comando.Parameters.AddWithValue("@motivo", motivoFin);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarSessionRecord(int idSessionRecord, int idSessionsFk, int idSessionResultsFk, int idCombatLogsFk)
        {
            string query = "INSERT INTO sessionRecord (id_sessionRecord, id_sessions_FK, `id_sessionResults(FK)`, id_combatLogsFK) " +
                           "VALUES (@id_sessionRecord, @id_sessions_FK, @id_sessionResults_FK, @id_combatLogsFK)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_sessionRecord", idSessionRecord);
                        comando.Parameters.AddWithValue("@id_sessions_FK", idSessionsFk);
                        comando.Parameters.AddWithValue("@id_sessionResults_FK", idSessionResultsFk);
                        comando.Parameters.AddWithValue("@id_combatLogsFK", idCombatLogsFk);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarLogCombate(int idCombatLogs, int idSessions, int idDestino, int idOrigen, int idImpacto, decimal valorImpacto, string tipoAccion, string coordenadas)
        {
            string query = "INSERT INTO combatLogs (id_combatLogs, id_sessions, id_destino, id_origen, id_impacto, valor_impacto, timestamp, tipo_accion, coordenadas) " +
                           "VALUES (@id_combatLogs, @id_sessions, @id_destino, @id_origen, @id_impacto, @valor_impacto, NOW(), @tipo_accion, @coordenadas)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_combatLogs", idCombatLogs);
                        comando.Parameters.AddWithValue("@id_sessions", idSessions);
                        comando.Parameters.AddWithValue("@id_destino", idDestino);
                        comando.Parameters.AddWithValue("@id_origen", idOrigen);
                        comando.Parameters.AddWithValue("@id_impacto", idImpacto);
                        comando.Parameters.AddWithValue("@valor_impacto", valorImpacto);
                        comando.Parameters.AddWithValue("@tipo_accion", tipoAccion);
                        comando.Parameters.AddWithValue("@coordenadas", coordenadas);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarCatalogoSesion(int idCatalogue, int idSessionsFk, int idDefensetypeFk, int idEnemyTypesFk)
        {
            string query = "INSERT INTO SessionCatalogue (id_catalogue, id_sessions_FK, id_defensetype_FK, id_enemyTypes_FK) " +
                           "VALUES (@id_catalogue, @id_sessions_FK, @id_defensetype_FK, @id_enemyTypes_FK)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_catalogue", idCatalogue);
                        comando.Parameters.AddWithValue("@id_sessions_FK", idSessionsFk);
                        comando.Parameters.AddWithValue("@id_defensetype_FK", idDefensetypeFk);
                        comando.Parameters.AddWithValue("@id_enemyTypes_FK", idEnemyTypesFk);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarTipoDefensa(int idDefensetype, string nombre, int rango, int vidaBase, int danoBase, int costoEnergia)
        {
            string query = "INSERT INTO defenseTypes (id_defensetype, nombre, rango, vida_base, daño_base, costo_energia) " +
                           "VALUES (@id_defensetype, @nombre, @rango, @vida_base, @dano_base, @costo_energia)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_defensetype", idDefensetype);
                        comando.Parameters.AddWithValue("@nombre", nombre);
                        comando.Parameters.AddWithValue("@rango", rango);
                        comando.Parameters.AddWithValue("@vida_base", vidaBase);
                        comando.Parameters.AddWithValue("@dano_base", danoBase);
                        comando.Parameters.AddWithValue("@costo_energia", costoEnergia);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarInstanciaDefensa(int idDefenseInstances, int idSessions, int idType, int vidaActual, string estadoOperativo, int x, int y)
        {
            string query = "INSERT INTO defenseInstances (id_defenseinstances, id_sessions, id_type, vida_actual, estado_operativo, x, y) " +
                           "VALUES (@id_defenseinstances, @id_sessions, @id_type, @vida_actual, @estado_operativo, @x, @y)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_defenseinstances", idDefenseInstances);
                        comando.Parameters.AddWithValue("@id_sessions", idSessions);
                        comando.Parameters.AddWithValue("@id_type", idType);
                        comando.Parameters.AddWithValue("@vida_actual", vidaActual);
                        comando.Parameters.AddWithValue("@estado_operativo", estadoOperativo);
                        comando.Parameters.AddWithValue("@x", x);
                        comando.Parameters.AddWithValue("@y", y);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarTipoEnemigo(int idEnemyType, string nombre, int vidaBase, int danoAtaque, float velocidad, int resistencia)
        {
            string query = "INSERT INTO enemyTypes (id_enemyType, nombre, vida_base, daño_ataque, velocidad, resistencia) " +
                           "VALUES (@id_enemyType, @nombre, @vida_base, @dano_ataque, @velocidad, @resistencia)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_enemyType", idEnemyType);
                        comando.Parameters.AddWithValue("@nombre", nombre);
                        comando.Parameters.AddWithValue("@vida_base", vidaBase);
                        comando.Parameters.AddWithValue("@dano_ataque", danoAtaque);
                        comando.Parameters.AddWithValue("@velocidad", velocidad);
                        comando.Parameters.AddWithValue("@resistencia", resistencia);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void RegistrarInstanciaEnemigo(int idEnemyInstances, int idTypeenemy, int idSessions, int vidaActual, int oleadaOriginal, int x, int y)
        {
            string query = "INSERT INTO enemyInstances (id_enemyInstances, id_typeenemy, id_sessions, vida_actual, `oleada original`, x, y) " +
                           "VALUES (@id_enemyInstances, @id_typeenemy, @id_sessions, @vida_actual, @oleada_original, @x, @y)";

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_enemyInstances", idEnemyInstances);
                        comando.Parameters.AddWithValue("@id_typeenemy", idTypeenemy);
                        comando.Parameters.AddWithValue("@id_sessions", idSessions);
                        comando.Parameters.AddWithValue("@vida_actual", vidaActual);
                        comando.Parameters.AddWithValue("@oleada_original", oleadaOriginal);
                        comando.Parameters.AddWithValue("@x", x);
                        comando.Parameters.AddWithValue("@y", y);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
    }
}
