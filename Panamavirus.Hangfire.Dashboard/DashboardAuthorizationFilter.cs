using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Panamavirus.Hangfire.Dashboard
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context) => true;
    }
}
