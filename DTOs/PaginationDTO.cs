namespace AI_Based_Pickleball_Tournament_Management.DTOs
{
    // The properties are nullable on purpose: [AsParameters] flattens this class
    // into individual query string parameters and treats non-nullable ones as
    // required, so "int Page" would make the query string mandatory. Static
    // members are never bound, so these resolvers are safe to live here.
    public class PaginationDTO
    {
        public int? Page { get; set; } = 1;
        public int? RecordsPerPage { get; set; } = 10;

        public static int ResolvePage(int? page) => page is null or < 1 ? 1 : page.Value;

        public static int ResolveRecordsPerPage(int? recordsPerPage) =>
            recordsPerPage is null or < 1 ? 10 : recordsPerPage.Value;
    }
}
