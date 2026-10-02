using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route("api/tasks")]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class TasksController : BaseController
{
    private readonly ITaskService m_taskService;

    public TasksController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_taskService = serviceProvider.GetRequiredService<ITaskService>();
    }

    [HttpPost]
    public Task<IActionResult> CreateAsync( [FromBody] TaskItemDto request )
    {
        return TryCatchAsync( async user =>
        {
            ServiceResult<TaskItemDto> result = await m_taskService
                .CreateAsync( user.Id, request, RequestDeviceId )
                .DefaultConfigureAwait();
            return ToActionResult( result, () => Created( $"api/tasks/{result.Value!.Id}", result.Value ) );
        } );
    }

    [HttpGet]
    public Task<IActionResult> GetByDateAsync( [FromQuery] DateOnly? date )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_taskService.GetByDateAsync( user.Id, date ).DefaultConfigureAwait() ) );
    }

    [HttpGet("all")]
    public Task<IActionResult> GetAllAsync()
    {
        return TryCatchAsync( async user =>
            Ok( await m_taskService.GetAllAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpPut( "{id:long}/status" )]
    public Task<IActionResult> UpdateStatusAsync( long id, [FromBody] UpdateTaskStatusDto request )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_taskService.UpdateStatusAsync( user.Id, id, request, RequestDeviceId ).DefaultConfigureAwait() ) );
    }

    [HttpPut( "{id:long}" )]
    public Task<IActionResult> UpdateAsync( long id, [FromBody] TaskItemDto request )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_taskService.UpdateAsync( user.Id, id, request, RequestDeviceId ).DefaultConfigureAwait() ) );
    }

    [HttpDelete( "{id:long}" )]
    public Task<IActionResult> DeleteAsync( long id )
    {
        return TryCatchAsync( async user =>
        {
            ServiceResult result = await m_taskService.DeleteAsync( user.Id, id, RequestDeviceId ).DefaultConfigureAwait();
            return ToActionResult( result, NoContent );
        } );
    }

    [HttpGet("inbox")]
    public Task<IActionResult> GetInboxTasksAsync()
    {
        return TryCatchAsync( async user =>
            Ok( await m_taskService.GetInboxAsync( user.Id ).DefaultConfigureAwait() ) );
    }
}
