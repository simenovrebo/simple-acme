using PKISharp.WACS.Clients.DNS;
using PKISharp.WACS.Plugins.Base.Capabilities;
using PKISharp.WACS.Plugins.Interfaces;
using PKISharp.WACS.Plugins.ValidationPlugins.Dns;
using PKISharp.WACS.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PKISharp.WACS.Plugins.ValidationPlugins
{
    [IPlugin.Plugin1<
        GigahostOptions, GigahostOptionsFactory,
        DnsValidationCapability, GigahostJson, GigahostArguments>
        ("d999acdf-0b3d-4d4f-b45f-ac9f1387156b",
        "Gigahost", "Create verification records in Gigahost DNS",
        External = true)]
    internal class GigahostValidation(
        GigahostOptions options,
        LookupClientProvider dnsClient,
        ILogService log,
        ISettings settings,
        IProxyService proxy,
        SecretServiceManager ssm) : DnsValidation<GigahostValidation, GigahostClient>(dnsClient, log, settings, proxy)
    {
        protected override async Task<GigahostClient> CreateClient(HttpClient httpClient)
        {
            var apiToken = await ssm.EvaluateSecret(options.ApiToken) ?? "";
            return new GigahostClient(httpClient, apiToken);
        }

        /// <summary>
        /// Create a DNS record required by the ACME server
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override async Task<bool> CreateRecord(DnsValidationRecord record)
        {
            try
            {
                var client = await GetClient();
                var zone = await client.GetZone(record.Authority.Domain);
                if (zone == null)
                {
                    _log.Error("Unable to find Gigahost zone for {challengeDomain}", record.Authority.Domain);
                    return false;
                }
                var host = RelativeRecordName(zone.ZoneName, record.Authority.Domain);
                _log.Debug("Creating TXT record for {host} with value {value}", host, record.Value);
                await client.CreateTxtRecord(zone, host, record.Value);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unhandled exception when attempting to create record");
                return false;
            }
        }

        /// <summary>
        /// Delete the TXT record after validation has been completed
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public override async Task DeleteRecord(DnsValidationRecord record)
        {
            try
            {
                var client = await GetClient();
                var zone = await client.GetZone(record.Authority.Domain);
                if (zone == null)
                {
                    _log.Warning("Unable to find Gigahost zone for {challengeDomain}", record.Authority.Domain);
                    return;
                }
                var host = RelativeRecordName(zone.ZoneName, record.Authority.Domain);
                _log.Debug("Deleting TXT record for {host} with value {value}", host, record.Value);
                await client.DeleteTxtRecord(zone, host, record.Value);
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "Unable to delete record");
            }
        }
    }
}
