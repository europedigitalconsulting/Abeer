using Abeer.Shared;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Abeer.Services
{
    public static class TemplateRenderManager
    {
        public static string GenerateHtmlTemplate(IServiceProvider serviceProvider, string rootPath, string templatePatern, Dictionary<string, string> keyValuePairs)
        {
            
            var htmlBody = LoadTemplate(serviceProvider, rootPath, templatePatern);

            foreach (var keyValue in keyValuePairs)
            {
                htmlBody = htmlBody.Replace($"[{keyValue.Key}]", keyValue.Value, StringComparison.OrdinalIgnoreCase);
            }

            return htmlBody;
        }

        private static string LoadTemplate(IServiceProvider serviceProvider, string rootPath, string templatePatern)
        {
            var reader = ActivatorUtilities.GetServiceOrCreateInstance<ITemplateFileReader>(serviceProvider);
            var templateFilePath = reader.GetFileName(rootPath, templatePatern, CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower());
            
            if (string.IsNullOrEmpty(templateFilePath))
                return string.Empty;

            return reader.ReadFile(templateFilePath);
        }
    }
}
