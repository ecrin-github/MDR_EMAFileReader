using System.Text.RegularExpressions;
using CommandLine;
namespace MDR_EMAFile_Reader;

internal class ParameterChecker 
{
    private readonly ILoggingHelper _loggingHelper;

    public ParameterChecker(ILoggingHelper loggingHelper)
    {
        _loggingHelper = loggingHelper;
    }
    
    public ParamsCheckResult CheckParams(string[]? args)
    {
        // Calls the CommandLine parser. If an error in the initial parsing, log it 
        // and return an error. If parameters can be passed, check their validity
        // and if invalid log the issue and return an error, otherwise return the 
        // parameters, processed as an instance of the Options class, and the source.

        var parsedArguments = Parser.Default.ParseArguments<Options>(args);
        if (parsedArguments.Errors.Any())
        {
            LogParseError(((NotParsed<Options>)parsedArguments).Errors);
            return new ParamsCheckResult(true, false, null);
        }
        
        var opts = parsedArguments.Value;
        return CheckArgumentValuesAreValid(opts);
    }
    
   
    public ParamsCheckResult CheckArgumentValuesAreValid(Options opts)
    {
        // 'opts' is passed by reference and may be changed by the checking mechanism.

        try
        {
            if (string.IsNullOrEmpty(opts.fileName))
            {
                throw new ArgumentException("No file name provided");
            }
            
            // file name should include 'ema'  (or 'EMA'), followed by an optional space,
            // followed by 8 digits representing the date, followed by 'xml'

            if (!Regex.Match(opts.fileName.ToLower(), @"^ema\s?[0-9]{8}\.xml$").Success)
            {
                string message = @"Incorrect format for XML file name. 
                File name should include 'ema'  (or 'EMA'), optionally followed by a space,
                followed by 8 digits representing the date (yyyyMMdd), followed by '.xml'";
                throw new ArgumentException(message);
            }
            
            // If reached here parameters are valid - return opts.

            return new ParamsCheckResult(false, false, opts);
        }


        catch (Exception e)
        {
            _loggingHelper.LogHeader("INVALID PARAMETERS");
            _loggingHelper.LogCommandLineParameters(opts);
            _loggingHelper.LogCodeError("Importer application aborted", e.Message, e.StackTrace);
            _loggingHelper.CloseLog();
            return new ParamsCheckResult(false, true, null);
        }

    }


    internal void LogParseError(IEnumerable<Error> errs)
    {
        _loggingHelper.LogHeader("UNABLE TO PARSE PARAMETERS");
        _loggingHelper.LogHeader("Error in input parameters");
        _loggingHelper.LogLine("Error in the command line arguments - they could not be parsed");

        int n = 0;
        foreach (Error e in errs)
        {
            n++;
            _loggingHelper.LogParseError("Error {n}: Tag was {Tag}", n.ToString(), e.Tag.ToString());
            if (e.GetType().Name == "UnknownOptionError")
            {
                _loggingHelper.LogParseError("Error {n}: Unknown option was {UnknownOption}", n.ToString(), ((UnknownOptionError)e).Token);
            }
            if (e.GetType().Name == "MissingRequiredOptionError")
            {
                _loggingHelper.LogParseError("Error {n}: Missing option was {MissingOption}", n.ToString(), ((MissingRequiredOptionError)e).NameInfo.NameText);
            }
            if (e.GetType().Name == "BadFormatConversionError")
            {
                _loggingHelper.LogParseError("Error {n}: Wrongly formatted option was {MissingOption}", n.ToString(), ((BadFormatConversionError)e).NameInfo.NameText);
            }
        }
        _loggingHelper.LogLine("MDR_Downloader application aborted");
        _loggingHelper.CloseLog();
    }

}

public class Options
{
    // Lists the command line arguments and options

    [Option('f', "file_name", Required = true, Separator = ',', HelpText = "Name of the file, (full path normally added from configuration file)")]
    public string? fileName { get; set; }
}


public class ParamsCheckResult
{
    internal bool ParseError { get; set; }
    internal bool ValidityError { get; set; }
    internal Options? Pars { get; set; }

    internal ParamsCheckResult(bool parseError, bool validityError, Options? pars)
    {
        ParseError = parseError;
        ValidityError = validityError;
        Pars = pars;
    }
}

