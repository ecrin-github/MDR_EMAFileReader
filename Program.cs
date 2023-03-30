using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MDR_EMAFile_Reader;

/*
Very small utility that takes an EMA XML file, that lists revised or added trials within
the EU CTR database, identifies the study registry Ids, and changes the value (from 2 to 0) of 
the 'download status' field in the EU CTR database source data table, of for new trials it adds a new record.
That allows the following DFR_Download program to target the relevant web pages for scraping, raher
than having to go through all of them!
Originally written in Go - this is a C# port.
*/

// Set up file based configuration environment.

string assemblyLocation = Assembly.GetExecutingAssembly().Location;
string? basePath = Path.GetDirectoryName(assemblyLocation);
if (string.IsNullOrWhiteSpace(basePath))
{
    return -1;
}

var configFiles = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
    .Build();

// Set up the host for the app, adding the singleton services used in the system to support DI

IHost host = Host.CreateDefaultBuilder()
    .UseContentRoot(basePath)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddConfiguration(configFiles); 
        
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<ICredentials, Credentials>();
        services.AddSingleton<ILoggingHelper, LoggingHelper>();
        services.AddSingleton<IMonDataLayer, MonDataLayer>();        
    })
    .Build();

// Establish logger and open the log file.
// Establish a new credentials class, and use both to establish the data layers.

LoggingHelper loggingHelper = ActivatorUtilities.CreateInstance<LoggingHelper>(host.Services);
Credentials credentials = ActivatorUtilities.CreateInstance<Credentials>(host.Services);
MonDataLayer monDataLayer = new(credentials);
DataLayer dataLayer = new(credentials, loggingHelper);

// Establish the parameter checker, which checks if the program's arguments can 
// be parsed and then if they are valid. If not an error condition is returned.

ParameterChecker paramChecker = new(loggingHelper);
ParamsCheckResult paramsCheck = paramChecker.CheckParams(args);
if (paramsCheck.ParseError || paramsCheck.ValidityError)
{
    return -1;  // End program, parameter errors should have been logged
}

// Should be able to proceed - file-name is valid. Create a Reader class and run the file reading process.

try
{
	var opts = paramsCheck.Pars!;
	FileReader reader = new(dataLayer, monDataLayer, loggingHelper, configFiles);
    return reader.Run(opts);
}

catch (Exception e)
{
    // If an error bubbles up to here there is an unexpected issue with the code.
    // A file should normally have been created (but just in case...).

    loggingHelper.LogHeader("UNHANDLED EXCEPTION");
    loggingHelper.LogCodeError("MDR_Importer application aborted", e.Message, e.StackTrace);
    loggingHelper.CloseLog();
    return -1;
}