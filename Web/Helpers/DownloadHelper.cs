using System.Net;
using System.Text.RegularExpressions;

namespace FileFlows.Web.Helpers;

/// <summary>
/// Helper to do a download
/// </summary>
public static class DownloadHelper
{
    /// <summary>
    /// The HttpClient
    /// </summary>
    private static HttpClient? client;
    
    /// <summary>
    /// Performs the download
    /// </summary>
    /// <param name="logger">the logger to use</param>
    /// <param name="url">the URL to download</param>
    /// <param name="destinationPath">the destination path</param>
    /// <param name="percentUpdate">the percent update</param>
    /// <returns>the name of the file if successful, otherwise an error</returns>
    public static Result<string> Download(ILogger logger, string url, string destinationPath, Action<float> percentUpdate)
    {
        if (client == null)
        {

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }


        try
        {
            var tempFile = Path.Combine(destinationPath, Guid.NewGuid().ToString());

            using (var response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    return Result<string>.Fail($"Failed to download URL: {url}. Status code: {response.StatusCode}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                if(string.IsNullOrEmpty(contentType) == false)
                    logger?.ILog("ContentType: " + contentType);
                var fileExtension = GetFileExtensionFromContentType(contentType);

                // Check if the URL response contains a filename
                if (response.Content.Headers.ContentDisposition?.FileName != null)
                {
                    var sanitizedFileName = SanitizeFileName(response.Content.Headers.ContentDisposition.FileName.Trim('"'));
                    tempFile = Path.Combine(destinationPath, sanitizedFileName);
                }
                else
                {
                    if (fileExtension == null)
                    {
                        // Check for common file headers if the content type is not recognized
                        var buffer = new byte[512];
                        using (var contentStream = response.Content.ReadAsStreamAsync().Result)
                        {
                            contentStream.Read(buffer, 0, buffer.Length);
                            fileExtension = GetFileExtensionFromHeader(buffer) ?? ".html";
                            contentStream.Position = 0; // Reset stream position for reading again
                        }
                    }

                    tempFile += fileExtension;
                }

                using (var contentStream = response.Content.ReadAsStreamAsync().Result)
                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    while (isMoreToRead)
                    {
                        var read = contentStream.ReadAsync(buffer, 0, buffer.Length).Result;
                        if (read == 0)
                        {
                            isMoreToRead = false;
                            continue;
                        }

                        fileStream.WriteAsync(buffer, 0, read).Wait();
                        totalRead += read;

                        if (totalBytes != -1)
                        {
                            var progress = (float)totalRead / totalBytes;
                            percentUpdate?.Invoke(progress);
                        }
                    }
                }
            }

            logger?.ILog($"Downloaded file saved to: {tempFile}");
            return tempFile;
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Exception during download: {ex.Message}");
        }
    }


    /// <summary>
    /// Gets the file extension from the content type.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The corresponding file extension, or null if not recognized.</returns>
    private static string? GetFileExtensionFromContentType(string? contentType)
    {
        switch (contentType)
        {
            case "text/html": return ".html";
            case "image/jpeg": return ".jpg";
            case "image/png": return ".png";
            case "image/gif": return ".gif";
            case "application/pdf": return ".pdf";
            case "application/zip": return ".zip";
            case "application/json": return ".json";
            case "text/plain": return ".txt";
            case "audio/mpeg": return ".mp3";
            case "video/mp4": return ".mp4";
            case "application/vnd.ms-excel": return ".xls";
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": return ".xlsx";
            case "application/msword": return ".doc";
            case "application/vnd.openxmlformats-officedocument.wordprocessingml.document": return ".docx";
            case "application/vnd.ms-powerpoint": return ".ppt";
            case "application/vnd.openxmlformats-officedocument.presentationml.presentation": return ".pptx";
            case "application/x-rar-compressed": return ".rar";
            case "application/x-tar": return ".tar";
            case "application/x-7z-compressed": return ".7z";
            // Add more content types and their corresponding file extensions as needed
            default: return null;
        }
    }

    /// <summary>
    /// Gets the file extension from the file header bytes.
    /// </summary>
    /// <param name="fileHeader">The first few bytes of the file to identify its type.</param>
    /// <returns>The corresponding file extension, or null if not recognized.</returns>
    private static string? GetFileExtensionFromHeader(byte[] fileHeader)
    {
        // Implement logic to identify file types based on header bytes
        // Example: Check for common file signatures
        if (fileHeader.Length >= 4)
        {
            // PDF file signature
            if (fileHeader[0] == 0x25 && fileHeader[1] == 0x50 && fileHeader[2] == 0x44 && fileHeader[3] == 0x46)
            {
                return ".pdf";
            }

            // ZIP file signature
            if (fileHeader[0] == 0x50 && fileHeader[1] == 0x4B &&
                (fileHeader[2] == 0x03 || fileHeader[2] == 0x05 || fileHeader[2] == 0x07) && fileHeader[3] == 0x08)
            {
                return ".zip";
            }

            // PNG file signature
            if (fileHeader[0] == 0x89 && fileHeader[1] == 0x50 && fileHeader[2] == 0x4E && fileHeader[3] == 0x47)
            {
                return ".png";
            }

            // JPEG file signature
            if (fileHeader[0] == 0xFF && fileHeader[1] == 0xD8 && fileHeader[fileHeader.Length - 2] == 0xFF &&
                fileHeader[fileHeader.Length - 1] == 0xD9)
            {
                return ".jpg";
            }
        }

        return null;
    }
    

    /// <summary>
    /// Sanitizes the filename to ensure it does not contain any path traversal characters or invalid characters.
    /// </summary>
    /// <param name="fileName">The filename to sanitize.</param>
    /// <returns>The sanitized filename.</returns>
    private static string SanitizeFileName(string fileName)
    {
        // Remove any path traversal characters
        fileName = Regex.Replace(fileName, @"\.\.\/|\\|\.\.\\|\/", string.Empty);

        // Only allow safe characters in the filename
        fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\-\.]", "_");

        return fileName;
    }
}