using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public interface IDeviceTelemetryClient
    {
        Task UpdateRuleAsync(RuleApiModel rule, string etag);
    }
}