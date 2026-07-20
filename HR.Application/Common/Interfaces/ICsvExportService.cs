using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Common.Interfaces;

public interface ICsvExportService
{
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data);
    Task<Stream> ExportToCsvStream<T>(IAsyncEnumerable<T> data);
}
