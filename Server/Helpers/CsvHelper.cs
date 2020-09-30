using System.IO;
using System.Text;

namespace Abeer.Server.Helpers
{
    internal static class CsvHelper
    {
        public static byte[] ConvertCsvToFileStream(StringBuilder csv)
        {
            var tempFileName = Path.ChangeExtension(Path.GetTempFileName(), ".csv");
            File.WriteAllText(tempFileName, csv.ToString());
            var bytes = File.ReadAllBytes(tempFileName);
            if (File.Exists(tempFileName))
                File.Delete(tempFileName);
            return bytes;
        }
    }
}
