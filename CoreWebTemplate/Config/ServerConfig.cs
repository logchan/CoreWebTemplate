namespace CoreWebTemplate.Config {
    public class ServerConfig {
        public string StaticFiles { get; set; }
        public bool BypassCors { get; set; } = false;
        public OAuthConfig OAuth { get; set; }
    }
}
