namespace DocuArchi.Api.Infrastructure.Security
{
    public partial class SesionActual
    {
        public string DA_ID_MODULO
        {
            get => GetString("DA_ID_MODULO");
            set => SetString("DA_ID_MODULO", value);
        }

        public string DA_ID_EMPRESA
        {
            get => GetString("DA_ID_EMPRESA");
            set => SetString("DA_ID_EMPRESA", value);
        }

        public string DA_NOMBRE_MODULO
        {
            get => GetString("DA_NOMBRE_MODULO");
            set => SetString("DA_NOMBRE_MODULO", value);
        }

        public string DA_IP_SERVER_MODULO
        {
            get => GetString("DA_IP_SERVER_MODULO");
            set => SetString("DA_IP_SERVER_MODULO", value);
        }

        public string DA_DB_NAME_MODULO
        {
            get => GetString("DA_DB_NAME_MODULO");
            set => SetString("DA_DB_NAME_MODULO", value);
        }

        public string DA_USER_DBMS_MODULO
        {
            get => GetString("DA_USER_DBMS_MODULO");
            set => SetString("DA_USER_DBMS_MODULO", value);
        }

        public string DA_PASW_DBMS_MODULO
        {
            get => GetString("DA_PASW_DBMS_MODULO");
            set => SetString("DA_PASW_DBMS_MODULO", value);
        }

        public string DA_TYPE_DBMS_MODULO
        {
            get => GetString("DA_TYPE_DBMS_MODULO");
            set => SetString("DA_TYPE_DBMS_MODULO", value);
        }

        public string DA_NUMERO_DBMS_CONEX
        {
            get => GetString("DA_NUMERO_DBMS_CONEX");
            set => SetString("DA_NUMERO_DBMS_CONEX", value);
        }

        public string DA_ACTIVA_POOL_DBMS
        {
            get => GetString("DA_ACTIVA_POOL_DBMS");
            set => SetString("DA_ACTIVA_POOL_DBMS", value);
        }

        public string DA_ENCRIPT_PASW
        {
            get => GetString("DA_ENCRIPT_PASW");
            set => SetString("DA_ENCRIPT_PASW", value);
        }

        public string DA_GABINETE_CONSULTA
        {
            get => GetString("DA_GABINETE_CONSULTA");
            set => SetString("DA_GABINETE_CONSULTA", value);
        }

        public int ID_USUARIO_DOCUARCHI
        {
            get => GetInt("ID_USUARIO_DOCUARCHI");
            set => SetInt("ID_USUARIO_DOCUARCHI", value);
        }

        public string DA_Login_Usuario
        {
            get => GetString("DA_Login_Usuario");
            set => SetString("DA_Login_Usuario", value);
        }

        public int DA_IMAGEN
        {
            get => GetInt("DA_IMAGEN");
            set => SetInt("DA_IMAGEN", value);
        }

        public int DA_Consuarchi1
        {
            get => GetInt("DA_Consuarchi1");
            set => SetInt("DA_Consuarchi1", value);
        }

        public int DA_Almarchi
        {
            get => GetInt("DA_Almarchi");
            set => SetInt("DA_Almarchi", value);
        }

        public int DA_Importex1
        {
            get => GetInt("DA_Importex1");
            set => SetInt("DA_Importex1", value);
        }

        public int DA_Actimp1
        {
            get => GetInt("DA_Actimp1");
            set => SetInt("DA_Actimp1", value);
        }

        public int FIRMA_DIGITAL_DOCUMENTO_DA
        {
            get => GetInt("FIRMA_DIGITAL_DOCUMENTO_DA");
            set => SetInt("FIRMA_DIGITAL_DOCUMENTO_DA", value);
        }

        public int ELIMINA_FIRMA_DIGITAL_DOCUMENTO_DA
        {
            get => GetInt("ELIMINA_FIRMA_DIGITAL_DOCUMENTO_DA");
            set => SetInt("ELIMINA_FIRMA_DIGITAL_DOCUMENTO_DA", value);
        }

        public string DA_TIPOMODULO
        {
            get => GetString("DA_TIPOMODULO");
            set => SetString("DA_TIPOMODULO", value);
        }

        public int DA_ACTIVA_WEB_SERVICE
        {
            get => GetInt("DA_ACTIVA_WEB_SERVICE");
            set => SetInt("DA_ACTIVA_WEB_SERVICE", value);
        }

        public string DA_URL_WEB_SERVICE
        {
            get => GetString("DA_URL_WEB_SERVICE");
            set => SetString("DA_URL_WEB_SERVICE", value);
        }

        public string DA_USER_WEB_SERVICE
        {
            get => GetString("DA_USER_WEB_SERVICE");
            set => SetString("DA_USER_WEB_SERVICE", value);
        }

        public string DA_PASW_WEB_SERVICE
        {
            get => GetString("DA_PASW_WEB_SERVICE");
            set => SetString("DA_PASW_WEB_SERVICE", value);
        }

        public int DA_gruposusu
        {
            get => GetInt("DA_gruposusu");
            set => SetInt("DA_gruposusu", value);
        }
    }
}
