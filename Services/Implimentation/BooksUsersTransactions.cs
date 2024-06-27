using AutoMapper;
using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using LibraryManagement.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NodaTime;
using System.Net;
using System.Transactions;

namespace LibraryManagement.API.Container.Implimentation
{
    public class BooksUsersTransactions : IBooksUsersTransactions
    {
        private readonly LibraryManagementContext _dbContext;
        private readonly IBooksService _bookSvc;
        private readonly IMapper _mapper;
        private readonly IEmailMessageService _emailMessageService;

        public BooksUsersTransactions(LibraryManagementContext dbContext, IBooksService bookService, IMapper mapper, IEmailMessageService emailMessageService)
        {
            this._dbContext = dbContext;
            this._bookSvc = bookService;
            _mapper = mapper;
            _emailMessageService = emailMessageService;
        }
        public async Task<APIResponse<List<IssueDTO>>> GetBooksByUserId(int userId)
        {
            var response = new APIResponse<List<IssueDTO>>();

            try
            {
                var data = await this._dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
                if (data == null)
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Data not found";
                    return response;
                }

                // Query the database to get books issued to the user with the specified userId
                var issuedBooks = await _dbContext.BookIssues
                    .Include(x => x.User)
                    .Include(x => x.Book)
                    .Include(x => x.Book.Category)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                response.Data = _mapper.Map<List<BookIssue>, List<IssueDTO>>(issuedBooks);
                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }

