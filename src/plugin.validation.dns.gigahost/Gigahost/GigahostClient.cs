using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PKISharp.WACS.Plugins.ValidationPlugins
{
    class GigahostClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gigahost API endpoint, as per https://gigahost.no/api-dokumentasjon
        /// </summary>
        private readonly string _gigahostEndpoint = "https://api.gigahost.no/api/v0/";

        /// <summary>
        /// Create a HttpClient for the Gigahost API, authenticating every
        /// request with the API key (flux_live_...) sent as a Bearer token
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="apiToken"></param>
        public GigahostClient(HttpClient httpClient, string apiToken)
        {
            httpClient.BaseAddress = new Uri(_gigahostEndpoint);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
            _httpClient = httpClient;
        }

        /// <summary>
        /// Find the zone that contains the record by walking up the
        /// domain hierarchy until one of the account zones matches
        /// </summary>
        /// <param name="recordName"></param>
        /// <returns></returns>
        internal async Task<GigahostZone?> GetZone(string recordName)
        {
            var response = await GetRequest<GigahostZoneList>("dns/zones", "retrieve zone list");
            var zones = response.Data.ToDictionary(z => z.ZoneName.TrimEnd('.'), z => z, StringComparer.OrdinalIgnoreCase);
            var parts = recordName.TrimEnd('.').Split('.');
            for (var i = 0; i < parts.Length; i++)
            {
                var candidate = string.Join('.', parts.Skip(i));
                if (zones.TryGetValue(candidate, out var zone))
                {
                    return zone;
                }
            }
            return null;
        }

        /// <summary>
        /// Common handler for GET requests to the Gigahost API.
        /// The custom HttpClient already handles request and response
        /// logging, so here we only catch errors and parse the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<T> GetRequest<T>(string url, string log)
        {
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to {log}: {response.ReasonPhrase}");
            }
            await using var stream = await response.Content.ReadAsStreamAsync();
            var parsed = await JsonSerializer.DeserializeAsync<T>(stream);
            return parsed ?? throw new Exception($"Unable to {log}");
        }

        /// <summary>
        /// Create a TXT record in the specified zone with the validation value
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="host">Record name relative to the zone, "@" for the apex</param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal async Task CreateTxtRecord(GigahostZone zone, string host, string value)
        {
            var json = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "record_name", host },
                { "record_type", "TXT" },
                { "record_value", value },
                { "record_ttl", 60 }
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"dns/zones/{zone.Id}/records";
            using var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to create TXT record: {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Find and delete the TXT record with the specified host and value
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="host">Record name relative to the zone, "@" for the apex</param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal async Task DeleteTxtRecord(GigahostZone zone, string host, string value)
        {
            var records = await GetRequest<GigahostRecordList>($"dns/zones/{zone.Id}/records", "retrieve record list");
            var record = records.Data.Find(r =>
                string.Equals(r.RecordType, "TXT", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.RecordName, host, StringComparison.OrdinalIgnoreCase) &&
                r.RecordValue.Trim('"') == value)
                ?? throw new Exception($"Unable to find exact record for deletion");

            // The name/type/value parameters must be sent so the API removes only
            // this specific TXT record. Without them the API cannot match the record
            // and leaves it in place, which matters when a wildcard certificate
            // produces two _acme-challenge records at the same name.
            var url = $"dns/zones/{zone.Id}/records/{record.Id}" +
                $"?name={WebUtility.UrlEncode(host)}" +
                $"&type=TXT" +
                $"&value={WebUtility.UrlEncode(value)}";
            using var response = await _httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to delete TXT record: {response.ReasonPhrase}");
            }
        }
    }
}
