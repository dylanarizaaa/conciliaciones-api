using NPOI.SS.UserModel;

namespace Conciliaciones.Dominio.DTOs.Requests.Reports
{
    public class ExcelStyles
    {
        public ICellStyle? Titulo { get; set; }
        public ICellStyle? Encabezado { get; set; }

        public ICellStyle? Normal { get; set; }

        public ICellStyle? Moneda { get; set; }

        public ICellStyle? Fecha { get; set; }

        public ICellStyle? Porcentaje { get; set; }

        public ICellStyle? Entero { get; set; }

        public ICellStyle? Decimal { get; set; }

    }
}
