using Conciliaciones.Dominio.DTOs.Requests.Reports;
using Conciliaciones.Dominio.DTOs.Response.Reports;
using Conciliaciones.Dominio.Ports.Interfaces.Reports;
using Microsoft.Extensions.Logging;
using NPOI.POIFS.Crypt;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Conciliaciones.Infraestructura.Reports
{
    public sealed class ConciliacionExcelReportService : IConciliacionExcelReportPort
    {
        // Editable: cambiar la  lñword aquí (vacío = workbook sin cifrado OLE2).
        public static string ExcelWorkbookPassword { get; set; } = "1234";

        private readonly ILogger<ConciliacionExcelReportService> _logger;

        public ConciliacionExcelReportService(ILogger<ConciliacionExcelReportService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public byte[] GenerarExcelConciliaciones(ConciliacionExportDataDto data)
        {
            ArgumentNullException.ThrowIfNull(data);

            _logger.LogInformation(
                "[ExportExcel][NPOI] Generando. columns={Cols} rows={Rows} regla={Regla}",
                data.Columns.Count,
                data.Rows.Count,
                data.NombreRegla);

            XSSFWorkbook workbook = new();
            XSSFSheet worksheet = (XSSFSheet)workbook.CreateSheet("Conciliaciones");
            ExcelStyles estilos = ConstruirEstilos(workbook);

            int colCount = Math.Max(data.Columns.Count, 1);
            DibujarCabecera(worksheet, data, estilos, colCount);
            int headerRowIndex = DibujarTabla(worksheet, data, estilos);

            for (int i = 0; i < data.Columns.Count; i++)
            {
                worksheet.SetColumnWidth(i, 22 * 256);
            }

            _logger.LogInformation(
                "[ExportExcel][NPOI] Tabla dibujada. headerRow={HeaderRow} lastDataHint={Rows}",
                headerRowIndex,
                data.Rows.Count);

            if (!string.IsNullOrWhiteSpace(ExcelWorkbookPassword))
            {
                // DEBUG: el base64 del API será el contenedor OLE2 cifrado (no xlsx plano).
                _logger.LogInformation("[ExportExcel][NPOI] Aplicando cifrado OLE2 con password configurable.");
                return GenerarExcelEncriptado(workbook, ExcelWorkbookPassword);
            }

            using MemoryStream stream = new();
            workbook.Write(stream, leaveOpen: false);
            return stream.ToArray();
        }

        private static void DibujarCabecera(
            XSSFSheet worksheet,
            ConciliacionExportDataDto data,
            ExcelStyles estilos,
            int colCount)
        {
            int lastCol = Math.Max(colCount - 1, 0);

            CrearCelda(worksheet, 0, 0, "Reporte de conciliaciones", estilos.Titulo!);
            worksheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, lastCol));

            CrearCelda(worksheet, 2, 0, "Regla", estilos.Encabezado!);
            CrearCelda(worksheet, 2, 1, "Casos", estilos.Encabezado!);
            CrearCelda(worksheet, 2, 2, "Monto afectado", estilos.Encabezado!);

            CrearCelda(worksheet, 3, 0, data.NombreRegla, estilos.Normal!);
            CrearCelda(worksheet, 3, 1, data.TotalCount.ToString(), estilos.Normal!);
            CrearCeldaNumerica(worksheet, 3, 2, (double)data.MontoAfectado, estilos.Moneda!);
        }

        private static int DibujarTabla(
            XSSFSheet worksheet,
            ConciliacionExportDataDto data,
            ExcelStyles estilos)
        {
            int filaActual = 5;
            CrearCelda(
                worksheet,
                filaActual,
                0,
                $"Registros exportados para la regla '{data.NombreRegla}'",
                estilos.Normal!);
            if (data.Columns.Count > 1)
            {
                worksheet.AddMergedRegion(new CellRangeAddress(filaActual, filaActual, 0, data.Columns.Count - 1));
            }

            // Encabezados = Header de UI (sin accion). DEBUG: orden = Order Visible de Fuente.
            filaActual++;
            int headerRow = filaActual;
            for (int i = 0; i < data.Columns.Count; i++)
            {
                CrearCelda(worksheet, filaActual, i, data.Columns[i].Header, estilos.Encabezado!);
            }

            filaActual++;
            foreach (IReadOnlyDictionary<string, object?> row in data.Rows)
            {
                IRow excelRow = worksheet.CreateRow(filaActual);
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    ConciliacionExportColumnDto column = data.Columns[i];
                    row.TryGetValue(column.RowKey, out object? raw);

                    // DEBUG: si la celda queda vacía, RowKey no coincide con keys del dictionary de la fila.
                    if (EsNumerico(column.CellType, raw, out double numero))
                    {
                        CrearCeldaNumericaEnFila(excelRow, i, numero, estilos.Moneda!);
                    } else
                    {
                        CrearCeldaEnFila(excelRow, i, FormatearValor(raw), estilos.Normal!);
                    }
                }

                filaActual++;
            }

            return headerRow;
        }

        private static bool EsNumerico(string? cellType, object? raw, out double numero)
        {
            numero = 0;
            if (raw == null)
            {
                return false;
            }

            bool tipadoMoneda = !string.IsNullOrWhiteSpace(cellType)
                && cellType.Equals("currency", StringComparison.OrdinalIgnoreCase);

            switch (raw)
            {
                case double d:
                    numero = d;
                    return true;
                case float f:
                    numero = f;
                    return true;
                case decimal m:
                    numero = (double)m;
                    return true;
                case int i when tipadoMoneda:
                    numero = i;
                    return true;
                case long l when tipadoMoneda:
                    numero = l;
                    return true;
                default:
                    if (tipadoMoneda
                        && double.TryParse(
                            Convert.ToString(raw),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double parsed))
                    {
                        numero = parsed;
                        return true;
                    }

                    return false;
            }
        }

        private static string FormatearValor(object? raw)
        {
            if (raw == null)
            {
                return string.Empty;
            }

            if (raw is DateTime dt)
            {
                return dt.ToString("dd/MM/yyyy HH:mm");
            }

            return Convert.ToString(raw) ?? string.Empty;
        }

        private static byte[] GenerarExcelEncriptado(XSSFWorkbook workbook, string password)
        {
            byte[] contenidoLimpio;
            using (MemoryStream tempStream = new())
            {
                workbook.Write(tempStream, leaveOpen: false);
                contenidoLimpio = tempStream.ToArray();
            }

            using MemoryStream msCifrado = new();
            // NPOI 2.7+: usar NPOIFSFileSystem + WriteFileSystem(Stream).
            NPOIFSFileSystem fs = new();
            EncryptionInfo info = new(EncryptionMode.Standard);
            Encryptor enc = info.Encryptor;
            enc.ConfirmPassword(password);

            using (Stream os = enc.GetDataStream(fs))
            {
                os.Write(contenidoLimpio, 0, contenidoLimpio.Length);
            }

            fs.WriteFileSystem(msCifrado);
            return msCifrado.ToArray();
        }

        private static void CrearCelda(XSSFSheet sheet, int rowIndex, int colIndex, string value, ICellStyle style)
        {
            IRow row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell = row.GetCell(colIndex) ?? row.CreateCell(colIndex);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        private static void CrearCeldaNumerica(XSSFSheet sheet, int rowIndex, int colIndex, double value, ICellStyle style)
        {
            IRow row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            ICell cell = row.GetCell(colIndex) ?? row.CreateCell(colIndex);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        private static void CrearCeldaEnFila(IRow row, int colIndex, string value, ICellStyle style)
        {
            ICell cell = row.CreateCell(colIndex);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        private static void CrearCeldaNumericaEnFila(IRow row, int colIndex, double value, ICellStyle style)
        {
            ICell cell = row.CreateCell(colIndex);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        private static ExcelStyles ConstruirEstilos(XSSFWorkbook workbook)
        {
            ExcelStyles estilos = new();
            IFont fontBold = workbook.CreateFont();
            fontBold.IsBold = true;
            IDataFormat format = workbook.CreateDataFormat();

            estilos.Titulo = workbook.CreateCellStyle();
            estilos.Titulo.Alignment = HorizontalAlignment.Center;
            estilos.Titulo.VerticalAlignment = VerticalAlignment.Center;
            estilos.Titulo.SetFont(fontBold);

            estilos.Encabezado = workbook.CreateCellStyle();
            estilos.Encabezado.Alignment = HorizontalAlignment.Center;
            estilos.Encabezado.VerticalAlignment = VerticalAlignment.Center;
            estilos.Encabezado.SetFont(fontBold);
            estilos.Encabezado.BorderTop = BorderStyle.Thin;
            estilos.Encabezado.BorderBottom = BorderStyle.Thin;
            estilos.Encabezado.BorderLeft = BorderStyle.Thin;
            estilos.Encabezado.BorderRight = BorderStyle.Thin;

            estilos.Normal = workbook.CreateCellStyle();
            estilos.Normal.BorderTop = BorderStyle.Thin;
            estilos.Normal.BorderBottom = BorderStyle.Thin;
            estilos.Normal.BorderLeft = BorderStyle.Thin;
            estilos.Normal.BorderRight = BorderStyle.Thin;

            estilos.Moneda = workbook.CreateCellStyle();
            estilos.Moneda.DataFormat = format.GetFormat("$ #,##0.00");
            estilos.Moneda.VerticalAlignment = VerticalAlignment.Center;
            estilos.Moneda.BorderTop = BorderStyle.Thin;
            estilos.Moneda.BorderBottom = BorderStyle.Thin;
            estilos.Moneda.BorderLeft = BorderStyle.Thin;
            estilos.Moneda.BorderRight = BorderStyle.Thin;

            return estilos;
        }
    }
}
