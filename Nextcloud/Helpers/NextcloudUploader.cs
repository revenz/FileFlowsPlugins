using System.Net;

namespace FileFlows.Nextcloud.Helpers;

/// <summary>
/// Helper class for uploading files to Nextcloud
/// </summary>
/// <param name="logger">the Logger to use</param>
/// <param name="nextcloudUrl">The URL of the Nextcloud instance.</param>
/// <param name="username">The username for authentication.</param>
/// <param name="password">The password for authentication.</param>
public class NextcloudUploader(ILogger logger, string nextcloudUrl, string username, string password)
{
    private static HttpClient client;

    static NextcloudUploader()
    {
        var handler = new HttpClientHandler
        {
            // Customize the handler as needed
            ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true // Ignore certificate errors
        };
        client = new HttpClient(handler);
    }


    /// <summary>
    /// Uploads a file to Nextcloud
    /// </summary>
    /// <param name="localFilePath">The full path to the file on disk to be uploaded.</param>
    /// <param name="remoteFilePath">The path in Nextcloud where the file should be uploaded.</param>
    /// <returns>True if the file was uploaded successfully; otherwise, false.</returns>
    public Result<bool> UploadFile(string localFilePath, string remoteFilePath)
    {
        logger?.ILog("Uploading file: " + localFilePath);
        try
        {
            string remoteFolder = remoteFilePath.Replace("\\", "/");
            remoteFolder = string.Join("/", remoteFolder.Split("/")[..^1]);
            logger?.ILog("Remote Folder: " + remoteFolder);

            var fileStream = File.OpenRead(localFilePath);

            int chunkIndex = 1;
            int chunkSize = 80_000_000; // Split into chunks of approximately 80 MB

            // Calculate the number of chunks based on the size of the compressed stream
            long fileSize = fileStream.Length;
            int numberOfChunks = (int)Math.Ceiling((double)fileSize / chunkSize);

            fileStream.Position = 0; // Reset stream position

            // Upload each chunk
            while (fileStream.Position < fileSize)
            {
                int bytesToRead = (int)Math.Min(chunkSize, fileSize - fileStream.Position);
                byte[] buffer = new byte[bytesToRead];
                _ = fileStream.Read(buffer, 0, bytesToRead);

                var chunkStream = new MemoryStream(buffer);

                if (chunkIndex == 1)
                    CreateFolder(remoteFolder).Wait();
                if (numberOfChunks == 1)
                {
                    // Upload the chunk here using UploadFilePart method
                    UploadFilePart(chunkStream, remoteFilePath).Wait();
                }
                else
                {
                    UploadFilePart(chunkStream, remoteFilePath).Wait();
                }

                chunkStream.Dispose();

                chunkIndex++;
            }

            logger?.ILog("Upload completed with chunks: " + numberOfChunks);
            return true; // All files uploaded successfully
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error uploading file: {ex.Message}");
        }
    }

    /// <summary>
    /// Uploads a file to Nextcloud
    /// </summary>
    /// <param name="compressedStream">The stream to upload.</param>
    /// <param name="remoteFilePath">The path in Nextcloud where the file should be uploaded.</param>
    /// <returns>True if the file was uploaded successfully; otherwise, false.</returns>
    private async Task<bool> UploadFilePart(MemoryStream compressedStream, string remoteFilePath)
    {
        logger?.ILog("Uploading file: " + remoteFilePath);

        try
        {
            // Create the WebDAV request URL
            string url = $"{nextcloudUrl.TrimEnd('/')}/remote.php/dav/files/{username}/{remoteFilePath.TrimStart('/')}";

            // Set the credentials
            var byteArray = new UTF8Encoding().GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            // Set the content
            compressedStream.Position = 0;
            using (StreamContent content = new StreamContent(compressedStream))
            {
                content.Headers.ContentLength = compressedStream.Length;

                // Send the PUT request
                HttpResponseMessage response = await client.PutAsync(url, content);

                // Check for success
                if (response.StatusCode == HttpStatusCode.Created)
                    return true;
            }
        }
        catch (Exception ex)
        {
            // Handle any errors
            logger?.ELog($"Error uploading file: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Creates a folder on the WebDAV server at the specified path, ensuring all parent directories are created.
    /// </summary>
    /// <param name="remoteFolderPath">The path of the folder to be created on the server.</param>
    /// <returns>True if the folder creation is successful, otherwise false.</returns>
    private async Task<bool> CreateFolder(string remoteFolderPath)
    {
        logger?.ILog("Creating folder: " + remoteFolderPath);

        try
        {
            // Create the WebDAV request URL
            string baseUrl = $"{nextcloudUrl.TrimEnd('/')}/remote.php/dav/files/{username}";

            // Split the remoteFolderPath into parts and create each folder sequentially
            string[] folders = remoteFolderPath.Trim('/').Split('/');
            string currentPath = string.Empty;

            foreach (var folder in folders)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? folder : $"{currentPath}/{folder}";
                string url = $"{baseUrl}/{currentPath}";

                // Set the credentials
                var byteArray = new UTF8Encoding().GetBytes($"{username}:{password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Create the MKCOL request
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("MKCOL"), url);

                // Send the request
                HttpResponseMessage response = await client.SendAsync(request);

                // Check for success, ignore if it already exists
                if (response.StatusCode != HttpStatusCode.Created &&
                    response.StatusCode != HttpStatusCode.MethodNotAllowed)
                {
                    logger?.ELog($"Failed to create folder: {currentPath}, Status: {response.StatusCode}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Handle any errors
            logger?.ELog($"Error creating folder: {ex.Message}");
            return false;
        }
    }

}