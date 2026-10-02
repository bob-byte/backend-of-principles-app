namespace BusinessLogic;

public interface IAreaOfLifeService
{
    Task<List<UserAreaOfLifeDto>> GetAllAsync( long userId );
}
