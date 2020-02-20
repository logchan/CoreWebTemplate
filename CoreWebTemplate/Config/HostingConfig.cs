﻿namespace CoreWebTemplate.Config {
    public class HostingConfig {
        public int Port { get; set; } = 5001;
        public long MaxRequestBodySize { get; set; } = 1 << 31; // Default 2GB
    }
}
