using Abeer.Shared;

using System.IO;

namespace Abeer.Services
{
    public class TemplateFileReader : ITemplateFileReader
    {
        public string RootPath { get; set; }
        public string GetFileName(string rootPath, string templatePatern, string culture)
        {
            var templatePath = Path.Combine(rootPath,
                        "Templates",
                        "Email",
                        $"{templatePatern}.{culture}.html");

            if (!System.IO.File.Exists(templatePath))
                templatePath = Path.Combine(rootPath,
                "Templates",
                "Email",
                $"{templatePatern}.html");

            return templatePath;
        }

        public string ReadFile(string templatePath)
        {
            string htmlBody = "";

            using (StreamReader SourceReader = System.IO.File.OpenText(templatePath))
            {
                htmlBody = SourceReader.ReadToEnd();
            }

            return htmlBody;
        }
    }
}
