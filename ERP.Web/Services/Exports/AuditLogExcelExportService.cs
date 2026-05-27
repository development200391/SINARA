using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ERP.Application.DTOs.Config;

namespace ERP.Web.Services.Exports;

public sealed class AuditLogExcelExportService : IAuditLogExcelExportService
{
    private const uint MaxExcelRows = 1_048_576;

    public async Task WriteAsync(string outputPath, IAsyncEnumerable<AuditLogDto> rows, CancellationToken ct = default)
    {
        using var document = SpreadsheetDocument.Create(outputPath, SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

        using (var writer = OpenXmlWriter.Create(worksheetPart))
        {
            writer.WriteStartElement(new Worksheet());
            writer.WriteStartElement(new SheetData());

            var rowIndex = 1u;
            WriteRow(writer, rowIndex++, "Created At", "User", "Action", "Entity", "Entity Id", "IP");

            await foreach (var row in rows.WithCancellation(ct))
            {
                if (rowIndex > MaxExcelRows)
                {
                    throw new InvalidOperationException("Export exceeds Excel row limit.");
                }

                WriteRow(
                    writer,
                    rowIndex++,
                    row.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    row.Username,
                    row.Action,
                    row.EntityName,
                    row.EntityId,
                    row.IpAddress);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        sheets.Append(new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "Audit Logs"
        });

        workbookPart.Workbook.Save();
    }

    private static void WriteRow(OpenXmlWriter writer, uint rowIndex, params string?[] values)
    {
        writer.WriteStartElement(new Row { RowIndex = rowIndex });

        foreach (var value in values)
        {
            writer.WriteElement(new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString
                {
                    Text = new Text(value ?? string.Empty)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    }
                }
            });
        }

        writer.WriteEndElement();
    }
}
