using EvaluatorApp.Services;
using EvaluatorApp.Models;

namespace EvaluatorApp;

public partial class SignUpPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;

    public SignUpPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        // Ideally update icon here too, but for speed skipping binding logic for icon swap
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text?.Trim();
        string email = EmailEntry.Text?.Trim().ToLower();
        string department = DepartmentPicker.SelectedItem?.ToString();
        string password = PasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(name) || 
            string.IsNullOrWhiteSpace(email) || 
            string.IsNullOrWhiteSpace(department) || 
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Por favor completa todos los campos.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        SignUpBtn.IsEnabled = false;
        SignUpBtn.Text = "Creando cuenta...";

        try
        {
            var newUser = new User
            {
                Username = email, // Using email as username for simplicity
                Email = email,
                Password = password,
                FullName = name,
                Department = department,
                Role = "Evaluador",
                IsVerified = false
            };

            await _mongoDBService.CreateUser(newUser);

            await DisplayAlert("Cuenta Creada", 
                "Tu cuenta ha sido creada exitosamente. \n\nSin embargo, para acceder, un administrador debe verificar y activar tu cuenta. Por favor espera la aprobación.", 
                "Entendido");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo crear la cuenta: {ex.Message}", "OK");
        }
        finally
        {
            SignUpBtn.IsEnabled = true;
            SignUpBtn.Text = "Crear Cuenta";
        }
    }
}
