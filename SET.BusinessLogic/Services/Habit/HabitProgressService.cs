using AutoMapper;

namespace BusinessLogic;

public class HabitProgressService : IHabitProgressService
{
    private readonly AppDbContext m_dbContext;
    private readonly IMapper m_mapper;

    public HabitProgressService( AppDbContext dbContext, IMapper mapper )
    {
        m_dbContext = dbContext;
        m_mapper = mapper;
    }

    public async Task<ServiceResult<ProgressSavedResponse>> UpdateProgressAsync( long userId, UpdateProgressDto progressDto )
    {
        #region Check param
        if (progressDto == null)
        {
            return ServiceError.BadRequest( "ProgressIsNull" );
        }

        if (progressDto.Date == default)
        {
            return ServiceError.BadRequest( $"Date is {progressDto.Date}" );
        }

        if (progressDto.HabitId == default)
        {
            return ServiceError.BadRequest( $"HabitId of ${progressDto.GetType().Name} is not set" );
        }
        #endregion

        bool habitExists = await m_dbContext.UserHabits
            .AnyAsync( h => h.Id == progressDto.HabitId && h.UserId == userId )
            .DefaultConfigureAwait();
        if (!habitExists)
        {
            return ServiceError.BadRequest( $"HabitIsNotFoundWithId {progressDto.HabitId}" );
        }

        ProgressOfHabit? progress =
            await m_dbContext.ProgressesOfHabits.FirstOrDefaultAsync( p =>
                p.HabitId == progressDto.HabitId && p.Date == progressDto.Date ).DefaultConfigureAwait();

        if (progress is null)
        {
            progress = m_mapper.Map<ProgressOfHabit>( progressDto );
            progress.Id = 0;
        }
        else
        {
            progress.Value = progressDto.Value;
        }

        await m_dbContext.ProgressesOfHabits.AddOrUpdateAsync( progress ).DefaultConfigureAwait();
        await m_dbContext.UserHabits
            .Where( h => h.Id == progressDto.HabitId && h.UserId == userId )
            .ExecuteUpdateAsync( s => s.SetProperty( h => h.UpdatedAt, DateTime.UtcNow ) )
            .DefaultConfigureAwait();
        await m_dbContext.SaveChangesAsync().DefaultConfigureAwait();

        return new ProgressSavedResponse { Id = progress.Id };
    }
}
