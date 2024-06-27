namespace LibraryManagement.API.Modal
{
    public class BillingDetails
    {
        public BillingSummaryModal BillingSummary { get; set; }
        public List<BillingBooksInfoModal> BillingBooksInfo { get; set; }
        public List<IssueDTO> IssueDTOs { get; set; }
    }
}
