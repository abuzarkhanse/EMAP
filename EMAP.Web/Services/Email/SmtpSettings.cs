namespace EMAP.Web.Services.Email
{
    public enum SmtpSecurity
    {
        None = 0,
        StartTls = 1,
        SslOnConnect = 2
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 25;
        public SmtpSecurity Security { get; set; } = SmtpSecurity.StartTls;

        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "EMAP";
    }
}
