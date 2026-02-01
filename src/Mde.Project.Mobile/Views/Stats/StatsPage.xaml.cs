using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Stats;

public partial class StatsPage : ContentPage
{
	private readonly StatsViewModel _viewModel;
    public StatsPage(StatsViewModel viewModel)
    {     
        InitializeComponent();     
        _viewModel = viewModel;
        BindingContext = _viewModel;       
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadStatsCommand.ExecuteAsync(null);
    }
}