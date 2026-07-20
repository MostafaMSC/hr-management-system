using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using System.Reflection;
using System.Linq;

namespace HR.Infrastructure.Services;

public class CsvExportService : ICsvExportService
{
    public async Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true)))
        {
            var properties = typeof(T).GetProperties();
            await writer.WriteLineAsync(string.Join(",", properties.Select(p => p.Name)));

            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString()?.Replace(",", " ") ?? "");
                await writer.WriteLineAsync(string.Join(",", values));
            }

            await writer.FlushAsync();
        }
        return memoryStream.ToArray();
    }

    public async Task<Stream> ExportToCsvStream<T>(IAsyncEnumerable<T> data)
    {
        var outputStream = new MemoryStream();
        var writer = new StreamWriter(outputStream, new UTF8Encoding(true), leaveOpen: true);

        var properties = typeof(T).GetProperties();
        await writer.WriteLineAsync(string.Join(",", properties.Select(p => p.Name)));

        await foreach (var item in data)
        {
            var values = properties.Select(p => p.GetValue(item)?.ToString()?.Replace(",", " ") ?? "");
            await writer.WriteLineAsync(string.Join(",", values));
        }

        await writer.FlushAsync();
        outputStream.Position = 0;
        return outputStream;
    }
}
