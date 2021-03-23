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

namespace Abeer.Client.Pages
{
    public partial class Activities : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient httpClient { get; set; }
        [Inject] private NavigationUrlService NavigationUrlService { get; set; }
        [CascadingParameter] private ScreenSize ScreenSize { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        public int ChartLargeWidth => ScreenSize.IsLarge ? 1024 : 350;
        public ViewApplicationUser UserProfile { get; set; } = new ViewApplicationUser();
        public List<SocialNetwork> AvailableSocialNetworks { get; set; } = new List<SocialNetwork>();
        public ClaimsPrincipal User { get; set; }
        public bool ReadOnly { get; set; }

        private Chart _chart;
        private Chart _chartRepartition;
        List<AdModel> Ads { get; set; }

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

            if (User.Identity.IsAuthenticated)
            {
                var response = await httpClient.GetAsync($"api/Profile/GetUserProfileNoDetail?userId={User.FindFirstValue(ClaimTypes.NameIdentifier)}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"GetUserProfileNoDetail:{json}");

                    UserProfile = JsonConvert.DeserializeObject<ViewApplicationUser>(json);

                    await TaskLoadEvolutionChart();
                    await TaskLoadRepartition();
                    await TaskLoadTop10Ads();
                }
            }
        }

        private async Task TaskLoadTop10Ads()
        {
            var getStatistics = await httpClient.GetAsync("api/ProfileStatistics/top10Ads");
            getStatistics.EnsureSuccessStatusCode();
            var jStatistics = await getStatistics.Content.ReadAsStringAsync();
            Ads = JsonConvert.DeserializeObject<List<AdModel>>(jStatistics);
        }

        private async Task TaskLoadRepartition()
        {
            var getStatistics = await httpClient.GetAsync("api/ProfileStatistics/repartition");
            getStatistics.EnsureSuccessStatusCode();

            var jStatistics = await getStatistics.Content.ReadAsStringAsync();
            var statistics = JsonConvert.DeserializeObject<List<StatisticKeyPoint>>(jStatistics);

            var socialNetworks = UserProfile.SocialNetworkConnected;

            foreach (var date in statistics.Select(s => s.Date.ToString("dd/MM")).Distinct().ToArray())
                _config.Data.Labels.Add(date);

            foreach (var socialNetwork in socialNetworks)
            {
                if (statistics.Any(d => d.Key == socialNetwork.Name) == false)
                    continue;

                var dataset1 = new BarDataset<int>(statistics.Where(s => s.Key == socialNetwork.Name).OrderBy(s => s.Date).Select(s => s.Value).ToArray())
                {
                    Label = socialNetwork.Name,
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
            var getStatistics = await httpClient.GetAsync("api/ProfileStatistics/evolution");
            getStatistics.EnsureSuccessStatusCode();

            var jStatistics = await getStatistics.Content.ReadAsStringAsync();
            var statistics = JsonConvert.DeserializeObject<List<StatisticDatePoint>>(jStatistics);

            IDataset<int> dataset1 = new LineDataset<int>(statistics.Select(s => s.Value).ToArray())
            {
                Label = Loc["NumberOfViews"],
                BackgroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Blue),
                BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Blue),
                Fill = FillingMode.Disabled
            };

            foreach (var date in statistics.Select(s => s.Date.ToString("dd/MM")))
                _config.Data.Labels.Add(date);

            _config.Data.Datasets.Add(dataset1);
            await _chart.Update();
        }
    }
}