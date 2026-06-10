using PKISharp.WACS.Plugins.Base.Options;
using PKISharp.WACS.Services.Serialization;
using System.Text.Json.Serialization;

namespace PKISharp.WACS.Plugins.ValidationPlugins
{
    [JsonSerializable(typeof(GigahostOptions))]
    internal partial class GigahostJson : JsonSerializerContext
    {
        public GigahostJson(WacsJsonPluginsOptionsFactory optionsFactory) : base(optionsFactory.Options) { }
    }

    internal class GigahostOptions : ValidationPluginOptions
    {
        public ProtectedString? ApiToken { get; set; }
    }
}