            return response;
        }
        public async Task<APIResponse<List<IssueDTO>>> GetDueBooks()
        {
            var response = new APIResponse<List<IssueDTO>>();

            try
            {
                // Query the database to get books issued to the user with the specified userId
                var issuedBooks = await _dbContext.BookIssues
                    .Include(x => x.User)
                    .Include(x => x.Book)
                    .Include(x => x.Book.Category)
                    .ToListAsync();

                response.Data = _mapper.Map<List<BookIssue>, List<IssueDTO>>(issuedBooks);
                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse<List<SubmitBooksInfo>>> GetBooksHistoryByUserId(int userId)
        {
            var response = new APIResponse<List<SubmitBooksInfo>>();
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    response.ResponseCode = 401;
                    response.ErrorMessage = "User not found";
                    return response;

                }
                var data = await _dbContext.SubmitBooksInfos.Where(i => i.UserId == userId).ToListAsync();
                if (data != null)
                {
                    response.Data = data;
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

        public async Task<APIResponse<List<HistoryModal>>> GetUsersHistoryByBookId(int bookId)
        {
            var response = new APIResponse<List<HistoryModal>>();
            try
            {
                var book = await _dbContext.Books.FindAsync(bookId);
                if (book == null)
                {
                    response.ResponseCode = 401;
                    response.ErrorMessage = "Book not found";
                    return response;
                }

                var data = await _dbContext.SubmitBooksInfos.Where(i => i.BookId == bookId).OrderBy(x => x.IssueDate).ToListAsync();

                if (data != null)
                {
                    var history = _mapper.Map<List<SubmitBooksInfo>, List<HistoryModal>>(data);
                    foreach (var item in history)
                    {
                        var userTemp = await _dbContext.Users.FindAsync(item.UserId);
                        item.Book = _mapper.Map<Book, BookModal>(book);
                        item.User = _mapper.Map<User, UserModal>(userTemp);
                    }
                    response.Data = history;
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

        public async Task<APIResponse<List<IssueDTO>>> GetUsersByBookId(int bookId)
        {
            var response = new APIResponse<List<IssueDTO>>();

            try
            {
                var data = await this._dbContext.Books.FirstOrDefaultAsync(x => x.BookId == bookId);
                if (data == null)
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Data not found";
                    return response;
                }

                // Query the database to get users who have issued the book with the specified bookId
                var issuedToUsers = await _dbContext.BookIssues
                    .Include(x => x.User)
                    .Include(x => x.Book.Category)
                    .Where(x => x.BookId == bookId)
                    .ToListAsync();

                response.Data = _mapper.Map<List<BookIssue>, List<IssueDTO>>(issuedToUsers);
                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }

            return response;
        }

        public async Task<APIResponse> IssueBook(IssueDTO issueDTO)
        {
            var response = new APIResponse();

            try
            {
                // Retrieve the book
                var book = await _dbContext.Books.FirstOrDefaultAsync(x => x.BookId == issueDTO.BookId);
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == issueDTO.UserId);

                if (book == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Invalid Book.";
                    return response;
                }
                else if (user == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Invalid User.";
                    return response;
                }

                // Check if the book is already issued
                var isBookIssued = await _dbContext.BookIssues.AnyAsync(bi => bi.BookId == issueDTO.BookId && bi.UserId == issueDTO.UserId);

                if (isBookIssued)
                {
                    response.ResponseCode = 400; // Bad Request
                    response.ErrorMessage = $"{user.FirstName} has already issued this book.";
                    return response;
                }

                // Check available quantity
                if (book.AvailableQuantity == 0)
                {
                    response.ResponseCode = 400; // Bad Request
                    response.ErrorMessage = "No available copies of this book.";
                    return response;
                }

                // Calculate the return date
                var returnDate = issueDTO.IssueDate?.AddDays(issueDTO.Days);

                // Begin a transaction
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Issue the book
                        var availableQty = book.AvailableQuantity - 1;
                        var issueQty = book.IssuedQuantity + 1;
                        book.AvailableQuantity = availableQty;
                        book.IssuedQuantity = issueQty;
                        await _dbContext.SaveChangesAsync();

                        // Create a book issue record
                        var bookIssue = new BookIssue()
                        {
                            BookId = issueDTO.BookId,
                            UserId = issueDTO.UserId,
                            IssueDate = DateTime.Now,
                            Days = issueDTO.Days,
                        };

                        await _dbContext.BookIssues.AddAsync(bookIssue);
                        await _dbContext.SaveChangesAsync();

                        // Commit the transaction
                        transaction.Commit();

                        response.IsSuccess = true;
                        response.ResponseCode = 200; // OK
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();

                        response.ErrorMessage = ex.Message;
                        response.ResponseCode = 500; // Internal Server Error
                    }
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }

            return response;
        }

        public async Task<APIResponse> IssueBooks(BillingDetails billingDetails)
        {
            var response = new APIResponse();

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var curIssueDTO in billingDetails.IssueDTOs)
                    {
                        // Retrieve the book
                        var book = await _dbContext.Books.FirstOrDefaultAsync(x => x.BookId == curIssueDTO.BookId);
                        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == curIssueDTO.UserId);

                        if (book == null)
                        {
                            response.ResponseCode = 404; // Not Found
                            response.ErrorMessage = "Invalid Book.";
                            return response;
                        }

                        else if (user == null)
                        {
                            response.ResponseCode = 404; // Not Found
                            response.ErrorMessage = "Invalid User.";
                            return response;
                        }
                        else if (user.AddressId == null)
                        {
                            response.ResponseCode = 404; // Not Found
                            string registrationLink = "ng-reflect-router-link=\"/Books/User/Address\"";
                            response.ErrorMessage = $"Please register your <a {registrationLink}>address</a> first!";
                            return response;

                        }

                        // Check if the book is already issued
                        var isBookIssued = await _dbContext.BookIssues.AnyAsync(bi => bi.BookId == curIssueDTO.BookId && bi.UserId == curIssueDTO.UserId);

                        if (isBookIssued)
                        {
                            response.ResponseCode = 400; // Bad Request
                            response.ErrorMessage = $"{user.FirstName} has already issued one of these book.";
                            return response;
                        }

                        // Check available quantity
                        if (book.AvailableQuantity == 0)
                        {
                            response.ResponseCode = 400; // Bad Request
                            response.ErrorMessage = "No available copies of this book.";
                            return response;
                        }


                        try
                        {
                            // Issue the book
                            var availableQty = book.AvailableQuantity - 1;
                            var issueQty = book.IssuedQuantity + 1;
                            book.AvailableQuantity = availableQty;
                            book.IssuedQuantity = issueQty;
                            await _dbContext.SaveChangesAsync();

                            // Create a book issue record
                            var bookIssue = new BookIssue()
                            {
                                BookId = curIssueDTO.BookId,
                                UserId = curIssueDTO.UserId,
                                IssueDate = DateTime.UtcNow,
                                Days = curIssueDTO.Days
                            };

                            await _bookSvc.RemoveFromCart(curIssueDTO.BookId, curIssueDTO.UserId);

                            await _dbContext.BookIssues.AddAsync(bookIssue);
                            await _dbContext.SaveChangesAsync();


                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction in case of an error
                            transaction.Rollback();

                            response.ErrorMessage = ex.Message;
                            response.ResponseCode = 500; // Internal Server Error
                            return response; // Return the response immediately upon error
                        }
                    }

                    var billResponse = await GenerateBill(billingDetails.BillingSummary, billingDetails.BillingBooksInfo);

                    // Check if bill is generated successfully, otherwisw throw error.
                    if (!billResponse.IsSuccess)
                    {
                        return billResponse;
                    }
                    // Commit the transaction after the loop
                    transaction.Commit();

                    if (billingDetails.IssueDTOs.Count > 0)
                    {

                        var user = await _dbContext.Users.FindAsync(billingDetails.IssueDTOs[0].UserId);

                        string subject = "Books Issued Successfully";

                        string bodyHtml = $@"
                                <div style='max-width: 600px; margin: 0 left;'>
                                    <h2>Books Issued Successfully</h2>
                                    <p>Dear {user.FirstName} {user.LastName},</p>
                                    <p>You have successfully issued the following books from the Library:</p>
                                    <table style='font-size: 14px; color: #666; border-collapse: collapse; width: 100%; border: 1px solid #333; border-radius: 8px;'>
                                        <tr>
                                            <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Book Title</th>
                                            <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Days</th>
                                            <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Due Date</th>
                                        </tr>
                            ";

                        foreach (var issueDTO in billingDetails.IssueDTOs)
                        {
                            var book = await _dbContext.Books.FindAsync(issueDTO.BookId);
                            var estimatedReturnDate = DateTime.UtcNow.AddDays(issueDTO.Days);
                            bodyHtml += $@"
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #333;'>{book?.Title}</td>
                                    <td style='padding: 8px; border: 1px solid #333;'>{issueDTO.Days}</td>
                                    <td style='padding: 8px; border: 1px solid #333;'>{TimeZoneConverter.ConvertUtcToTimeZone(estimatedReturnDate, user.Timezone).ToString("f")} (EOD)</td>
                                ";
                        }

                        bodyHtml += @"
                                </table>
                                <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                                <br> <!-- Added space before the closing remarks -->
                                <p><strong>Important Note:</strong></p>
                                <p>If any of the issued books are submitted after the due date, a penalty may be applied as per our Library Policy. Please ensure timely return to avoid any additional charges.</p>
                                <br> <!-- Added space before the closing remarks -->
                                <p>Thank you for using our Library Management System.</p>
                                <p style='text-align: left;'>Sincerely,<br>Vedant Patel</p>
                                <p style='text-align: left;'>Library Owner/Administrator</p>
                                <br/>
                                <br/>
                            </div>
                        ";
                        await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);
                    }



                    response.IsSuccess = true;
                    response.ResponseCode = 200; // OK
                }
                catch (Exception ex)
                {
                    response.ErrorMessage = ex.Message;
                    response.ResponseCode = 500; // Internal Server Error
                }
            }

            return response;
        }


        public async Task<APIResponse> SendTemp(int userId)
        {
            APIResponse response = new APIResponse();
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Retrieve user information
                    var user = await _dbContext.Users.FindAsync(userId);
                    string subject = "Hello Darling ";

                    for (int i = 0; i < 50; i++)
                    {
                        // Compose the email body with the custom message
                        string bodyHtml = $@"
                    <div style='max-width: 600px; margin: 0 left;'>
                        <h2>Hello Darling Reminder</h2>
                        <p>Dear {user.FirstName} {user.LastName},</p>
                        <p>{i + 1}. Hello darling!</p>
                        <br> <!-- Added space before the closing remarks -->
                        <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                        <br> <!-- Added space before the closing remarks -->
                        <p>Thank you for being part of our community.</p>
                        <p style='text-align: left;'>Sincerely,<br>Your Name</p>
                        <br/>
                        <br/>
                    </div>
                ";

                        // Send the email
                        await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);
                    }

                    // Commit the transaction after sending reminders
                    transaction.Commit();

                    response.IsSuccess = true;
                    response.ResponseCode = 200; // OK
                }
                catch (Exception ex)
                {
                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();

                    response.ErrorMessage = ex.Message;
                    response.ResponseCode = 500; // Internal Server Error
                }
            }

            return response;
        }

        public async Task<APIResponse> SubmitBook(SubmitDTO SubmitDTO)
        {
            var response = new APIResponse();

            try
            {
                // Retrieve the book
                var book = await _dbContext.Books.Include(c => c.Category).FirstOrDefaultAsync(x => x.BookId == SubmitDTO.BookId);
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == SubmitDTO.UserId);

                if (book == null || user == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Invalid Data.";
                    return response;
                }


                // Check if the book is already issued
                var isBookIssued = await _dbContext.BookIssues.AnyAsync(bi => bi.BookId == SubmitDTO.BookId && bi.UserId == SubmitDTO.UserId);

                if (!isBookIssued)
                {
                    response.ResponseCode = 400; // Bad Request
                    response.ErrorMessage = $"{user.FirstName} has not issued this book.";
                    return response;
                }

                // Check available quantity
                if (book.AvailableQuantity < 0)
                {
                    response.ResponseCode = 400; // Bad Request
                    response.ErrorMessage = "Inaccurate book count.";
                    return response;
                }

                // Begin a transaction
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try

                    {
                        // Getting a book issue record
                        var bookIssued = await _dbContext.BookIssues.FirstOrDefaultAsync(x => x.UserId == SubmitDTO.UserId && x.BookId == SubmitDTO.BookId);

                        // Generate bill record if penalty is there


                        // Calculate late days without considering the time
                        int lateDays = (TimeZoneConverter.ConvertUtcToTimeZone(DateTime.UtcNow, user.Timezone).Date - TimeZoneConverter.ConvertUtcToTimeZone(bookIssued.IssueDate.AddDays(bookIssued.Days), user.Timezone).Date).Days;
                        var latePenaltyMessage = "";
                        decimal taxRate = 0.13M;
                        if (lateDays > 0)
                        {
                            // Assuming penalty is $1 per day
                            decimal penaltyAmount = 1 * lateDays;
                            // Calculate tax on the penalty amount
                            decimal tax = penaltyAmount * taxRate;
                            // Calculate the total amount including penalty and tax
                            decimal totalAmount = penaltyAmount + tax;

                            var billingSummary = new BillingSummary
                            {
                                UserId = SubmitDTO.UserId,
                                UserFirstName = user.FirstName,
                                UserLastName = user.LastName,
                                UserEmail = user.Email,
                                UserPhone = user.Phone,
                                Date = DateTime.UtcNow,
                                BookQuantity = 1,
                                Delivery = false,
                                Pickup = false,
                                AddressId = (int)user.AddressId,
                                Tax = tax,
                                TotalAmount = totalAmount,
                            };
                            await _dbContext.BillingSummaries.AddAsync(billingSummary);
                            await _dbContext.SaveChangesAsync();

                            var billingbookInfo = new BillingBooksInfo
                            {
                                BookId = book.BookId,
                                BookName = book.Title,
                                RentDays = bookIssued.Days,
                                EstimatedReturnDate = bookIssued.IssueDate.AddDays(bookIssued.Days),
                                BookAuthor = book.Author,
                                BookCategory = book.Category.Name,
                                BookOriginalPrice = book.Price,
                                BookRentPrice = penaltyAmount,
                                BookImageUrl = book.ImageUrl,
                                BillingId = billingSummary.BillingId
                            };
                            await _dbContext.BillingBooksInfos.AddAsync(billingbookInfo);
                            await _dbContext.SaveChangesAsync();

                            latePenaltyMessage = $"<p style='color: red;'>Late Penalty Applied: ${penaltyAmount} for {lateDays} days.</p>";

                            // Generate an invoice for late penalty
                            // Add the necessary code to create an invoice and save it to the database or file system
                            // ...

                            // You can include a link to the invoice in the email or provide instructions on where to find it.
                            latePenaltyMessage += "<p>Your invoice for  late penalty is available in the Invoice section of your account.</p>";

                        }

                        // Update the book count
                        var availableQty = book.AvailableQuantity + 1;
                        var issueQu = book.IssuedQuantity - 1;
                        book.AvailableQuantity = availableQty;
                        book.IssuedQuantity = issueQu;


                        // Add book to submitInfo

                        var submittedBook = new SubmitBooksInfo()
                        {
                            BookId = bookIssued.BookId,
                            BookTitle = book.Title,
                            UserId = bookIssued.UserId,
                            IssueDate = bookIssued.IssueDate,
                            ReturnDate = DateTime.UtcNow,
                            Days = bookIssued.Days,
                        };

                        _dbContext.SubmitBooksInfos.Add(submittedBook);
                        _dbContext.BookIssues.Remove(bookIssued);


                        await _dbContext.SaveChangesAsync();

                        string subject = "Book Submission Confirmation";

                        var issueDate = submittedBook?.IssueDate ?? DateTime.Now; // Use the issue date from the book record or current date as fallback
                        var dueDate = issueDate.AddDays(submittedBook.Days); // Calculate the due date
                        var submissionDate = DateTime.UtcNow; // Get the current date and time of submission

                        string bodyHtml = $@"
                            <div style='max-width: 700px; margin: 0 left;'>
                                <h2 style='text-align: left;'>Book Submission Confirmation</h2>
                                <p>Dear {user.FirstName} {user.LastName},</p>
                                <p>You have successfully submitted the following book to the Library:</p>
                                <table style='font-size: 14px; color: #666; border-collapse: collapse; width: 100%; border: 1px solid #333; border-radius: 8px;'>
                                    <tr>
                                        <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Book Title</th>
                                        <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Due Date</th>
                                        <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Submitted Date</th>
                                        <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Submit on Time</th>
                                    </tr>
                                     <tr>
                                        <td style='padding: 8px; border: 1px solid #333;'>{book.Title}</td>
    
                                        <!-- Convert dueDate to user's time zone -->
                                        <td style='padding: 8px; border: 1px solid #333;'> {TimeZoneConverter.ConvertUtcToTimeZone(dueDate, user.Timezone).ToString("f")}</td>
    
                                        <!-- Convert submissionDate to user's time zone -->
                                        <td style='padding: 8px; border: 1px solid #333;'>{TimeZoneConverter.ConvertUtcToTimeZone(submissionDate, user.Timezone).ToString("f")}</td>
    
                                        <!-- Check if submissionDate is on or before dueDate -->
                                        <td style='padding: 8px; border: 1px solid #333;'>{(TimeZoneConverter.ConvertUtcToTimeZone(submissionDate, user.Timezone).Date <= TimeZoneConverter.ConvertUtcToTimeZone(dueDate, user.Timezone).Date ? "Yes" : "No")}</td>
                                    </tr>
                                </table>
                                {latePenaltyMessage}
                                <br/>
                                <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                                <br> <!-- Added space before the closing remarks -->
                                <p>Thank you for using our Library Management System.</p>
                                <p style='text-align: left;'>Sincerely,<br>Vedant Patel</p>
                                <p style='text-align: left;'>Library Owner/Administrator</p>
                                <br/>
                                <br/>
                            </div>
                        ";
                        await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);
                        // Commit the transaction
                        transaction.Commit();

                        response.IsSuccess = true;
                        response.ResponseCode = 200; // OK
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();

                        response.ErrorMessage = ex.Message;
                        response.ResponseCode = 500; // Internal Server Error
                    }
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }

            return response;
        }

        public async Task<APIResponse> SendReminderForPendingBooks(int userId)
        {
            APIResponse response = new APIResponse();
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Retrieve the user's issued books
                    var issuedBooks = await _dbContext.BookIssues
                        .Include(x => x.Book)
                        .Where(x => x.UserId == userId)
                        .ToListAsync();

                    if (issuedBooks.Count > 0)
                    {
                        var user = await _dbContext.Users.FindAsync(userId);
                        string subject = "Library Book Return Reminder";

                        string bodyHtml = $@"
                    <div style='max-width: 600px; margin: 0 left;'>
                        <h2>Library Book Return Reminder</h2>
                        <p>Dear {user.FirstName} {user.LastName},</p>
                        <p>This is a friendly reminder about the following books currently issued from the Library:</p>
                        <table style='font-size: 14px; color: #666; border-collapse: collapse; width: 100%; border: 1px solid #333; border-radius: 8px;'>
                            <tr>
                                <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Book Title</th>
                                <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Due Date</th>
                            </tr>
                         ";

                        foreach (var issuedBook in issuedBooks)
                        {
                            var dueDate = issuedBook.IssueDate.AddDays(issuedBook.Days); // Calculate the due date

                            bodyHtml += $@"
                        <tr>
                            <td style='padding: 8px; border: 1px solid #333;'>{issuedBook.Book?.Title}</td>
                            <td style='padding: 8px; border: 1px solid #333;'>{TimeZoneConverter.ConvertUtcToTimeZone(dueDate, user.Timezone).ToString("f")} (EOD)</td>
                        ";
                        }

                        bodyHtml += @"
                        </table>
                        <p>Check invoice for this order in 'Your Invoices' section.</p>
                        <br> <!-- Added space before the closing remarks -->
                        <p>Please ensure to return these books by the due date to avoid any late fees or penalties.</p>
                        <p>If you have already returned these books, kindly disregard this reminder.</p>
                        <br> <!-- Added space before the closing remarks -->
                        <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                        <br> <!-- Added space before the closing remarks -->
                        <p>Thank you for using our Library Management System.</p>
                        <p style='text-align: left;'>Sincerely,<br>Vedant Patel</p>
                        <p style='text-align: left;'>Library Owner/Administrator</p>
                        <br/>
                        <br/>
                    </div>
                ";
                        await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);
                    }

                    // Commit the transaction after sending reminders
                    transaction.Commit();

                    response.IsSuccess = true;
                    response.ResponseCode = 200; // OK
                }
                catch (Exception ex)
                {
                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();

                    response.ErrorMessage = ex.Message;
                    response.ResponseCode = 500; // Internal Server Error
                }
            }

            return response;
        }
        public async Task<APIResponse> SendReminderForPendingBook(int userId, int bookId)
        {
            APIResponse response = new APIResponse();

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Retrieve the user's issued book with the specified bookId
                    var issuedBook = await _dbContext.BookIssues
                        .Include(x => x.Book)
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == bookId);

                    if (issuedBook != null)
                    {
                        var user = await _dbContext.Users.FindAsync(userId);
                        string subject = "Library Book Return Reminder";

                        // Calculate the due date for the specific book
                        var dueDate = issuedBook.IssueDate.AddDays(issuedBook.Days);
                        //dueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);

                        string bodyHtml = $@"
                            <div style='max-width: 700px; margin: 0 left;'>
                                <h2>Library Book Return Reminder</h2>
                                <p>Dear {user.FirstName} {user.LastName},</p>
                                <p>This is a friendly reminder about the book currently issued from the Library:</p>
                                <table style='font-size: 14px; color: #666; border-collapse: collapse; width: 100%; border: 1px solid #333; border-radius: 8px;'>
                                    <tr>
                                        <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Book Title</th>
                                        <th style='padding: 8px; text-align: left; border: 1px solid #333;'>Due Date</th>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border: 1px solid #333;'>{issuedBook.Book?.Title}</td>
                                        <td style='padding: 8px; border: 1px solid #333;'>{TimeZoneConverter.ConvertUtcToTimeZone(dueDate, user.Timezone).ToString("f")} (EOD)</td>
                                    </tr>
                                </table>
                                <p>Please ensure to return this book by the due date to avoid any late fees or penalties.</p>
                                <p>If you have already returned this book, kindly disregard this reminder.</p>
                                <br> <!-- Added space before the closing remarks -->
                                <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                                <br> <!-- Added space before the closing remarks -->
                                <p>Thank you for using our Library Management System.</p>
                                <p style='text-align: left;'>Sincerely,<br>Vedant Patel</p>
                                <p style='text-align: left;'>Library Owner/Administrator</p>
                                <br/>
                                <br/>
                            </div>
                        ";

                        await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);

                        // Commit the transaction after sending the reminder
                        transaction.Commit();

                        response.IsSuccess = true;
                        response.ResponseCode = 200; // OK
                    }
                    else
                    {
                        response.ResponseCode = 404; // Not Found
                        response.ErrorMessage = "Book not found or not issued to the user.";
                    }
                }
                catch (Exception ex)
                {
                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();

                    response.ErrorMessage = ex.Message;
                    response.ResponseCode = 500; // Internal Server Error
                }
            }

            return response;
        }

        public async Task<APIResponse> GenerateBill(BillingSummaryModal billingSummary, List<BillingBooksInfoModal> booksInfo)
        {
            APIResponse response = new APIResponse();

            try
            {
                var billingSummaryData = _mapper.Map<BillingSummaryModal, BillingSummary>(billingSummary);

                var user = await _dbContext.Users.FindAsync(billingSummaryData.UserId);
                billingSummaryData.Date = DateTime.UtcNow;
                billingSummaryData.UserLastName = user.LastName;
                billingSummaryData.UserFirstName = user.FirstName;
                billingSummaryData.UserPhone = user.Phone;
                billingSummaryData.UserEmail = user.Email;
                billingSummaryData.AddressId = (int)user.AddressId;

                _dbContext.BillingSummaries.Add(billingSummaryData);
                await _dbContext.SaveChangesAsync();

                var booksInfoData = _mapper.Map<List<BillingBooksInfoModal>, List<BillingBooksInfo>>(booksInfo);
                foreach (var book in booksInfoData)
                {
                    var curBook = await _dbContext.Books.Include(x => x.Category).FirstOrDefaultAsync(x => x.BookId == book.BookId);
                    book.BookName = curBook.Title;
                    book.BookAuthor = curBook.Author;
                    book.BookCategory = curBook.Category.Name;
                    book.BookImageUrl = curBook.ImageUrl;
                    book.BillingId = billingSummaryData.BillingId;
                    book.EstimatedReturnDate = DateTime.UtcNow.AddDays(book.RentDays);

                    _dbContext.BillingBooksInfos.Add(book);
                    await _dbContext.SaveChangesAsync();
                }

                // If everything is successful, commit the transaction

                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (Exception ex)
            {
                // If an exception occurs, rollback the transaction

                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }

            return response;
        }

        public async Task<APIResponse<List<BillingSummaryModal>>> GetBillsByUserID(int userId)
        {

            var response = new APIResponse<List<BillingSummaryModal>>();

            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "User not found";
                    return response;
                }

                var billingSummary = await _dbContext.BillingSummaries.Include(x => x.BillingBooksInfos).Include(x => x.Address).Where(x => x.UserId == userId).ToListAsync();


                response.Data = _mapper.Map<List<BillingSummary>, List<BillingSummaryModal>>(billingSummary); ;
                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }

            return response;
        }

        public async Task<APIResponse<BillingSummaryModal>> GetBillByBillID(int billId)
        {

            var response = new APIResponse<BillingSummaryModal>();
            try
            {
                var bill = await _dbContext.BillingSummaries.Include(x => x.BillingBooksInfos).Include(x => x.Address).FirstOrDefaultAsync(x => x.BillingId == billId);

                if (bill == null)
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Bill not found";
                    return response;
                }
                response.Data = _mapper.Map<BillingSummary, BillingSummaryModal>(bill);
                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse> SendPDF(IFormFile pdf, int userId)
        {
            var response = new APIResponse();
            try
            {
                // Get the user's email address (replace this with your logic to retrieve the email)
                var user = await _dbContext.Users.FindAsync(userId);
                var userEmail = user.Email;

                // Define email details
                string subject = "Your Library Transaction Receipt";
                string bodyHtml = "Thank you for using our Library Management Service. Here is your transaction receipt:";

                // Convert IFormFile to byte array
                using (var memoryStream = new MemoryStream())
                {
                    await pdf.CopyToAsync(memoryStream);
                    var pdfBytes = memoryStream.ToArray();

                    // Call the SendMessageWithAttachment method to send the email with the PDF attachment
                    await _emailMessageService.SendMessageWithAttachment(userEmail, subject, bodyHtml, pdfBytes, "InvoiceVP");
                }

                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
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
