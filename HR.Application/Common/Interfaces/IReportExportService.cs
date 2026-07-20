using System.Data;

namespace HR.Application.Common.Interfaces;

public interface IReportExportService
{
    byte[] GenerateExcel(DataTable data, string sheetName);
    byte[] GeneratePdf(DataTable data, string title);
}
