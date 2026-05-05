using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Infrastructure.Repositoryes.DataBase;

namespace Infrastructure.Repositories.DataBase
{
    public class NoteRepository(AppDbContext context) : GenericRepository<Note, NoteDto>(context)
    {
    }
}
