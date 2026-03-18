namespace DocuArchi.Api.Infrastructure.Security
{
    public partial class SesionActual
    {
        public int InicioSesion
        {
            get => GetInt("InicioSesion");
            set => SetInt("InicioSesion", value);
        }

        public int DuracionSesion
        {
            get => GetInt("DuracionSesion");
            set => SetInt("DuracionSesion", value);
        }

        public string DefaultDbAliasDA
        {
            get => GetString("defaultDbAliasDA");
            set => SetString("defaultDbAliasDA", value);
        }

        public string DefaultDbAliasWF
        {
            get => GetString("defaultDbAliasWF");
            set => SetString("defaultDbAliasWF", value);
        }

        public string EXTENSION_ARCHIVO_ADJUNTA
        {
            get => GetString("EXTENSION_ARCHIVO_ADJUNTA");
            set => SetString("EXTENSION_ARCHIVO_ADJUNTA", value);
        }

        public string RUTA_TEMPORAL_ARCHIVO_ADJUNTA
        {
            get => GetString("RUTA_TEMPORAL_ARCHIVO_ADJUNTA");
            set => SetString("RUTA_TEMPORAL_ARCHIVO_ADJUNTA", value);
        }

        public int TIPO_ADJUNTA_STATE
        {
            get => GetInt("TIPO_ADJUNTA_STATE");
            set => SetInt("TIPO_ADJUNTA_STATE", value);
        }

        public string DETALLE_SESION
        {
            get => GetString("DETALLE_SESION");
            set => SetString("DETALLE_SESION", value);
        }

        public string ID_MODULO
        {
            get => GetString("ID_MODULO");
            set => SetString("ID_MODULO", value);
        }

        public string ID_EMPRESA
        {
            get => GetString("ID_EMPRESA");
            set => SetString("ID_EMPRESA", value);
        }

        public string NOMBRE_MODULO
        {
            get => GetString("NOMBRE_MODULO");
            set => SetString("NOMBRE_MODULO", value);
        }

        public string IP_SERVER_MODULO
        {
            get => GetString("IP_SERVER_MODULO");
            set => SetString("IP_SERVER_MODULO", value);
        }

        public string DB_NAME_MODULO
        {
            get => GetString("DB_NAME_MODULO");
            set => SetString("DB_NAME_MODULO", value);
        }

        public string USER_DBMS_MODULO
        {
            get => GetString("USER_DBMS_MODULO");
            set => SetString("USER_DBMS_MODULO", value);
        }

        public string PASW_DBMS_MODULO
        {
            get => GetString("PASW_DBMS_MODULO");
            set => SetString("PASW_DBMS_MODULO", value);
        }

        public string TYPE_DBMS_MODULO
        {
            get => GetString("TYPE_DBMS_MODULO");
            set => SetString("TYPE_DBMS_MODULO", value);
        }

        public string NUMERO_DBMS_CONEX
        {
            get => GetString("NUMERO_DBMS_CONEX");
            set => SetString("NUMERO_DBMS_CONEX", value);
        }

        public string ACTIVA_POOL_DBMS
        {
            get => GetString("ACTIVA_POOL_DBMS");
            set => SetString("ACTIVA_POOL_DBMS", value);
        }

        public string ENCRIPT_PASW
        {
            get => GetString("ENCRIPT_PASW");
            set => SetString("ENCRIPT_PASW", value);
        }

        public int ACTIVA_WEB_SERVICE
        {
            get => GetInt("ACTIVA_WEB_SERVICE");
            set => SetInt("ACTIVA_WEB_SERVICE", value);
        }

        public string URL_WEB_SERVICE
        {
            get => GetString("URL_WEB_SERVICE");
            set => SetString("URL_WEB_SERVICE", value);
        }

        public string USER_WEB_SERVICE
        {
            get => GetString("USER_WEB_SERVICE");
            set => SetString("USER_WEB_SERVICE", value);
        }

        public string PASW_WEB_SERVICE
        {
            get => GetString("PASW_WEB_SERVICE");
            set => SetString("PASW_WEB_SERVICE", value);
        }

        public string SesionActiva
        {
            get => GetString("SesionActiva");
            set => SetString("SesionActiva", value);
        }

        public int ESTADOFILESERVER
        {
            get => GetInt("ESTADOFILESERVER");
            set => SetInt("ESTADOFILESERVER", value);
        }

        public string RAZON_SOCIAL_EMPRESA
        {
            get => GetString("RAZON_SOCIAL_EMPRESA");
            set => SetString("RAZON_SOCIAL_EMPRESA", value);
        }
    }
}
