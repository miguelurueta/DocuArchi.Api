using System.Text.Json;
using DocuArchiCore.Abstractions.Security;
using Microsoft.AspNetCore.Http;

namespace DocuArchi.Api.Infrastructure.Security
{
    public partial class SesionActual :
        ISesionActual,
        ISesionDocuArchi,
        ISesionGeneral,
        ISesionGestionDocumental,
        ISesionRadicacion,
        ISesionWorkflow
    {
        private readonly IHttpContextAccessor _http;

        public SesionActual(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ISession Session => _http.HttpContext!.Session;

        protected string GetString(string key, string defaultValue = "")
        {
            return Session.GetString(key) ?? defaultValue;
        }

        protected void SetString(string key, string value)
        {
            Session.SetString(key, value ?? "");
        }

        protected int GetInt(string key, int defaultValue = 0)
        {
            var str = Session.GetString(key);
            return int.TryParse(str, out var r) ? r : defaultValue;
        }

        protected void SetInt(string key, int value)
        {
            Session.SetString(key, value.ToString());
        }

        public void AsignarDesdePerfil(object perfil)
        {
            if (perfil == null) return;

            var perfilProps = perfil.GetType().GetProperties();
            var sesionProps = this.GetType().GetProperties();

            foreach (var sp in sesionProps)
            {
                var perfilProp = perfilProps.FirstOrDefault(p =>
                    p.Name.Equals(sp.Name.Replace("GA_", "").Replace("_", ""), StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Equals(sp.Name, StringComparison.OrdinalIgnoreCase));

                if (perfilProp != null)
                {
                    var valor = perfilProp.GetValue(perfil);
                    if (valor != null)
                    {
                        sp.SetValue(this, valor);
                    }
                }
            }
        }

        protected T? GetObject<T>(string key)
        {
            var json = Session.GetString(key);
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        protected void SetObject<T>(string key, T? value)
        {
            if (value == null)
            {
                Session.Remove(key);
                return;
            }

            var json = JsonSerializer.Serialize(value);
            Session.SetString(key, json);
        }

        public string EMPRESA_GESTION
        {
            get => GetString("EMPRESA_GESTION");
            set => SetString("EMPRESA_GESTION", value);
        }

        public int GA_IDEMPRESA
        {
            get => GetInt("GA_IDEMPRESA");
            set => SetInt("GA_IDEMPRESA", value);
        }

        public string VALIDA_VISOR_EXPRES
        {
            get => GetString("VALIDA_VISOR_EXPRES");
            set => SetString("VALIDA_VISOR_EXPRES", value);
        }

        public string TIPOMODULO
        {
            get => GetString("TIPOMODULO");
            set => SetString("TIPOMODULO", value);
        }

        public string SESION_STATE
        {
            get => GetString("SESION_STATE");
            set => SetString("SESION_STATE", value);
        }

        public string DefaultDbAlias
        {
            get => GetString("DefaultDbAlias");
            set => SetString("DefaultDbAlias", value);
        }
    }
}
