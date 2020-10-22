using Abeer.Data.UnitOfworks;
using Abeer.Shared;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Abeer.Services
{
    public class CountriesService
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly ILogger<CountriesService> _logger;

        public CountriesService(FunctionalUnitOfWork functionalUnitOfWork, ILogger<CountriesService> logger)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
            _logger = logger;
        }

        public async Task<IList<Country>> GetCountries(string culture)
        {
            return await _functionalUnitOfWork.CountriesRepository.GetCountries(culture);
        }

        public void SeedData(string webRootPath)
        {
            var files = Directory.GetFiles(Path.Combine(webRootPath, "App_data"), "countries*.json");

            foreach (var file in files)
            {
                ImportJsonFile(file);
            }
        }

        public void ImportJsonFile(string file)
        {
            var fileName = Path.GetFileName(file);
            var parts = fileName.Split('.');
            var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();

            if (parts.Length >= 2)
            {
                culture = parts[1];
            }

            if (!_functionalUnitOfWork.CountriesRepository.Any(c => c.Culture == culture))
            {
                _logger.LogInformation($"start import Countries file {file} with culture {culture}");
                var json = File.ReadAllText(file);
                var countries = JsonConvert.DeserializeObject<List<Country>>(json);

                foreach (var imported in countries)
                {
                    var country = new Country
                    {
                        Culture = culture,
                        Eeacode = imported.Eeacode,
                        Estatcode = imported.Estatcode,
                        Faocode = imported.Faocode,
                        Gdp = imported.Gdp,
                        Label = imported.Label,
                        Name = imported.Name,
                        Nutscode = imported.Nutscode
                    };

                    _functionalUnitOfWork.CountriesRepository.Add(country);
                }
            }
        }
    }
}
