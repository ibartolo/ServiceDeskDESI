using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskDESIWebApi.Services
{
    public class AreaService
    {
        private readonly DbWrapper _dbWrapper;

        public AreaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse ObtenerAreas(string usuario)
        {
            try
            {
                // La validación de permisos y existencia del usuario se maneja en el DbWrapper
                // Si el usuario no existe o no tiene permisos, simplemente no devuelve datos
                return _dbWrapper.ObtenerAreas(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.ObtenerAreas para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las áreas."
                };
            }
        }

        public ModelResponse ObtenerAreaPorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerAreaPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.ObtenerAreaPorId para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener el área."
                };
            }
        }

        public ModelResponse GuardarOActualizarArea(Area area, string usuario)
        {
            try
            {
                // Validar reglas de negocio (no dependen de la existencia del usuario)
                if (string.IsNullOrWhiteSpace(area.Nombre))
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El nombre del área es requerido."
                    };
                }

                if (area.Nombre.Length > 250)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El nombre no puede exceder los 250 caracteres."
                    };
                }

                if (area.Descripcion != null && area.Descripcion.Length > 500)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "La descripción no puede exceder los 500 caracteres."
                    };
                }

                if (area.Correo != null && area.Correo.Length > 100)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El correo no puede exceder los 100 caracteres."
                    };
                }

                if (string.IsNullOrWhiteSpace(area.CreadoPor))
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El usuario creador es requerido."
                    };
                }

                // Asignar usuario para validación en DbWrapper
                area.CreadoPor = usuario;

                return _dbWrapper.GuardarOActualizarArea(area);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.GuardarOActualizarArea para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el área."
                };
            }
        }

        public ModelResponse EliminarArea(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                // Validar reglas de negocio
                if (id <= 0)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El ID del área es requerido."
                    };
                }

                if (string.IsNullOrWhiteSpace(modificadoPor))
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El usuario modificador es requerido."
                    };
                }

                return _dbWrapper.EliminarArea(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.EliminarArea para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar el área."
                };
            }
        }
    }
}