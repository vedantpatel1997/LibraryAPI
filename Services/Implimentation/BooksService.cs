using AutoMapper;
using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos;
using LibraryManagement.API.Repos.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Container.Implimentation
{
    public class BooksService : IBooksService
    {
        private readonly Repos.Models.LibraryManagementContext _dbContext;
        private readonly IMapper _mapper;

        public BooksService(LibraryManagementContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<APIResponse<List<BookModal>>> GetAll()
        {
            var response = new APIResponse<List<BookModal>>();
            var sellingCount = 10;
            try
            {
                var data = await _dbContext.Books.Include(book => book.Category).ToListAsync();
                var bookModalData = _mapper.Map<List<Book>, List<BookModal>>(data);

                foreach (var book in bookModalData)
                {
                    var rentalSubmittedCount = await _dbContext.SubmitBooksInfos
                        .Where(x => x.BookId == book.BookId)
                        .CountAsync();

                    var rentalIssuedCount = await _dbContext.BookIssues
                        .Where(x => x.BookId == book.BookId)
                        .CountAsync();

                    var bestSellerCount = rentalIssuedCount + rentalSubmittedCount;
                    if (bestSellerCount >= sellingCount)
                    {
                        book.IsBestSeller = true;
                    }
                    else { book.IsBestSeller = false; }
                }

                response.Data = bookModalData;
                response.IsSuccess = true;
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse<BookUpdateModal>> Create(BookUpdateModal book)
        {
            var response = new APIResponse<BookUpdateModal>();
            try
            {
                if (book.TotalQuantity < 0)
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Total quantity cannot be less than 0";
                    return response;
                }
                var data = _mapper.Map<BookUpdateModal, Book>(book);
                data.AvailableQuantity = data.TotalQuantity;

                await _dbContext.AddAsync(data);
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<Book, BookUpdateModal>(data);
                response.IsSuccess = true;
                response.ResponseCode = 201; // Created
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse<BookModal>> GetById(int bookId)
        {
            var response = new APIResponse<BookModal>();
            try
            {
                var data = await _dbContext.Books.Include(book => book.Category).FirstOrDefaultAsync(i => i.BookId == bookId);
                if (data != null)
                {
                    response.Data = _mapper.Map<Book, BookModal>(data);
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }
        public async Task<APIResponse<List<BookModal>>> GetBooksByIds(int[] bookIds)
        {
            var response = new APIResponse<List<BookModal>>();
            try
            {
                var data = await _dbContext.Books
                    .Include(book => book.Category)
                    .Where(book => bookIds.Contains(book.BookId))
                    .ToListAsync();

                if (data != null && data.Count > 0)
                {
                    response.Data = _mapper.Map<List<Book>, List<BookModal>>(data);
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse> Remove(int bookId)
        {
            var response = new APIResponse();

            try
            {
                // Check if the book exists
                var book = await _dbContext.Books.SingleOrDefaultAsync(b => b.BookId == bookId);

                if (book == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                    return response;
                }

                // Check if the book is issued by any user
                var isBookIssued = await _dbContext.BookIssues.AnyAsync(bi => bi.BookId == bookId);

                if (isBookIssued)
                {
                    // Book is issued, generate an error response
                    response.ResponseCode = 403; // Forbidden
                    response.ErrorMessage = "This book is issued by a user and cannot be deleted.";
                }
                else
                {
                    // Book is not issued, proceed with deletion
                    _dbContext.Remove(book);
                    await _dbContext.SaveChangesAsync();
                    response.IsSuccess = true;
                    response.ResponseCode = 204; // No Content (successful delete)
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }

            return response;
        }

        public async Task<APIResponse> Update(BookUpdateModal book, int bookId)
        {
            var response = new APIResponse();
            try
            {
                if (book.TotalQuantity < 0)
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Total quantity cannot be less than 0";
                    return response;
                }
                var existingBook = await _dbContext.Books.FirstOrDefaultAsync(i => i.BookId == bookId);
                if (existingBook != null)
                {
                    if (existingBook.IssuedQuantity > book.TotalQuantity)
                    {
                        response.ResponseCode = 404;
                        response.ErrorMessage = "Total quantity can not be less than issued quantity";
                        return response;
                    }
                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.TotalQuantity = book.TotalQuantity;
                    existingBook.AvailableQuantity = existingBook.TotalQuantity - existingBook.IssuedQuantity;
                    existingBook.Price = book.Price;
                    existingBook.CategoryId = book.CategoryId;
                    existingBook.ImageUrl = book.ImageURL;
                    await _dbContext.SaveChangesAsync();
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse> RemoveFromCart(int bookId, int userId)
        {
            var response = new APIResponse();
            var cartItem = await _dbContext.Carts.FirstOrDefaultAsync(a => a.BookId == bookId && a.UserId == userId);
            try
            {
                if (cartItem == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Invalid data";

                    return response;
                }

                _dbContext.Carts.Remove(cartItem);
                await _dbContext.SaveChangesAsync();
                response.ResponseCode = 200;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse> AddToCart(int bookId, int userId)
        {
            var response = new APIResponse();

            var cartitem = await _dbContext.Carts.FirstOrDefaultAsync(a => a.UserId == userId && a.BookId == bookId);

            if (cartitem != null)
            {
                response.ResponseCode = 200;
                response.ErrorMessage = "Book is Already in the cart";
                return response;
            }
            var book = await _dbContext.Books.FindAsync(bookId);
            var user = await _dbContext.Users.FindAsync(userId);

            try
            {
                if (book != null && user != null)
                {
                    var addItem = new Cart()
                    {
                        BookId = bookId,
                        UserId = userId,
                    };
                    await _dbContext.Carts.AddAsync(addItem);
                    await _dbContext.SaveChangesAsync();
                    response.ResponseCode = 200;
                    response.IsSuccess = true;

                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse<List<BookModal>>> GetCartItemsByUserId(int userId)
        {
            var response = new APIResponse<List<BookModal>>();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                response.ResponseCode = 404; // Not Found
                response.ErrorMessage = "User not found";
                return response;
            }
            try
            {
                var Bookdata = from c in _dbContext.Carts
                               join b in _dbContext.Books on c.BookId equals b.BookId
                               join u in _dbContext.Users on c.UserId equals u.UserId
                               where u.UserId == userId
                               select b;

                var books = await Bookdata.ToListAsync();

                if (Bookdata != null)
                {
                    response.Data = _mapper.Map<List<Book>, List<BookModal>>(books);
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }
    }
}
