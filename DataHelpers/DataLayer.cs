using Dapper;
using Npgsql;

namespace MDR_EMAFile_Reader;

public class DataLayer : IDataLayer
{
	private readonly ILoggingHelper _loggingHelper;
	private readonly ICredentials _credentials;
	private readonly string? _dbConnString;
    
	public DataLayer(ICredentials credentials, LoggingHelper loggingHelper)
	{
		_credentials = credentials;
		_dbConnString = credentials.GetConnectionString("euctr");
		_loggingHelper = loggingHelper;
	}

	public DateTime revised { get; set; }

	bool ProcessFileIDData(string id)
	{
		// get remote link from the id in the file

		string base_id = id[..14];
		string link_country = id[15..];
		
	    if (link_country.Length > 2 && link_country[..3] == "Out")
		{
			link_country = "3rd";
		}
		string link_id = base_id + "/" + link_country;
		string remote_link = "https://www.clinicaltrialsregister.eu/ctr-search/trial/" + link_id;

		// check if this record exists. If it does update it, 
		// otherwise add a new record to the source_data table.
		
		bool added = false;
		int rec_id = CheckIfRecordAlreadyExists(remote_link);
		if (rec_id > 0)
		{
			UpdateSourceDataDownloadStatus(rec_id, revised);
		}
		else
		{
			AddNewRecord(base_id, remote_link, revised);
			added = true;
		}
		return added;
	}
	
	private int CheckIfRecordAlreadyExists(string link)
	{
		string sql_string = $"select id from mn.source_data where sd_id = {link}";
		using NpgsqlConnection conn = new(_dbConnString);
		return conn.Query<int>(sql_string).FirstOrDefault();
	}
	
	private void UpdateSourceDataDownloadStatus(int id, DateTime revised)
	{
		string sql_string = @$"update mn.source_data set last_revised = {revised}, 
                               download_status = 0 where sd_id = {id}";
		using NpgsqlConnection conn = new(_dbConnString);
		conn.Execute(sql_string);
	}
	
	private void AddNewRecord(string sid, string url, DateTime revised)
	{
		string sql_string = @$"insert into mn.source_data (sd_id, remote_url, last_revised, download_status) 
		                       values ({sid}, {url}, {revised}, 0)";
		using NpgsqlConnection conn = new(_dbConnString);
		conn.Execute(sql_string);
	}
}