using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ServiceDeskDESIWebApi.DAL
{
	public partial class DbWrapper
	{
		public ModelResponse ObtenerRoles()
		{
			var modelResponse = new ModelResponse();

			try
			{
				var roles = GetObjects("ObtenerRoles", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
					new Func<IDataReader, Rol>((reader) =>
					{
						var rol = LlenarEntidad<Rol>(reader);
						return rol;
					}));

				modelResponse.IsSuccess = true;
				modelResponse.Response = roles;
				modelResponse.Message = "Roles obtenidos correctamente";
			}
			catch (Exception ex)
			{
				modelResponse.IsSuccess = false;
				modelResponse.Message = "Ocurrió un error al obtener los roles";
			}

			return modelResponse;
		}

		public ModelResponse ObtenerRolPorId(long id)
		{
			var modelResponse = new ModelResponse();

			try
			{
				if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }

				var rol = GetObject("ObtenerRolPorId", CommandType.StoredProcedure,
					new[] { new SqlParameter("@Id", id) },
					new Func<IDataReader, Rol>((reader) =>
					{
						var r = LlenarEntidad<Rol>(reader);
						return r;
					}));

				if (rol == null)
				{
					modelResponse.IsSuccess = false;
					modelResponse.Message = "No se encontró el rol especificado.";
					return modelResponse;
				}

				modelResponse.IsSuccess = true;
				modelResponse.Response = rol;
				modelResponse.Message = "Rol obtenido correctamente";
			}
			catch (ArgumentException ex)
			{
				modelResponse.IsSuccess = false;
				modelResponse.Message = ex.Message;
			}
			catch (Exception ex)
			{
				modelResponse.IsSuccess = false;
				modelResponse.Message = "Ocurrió un error al obtener el rol";
			}

			return modelResponse;
		}

		public ModelResponse GuardarOActualizarRol(Rol r)
		{
			var modelResponse = new ModelResponse();

			try
			{
				// Validaciones
				if (string.IsNullOrWhiteSpace(r.Nombre)) { throw new ArgumentException("El nombre del rol es requerido."); }
				if (r.Nombre.Length > 50) { throw new ArgumentException("El nombre no puede exceder los 50 caracteres."); }
				if (r.Descripcion != null && r.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
				if (string.IsNullOrWhiteSpace(r.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

				var parametros = ObtenerParametrosSQL(r).ToArray();
				var rolId = ExecuteScalar("GuardarOActualizarRol", CommandType.StoredProcedure, parametros);
				r.Id = Convert.ToInt64(rolId);

				modelResponse.IsSuccess = true;
				modelResponse.Response = r;
				modelResponse.Message = "Rol guardado correctamente";
			}
			catch (ArgumentException ex)
			{
				modelResponse.IsSuccess = false;
				modelResponse.Message = ex.Message;
			}
			catch (Exception ex)
			{
				modelResponse.IsSuccess = false;
				modelResponse.Message = "Ocurrió un error al guardar el rol";
			}

			return modelResponse;
		}

		public ModelResponse EliminarRol(long id, string modificadoPor, DateTime fechaModificacion)
		{
			var modelResponse = new ModelResponse();

			try
			{
				if (id <= 0) { throw new ArgumentException("El ID del rol es requerido."); }
				if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

				ExecuteNonQuery("EliminarRol", CommandType.StoredProcedure, new SqlParameter[]
				{
			new SqlParameter("@Id", id),
			new SqlParameter("@ModificadoPor", modificadoPor),
			new SqlParameter("@FechaModificacion", fechaModificacion)
				});

				modelResponse.IsSuccess = true;
				modelResponse.Message = "Rol eliminado correctamente";
			}
			catch (ArgumentException ex)
			{
				modelResponse.IsSuccess = false;
				modelResponse.Message = ex.Message;
			}
			catch (Exception ex)
			{
				modelResponse.IsSuccess = false;
				modelResponse.Message = "Ocurrió un error al eliminar el rol";
			}

			return modelResponse;
		}
	}
}