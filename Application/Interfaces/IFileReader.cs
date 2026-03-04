using Domain.Entities.CSV;

namespace Application.Interfaces
{
    public interface IFileReader
    {
        IEnumerable<string> GetFileNames(DataType dataType);
        IEnumerable<T> ReadData<T>(DataType dataType, string fileName, DateTime? startDate = null, DateTime? endDate = null) where T : class;
    }
}
