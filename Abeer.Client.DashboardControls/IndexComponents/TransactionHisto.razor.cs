using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.BarChart.Axes;
using ChartJs.Blazor.ChartJS.Common;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Wrappers;
using ChartJs.Blazor.Charts;

using Abeer.Client.Shared;

using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abeer.Client.Pages
{
    public partial class TransactionHisto : ComponentBase
    {
        private BarConfig barConfig;
        protected ChartJsBarChart _ChartJsBarChart;
        private Random rnd = new Random();
        protected AuthorizationSwitcher _authorizationSwitcher;

        protected override async Task OnParametersSetAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            barConfig = new BarConfig
            {
                Options = new BarOptions
                {
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = GetTitle(user)
                    },
                    Scales = new BarScales
                    {
                        XAxes = new List<CartesianAxis>
                    {
                        new BarCategoryAxis
                        {
                            BarPercentage = 0.5,
                            BarThickness = BarThickness.Flex
                        }
                    },
                        YAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks
                            {
                                BeginAtZero = true
                            }
                        }
                    }
                    },
                    Responsive = true
                }
            };

            barConfig.Data.Labels.AddRange(DateTime.Now.DaysInMonth().Select(d => d.ToString("dd/MM")));

            AddBalance();
        }

        private void AddBalance()
        {

            var barBalanceSet = new BarDataset<Int32Wrapper>
            {
                Label = LabelBalances,
                BackgroundColor = new[] { "#242968", "#218ba5", "#11af66", "#af9d11" },
                BorderWidth = 0,
                HoverBackgroundColor = "#f06384",
                HoverBorderColor = "#f06384",
                HoverBorderWidth = 1,
                BorderColor = "#ffffff",
            };

            barBalanceSet.AddRange(Enumerable.Range(1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).Select(i => rnd.Next(30)).Wrap());

            barConfig.Data.Datasets.Add(barBalanceSet);
        }


        private string GetTitle(System.Security.Claims.ClaimsPrincipal user)
        {
            return user.Identity.IsAuthenticated ? LabelTransactionHistoric : LabelMarketEvolution;
        }
    }
}
