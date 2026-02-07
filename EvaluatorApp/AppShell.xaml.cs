namespace EvaluatorApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(ProjectDetailsPage), typeof(ProjectDetailsPage));
        Routing.RegisterRoute(nameof(EvaluationResultPage), typeof(EvaluationResultPage));
	}
}
