namespace Csharp.Api.DTOs
{
    public class PaginatedResponseDto<T>
    {
        public int TotalItems { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}