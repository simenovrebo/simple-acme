using PKISharp.WACS.Configuration;
using PKISharp.WACS.Plugins.Base.Factories;
using PKISharp.WACS.Services;
using PKISharp.WACS.Services.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PKISharp.WACS.Plugins.ValidationPlugins.Dns
{
    internal class GigahostOptionsFactory(ArgumentsInputService arguments) : PluginOptionsFactory<GigahostOptions>
    {
        private ArgumentResult<ProtectedString?> ApiToken => arguments.
            GetProtectedString<GigahostArguments>(a => a.ApiToken).
            Required();

        public override async Task<GigahostOptions?> Aquire(IInputService input, RunLevel runLevel)
        {
            return new GigahostOptions()
            {
                ApiToken = await ApiToken.Interactive(input).GetValue(),
            };
        }

        public override async Task<GigahostOptions?> Default()
        {
            return new GigahostOptions()
            {
                ApiToken = await ApiToken.GetValue(),
            };
        }

        public override IEnumerable<(CommandLineAttribute, object?)> Describe(GigahostOptions options)
        {
            yield return (ApiToken.Meta, options.ApiToken);
        }
    }
}
