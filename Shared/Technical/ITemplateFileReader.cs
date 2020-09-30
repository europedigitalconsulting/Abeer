namespace Abeer.Shared
{
    public interface ITemplateFileReader
    {
        string RootPath { get; set; }

        string GetFileName(string rootPath, string templatePatern, string culture);
        string ReadFile(string templatePath);
    }
}