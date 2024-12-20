using Finance.ViewModels;

namespace Finance.Views;

public partial class IncomeView : ContentPage
{
	public IncomeView(IncomeViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	protected override bool OnBackButtonPressed()
	{
		Shell.Current.GoToAsync($"/{nameof(TransactionView)}");
		return true;
	}


}