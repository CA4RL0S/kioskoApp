namespace EvaluatorApp.Services;

public class VideoService
{
    private readonly HttpClient _httpClient;

    public VideoService()
    {
        _httpClient = new HttpClient();
    }

    public string GetLocalFilePath(string videoUrl)
    {
        if (string.IsNullOrEmpty(videoUrl)) return null;
        
        // Create a safe filename from URL
        var fileName = Path.GetFileName(new Uri(videoUrl).LocalPath);
        if (string.IsNullOrEmpty(fileName)) fileName = Guid.NewGuid().ToString() + ".mp4";
        
        return Path.Combine(FileSystem.AppDataDirectory, fileName);
    }

    public bool IsVideoDownloaded(string videoUrl)
    {
        var localPath = GetLocalFilePath(videoUrl);
        return File.Exists(localPath);
    }

    public async Task<string> DownloadVideoAsync(string videoUrl, IProgress<double> progress = null)
    {
        var localPath = GetLocalFilePath(videoUrl);

        if (File.Exists(localPath)) return localPath;

        try
        {
            using (var response = await _httpClient.GetAsync(videoUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1 && progress != null;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            if (canReportProgress)
                            {
                                progress.Report((double)totalRead / totalBytes);
                            }
                        }
                    }
                    while (isMoreToRead);
                }
            }
            return localPath;
        }
        catch (Exception ex)
        {
            // Clean up partial file
            if (File.Exists(localPath)) File.Delete(localPath);
            throw new Exception($"Error downloading video: {ex.Message}");
        }
    }

    public void DeleteVideo(string videoUrl)
    {
        var localPath = GetLocalFilePath(videoUrl);
        if (File.Exists(localPath))
        {
            File.Delete(localPath);
        }
    }
    
    public void DeleteAllVideos()
    {
        var files = Directory.GetFiles(FileSystem.AppDataDirectory, "*.mp4");
        foreach (var file in files)
        {
            try { File.Delete(file); } catch { }
        }
        
        var filesMov = Directory.GetFiles(FileSystem.AppDataDirectory, "*.mov");
        foreach (var file in filesMov)
        {
            try { File.Delete(file); } catch { }
        }
    }
}
