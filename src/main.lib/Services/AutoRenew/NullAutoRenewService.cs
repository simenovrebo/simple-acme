using System.Threading.Tasks;

namespace PKISharp.WACS.Services.AutoRenew
{
    /// <summary>
    /// Used on platforms without a supported scheduler (e.g. macOS),
    /// where the user is responsible for periodically running the
    /// --renew command themselves
    /// </summary>
    internal class NullAutoRenewService(ILogService log) : IAutoRenewService
    {
        public bool ConfirmAutoRenew()
        {
            log.Warning("Automatic renewal is not supported on this platform, please run with --renew periodically (e.g. from a cronjob or launchd)");
            return false;
        }

        public Task EnsureAutoRenew(RunLevel runLevel)
        {
            ConfirmAutoRenew();
            return Task.CompletedTask;
        }

        public Task SetupAutoRenew(RunLevel runLevel)
        {
            ConfirmAutoRenew();
            return Task.CompletedTask;
        }
    }
}
