using PKISharp.WACS.Configuration;
using PKISharp.WACS.Configuration.Arguments;

namespace PKISharp.WACS.Plugins.ValidationPlugins
{
    public sealed class GigahostArguments : BaseArguments
    {
        [CommandLine(Description = "API key (flux_live_...) for the Gigahost API.", Secret = true)]
        public string? ApiToken { get; set; }
    }
}
