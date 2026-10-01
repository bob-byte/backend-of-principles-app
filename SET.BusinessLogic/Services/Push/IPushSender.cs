using System.Threading;

namespace BusinessLogic;

public enum PushSendResult
{
    Sent,

    /// <summary>The token is gone (app uninstalled / token rotated); drop the device row.</summary>
    InvalidToken,

    Failed,
    NotConfigured,
}

public interface IPushSender
{
    bool IsConfigured { get; }

    Task<PushSendResult> SendDataAsync(
        string token,
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken = default );
}
