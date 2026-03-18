using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core;
using EvaluatorApp.Models;

namespace EvaluatorApp.Components;

public partial class ProjectCard : ContentView
{
    // --- Static Registry: all active ProjectCards register here ---
    private static readonly List<ProjectCard> _activeCards = new();
    public static IReadOnlyList<ProjectCard> ActiveCards => _activeCards;

    private bool _isVideoLoaded = false;
    private bool _isPlaying = false;

    public ProjectCard()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        _isVideoLoaded = false;
        _isPlaying = false;

        // Unregister old instance and re-register
        _activeCards.Remove(this);

        if (BindingContext is Project project)
        {
            _activeCards.Add(this);

            if (project.HasVideo)
            {
                VideoBadge.IsVisible = true;
            }
            else
            {
                VideoBadge.IsVisible = false;
                VideoPlayer.IsVisible = false;
            }
        }
    }

    /// <summary>
    /// Gets the Y position of this card relative to its containing ScrollView content.
    /// </summary>
    public double GetYInParent()
    {
        double y = 0;
        var current = this as VisualElement;

        // Walk up the tree, summing Y positions, until we hit the ScrollView content
        while (current != null)
        {
            y += current.Y;
            if (current.Parent is ScrollView)
                break;
            current = current.Parent as VisualElement;
        }

        return y;
    }

    public double CardHeight => this.Height > 0 ? this.Height : 260; // fallback

    public bool HasVideo => BindingContext is Project p && p.HasVideo;

    public void PlayVideo()
    {
        if (BindingContext is not Project project || !project.HasVideo)
            return;

        if (_isPlaying)
            return;

        try
        {
            var videoUrl = project.Videos[0]?.Url;
            if (string.IsNullOrEmpty(videoUrl))
                return;

            if (!_isVideoLoaded)
            {
                if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri))
                    return;

                VideoPlayer.Source = MediaSource.FromUri(uri);
                _isVideoLoaded = true;
            }

            VideoPlayer.IsVisible = true;
            VideoPlayer.ShouldAutoPlay = true;

            // Use delayed play to ensure the MediaElement is ready
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(200);
                try
                {
                    VideoPlayer.Play();
                    _isPlaying = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProjectCard] Play error: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectCard] PlayVideo error: {ex.Message}");
        }
    }

    public void PauseVideo()
    {
        if (!_isPlaying)
            return;

        try
        {
            VideoPlayer.ShouldAutoPlay = false;
            VideoPlayer.Pause();
            _isPlaying = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectCard] PauseVideo error: {ex.Message}");
        }
    }

    public void StopAndRelease()
    {
        try
        {
            VideoPlayer.ShouldAutoPlay = false;
            VideoPlayer.Stop();
            VideoPlayer.IsVisible = false;
            _isPlaying = false;
            _isVideoLoaded = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectCard] StopAndRelease error: {ex.Message}");
        }
    }
}
