using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.ViewModels;
using Mde.Project.Mobile.Views.Auth;
using Mde.Project.Mobile.Views.Categories;
using Mde.Project.Mobile.Views.Dashboard;
using Mde.Project.Mobile.Views.Settings;
using Mde.Project.Mobile.Views.Stats;
using Mde.Project.Mobile.Views.Transactions;
using Microsoft.Extensions.Logging;
using Mde.Project.Mobile.Views.RecurringTransactions;
using CommunityToolkit.Maui;
using Mde.Project.Mobile.Domain.Services;
using Plugin.LocalNotification;
using INotificationService = Mde.Project.Mobile.Core.Services.Interfaces.INotificationService;
using Mde.Project.Mobile.Core.Services;

namespace Mde.Project.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()           
                 .UseSkiaSharp()
                  .UseLiveCharts()
                  .UseMauiCommunityToolkit()
                  .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FaSolid");
                });
            
      
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IPhotoService, PhotoService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<ISupabaseAuthService, SupabaseAuthService>();
            builder.Services.AddSingleton<ISyncService, SyncService>();
           
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<ICategoryService, CategoryService>();
            builder.Services.AddTransient<ITransactionRecordService,TransactionRecordService>();
            builder.Services.AddTransient<IRecurringTransactionService, RecurringTransactionService>();
           

            //pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<TransactionPage>();
            builder.Services.AddTransient<TransactionDetailPage>();
            builder.Services.AddTransient<TransactionAddPage>();
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddTransient<CategoryDetailPage>();
            builder.Services.AddTransient<CategoryAddPage>();
            builder.Services.AddTransient<StatsPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<RecurringTransactionsPage>();
            builder.Services.AddTransient<RecurringTransactionAddPage>();

            //models
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<TransactionViewModel>();
            builder.Services.AddTransient<TransactionDetailViewModel>();
            builder.Services.AddTransient<TransactionAddViewModel>();
            builder.Services.AddTransient<CategoriesViewModel>();
            builder.Services.AddTransient<CategoryDetailViewModel>();
            builder.Services.AddTransient<CategoryAddViewModel>();
            builder.Services.AddTransient<StatsViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<RecurringTransactionViewModel>();
            builder.Services.AddTransient<RecurringTransactionAddViewModel>();

            return builder.Build();
        }
    }
}
