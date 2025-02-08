using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Btc.Database;

public class CsvExtractor<T>(string filePath)
{
    public List<T> Extract()
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        var records = csv.GetRecords<T>().ToList();
        return records;
    }

    public string GetFileName()
    {
        var fileInfo = new FileInfo(filePath);
        return fileInfo.Name;
    }
}