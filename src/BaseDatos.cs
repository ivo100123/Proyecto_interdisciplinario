using System;
using System.Data;
using System.Numerics;
using MySqlConnector;

namespace robot_defender
{
    public class ConexionBD
    {
        private string stringConexion = "Server=localhost;Port=3306;Database=robot_defender;User=root;Pwd=";
        public int jugadorActual = 1;
        public int sesionActual = 1;
        public int log = 1;

        public void CrearPlayer(int id, int? oleadaMax, int? tiempoMaximo)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearPlayer", conexion)) //ejecutamos el procedure de crear
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_oleadaMax", oleadaMax);
                        comando.Parameters.AddWithValue("p_tiempoDefendido", tiempoMaximo);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("El jugador ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public DataTable ListarPlayer() //este es el de leer
        {
            DataTable tabla = new DataTable();
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ListarPlayer", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
            return tabla;
        }

        public void ActualizarPlayer(int id, int oleadaMax, int tiempoDefendido) // procedure para editar
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ActualizarPlayer", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_oleadaMax", oleadaMax);
                        comando.Parameters.AddWithValue("p_tiempoDefendido", tiempoDefendido);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public void EliminarPlayer(int id) //procedure de eliminar
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarPlayer", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void CrearTablaClasificacion(int id, int playerFK, int sessionsFK) //Solo hace falta 1 pero cree el metodo para tener el crud completo
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearTablaClasificacion", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_playerFK", playerFK);
                        comando.Parameters.AddWithValue("p_sessionsFK", sessionsFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("El registro ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public DataTable ListarTablaClasificacion() //procedure de leer toda la tabla de clasificacion
        {
            DataTable tabla = new DataTable();
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ListarTablaClasificacion", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
            return tabla;
        }


        public void ActualizarTablaClasificacion(int id, int playerFK, int sessionsFK)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ActualizarTablaClasificacion", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_playerFK", playerFK);
                        comando.Parameters.AddWithValue("p_sessionsFK", sessionsFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }



        public void CrearSessions(int id, DateTime? fechaFin, int oleada, float tiempoDefendido)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearSessions", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_fechaFin", fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                        comando.Parameters.AddWithValue("p_oleada", oleada);
                        comando.Parameters.AddWithValue("p_tiempoDefendido", tiempoDefendido);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("La sesión ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public DataTable ListarSessions()
        {
            DataTable tabla = new DataTable();
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ListarSessions", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
            }


            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
            return tabla;
        }
        public void ActualizarSessions(int id, DateTime? fechaFin, int oleada, int tiempoDefendido, int catalogoEnemigosFK)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ActualizarSessions", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_fechaFin", fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                        comando.Parameters.AddWithValue("p_oleada", oleada);
                        comando.Parameters.AddWithValue("p_tiempoDefendido", tiempoDefendido);
                        comando.Parameters.AddWithValue("p_catalogoEnemigosFK", catalogoEnemigosFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public void EliminarSessions(int id)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarSessions", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void CrearCatalogoEnemigos(int id, int sessionsFK) // Solo hace falta 1 al iniciar
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearCatalogoEnemigos", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_sessionsFK", sessionsFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("El registro ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }



        public void CrearEnemyType(int id, float velocidad, string nombre, int catalogoEnemigosFK) //Registra un tipo de enemigo
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearEnemyType", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_velocidad", velocidad);
                        comando.Parameters.AddWithValue("p_nombre", nombre);
                        comando.Parameters.AddWithValue("p_catalogoEnemigosFK", catalogoEnemigosFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("El tipo de enemigo ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public DataTable ListarEnemyType() //muestra los tipos de enemigo
        {
            DataTable tabla = new DataTable();
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ListarEnemyType", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
            return tabla;
        }

        public void EliminarEnemyType(int id) //Quitar un tipo de enemigo
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarEnemyType", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public void CrearEnemyInstance(int id, float velocidadDisparo, float x, float y, int enemyTypeFK) //registrar nueva instancia de enemigo
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearEnemyInstance", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_velocidadDisparo", velocidadDisparo);
                        comando.Parameters.AddWithValue("p_x", x);
                        comando.Parameters.AddWithValue("p_y", y);
                        comando.Parameters.AddWithValue("p_enemyTypeFK", enemyTypeFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("La instancia de enemigo ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public DataTable ListarEnemyInstance() //mostrar todas las instancias
        {
            DataTable tabla = new DataTable();
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ListarEnemyInstance", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
            return tabla;
        }
        public void ActualizarEnemyInstance(int id, float velocidadDisparo, float x, float y, int enemyTypeFK)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ActualizarEnemyInstance", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_velocidadDisparo", velocidadDisparo);
                        comando.Parameters.AddWithValue("p_x", x);
                        comando.Parameters.AddWithValue("p_y", y);
                        comando.Parameters.AddWithValue("p_enemyTypeFK", enemyTypeFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public void EliminarEnemyInstance(int id) //quita una instancia
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarEnemyInstance", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public void CrearCombatLogs(int id, string tipoAccion, Microsoft.Xna.Framework.Vector2 origen, Microsoft.Xna.Framework.Vector2 destino, float stamptime, int sessionsFK) //registra una accion
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearCombatLogs", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_tipoAccion", tipoAccion);
                        comando.Parameters.AddWithValue("p_origen", origen.ToString());
                        comando.Parameters.AddWithValue("p_destino", destino.ToString());
                        comando.Parameters.AddWithValue("p_stamptime", stamptime);
                        comando.Parameters.AddWithValue("p_sessionsFK", sessionsFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("El log ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public DataTable ListarCombatLogs() //mostrar todas las acciones
        {
            DataTable tabla = new DataTable();
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ListarCombatLogs", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
            return tabla;
        }
        public void ActualizarCombatLogs(int id, string tipoAccion, string origen, string destino, float stamptime, int sessionsFK)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ActualizarCombatLogs", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_tipoAccion", tipoAccion);
                        comando.Parameters.AddWithValue("p_origen", origen);
                        comando.Parameters.AddWithValue("p_destino", destino);
                        comando.Parameters.AddWithValue("p_stamptime", stamptime);
                        comando.Parameters.AddWithValue("p_sessionsFK", sessionsFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public void EliminarCombatLogs(int id) //borra una accion
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarCombatLogs", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public void CrearSessionResults(int id, float tiempoMuerte, string motivoFin, int enemigosDestruidos, int sessionsFK) //al terminar una partida crea los resultados
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("CrearSessionResults", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_tiempoMuerte", tiempoMuerte);
                        comando.Parameters.AddWithValue("p_motivoFin", motivoFin);
                        comando.Parameters.AddWithValue("p_enemigosDestruidos", enemigosDestruidos);
                        comando.Parameters.AddWithValue("p_sessionsFK", sessionsFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    System.Diagnostics.Debug.WriteLine("El resultado ya existe.");
                else
                    System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public DataTable ListarSessionResults() //mostrar los resultados
        {
            DataTable tabla = new DataTable();
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ListarSessionResults", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
            return tabla;
        }
        public void ActualizarSessionResults(int id, int tiempoMuerte, string motivoFin, int enemigosDestruidos, int sessionsFK)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("ActualizarSessionResults", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.Parameters.AddWithValue("p_tiempoMuerte", tiempoMuerte);
                        comando.Parameters.AddWithValue("p_motivoFin", motivoFin);
                        comando.Parameters.AddWithValue("p_enemigosDestruidos", enemigosDestruidos);
                        comando.Parameters.AddWithValue("p_sessionsFK", sessionsFK);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }
        public void EliminarSessionResults(int id) //elimina los resultados de una sesion
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarSessionResults", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }


        public void EliminarCatalogoEnemigos(int id)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarCatalogoEnemigos", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("MYSQL ERROR: " + ex.Message);
            }
        }

        public void EliminarTablaClasificacion(int id)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(stringConexion))
                {
                    conexion.Open();
                    using (MySqlCommand comando = new MySqlCommand("EliminarTablaClasificacion", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("p_id", id);
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
