namespace EvaluatorApp;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Perfil", "Navegar a Editar Perfil", "OK");
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Seguridad", "Navegar a Cambiar Contraseña", "OK");
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Ayuda", "Navegar a Soporte", "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que quieres cerrar sesión?", "Sí", "No");
        if (answer)
        {
            // Reset main page to logic to create a new session or go to login
             await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
