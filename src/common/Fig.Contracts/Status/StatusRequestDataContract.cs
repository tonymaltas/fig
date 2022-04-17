using System;

namespace Fig.Contracts.Status
{
    public class StatusRequestDataContract
    {
        public Guid RunSessionId { get; set; }

        public double UptimeSeconds { get; set; }

        public DateTime LastSettingUpdate { get; set; }

        public int PollIntervalMs { get; set; }

        public bool LiveReload { get; set; }
    }
}