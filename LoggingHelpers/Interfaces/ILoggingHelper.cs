namespace MDR_EMAFile_Reader;

public interface ILoggingHelper
{
   
    void LogLine(string message, string identifier = "");    
    void LogHeader(string header_text);
    
    void LogError(string message);
    void LogCodeError(string header, string errorMessage, string? stackTrace);
    void LogParseError(string header, string errorNum, string errorType);   
    
    void LogCommandLineParameters(Options opts);

    void CloseLog();   
   
}
