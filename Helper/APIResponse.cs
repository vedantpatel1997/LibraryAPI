namespace LibraryManagement.API.Helper
{
    public class APIResponse<T>
    {
        public int ResponseCode { get; set; }
        public T? Data { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class APIResponse
    {
        public int ResponseCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
