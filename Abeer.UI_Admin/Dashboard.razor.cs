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

namespace Abeer.UI_Admin
{
    public partial class Dashboard : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private HttpClient httpClient { get; set; }
        [Inject] private NavigationUrlService NavigationUrlService { get; set; }
        [CascadingParameter] private ScreenSize ScreenSize { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        public int ChartLargeWidth => ScreenSize.IsLarge ? 1024 : 350;
        public ClaimsPrincipal User { get; set; }
        public ViewApplicationUser UserProfile { get; private set; }
        public bool ReadOnly { get; set; }
        public DashboardInformation DashboardInfo { get; set; }

        private Chart _chartEvolution;
        List<AdModel> Ads { get; set; }

        private LineConfig _configEvolution = new LineConfig
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
                                LabelString = "Nombre"
                            }
                        }
                    }
                }
            }
        };

        protected override async Task OnInitializedAsync()
        {

            var authState = await authenticationStateTask;

            User = authState.User;

            if (User.Identity.IsAuthenticated)
            {
                UserProfile = (ViewApplicationUser)User;
                
                if (!UserProfile.IsAdmin)
                    NavigationManager.NavigateTo("/");

                var response = await httpClient.GetAsync("api/AdminDashboard");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    DashboardInfo = JsonConvert.DeserializeObject<DashboardInformation>(json);
                    await TaskLoadEvolutionChart();
                }
            }
        }

        private async Task TaskLoadEvolutionChart()
        {
            foreach (var date in DashboardInfo.StatisticsDays.Select(s => s.Date.ToString("dd/MM")))
                _configEvolution.Data.Labels.Add(date);

            IDataset<int> dataset1 = new LineDataset<int>(DashboardInfo.StatisticsDays.Select(d=>d.NbVisits))
            {
                Label = Loc["NumberOfVisits"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.Blue),
                BorderColor = ColorUtil.FromDrawingColor(Color.Blue),
                Fill = FillingMode.Disabled
            };

            _configEvolution.Data.Datasets.Add(dataset1);

            IDataset<int> dataset2 = new LineDataset<int>(DashboardInfo.StatisticsDays.Select(d => d.NbRegistar))
            {
                Label = Loc["NbRegistar"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.Red),
                BorderColor = ColorUtil.FromDrawingColor(Color.Red),
                Fill = FillingMode.Disabled
            };

            _configEvolution.Data.Datasets.Add(dataset2);

            IDataset<int> dataset3 = new LineDataset<int>(DashboardInfo.StatisticsDays.Select(d => d.NbLogin))
            {
                Label = Loc["NbLogins"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.Silver),
                BorderColor = ColorUtil.FromDrawingColor(Color.Silver),
                Fill = FillingMode.Disabled
            };

            _configEvolution.Data.Datasets.Add(dataset3);

            IDataset<int> dataset4 = new LineDataset<int>(DashboardInfo.StatisticsDays.Select(d => d.NbRequestSubscription))
            {
                Label = Loc["NbRequestSubscription"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.Silver),
                BorderColor = ColorUtil.FromDrawingColor(Color.Silver),
                Fill = FillingMode.Disabled
            };

            _configEvolution.Data.Datasets.Add(dataset4);

            IDataset<int> dataset5 = new LineDataset<int>(DashboardInfo.StatisticsDays.Select(d => d.NbFinishSubscription))
            {
                Label = Loc["NbFinishSubscription"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.DarkGreen),
                BorderColor = ColorUtil.FromDrawingColor(Color.DarkGreen),
                Fill = FillingMode.Disabled
            };

            _configEvolution.Data.Datasets.Add(dataset5);

            IDataset<int> dataset6 = new LineDataset<int>(DashboardInfo.StatisticsDays.Select(d => d.NbAds))
            {
                Label = Loc["NbAds"],
                BackgroundColor = ColorUtil.FromDrawingColor(Color.GreenYellow),
                BorderColor = ColorUtil.FromDrawingColor(Color.GreenYellow),
                Fill = FillingMode.Disabled
            };

            _configEvolution.Data.Datasets.Add(dataset6);
            await _chartEvolution.Update();
        }
    }
}