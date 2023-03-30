using Microsoft.Extensions.Configuration;
namespace MDR_EMAFile_Reader;

public class LoggingHelper : ILoggingHelper
{
    private StreamWriter? _sw;

    public LoggingHelper()
    {
        IConfigurationRoot settings = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string logFolderPath = settings["logFilePath"] ?? "";

        // Open file as part of constructor. N.B. logFolderPath already includes '\ema\'
        string dtString = DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture)
            .Replace(":", "").Replace("T", " ");

        if (!Directory.Exists(logFolderPath))
        {
            Directory.CreateDirectory(logFolderPath);
        }
        
        string logFileName = "EMA " + dtString + ".log";
        string logfilePath = Path.Combine(logFolderPath, logFileName);
        _sw = new StreamWriter(logfilePath, true, System.Text.Encoding.UTF8);
    }
   
    public void LogLine(string message, string identifier = "")
    {
        string dtPrefix = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string feedback = dtPrefix + message + identifier;
        Transmit(feedback);
    }


    public void LogHeader(string message)
    {
        string dtPrefix = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string header = dtPrefix + "**** " + message.ToUpper() + " ****";
        Transmit("");
        Transmit(header);
    }

    
    public void LogError(string message)
    {
        string dtPrefix = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string errorMessage = dtPrefix + "***ERROR*** " + message;
        Transmit("");
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit(errorMessage);
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit("");
    }


    public void LogCodeError(string header, string errorMessage, string? stackTrace)
    {
        string dtPrefix = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string headerMessage = dtPrefix + "***ERROR*** " + header + "\n";
        Transmit("");
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit(headerMessage);
        Transmit(errorMessage + "\n");
        Transmit(stackTrace ?? "");
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit("");
    }


    public void LogParseError(string header, string errorNum, string errorType)
    {
        string dtPrefix = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string errorMessage = dtPrefix + "***ERROR*** " + "Error " + errorNum + ": " + header + " " + errorType;
        Transmit(errorMessage);
    }
            

    public void LogCommandLineParameters(Options opts)
    {
        LogLine("Source file name is: " + opts.fileName);
        LogLine("");
    }


    public void CloseLog()
    {
        if (_sw is not null)
        {
            LogHeader("Closing Log");
            _sw.Flush();
            _sw.Close();
        }
    }
    
    
    private void Transmit(string message)
    {
        _sw!.WriteLine(message);
        Console.WriteLine(message);
    }

}
