using AutoMapper;

namespace BusinessLogic;

public class AreaOfLifeService : IAreaOfLifeService
{
    private readonly AppDbContext m_dbContext;
    private readonly IMapper m_mapper;

    public AreaOfLifeService( AppDbContext dbContext, IMapper mapper )
    {
        m_dbContext = dbContext;
        m_mapper = mapper;
    }

    public async Task<List<UserAreaOfLifeDto>> GetAllAsync( long userId )
    {
        List<UserAreaOfLife> userAreasOfLife = await m_dbContext.
            UserAreasOfLife.
            Where( u => u.UserId == userId ).
            ToListAsync();

        return m_mapper.Map<List<UserAreaOfLifeDto>>( userAreasOfLife );
    }
}
