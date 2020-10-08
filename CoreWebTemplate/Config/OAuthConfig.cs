using System.Collections.Generic;

namespace CoreWebTemplate.Config {
    public sealed class OAuthConfig {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public List<string> Scopes { get; set; } = new List<string>();

        public string AuthorizationEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInformationEndpoint { get; set; }
        public string AuthorizationHeaderKey { get; set; }
        public string IdentityKey { get; set; }

        /// <summary>
        /// Auth cookie expiration in hours.
        /// </summary>
        public int AuthExpiration { get; set; } = 1;

        public bool DumpOAuthUserInformation { get; set; }
    }
}
