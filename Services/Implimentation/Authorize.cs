using AutoMapper;
using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using LibraryManagement.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.API.Container.Implimentation
{
    public class Authorize : IAuthorize
    {
        private readonly LibraryManagementContext _dbContext;
        private readonly JWTSettings jwtSettings;
        private readonly IEmailMessageService _emailService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly int tokenTime = 120; // seconds
        private readonly int refreshTokenTime = 300;//seconds

        public Authorize(LibraryManagementContext dbContext, IOptions<JWTSettings> options, IPasswordHasher passwordHasher, IMapper mapper, IEmailMessageService emailService)
        {
            this._dbContext = dbContext;
            this.jwtSettings = options.Value;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            this._emailService = emailService;
        }

        public async Task<APIResponse<TokenResponse>> GenerateToken(UserCredentails usercred)
        {
            APIResponse<TokenResponse> response = new();
            try
            {
                var user = await this._dbContext.Users.Include(x => x.Address).FirstOrDefaultAsync(x => (x.Username == usercred.Username || x.Email == usercred.Username));
                if (user != null)
                {
                    var result = _passwordHasher.Verify(user.Password, usercred.Password);
                    if (!result)
                    {
                        response.ResponseCode = 401;
                        response.ErrorMessage = "Invalid Password";
                        return response;
                    }

                    // Generate Token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var tokenKey = Encoding.UTF8.GetBytes(this.jwtSettings.securityKey);
                    var tokenDesc = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                        new Claim("name", user.Username.Trim()),
                        new Claim(ClaimTypes.Role, user.Role.Trim()),
                        new Claim("UserId", user.UserId.ToString()) // Add the user's ID as a claim
                        }),
                        Expires = DateTime.UtcNow.AddSeconds(tokenTime),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
                    };
                    var token = tokenHandler.CreateToken(tokenDesc);
                    var finalToken = tokenHandler.WriteToken(token);

                    // Sending mail 
                    SendMail(user);

                    response.Data = new TokenResponse() { Token = finalToken, RefreshToken = await GenerateRefreshToken(user.Username), userData = _mapper.Map<User, UserModal>(user) };
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 401;
                    response.ErrorMessage = "User does not exist";
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately based on your application's logging and exception handling strategy.
                response.ResponseCode = 500; // Internal Server Error
                response.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            return response;
        }

        public async Task<APIResponse<TokenResponse>> GenerateRefreshToken(TokenResponse tokenResponse)
        {
            APIResponse<TokenResponse> response = new();
            try
            {


                var _refreshToken = await this._dbContext.AuthenticationRefreshTokens.FirstOrDefaultAsync(x => x.RefreshToken == tokenResponse.RefreshToken);
                if (_refreshToken != null)
                {
                    // Retrieve the user's role from the database
                    var user = await this._dbContext.Users.Include(x => x.Address).FirstOrDefaultAsync(x => x.Username == _refreshToken.UserId);

                    if (user != null)
                    {
                        // Create a new claims identity with the user's role and name
                        var newClaims = new List<Claim>
                    {
                        new Claim("name", user.Username),
                        new Claim("role", user.Role),
                        new Claim("UserId", user.UserId.ToString()) // Add the user's ID as a claim
                    };

                        // Generate Token
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenKey = Encoding.UTF8.GetBytes(this.jwtSettings.securityKey);

                        var newToken = new JwtSecurityToken(
                            claims: newClaims,
                            expires: DateTime.Now.AddSeconds(refreshTokenTime),
                            signingCredentials: new SigningCredentials(
                                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtSettings.securityKey)), SecurityAlgorithms.HmacSha256));
                        var finalToken = tokenHandler.WriteToken(newToken);

                        response.Data = new TokenResponse() { Token = finalToken, RefreshToken = await GenerateRefreshToken(user.Username), userData = _mapper.Map<User, UserModal>(user) };
                        response.IsSuccess = true;
                        response.ResponseCode = 200;
                    }
                    else
                    {
                        response.ResponseCode = 401;
                        response.ErrorMessage = "User not found";
                    }
                }
                else
                {
                    response.ResponseCode = 401;
                    response.ErrorMessage = "UnAuthorized";
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately based on your application's logging and exception handling strategy.
                response.ResponseCode = 500; // Internal Server Error
                response.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            return response;
        }


        public async Task<string> GenerateRefreshToken(string username)
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                string refreshToken = Convert.ToBase64String(randomNumber);
                var ExistingToken = await _dbContext.AuthenticationRefreshTokens.FirstOrDefaultAsync(x => x.UserId == username);
                if (ExistingToken != null)
                {
                    ExistingToken.RefreshToken = refreshToken;
                }
                else
                {
                    await this._dbContext.AuthenticationRefreshTokens.AddAsync(new AuthenticationRefreshToken
                    {
                        UserId = username,
                        TokenId = new Random().Next().ToString(),
                        RefreshToken = refreshToken,
                    });
                }
                await this._dbContext.SaveChangesAsync();
                return refreshToken;
            }
        }

        private void SendMail(User user)
        {
            var ownerTimezone = "America/New_York";
            var subject = "Login detected";
            var mailBody = $@"
                            <html>
                                <body>
                                    <p>Dear Owner,</p>
                
                                    <p>This is to inform you that the following user has successfully logged in:</p>
                
                                    <ul>
                                        <li><strong>Username:</strong> {user.Username}</li>
                                        <li><strong>Name:</strong> {user.FirstName} {user.LastName}</li>
                                        <li><strong>Email:</strong> {user.Email}</li>
                                        <li><strong>Login Date and Time:</strong> {TimeZoneConverter.ConvertUtcToTimeZone(DateTime.UtcNow, ownerTimezone).ToString("F")} according to owner timezone</li>
                                        <li><strong>Login Date and Time:</strong> {TimeZoneConverter.ConvertUtcToTimeZone(DateTime.UtcNow, user.Timezone).ToString("F")} according to user timezone</li>
                                    </ul>
                
                                    <p>Thank you for your attention.</p>
                
                                    <p>Best regards,<br/>
                                    Library Management System</p>
                                </body>
                            </html>";
            _emailService.SendMessage("vedantp9@gmail.com", subject, mailBody);
        }
    }
}
