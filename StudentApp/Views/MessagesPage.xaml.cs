using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Views;

public partial class MessagesPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;

    public MessagesPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNotifications();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        try
        {
            await LoadNotifications();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MessagesPage refresh error: {ex}");
        }
        finally
        {
            NotifRefreshView.IsRefreshing = false;
        }
    }

    private async Task LoadNotifications()
    {
        if (string.IsNullOrEmpty(MainPage.CurrentStudentEmail)) return;

        try
        {
            // Get student to find matrícula
            var student = await _mongoDBService.GetOrCreateStudent(
                MainPage.CurrentStudentEmail, MainPage.CurrentStudentName);
            if (student == null) return;

            var notifications = await _mongoDBService.GetNotifications(student.Matricula);

            if (notifications == null || notifications.Count == 0)
            {
                BindableLayout.SetItemsSource(NotificationListView, null);
                EmptyState.IsVisible = true;
                UnreadBadge.IsVisible = false;
                return;
            }

            EmptyState.IsVisible = false;
            BindableLayout.SetItemsSource(NotificationListView, notifications);

            // Update unread badge
            int unreadCount = notifications.Count(n => !n.IsRead);
            if (unreadCount > 0)
            {
                UnreadBadge.IsVisible = true;
                UnreadCountLabel.Text = unreadCount.ToString();
            }
            else
            {
                UnreadBadge.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadNotifications error: {ex}");
        }
    }

    private async void OnNotificationTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Notification notification)
        {
            // Mark as read
            if (!notification.IsRead)
            {
                try
                {
                    await _mongoDBService.MarkNotificationAsRead(notification.Id);
                    notification.IsRead = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MarkAsRead error: {ex}");
                }
            }

            // Navigate to project details if we have a project ID
            if (!string.IsNullOrEmpty(notification.ProjectId))
            {
                try
                {
                    var project = await _mongoDBService.GetProject(notification.ProjectId);
                    if (project != null)
                    {
                        project.RestoreVisuals();
                        await Shell.Current.GoToAsync(nameof(ProjectDetailsPage), new Dictionary<string, object>
                        {
                            { "Project", project }
                        });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigate to project error: {ex}");
                }
            }

            // Reload to show updated state
            await LoadNotifications();
        }
    }
}
