using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTrafficInspector.Models
{
    public class SessionData
    {
        public string SessionName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public string Description { get; set; }
        public List<TrafficEntry> TrafficEntries { get; set; } = new List<TrafficEntry>();
        public SessionMetadata Metadata { get; set; } = new SessionMetadata();
    }

    public class SessionMetadata
    {
        public string Version { get; set; } = "1.0";
        public int TotalRequests { get; set; }
        public int HttpRequests { get; set; }
        public int HttpsRequests { get; set; }
        public List<string> UniqueHosts { get; set; } = new List<string>();
        public Dictionary<string, int> MethodCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<int, int> StatusCounts { get; set; } = new Dictionary<int, int>();
    }
}
