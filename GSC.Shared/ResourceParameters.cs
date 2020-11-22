namespace GSC.Shared
{
    public abstract class ResourceParameters
    {
        private const int MaxPageSize = 100;

        private int _pageSize = 10;

        public ResourceParameters(string orderBy)
        {
            OrderBy = orderBy;
        }

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string SearchQuery { get; set; }

        public string OrderBy { get; set; }

        public string Fields { get; set; }
    }
}