using Microsoft.Extensions.Configuration;

public class ConnectionStringService
{
    private readonly IConfiguration _configuration;
    private string _currentDbKey; // Track the database key ("old" or "new")
    private string _currentConnectionString;

    public ConnectionStringService(IConfiguration configuration)
    {
        _configuration = configuration;
        _currentDbKey = "new"; // Default to "new" database
        _currentConnectionString = _configuration.GetConnectionString("SQL_Server_Conn_Microsoft_DB"); // Default to "new" database connection string
    }

    public string GetConnectionString() => _currentConnectionString;
    public string GetCurrentDbKey() => _currentDbKey; // Expose the current key

    public void SetConnectionString(string dbKey)
    {
        if (dbKey == "old")
        {
            _currentDbKey = "old";
            _currentConnectionString = _configuration.GetConnectionString("SQL_Server_Conn_Conestoga_DB");
        }
        else if (dbKey == "new")
        {
            _currentDbKey = "new";
            _currentConnectionString = _configuration.GetConnectionString("SQL_Server_Conn_Microsoft_DB");
        }
        else
        {
            throw new ArgumentException("Invalid dbKey. Use 'old' or 'new'.");
        }
    }
}