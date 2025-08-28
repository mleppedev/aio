using System.Threading;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Chat;

public interface IChatProvider
{
    Task<string> GenerateAsync(string prompt, CancellationToken ct = default);
}
