using Abeer.Shared;
using Abeer.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Abeer.Services
{
    public static class TemplateRenderManager
    {
        public static string GenerateHtmlTemplate(IServiceProvider serviceProvider, string rootPath, EmailTemplateEnum emailTemplate, Dictionary<string, string> keyValuePairs)
        {
            
            var htmlBody = LoadTemplate(serviceProvider, rootPath, emailTemplate);

            foreach (var keyValue in keyValuePairs)
            {
                htmlBody = htmlBody.Replace($"[{keyValue.Key}]", keyValue.Value, StringComparison.OrdinalIgnoreCase);
            }

            return htmlBody;
        }

        private static string LoadTemplate(IServiceProvider serviceProvider, string rootPath, EmailTemplateEnum emailTemplate)
        {
            var reader = ActivatorUtilities.GetServiceOrCreateInstance<ITemplateFileReader>(serviceProvider);
            var templateFilePath = reader.GetFileName(rootPath, emailTemplate.GetName(), CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower());
            
            if (string.IsNullOrEmpty(templateFilePath))
                return string.Empty;

            return reader.ReadFile(templateFilePath);
        }
    }
}
