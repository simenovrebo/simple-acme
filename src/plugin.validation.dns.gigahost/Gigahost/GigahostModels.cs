using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PKISharp.WACS.Plugins.ValidationPlugins
{
    class GigahostZoneList
    {
        [JsonPropertyName("data")]
        public List<GigahostZone> Data { get; set; } = new();
    }

    class GigahostZone
    {
        /// <summary>
        /// Stored as JsonElement because the API may return either a string or a number
        /// </summary>
        [JsonPropertyName("zone_id")]
        public JsonElement ZoneId { get; set; }

        [JsonPropertyName("zone_name")]
        public string ZoneName { get; set; } = string.Empty;

        [JsonIgnore]
        public string Id => ZoneId.ToString();
    }

    class GigahostRecordList
    {
        [JsonPropertyName("data")]
        public List<GigahostRecord> Data { get; set; } = new();
    }

    class GigahostRecord
    {
        /// <summary>
        /// Stored as JsonElement because the API may return either a string or a number
        /// </summary>
        [JsonPropertyName("record_id")]
        public JsonElement RecordId { get; set; }

        [JsonPropertyName("record_name")]
        public string RecordName { get; set; } = string.Empty;

        [JsonPropertyName("record_type")]
        public string RecordType { get; set; } = string.Empty;

        [JsonPropertyName("record_value")]
        public string RecordValue { get; set; } = string.Empty;

        [JsonIgnore]
        public string Id => RecordId.ToString();
    }
}
