using AutoMapper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;

namespace LibraryManagement.API.Helper
{
    public class AutoMapperHandler : Profile
    {
        public AutoMapperHandler()
        {
           
            CreateMap<Book, BookModal>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.Trim()))
                .ReverseMap();

            CreateMap<User, UserModal>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName.Trim()))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Trim()))
                .ReverseMap();

            CreateMap<BookUpdateModal, Book>().ReverseMap();
            CreateMap<Category, CategoryModal>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ReverseMap();

            CreateMap<Address, AddressModal>().ReverseMap();

            CreateMap<BookIssue, IssueDTO>()
                .ForMember(dest => dest.Book, opt => opt.MapFrom(src => src.Book))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));

            CreateMap<SubmitBooksInfo, HistoryModal>()
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.ReturnDate, opt => opt.MapFrom(src => src.ReturnDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
                
            CreateMap<BillingSummary, BillingSummaryModal>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ReverseMap();

            CreateMap<BillingBooksInfo, BillingBooksInfoModal>()
                .ForMember(dest => dest.BookCategory, opt => opt.MapFrom(src => src.BookCategory.Trim()))
                .ForMember(dest => dest.EstimatedReturnDate, opt => opt.MapFrom(src => src.EstimatedReturnDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ReverseMap();
        }
    }
}
