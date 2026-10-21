using System.Threading.Tasks;

namespace Account.Tasks.Jobs
{
    public interface IScheduledJob
    {
        string Schedule { get; }

        Task DoWorkAsync();
    }
}
