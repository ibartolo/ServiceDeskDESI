using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper : BaseDbWrapper
    {
        protected override string SQLConnectionString { get; }
        protected override TimeSpan SQLCommandTimeOut { get; }

        public DbWrapper()
        {
            // 1) Variable de entorno del hosting (SmarterASP: Advanced Tools > Pool Manager > Environment Variables).
            //    En produccion toma prioridad y evita exponer la contrasena en el web.config publicado.
            var conexionDesdeEntorno = Environment.GetEnvironmentVariable("sConSql");
            var conexionDesdeConfig = System.Configuration.ConfigurationManager.ConnectionStrings["cCon"]?.ConnectionString;

            SQLConnectionString = !string.IsNullOrWhiteSpace(conexionDesdeEntorno)
                ? conexionDesdeEntorno
                : conexionDesdeConfig;

            if (string.IsNullOrWhiteSpace(SQLConnectionString))
                throw new InvalidOperationException(
                    "Cadena de conexión no configurada. Defina la variable de entorno 'sConSql' en el hosting " +
                    "(Advanced Tools > Pool Manager > Environment Variables) o en la seccion <connectionStrings> del web.config.");

            SQLCommandTimeOut = TimeSpan.FromSeconds(15);
        }
        public T MapearPorpiedades<T>(object item)
        {
            if (item == null || item is DBNull)
                return default(T);
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)Convert.ChangeType(item, targetType);
        }

        private T LlenarEntidad<T>(IDataReader reader) where T : class, new()
        {
            T e = new T();
            for (int j = 0; j < reader.FieldCount; j++)
            {
                foreach (var item in e.GetType().GetProperties())
                {
                    if (reader.GetName(j).ToUpper().Equals(item.Name.ToUpper()))
                    {
                        if (reader[j] is DBNull)
                        {
                            item.SetValue(e, null);
                        }
                        else if (item.PropertyType.IsEnum)
                        {
                            var valor = reader[j].ToString();
                            if (valor.Length > 0 && char.IsDigit(valor.First()) && reader[j].GetType() == typeof(string))
                                valor = string.Concat("Item", valor);

                            item.SetValue(e, Enum.Parse(item.PropertyType, valor));
                        }
                        else
                        {
                            // Convierte el valor al tipo de la propiedad (p. ej. int -> long)
                            // para que el mapeo no falle con desajustes de tipo numérico.
                            var targetType = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType;
                            item.SetValue(e, Convert.ChangeType(reader[j], targetType));
                        }
                    }

                }
            }
            return e;
        }

        public List<SqlParameter> ObtenerParametrosSQL<T>(T o)
        {
            var listParameters = new List<SqlParameter>();
            var parametersName = o.GetType().GetProperties();

            foreach (var p in parametersName)
            {
                listParameters.Add(new SqlParameter($"@{p.Name}", p.GetValue(o))
                {
                    IsNullable = true
                });
            }

            return listParameters;
        }

        /// <summary>
        /// Indica si ya existe un usuario con ese NombreUsuario (búsqueda global, sin filtro
        /// de empresa). Se usa para garantizar usuarios únicos al registrar una nueva empresa.
        /// </summary>
        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return false;

            object resultado = ExecuteScalar(
                "SELECT COUNT(1) FROM Usuarios WHERE NombreUsuario = @NombreUsuario",
                CommandType.Text,
                new[] { new SqlParameter("@NombreUsuario", nombreUsuario) });

            return Convert.ToInt32(resultado) > 0;
        }
    }
}