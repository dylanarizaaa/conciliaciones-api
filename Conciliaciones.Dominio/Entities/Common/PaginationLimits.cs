namespace Conciliaciones.Dominio.Entities.Common
{
    public static class PaginationLimits
    {
        public const int DefaultPageSize = 10;

        public const int MaxPageSize = 100;

        public static int NormalizePageNumber(int pageNumber) =>
            pageNumber < 1 ? 1 : pageNumber;

        public static int NormalizePageSize(int pageSize)
        {
            if (pageSize < 1)
            {
                return DefaultPageSize;
            }

            return pageSize > MaxPageSize ? MaxPageSize : pageSize;
        }
    }
}
