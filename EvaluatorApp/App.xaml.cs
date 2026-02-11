using Microsoft.Extensions.DependencyInjection;
using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		ThemeService.LoadSavedTheme();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
