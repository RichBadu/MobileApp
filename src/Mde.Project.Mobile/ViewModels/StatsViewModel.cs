using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using SkiaSharp;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class StatsViewModel : ObservableObject
    {
        private readonly ITransactionRecordService _transactionService;
        private readonly ICategoryService _categoryService;
        private string? UserId => CurrentUser.GetUserId();

        public StatsViewModel(
            ITransactionRecordService transactionService,
            ICategoryService categoryService)
        {
            _transactionService = transactionService;
            _categoryService = categoryService;

            
            selectedMonth = DateTime.Now;


        }

        [ObservableProperty]
        private DateTime selectedMonth;

        [ObservableProperty]
        private decimal totalExpenses;

        [ObservableProperty] 
        private ISeries[]? categorySeries;
        [ObservableProperty]
        private Axis[]? categoryXAxes;
        [ObservableProperty]
        private Axis[]? categoryYAxes;

        [ObservableProperty]
        private ISeries[]? trendSeries;
        [ObservableProperty] 
        private Axis[]? trendXAxes;
        [ObservableProperty]
        private Axis[]? trendYAxes;

        public string MonthYearDisplay => SelectedMonth.ToString("MMMM yyyy");

        [RelayCommand]
        async Task LoadStats()
        {
          
    
            try
            {
                
                int month = SelectedMonth.Month;
                int year = SelectedMonth.Year;

                
                TotalExpenses = await _transactionService.GetTotalExpensesAsync(UserId, month, year);

                
                await LoadCategoryChartData(month, year);

                
                await LoadTrendChartData();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load stats: {ex.Message}", "OK");
            }
           
        }

        private async Task LoadCategoryChartData(int month, int year)
        {
         

            var expensesByCategory = await _transactionService.GetExpensesByCategoryAsync(UserId, month, year);
            var categories = await _categoryService.GetAllCategoriesAsync(UserId);

            

            var topCategories = expensesByCategory.OrderByDescending(x => x.Value).Take(5);


            if (!topCategories.Any())
            {
                CategorySeries = Array.Empty<ISeries>();
                return;
            }
           
            var values = new List<double>();
            var labels = new List<string>();
            var colors = new List<SKColor>();

            foreach (var kvp in topCategories)
            {
                var category = categories.FirstOrDefault(c => c.Id == kvp.Key);
                if (category != null)
                {
                    values.Add((double)kvp.Value);
                    labels.Add(category.Name);
                    colors.Add(SKColor.Parse(category.Color));
                }            
            }

            CreateBarChart(values, labels);
        }

        private async Task LoadTrendChartData()
        {
            var values = new List<double>();
            var labels = new List<string>();


            for (int i = 5; i >= 0; i--)
            {
                var date = SelectedMonth.AddMonths(-i);
                var expenses = await _transactionService.GetTotalExpensesAsync(UserId, date.Month, date.Year);

                values.Add((double)expenses);
                labels.Add(date.ToString("MMM"));

            }

            CreateLineChart(values, labels);
        }

        [RelayCommand]
        async Task PreviousMonth()
        {
            SelectedMonth = SelectedMonth.AddMonths(-1);
            OnPropertyChanged(nameof(MonthYearDisplay));
            await LoadStats();
        }

        [RelayCommand]
        async Task NextMonth()
        {
            SelectedMonth = SelectedMonth.AddMonths(1);
            OnPropertyChanged(nameof(MonthYearDisplay));
            await LoadStats();
        }

        private void CreateBarChart(List<double> values, List<string> labels)
        {

            var shortenedLabels = labels.Select(label =>
            {
                if(label.Length > 6)
                    return label.Substring(0, 4);
                return label;
     
            }).ToList();

            CategorySeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = values,
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.DeepSkyBlue.WithAlpha(180)),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsFormatter = point => $"€{point.Coordinate.PrimaryValue:N0}",
                    DataLabelsSize = 14
                }
             };

            CategoryXAxes = new[]
            {
               new Axis
               {
                 Labels = shortenedLabels,
                 LabelsRotation = 0,                 
                 TextSize = 12,
               }
            };

            CategoryYAxes = new[]
            {
               new Axis
               {
                  Labeler = value => $"€{value:N0}",
                  TextSize = 12,
                  MinLimit = 0
               }
            };
        }

        private void CreateLineChart(List<double> values, List<string> labels)
        {
            TrendSeries = new ISeries[]
            {
               new LineSeries<double>
               {
                  Values = values,
                  Stroke = new SolidColorPaint(SKColors.MediumPurple, 4),
                  Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(60)),
                  GeometrySize = 12,
                  LineSmoothness = 0.4,
            
               }
            };

            TrendXAxes = new[]
            {
               new Axis
               {
                  Labels = labels,
                  TextSize = 14,
           
               }
            };

            TrendYAxes = new[]
            {
              new Axis
              {
                 Labeler = value => $"€{value:N0}",
                 TextSize = 12,
                 MinLimit = 0
              }
            };
        }


    }

   

}

