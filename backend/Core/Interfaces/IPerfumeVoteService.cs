using Core.Enums;

namespace Core.Interfaces;

public interface IPerfumeVoteService
{
    Task SetGenderVoteAsync(int perfumeId, int userId, GenderEnum? gender);
    Task SetSillageVoteAsync(int perfumeId, int userId, SillageEnum? sillage);
    Task SetLongevityVoteAsync(int perfumeId, int userId, LongevityEnum? longevity);
    Task SetSeasonVoteAsync(int perfumeId, int userId, SeasonEnum? season);
    Task SetDaytimeVoteAsync(int perfumeId, int userId, DaytimeEnum? daytime);
}
