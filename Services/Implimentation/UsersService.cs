using AutoMapper;
using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using LibraryManagement.API.Services.Implimentation;
using LibraryManagement.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Text;

namespace LibraryManagement.API.Container.Implimentation
{
    public class UsersService : IUsersService
    {
        private readonly LibraryManagementContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IEmailMessageService _emailMessageService;
        private readonly IPasswordHasher _passwordHasher;

        public UsersService(LibraryManagementContext dbContext, IMapper mapper, IEmailMessageService emailMessageService, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _emailMessageService = emailMessageService;
            _passwordHasher = passwordHasher;
        }


        public async Task<APIResponse<List<UserModal>>> GetAll()
        {
            APIResponse<List<UserModal>> response = new APIResponse<List<UserModal>>();
            try
            {
                var data = await _dbContext.Users.Where(x => x.Role != "Owner").ToListAsync();
                response.Data = _mapper.Map<List<User>, List<UserModal>>(data);
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

        public async Task<APIResponse<UserModal>> Create(UserModal user)
        {
            APIResponse<UserModal> response = new APIResponse<UserModal>();
            try
            {
                // Check for duplicate values
                var duplicateValue = await GetDuplicateValue(user);
                if (!string.IsNullOrEmpty(duplicateValue))
                {
                    response.ResponseCode = 400; // Bad Request
                    response.ErrorMessage = duplicateValue;
                }
                else
                {
                    // Storing decrypted password to send in email
                    var decryptedPass = user.Password;
                    // Hashing the password for security
                    var passwordHash = _passwordHasher.Hash(user.Password);
                    user.Password = passwordHash;

                    User data = _mapper.Map<UserModal, User>(user);
                    await _dbContext.Users.AddAsync(data);
                    await _dbContext.SaveChangesAsync();
                    string subject = "Welcome to the Library Management System - Your Registration Details";

                    string bodyHtml = $@"
                        <h2>Welcome to the Library Management System</h2>
                        <p>Dear {user.FirstName} {user.LastName},</p>
                        <p>Congratulations! You have successfully registered for our Library Management System. Your account details are as follows:</p>

                        <table>
                            <tr>
                                <td><strong>Email:</strong></td>
                                <td><span>{user.Email}</span></td>
                            </tr>
                            <tr>
                                <td><strong>Username:</strong></td>
                                <td><span>{user.Username}</span></td>
                            </tr>
                            <tr>
                                <td><strong>Password:</strong></td>
                                <td><span>{decryptedPass}</span></td>
                            </tr>
                        </table>

                        <p>Please keep your username and password secure and do not share them with anyone. You now have access to our extensive library collection, where you can borrow and reserve books, manage your account, and enjoy a variety of services.</p>

                        <p>If you have any questions or require assistance, please don't hesitate to reach out to our library staff. We're here to help you make the most of your library experience.</p>

                        <p>Thank you for choosing our Library Management System. We look forward to serving you!</p>

                        <p>Sincerely,<br>Vedant Patel<br>Library Owner/Administrator</p>
                    ";

                    await _emailMessageService.SendMessage(data.Email, subject, bodyHtml);

                    response.Data = _mapper.Map<User, UserModal>(data);
                    response.IsSuccess = true;
                    response.ResponseCode = 201; // Created
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse<UserModal>> GetById(int userId)
        {
            APIResponse<UserModal> response = new APIResponse<UserModal>();
            try
            {
                var data = await _dbContext.Users.FirstOrDefaultAsync(i => i.UserId == userId);
                if (data != null)
                {
                    response.Data = _mapper.Map<User, UserModal>(data);
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

        public async Task<APIResponse> Remove(int userId)
        {
            var response = new APIResponse();

            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                    return response;
                }

                // Check if the user has any pending books to submit
                var hasPendingBooks = await _dbContext.BookIssues
                    .AnyAsync(bi => bi.UserId == userId);

                if (hasPendingBooks)
                {
                    // User has pending books, generate an error response
                    response.ResponseCode = 403; // Forbidden
                    response.ErrorMessage = "This user has some books to submit. Cannot delete.";
                }
                else
                {
                    // Empty the user's cart
                    var userCartItems = await _dbContext.Carts.Where(c => c.UserId == userId).ToListAsync();
                    _dbContext.Carts.RemoveRange(userCartItems);
                    await _dbContext.SaveChangesAsync();


                    // User has no pending books, proceed with deletion
                    _dbContext.Remove(user);
                    await _dbContext.SaveChangesAsync();
                    string subject = "Account Deletion Confirmation";

                    string bodyHtml = $@"
                        <h2>Account Deletion Confirmation</h2>
                        <p>Dear {user.FirstName} {user.LastName},</p>
                        <p>Your account in the Library Management System has been successfully deleted.</p>
                        <p>If you did not initiate this deletion, please contact our support team immediately for assistance.</p>
                        <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                        <br> <!-- Added space before closing remarks -->
                        <p>We're sorry to see you go and hope you had a positive experience using our Library Management System.</p>
                        <p>Sincerely,<br>Vedant Patel</p>
                        <p>Library Owner/Administrator</p>
                    ";

                    await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);

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

        public async Task<APIResponse> Update(UserModal user, int userId)
        {
            APIResponse response = new APIResponse();
            try
            {
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(i => i.UserId == userId);
                if (existingUser != null)
                {
                    // Check for duplicate values
                    var duplicateValue = await GetDuplicateValue(user);
                    if (!string.IsNullOrEmpty(duplicateValue))
                    {
                        response.ResponseCode = 400; // Bad Request
                        response.ErrorMessage = duplicateValue;
                    }
                    else
                    {
                        // Saving the data
                        existingUser.FirstName = user.FirstName;
                        existingUser.LastName = user.LastName;
                        existingUser.Username = user.Username;
                        existingUser.Email = user.Email;
                        existingUser.Dob = user.Dob;
                        existingUser.Gender = user.Gender;
                        existingUser.Phone = user.Phone;

                        await _dbContext.SaveChangesAsync();
                        string subject = "User Information Update Confirmation";

                        string bodyHtml = $@"
                            <h2>User Information Update Confirmation</h2>
                            <p>Dear {user.FirstName} {user.LastName},</p>
                            <p>Your updated user details are as follows:</p>
                            <ul>
                                <li><strong>Username:</strong> {user.Username}</li>
                                <li><strong>Email:</strong> {user.Email}</li>
                                <li><strong>Phone Number:</strong> +1 {user.Phone}</li>
                                <li><strong>Date of Birth:</strong> {user.Dob.ToString("MMMM dd, yyyy")}</li>
                            </ul>
                            <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                            <br> <!-- Added space before closing remarks -->
                            <p>Thank you for using our Library Management System.</p>
                            <p>Sincerely,<br>Vedant Patel</p>
                            <p>Library Owner/Administrator</p>
                            <br/>
                            <br/>
                        ";

                        await _emailMessageService.SendMessage(existingUser.Email, subject, bodyHtml);
                        response.IsSuccess = true;
                        response.ResponseCode = 200;
                    }
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

        private async Task<string> GetDuplicateValue(UserModal user)
        {
            if (await _dbContext.Users.AnyAsync(u =>
                (u.UserId != user.UserId) && // Exclude the current user being updated
                (u.Username == user.Username)))
            {
                return "Username is already registered with us.";
            }
            if (await _dbContext.Users.AnyAsync(u =>
                (u.UserId != user.UserId) && // Exclude the current user being updated
                (u.Email == user.Email)))
            {
                return "Email already exists.";
            }
            //if (await _dbContext.Users.AnyAsync(u =>
            //    (u.UserId != user.UserId) && // Exclude the current user being updated
            //    (u.Phone == user.Phone)))
            //{
            //    return "Phone number is already registered with us.";
            //}
            return null;
        }

        public async Task<APIResponse<AddressModal>> CreateAddress(AddressModal addressData, int userId)
        {
            APIResponse<AddressModal> response = new APIResponse<AddressModal>();
            try
            {
                // Check if the user already has an address
                var existingUser = await _dbContext.Users.FindAsync(userId);
                if (existingUser == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "User not found.";
                    return response;
                }

                if (existingUser.AddressId != null)
                {
                    response.ResponseCode = 400; // Bad Request
                    response.ErrorMessage = "User already has an address registered.";
                    return response;
                }

                Address data = _mapper.Map<AddressModal, Address>(addressData);
                await _dbContext.Addresses.AddAsync(data);
                await _dbContext.SaveChangesAsync();
                int newAddressId = data.AddressId;

                // Update the user's AddressId
                existingUser.AddressId = newAddressId;
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<Address, AddressModal>(data);
                response.IsSuccess = true;
                response.ResponseCode = 201; // Created
            }
            catch (DbUpdateException ex)
            {
                response.ResponseCode = 400; // Bad Request
                response.ErrorMessage = "Failed to create the address. Please check the data provided.";
            }
            catch (Exception ex)
            {
                response.ResponseCode = 500; // Internal Server Error
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse<AddressModal>> GetAddressByUserId(int userId)
        {
            APIResponse<AddressModal> response = new APIResponse<AddressModal>();
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "User not found";
                }
                else if (user.AddressId == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Address data not found for this user";
                }
                else
                {
                    var addressData = await _dbContext.Addresses.FindAsync(user.AddressId);
                    if (addressData != null)
                    {
                        response.Data = _mapper.Map<Address, AddressModal>(addressData);
                        response.IsSuccess = true;
                        response.ResponseCode = 200;
                    }
                    else
                    {
                        response.ResponseCode = 404; // Not Found
                        response.ErrorMessage = "Address data not found";
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
        public async Task<APIResponse<AddressModal>> UpdateAddress(AddressModal addressData, int userId)
        {
            APIResponse<AddressModal> response = new APIResponse<AddressModal>();
            try
            {
                // Find the address to be updated
                var existingAddress = await _dbContext.Addresses.FindAsync(addressData.AddressId);
                if (existingAddress == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Address not found.";
                    return response;
                }

                // Update the address data
                _mapper.Map(addressData, existingAddress);
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<Address, AddressModal>(existingAddress);
                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (DbUpdateException ex)
            {
                response.ResponseCode = 400; // Bad Request
                response.ErrorMessage = "Failed to update the address. Please check the data provided.";
            }
            catch (Exception ex)
            {
                response.ResponseCode = 500; // Internal Server Error
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> UpdatePassword(UpdatePassword password)
        {
            APIResponse response = new APIResponse();
            try
            {
                // Find the user whose password you want to update
                var user = await _dbContext.Users.FindAsync(password.userId); // Replace userId with the appropriate method to find the user based on your application's logic

                if (user == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "User not found.";
                    return response;
                }

                // Check if the old password matches the current password

                if (!_passwordHasher.Verify(user.Password, password.oldPassword))
                {
                    response.ResponseCode = 400; // Bad Request
                    response.ErrorMessage = "Password not match";
                    return response;
                }


                user.Password = _passwordHasher.Hash(password.newPassword);

                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                string subject = "Password Update Confirmation";

                string bodyHtml = $@"
                        <h2>Password Update Confirmation</h2>
                        <p>Dear {user.FirstName} {user.LastName},</p>
                        <p>Your password for the Library Management System has been successfully updated.</p>
                        <p>If you did not initiate this password change, please contact our support team immediately for assistance.</p>
                        <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                        <br> <!-- Added space before contact information -->
                        <p>Thank you for using our Library Management System.</p>
                        <p>Sincerely,<br>Vedant Patel</p>
                        <p>Library Owner/Administrator</p>
                        <br>
                        <br>
                    ";

                await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);


                response.IsSuccess = true;
                response.ResponseCode = 200; // OK
            }
            catch (DbUpdateException ex)
            {
                response.ResponseCode = 400; // Bad Request
                response.ErrorMessage = "Failed to update the password.";
            }
            catch (Exception ex)
            {
                response.ResponseCode = 500; // Internal Server Error
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> sendPersonalInfo(int userId)
        {
            APIResponse response = new APIResponse();
            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.Address) // Include the Address navigation property
                    .FirstOrDefaultAsync(i => i.UserId == userId);

                if (user != null)
                {
                    string userEmail = user.Email; // Replace with the actual admin's email
                    string subject = $@"{user.FirstName} Personal Information";

                    string addressInfo = user.Address != null
                        ? $"<li><strong>Address:</strong> {user.Address.AddressLine1}, {user.Address.AddressLine2}, {user.Address.City}, {user.Address.Province}, {user.Address.Country}, {user.Address.Postalcode}</li>"
                        : "<li><strong>Address:</strong> Not registered</li>";

                    string bodyHtml = $@"
                <h2>Your Personal Information</h2>
                <p>Dear {user.FirstName} {user.LastName},</p>
                <p>Your user details are as follows:</p>
                <ul>
                    <li><strong>Username:</strong> {user.Username}</li>
                    <li><strong>FirstName:</strong> {user.FirstName}</li>
                    <li><strong>LastName:</strong> {user.LastName}</li>
                    <li><strong>Email:</strong> {user.Email}</li>
                    <li><strong>Phone Number:</strong> +1 {user.Phone}</li>
                    <li><strong>Date of Birth:</strong> {user.Dob.ToString("MMMM dd, yyyy")}</li>
                    <li><strong>Gender:</strong> {user.Gender}</li>
                    {addressInfo}
                </ul>
                <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                <br> <!-- Added space before closing remarks -->
                <p>Thank you for using our Library Management System.</p>
                <p>Sincerely,<br>Your Library Administrator</p>
                <br/>
                <br/>
            ";

                    await _emailMessageService.SendMessage(userEmail, subject, bodyHtml);
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "User data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }


        public async Task<APIResponse> SendResetPassword(int userId)
        {
            APIResponse response = new APIResponse();
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = await _dbContext.Users.FirstOrDefaultAsync(i => i.UserId == userId);

                    if (user != null)
                    {
                        // Generate a random 9-character password
                        string newPassword = GenerateRandomPassword(9);

                        var passwordHash = _passwordHasher.Hash(newPassword);

                        // Update the user's password in the database
                        user.Password = passwordHash;
                        await _dbContext.SaveChangesAsync();

                        // Send an email with the new password
                        string subject = "Password Reset Confirmation";
                        string bodyHtml = $@"
                    <h2>Password Reset Confirmation</h2>
                    <p>Dear {user.FirstName} {user.LastName},</p>
                    <p>Your password has been reset. Please use the following temporary password to log in:</p>
                    <p><strong>New Temporary Password:</strong> {newPassword}</p>
                    <p>We recommend changing your password after logging in for security reasons.</p>
                    <br> <!-- Added space before closing remarks -->
                    <p>If you have any questions or need further assistance, please feel free to reach out to our support team.</p>
                    <p>Thank you for using our Library Management System.</p>
                    <p>Sincerely,<br>Your Library Administrator</p>
                    <br/>
                    <br/>
                ";

                        await _emailMessageService.SendMessage(user.Email, subject, bodyHtml);

                        // If everything is successful, commit the transaction
                        await transaction.CommitAsync();

                        response.IsSuccess = true;
                        response.ResponseCode = 200;
                    }
                    else
                    {
                        response.ResponseCode = 404; // Not Found
                        response.ErrorMessage = "User data not found";
                    }
                }
                catch (Exception ex)
                {
                    // If an exception occurs, rollback the transaction
                    await transaction.RollbackAsync();

                    response.ErrorMessage = ex.Message;
                    response.ResponseCode = 500; // Internal Server Error
                }
            }
            return response;
        }

        // Helper method to generate a random password
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<APIResponse<string>> MakeUser(int userId)
        {
            APIResponse<string> response = new();
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "User not found";
                    return response;
                }
                user.Role = "User";
                await _dbContext.SaveChangesAsync();
                response.ErrorMessage = $"{user.FirstName} {user.LastName} has now 'User' role";
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

        public async Task<APIResponse<string>> MakeAdmin(int userId)
        {
            APIResponse<string> response = new();
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "User not found";
                    return response;
                }
                user.Role = "Admin";
                await _dbContext.SaveChangesAsync();
                response.ErrorMessage = $"{user.FirstName} {user.LastName} has now 'Admin' role";
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
    }

}