namespace MDR_EMAFile_Reader;

public interface ICredentials
{
    string? Password { get; set; }
    string? Username { get; set; }
    string GetConnectionString(string databaseName);
}
