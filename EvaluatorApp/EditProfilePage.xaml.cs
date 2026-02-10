using EvaluatorApp.Models;
using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class EditProfilePage : ContentPage
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMongoDBService _mongoDBService;
    private string _currentImageUrl;
    private const string DefaultProfileImage = "https://res.cloudinary.com/djwpi6z29/image/upload/v1770699551/avatar-default-user-profile-icon-social-media-vector-57234208_y8gtgs.jpg";

    public EditProfilePage(ICloudinaryService cloudinaryService, IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _cloudinaryService = cloudinaryService;
        _mongoDBService = mongoDBService;
        
        DepartmentPicker.ItemsSource = new List<string>
        {
            "Ingeniería de Software",
            "Diseño Industrial",
            "Administración de Empresas",
            "Biocencias",
            "Ingeniería Civil"
        };
        
        LoadUserData();
    }

    private void LoadUserData()
    {
        FullNameEntry.Text = Preferences.Get("UserFullName", "");
        EmailEntry.Text = Preferences.Get("UserEmail", "");
        DepartmentPicker.SelectedItem = Preferences.Get("UserDepartment", "");
        PronounsEntry.Text = Preferences.Get("UserPronouns", "");
        
        _currentImageUrl = Preferences.Get("UserProfileImage", DefaultProfileImage);
        if (string.IsNullOrEmpty(_currentImageUrl)) _currentImageUrl = DefaultProfileImage;
        
        ProfileImage.Source = _currentImageUrl;
    }

    private async void OnChangePhotoClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Cambiar Foto de Perfil", "Cancelar", null, "Subir nueva foto", "Eliminar foto actual");

        if (action == "Subir nueva foto")
        {
            await PickAndUploadPhoto();
        }
        else if (action == "Eliminar foto actual")
        {
            _currentImageUrl = DefaultProfileImage;
            ProfileImage.Source = _currentImageUrl;
        }
    }

    private async Task PickAndUploadPhoto()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona una imagen",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                // Show loading indicator usually, here simple alert/swapping
                using var stream = await result.OpenReadAsync();
                var imageUrl = await _cloudinaryService.UploadImage(stream, result.FileName);
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    _currentImageUrl = imageUrl;
                    ProfileImage.Source = _currentImageUrl;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
            {
                await DisplayAlert("Error", "El nombre no puede estar vacío.", "OK");
                return;
            }

            var userId = Preferences.Get("UserId", string.Empty);
            if (string.IsNullOrEmpty(userId))
            {
                await DisplayAlert("Error", "Sesión inválida. Por favor, inicia sesión nuevamente.", "OK");
                return;
            }

            var user = new User
            {
                Id = userId,
                FullName = FullNameEntry.Text,
                Department = DepartmentPicker.SelectedItem?.ToString() ?? "",
                Pronouns = PronounsEntry.Text,
                ProfileImageUrl = _currentImageUrl
            };

            // Update in DB
            await _mongoDBService.UpdateUser(user);

            // Update Local Preferences
            Preferences.Set("UserFullName", user.FullName);
            Preferences.Set("UserDepartment", user.Department);
            Preferences.Set("UserPronouns", user.Pronouns);
            Preferences.Set("UserProfileImage", user.ProfileImageUrl);

            await DisplayAlert("Éxito", "Perfil actualizado correctamente.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
