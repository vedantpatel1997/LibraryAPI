using AutoMapper;
using LibraryManagement.API.Container.Implimentation;
using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using LibraryManagement.API.Services.Implimentation;
using LibraryManagement.API.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);


// Retrieve the Key Vault URL from appsettings or environment variable
string keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];

// Add Azure Key Vault secrets to configuration
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    var credential = new DefaultAzureCredential();
    // Use DefaultAzureCredential to handle authentication automatically

    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registering Services
// Add this line in the configuration section of `Program.cs`
builder.Services.Configure<APIInfo>(builder.Configuration.GetSection("APIInfo"));
builder.Services.AddTransient<IAuthorize, Authorize>();
builder.Services.AddTransient<IBooksService, BooksService>();
builder.Services.AddTransient<IUsersService, UsersService>();
builder.Services.AddTransient<IEmailMessageService, EmailMessageService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<IBooksUsersTransactions, BooksUsersTransactions>();

// Register DbContext with connection string from Key Vault
builder.Services.AddDbContext<LibraryManagementContext>(options =>
{
    var connectionString = builder.Configuration["APIConnection"];
    options.UseSqlServer(connectionString);
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });


// JWT Authorization
var authKey = builder.Configuration.GetValue<string>("JWTSettings:securityKey");
builder.Services.AddAuthentication(item =>
{
    item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(item =>
{
    item.RequireHttpsMetadata = true;
    item.SaveToken = true;
    item.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

//Automapper
var automapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
IMapper mapper = automapper.CreateMapper();
builder.Services.AddSingleton(mapper);

//CORS Policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    });

    options.AddPolicy("corsPolicy", builder =>
    {
        builder.WithOrigins("https://libraryconestoga.netlify.app").AllowAnyMethod().AllowAnyHeader();
    });
});

var jwtSettings = builder.Configuration.GetSection("JWTSettings");
builder.Services.Configure<JWTSettings>(jwtSettings);


// Configure Kestrel to listen on port 443
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(443); // Bind to port 443
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
