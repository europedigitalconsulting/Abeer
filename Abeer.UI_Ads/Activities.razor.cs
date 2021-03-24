using Abeer.Client.UISdk;
using Abeer.Shared;
using Abeer.Shared.Functional;
using Abeer.Shared.ViewModels;
using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Abeer.UI_Ads
{
    public partial class Activities : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient httpClient { get; set; }
        [Inject] private NavigationUrlService NavigationUrlService { get; set; }
        [CascadingParameter] private ScreenSize ScreenSize { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        public int ChartLargeWidth => ScreenSize.IsLarge ? 1024 : 350;
        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();
        public ClaimsPrincipal User { get; set; }
        public bool ReadOnly { get; set; }
        [Inject] private HttpClient HttpClient { get; set; }

        private Chart _chart;
        private Chart _chartRepartition;
        
        [Parameter]
        public string Id { get; set; }

        public AdViewModel Ad { get; private set; }

        private LineConfig _config = new LineConfig
        {
            Options = new LineOptions
            {
                Responsive = true,
                Title = new ChartJs.Blazor.Common.OptionsTitle
                {
                    Display = true,
                    Text = "Evolution"
                },
                Tooltips = new Tooltips
                {
                    Mode = InteractionMode.Nearest,
                    Intersect = true
                },
                Hover = new Hover
                {
                    Mode = InteractionMode.Nearest,
                    Intersect = true
                },
                Scales = new Scales
                {
                    XAxes = new List<CartesianAxis>
                    {
                        new CategoryAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Jour"
                            }
                        }
                    },
                    YAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Vues"
                            }
                        }
                    }
                }
            }
        };

        private BarConfig _repartitionConfig = new BarConfig
        {
            Options = new BarOptions
            {
                Responsive = true,
                Legend = new Legend
                {
                    Position = Position.Top
                },
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Répartition des vues"
                }
            }
        };

        protected override async Task OnInitializedAsync()
        {

            var authState = await authenticationStateTask;

            User = authState.User;

            var apiUrl = $"/api/ads/{Id}";
            var getDetail = await HttpClient.GetAsync(apiUrl);
            getDetail.EnsureSuccessStatusCode();
            
            var json = await getDetail.Content.ReadAsStringAsync();
            Ad = JsonConvert.DeserializeObject<AdViewModel>(json);

            await TaskLoadEvolutionChart();
            await TaskLoadRepartition();
        }

        private async Task TaskLoadRepartition()
        {
            var getStatistics = await httpClient.GetAsync($"api/ads/repartition/{Id}");
            getStatistics.EnsureSuccessStatusCode();

            var jStatistics = await getStatistics.Content.ReadAsStringAsync();
            var statistics = JsonConvert.DeserializeObject<List<StatisticKeyPoint>>(jStatistics);

            foreach (var date in statistics.Select(s => s.Date.ToString("dd/MM")).Distinct().ToArray())
                _config.Data.Labels.Add(date);

            var socialNetworks = statistics.Where(s => s.Key != "Total" && s.Key != "Direct").Select(s => s.Key).ToList();

            foreach (var socialNetwork in socialNetworks)
            {
                if (statistics.Any(d => d.Key == socialNetwork) == false)
                    continue;

                var dataset1 = new BarDataset<int>(statistics.Where(s => s.Key == socialNetwork).OrderBy(s => s.Date).Select(s => s.Value).ToArray())
                {
                    Label = socialNetwork,
                    BorderWidth = 1
                };

                _config.Data.Datasets.Add(dataset1);
            }

            var dataSetdirect = new BarDataset<int>(statistics.Where(s => s.Key == "Direct").OrderBy(s => s.Date).Select(s => s.Value).ToArray())
            {
                Label = Loc["Direct"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.Green),
                BorderWidth = 1
            };

            _repartitionConfig.Data.Datasets.Add(dataSetdirect);
            await _chartRepartition.Update();
        }

        private async Task TaskLoadEvolutionChart()
        {
            var getStatistics = await httpClient.GetAsync($"api/ads/evolution/{Id}");
            getStatistics.EnsureSuccessStatusCode();

            var jStatistics = await getStatistics.Content.ReadAsStringAsync();
            var statistics = JsonConvert.DeserializeObject<List<StatisticDatePoint>>(jStatistics);

            IDataset<int> dataset1 = new LineDataset<int>(statistics.Select(s => s.Value).ToArray())
            {
                Label = Loc["NumberOfViews"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.Blue),
                BorderColor = ColorUtil.FromDrawingColor(Color.Blue),
                Fill = FillingMode.Disabled
            };

            foreach (var date in statistics.Select(s => s.Date.ToString("dd/MM")))
                _config.Data.Labels.Add(date);

            _config.Data.Datasets.Add(dataset1);
            await _chart.Update();
        }
    }
}