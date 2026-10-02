using Microsoft.Extensions.DependencyInjection;

namespace SET.WebAPI.Controllers;

[Route( "api/ai/conversations" )]
[ApiController]
[Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
public class AiConversationsController : BaseController
{
    private readonly IAiConversationService m_conversationService;

    public AiConversationsController( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_conversationService = serviceProvider.GetRequiredService<IAiConversationService>();
    }

    [HttpGet]
    public Task<IActionResult> ListAsync()
    {
        return TryCatchAsync( async user =>
            Ok( await m_conversationService.GetAllAsync( user.Id ).DefaultConfigureAwait() ) );
    }

    [HttpGet( "{id:long}" )]
    public Task<IActionResult> GetByIdAsync( long id )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_conversationService.GetByIdAsync( user.Id, id ).DefaultConfigureAwait() ) );
    }

    [HttpGet( "by-client/{clientId}" )]
    public Task<IActionResult> GetByClientIdAsync( string clientId )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_conversationService.GetByClientIdAsync( user.Id, clientId ).DefaultConfigureAwait() ) );
    }

    [HttpPost]
    public Task<IActionResult> CreateAsync( [FromBody] AiConversationDto request )
    {
        return TryCatchAsync( async user =>
        {
            ServiceResult<AiConversationCreateResult> result = await m_conversationService
                .CreateAsync( user.Id, request )
                .DefaultConfigureAwait();
            return ToActionResult( result, () =>
            {
                AiConversationDto conversation = result.Value!.Conversation;
                return result.Value.IsCreated
                    ? Created( $"api/ai/conversations/{conversation.Id}", conversation )
                    : Ok( conversation );
            } );
        } );
    }

    [HttpPut( "{id:long}" )]
    public Task<IActionResult> UpdateAsync( long id, [FromBody] AiConversationDto request )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_conversationService.UpdateAsync( user.Id, id, request ).DefaultConfigureAwait() ) );
    }

    [HttpPut( "by-client/{clientId}" )]
    public Task<IActionResult> UpsertByClientIdAsync( string clientId, [FromBody] AiConversationDto request )
    {
        return TryCatchAsync( async user =>
            ToActionResult( await m_conversationService.UpsertByClientIdAsync( user.Id, clientId, request ).DefaultConfigureAwait() ) );
    }

    [HttpDelete( "{id:long}" )]
    public Task<IActionResult> DeleteAsync( long id )
    {
        return TryCatchAsync( async user =>
        {
            ServiceResult result = await m_conversationService.DeleteAsync( user.Id, id ).DefaultConfigureAwait();
            return ToActionResult( result, NoContent );
        } );
    }

    [HttpDelete( "by-client/{clientId}" )]
    public Task<IActionResult> DeleteByClientIdAsync( string clientId )
    {
        return TryCatchAsync( async user =>
        {
            ServiceResult result = await m_conversationService.DeleteByClientIdAsync( user.Id, clientId ).DefaultConfigureAwait();
            return ToActionResult( result, NoContent );
        } );
    }
}
