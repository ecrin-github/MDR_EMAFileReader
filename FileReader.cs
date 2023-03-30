using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
namespace MDR_EMAFile_Reader;

public class FileReader
{
    private readonly ILoggingHelper _loggingHelper;
    private readonly IMonDataLayer _monDataLayer;
    private readonly IDataLayer _dataLayer;
    private readonly string _emaFolder;

    public FileReader(IDataLayer dataLayer, IMonDataLayer monDataLayer, ILoggingHelper loggingHelper, IConfiguration settings)
    {
        _dataLayer = dataLayer;
        _monDataLayer = monDataLayer;
        _loggingHelper = loggingHelper;
        _emaFolder = settings["emaFilePath"]!;
    }

    public int Run(Options opts)
    {
        // Firstly does the specified file actually exist?
        // Construct the full path to the file using the appsettings data.
        
        string file_name = opts.fileName!;
        string full_path = Path.Combine(_emaFolder, file_name);
        if (!File.Exists(full_path))
        {
            _loggingHelper.LogError($"File does not appear to exist at {full_path}");
            return -1;
        }
        
        // Inject the file date into the date revised property of the data layer.
       
        string date = Regex.Match(file_name, @"\d{8}").Value;
        int y = int.Parse(date[..4]);
        int m = int.Parse(date[4..6]);
        int d = int.Parse(date[6..]);
        _dataLayer.revised = new DateTime(y, m, d);
        
        // then open and read the file
        
        using StreamReader streamReader = new StreamReader(full_path, Encoding.UTF8);
        string responseBodyAsString = streamReader.ReadToEnd();
        trials? foundTrials = Deserialize<trials?>(responseBodyAsString, _loggingHelper);
        if (foundTrials is not null)
        {
            // 
        }
        
        return 0;
    }

    
        // Obtain listed id for each trial
        // and add to a list of strings.
        //var ids List<string>
        //foreach (var t in foundTrials)
        //{
        //    ids.Add(t.TrialId);
        // }

        // for testing, there is an additon of two obviously silly records
        //ids = append(ids, "2100-123456-44-XL")
        //ids = append(ids, "2100-987654-66-SE")

        // Update source studies table in database.
        // using the datalayer, sending the strings as input
        // Use a res object...
        // num_updated, num_added := data.ProcessFileIDData(ids, date_string)

        // log the result
        //g.InfoLogger.Printf("Number of records updated: %d", num_updated)
        //g.InfoLogger.Printf("Num of records added: %d", num_added)

        // also update the overall event record
        //data.StoreSAFRecord(trialsFilePath, start_time, num_updated, num_added)
        // General XML Deserialize function.

        
    private T? Deserialize<T>(string? inputString, ILoggingHelper logging_helper)
    {
        if (inputString is null)
        {
            return default;
        }

        T? instance;
        try
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(inputString);
            instance = (T?)xmlSerializer.Deserialize(stringReader);
            return instance;
        }
        catch(Exception e)
        {
            logging_helper.LogCodeError("Error when de-serialising " + inputString, e.Message, e.StackTrace);
            return default;
        }

    }

}
