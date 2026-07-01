using Domain.Models.Dto.Statistic;

namespace Application.Interfaces
{
    public interface INoteService
    {
        Task<NoteStatistic> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken);
    }
}
