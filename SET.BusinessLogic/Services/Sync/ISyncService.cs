namespace BusinessLogic;

public interface ISyncService
{
    /// <summary>Full snapshot for first sign-in / cold start.</summary>
    Task<SyncBootstrapResponse> GetBootstrapAsync( User user );

    /// <summary>
    /// Incremental catch-up since the client's last successful sync cursor. Sets
    /// <see cref="SyncChangesResponse.RequiresFullBootstrap"/> when <paramref name="since"/> is missing or too old.
    /// </summary>
    Task<SyncChangesResponse> GetChangesAsync( User user, DateTime? since );
}
