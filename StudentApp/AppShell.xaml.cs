namespace StudentApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Views.ProjectDetailsPage), typeof(Views.ProjectDetailsPage));
		Routing.RegisterRoute(nameof(Views.EvaluationDetailPage), typeof(Views.EvaluationDetailPage));
	}
}
