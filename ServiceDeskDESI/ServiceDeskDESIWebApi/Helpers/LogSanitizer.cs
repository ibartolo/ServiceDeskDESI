using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ServiceDeskDESIWebApi.Helpers
{
    /// <summary>
    /// Serializa objetos y parámetros SQL a JSON para logging, ENMASCARANDO datos
    /// sensibles (contraseñas, firmas, tokens) para no registrarlos en claro.
    /// Regla R4 del cambio `logging-estandarizado`.
    /// </summary>
    public static class LogSanitizer
    {
        private static readonly string[] SensitiveKeys =
        {
            "Contrasena", "Contraseña", "Password", "Firma",
            "Token", "TokenConfirmacion", "access_token", "refresh_token", "client_secret"
        };

        /// <summary>Serializa un objeto a JSON de una línea, enmascarando sensibles.</summary>
        public static string ToJson(object obj)
        {
            if (obj == null) return "null";
            try
            {
                var token = JToken.FromObject(obj);
                Mask(token);
                return token.ToString(Formatting.None);
            }
            catch (Exception ex)
            {
                return $"<no serializable: {ex.Message}>";
            }
        }

        /// <summary>Convierte una lista de SqlParameter a JSON (nombre: valor), enmascarando sensibles.</summary>
        public static string FromSqlParameters(IEnumerable<SqlParameter> parameters)
        {
            if (parameters == null) return "{}";

            var dict = new Dictionary<string, object>();
            foreach (var p in parameters)
            {
                var name = (p.ParameterName ?? string.Empty).TrimStart('@');
                if (string.IsNullOrEmpty(name)) continue;

                if (IsSensitive(name))
                    dict[name] = "***";
                else
                    dict[name] = p.Value == DBNull.Value ? null : p.Value;
            }
            return ToJson(dict);
        }

        private static bool IsSensitive(string name) =>
            !string.IsNullOrEmpty(name) &&
            SensitiveKeys.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

        private static void Mask(JToken token)
        {
            if (token is JObject obj)
            {
                foreach (var prop in obj.Properties().ToList())
                {
                    if (IsSensitive(prop.Name))
                        prop.Value = "***";
                    else
                        Mask(prop.Value);
                }
            }
            else if (token is JArray arr)
            {
                foreach (var item in arr) Mask(item);
            }
        }
    }
}
