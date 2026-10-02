using TaskEntity = SET.Shared.Models.Task;

namespace BusinessLogic;

public class TaskService : ITaskService
{
    private readonly AppDbContext m_dbContext;
    private readonly ISyncPushService m_syncPushService;

    public TaskService( AppDbContext dbContext, ISyncPushService syncPushService )
    {
        m_dbContext = dbContext;
        m_syncPushService = syncPushService;
    }

    public async Task<ServiceResult<TaskItemDto>> CreateAsync( long userId, TaskItemDto request, string? originDeviceId )
    {
        if (request is null)
        {
            return ServiceError.BadRequest( "TaskIsNull" );
        }

        if (string.IsNullOrWhiteSpace( request.Name ))
        {
            return ServiceError.BadRequest( "NameIsNullOrWhiteSpace" );
        }

        if (request.Name.Length > 255)
        {
            return ServiceError.BadRequest( "NameIsTooLong" );
        }

        TaskEntity entity = new()
        {
            UserId = userId,
            IsCompleted = request.IsCompleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        TaskDtoMapper.ApplyDto( entity, request );

        await m_dbContext.Tasks.AddAsync( entity ).DefaultConfigureAwait();
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        m_syncPushService.NotifyOtherDevices( userId, originDeviceId );

        return TaskDtoMapper.ToDto( entity );
    }

    public async Task<ServiceResult<List<TaskItemDto>>> GetByDateAsync( long userId, DateOnly? date )
    {
        if (date is null)
        {
            return ServiceError.BadRequest( "DateIsNotSpecified" );
        }

        List<TaskEntity> entities = await m_dbContext.Tasks
            .Include( t => t.Subtasks )
            .Where( t => t.UserId == userId && t.Date == date )
            .OrderBy( t => t.Time )
            .ToListAsync()
            .DefaultConfigureAwait();

        return entities.Select( TaskDtoMapper.ToDto ).ToList();
    }

    public async Task<List<TaskItemDto>> GetAllAsync( long userId )
    {
        List<TaskEntity> entities = await m_dbContext.Tasks
            .Include( t => t.Subtasks )
            .Where( t => t.UserId == userId )
            .OrderBy( t => t.Date )
            .ThenBy( t => t.Time )
            .ToListAsync()
            .DefaultConfigureAwait();

        return entities.Select( TaskDtoMapper.ToDto ).ToList();
    }

    public async Task<List<TaskItemDto>> GetInboxAsync( long userId )
    {
        var today = DateOnly.FromDateTime( DateTime.Today );

        List<TaskEntity> entities = await m_dbContext.Tasks
            .Include( t => t.Subtasks )
            .Where( t => t.UserId == userId
                      && t.IsCompleted == false
                      && (t.Date == null || t.Date >= today) )
            .OrderBy( t => t.Date )
            .ThenBy( t => t.Time )
            .ToListAsync()
            .DefaultConfigureAwait();

        return entities.Select( TaskDtoMapper.ToDto ).ToList();
    }

    public async Task<ServiceResult<TaskItemDto>> UpdateStatusAsync( long userId, long id, UpdateTaskStatusDto request, string? originDeviceId )
    {
        if (id <= 0)
        {
            return ServiceError.BadRequest( "TaskIdIsZeroOrNegative" );
        }

        if (request is null)
        {
            return ServiceError.BadRequest( "RequestIsNull" );
        }

        TaskEntity? entity = await FindWithSubtasksAsync( userId, id ).DefaultConfigureAwait();
        if (entity is null)
        {
            return ServiceError.NotFound( $"TaskIsNotFoundWithId {id}" );
        }

        entity.IsCompleted = request.IsCompleted;
        entity.UpdatedAt = DateTime.UtcNow;

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        m_syncPushService.NotifyOtherDevices( userId, originDeviceId );

        return TaskDtoMapper.ToDto( entity );
    }

    public async Task<ServiceResult<TaskItemDto>> UpdateAsync( long userId, long id, TaskItemDto request, string? originDeviceId )
    {
        if (id <= 0) return ServiceError.BadRequest( "TaskIdIsZeroOrNegative" );
        if (request is null) return ServiceError.BadRequest( "RequestIsNull" );
        if (string.IsNullOrWhiteSpace( request.Name )) return ServiceError.BadRequest( "NameIsNullOrWhiteSpace" );

        TaskEntity? entity = await FindWithSubtasksAsync( userId, id ).DefaultConfigureAwait();
        if (entity is null) return ServiceError.NotFound( $"TaskIsNotFoundWithId {id}" );

        TaskDtoMapper.ApplyDto( entity, request );

        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        m_syncPushService.NotifyOtherDevices( userId, originDeviceId );

        return TaskDtoMapper.ToDto( entity );
    }

    public async Task<ServiceResult> DeleteAsync( long userId, long id, string? originDeviceId )
    {
        if (id <= 0)
        {
            return ServiceError.BadRequest( "TaskIdIsZeroOrNegative" );
        }

        TaskEntity? entity = await m_dbContext.Tasks
            .FirstOrDefaultAsync( t => t.Id == id && t.UserId == userId )
            .DefaultConfigureAwait();

        if (entity is null)
        {
            return ServiceError.NotFound( $"TaskIsNotFoundWithId {id}" );
        }

        m_dbContext.SyncDeletions.Add( new SyncDeletion
        {
            UserId = userId,
            EntityType = SyncEntityTypes.Task,
            EntityId = entity.Id,
            DeletedAt = DateTime.UtcNow,
        } );
        m_dbContext.Tasks.Remove( entity );
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();
        m_syncPushService.NotifyOtherDevices( userId, originDeviceId, deletedTaskIds: new[] { id } );

        return ServiceResult.Success;
    }

    private Task<TaskEntity?> FindWithSubtasksAsync( long userId, long id )
    {
        return m_dbContext.Tasks
            .Include( t => t.Subtasks )
            .FirstOrDefaultAsync( t => t.Id == id && t.UserId == userId );
    }
}
