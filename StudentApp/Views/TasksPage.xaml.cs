using StudentApp.Models;
using StudentApp.Services;
using System.Collections.ObjectModel;

namespace StudentApp.Views;

public partial class TasksPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    public ObservableCollection<StudentTask> Tasks { get; set; } = new ObservableCollection<StudentTask>();

    public TasksPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        TasksCollectionView.ItemsSource = Tasks;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTasks();
    }

    private async Task LoadTasks()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            TasksRefreshView.IsRefreshing = true;

            // Get Current Student Logic (Ideally this should be in a UserSession service)
            // For now, we rely on the static properties we set in MainPage or LoginPage
            string studentEmail = StudentApp.MainPage.CurrentStudentEmail;
            
            if (string.IsNullOrEmpty(studentEmail))
            {
                // Fallback or error if no session
                await DisplayAlert("Error", "No authentication session found.", "OK");
                return;
            }

            // We need the ProjectId. 
            // In a real app we might store this in the session too.
            // Let's re-fetch the student to get the ProjectId to be safe.
            var student = await _mongoDBService.GetOrCreateStudent(studentEmail, "Student"); // Name irrelevant for fetch

            if (student != null && !string.IsNullOrEmpty(student.ProjectId))
            {
                var tasks = await _mongoDBService.GetTasksByProject(student.ProjectId);
                
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }

                // Temporary: If no tasks, add a dummy task to verify UI
                if (Tasks.Count == 0)
                {
                   /* Tasks.Add(new StudentTask 
                    { 
                        Title = "Tarea de Ejemplo", 
                        Description = "Esta es una tarea generada automáticamente para probar la interfaz.", 
                        Status = "Pendiente",
                        DueDate = DateTime.Now.AddDays(7),
                        IsCompleted = false
                    }); */
                }
            }
            else
            {
                // Student has no project assigned yet
                Tasks.Clear();
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar las tareas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            TasksRefreshView.IsRefreshing = false;
        }
    }

    private async void TasksRefreshView_Refreshing(object sender, EventArgs e)
    {
        await LoadTasks();
    }
}
