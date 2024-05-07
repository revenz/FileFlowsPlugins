using HtmlAgilityPack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Metadata;

//using ImageMagick;

namespace FileFlows.SiteScraping.Scraping;

/// <summary>
/// Scrapes a website, follows links within a specified depth, and saves images over a certain resolution.
/// </summary>
public class WebImageScraper : Node
{
    private HashSet<string> savedImages = new ();
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(5); // Limit to 5 concurrent image downloads

    private int PageCount = 0;

    /// <inheritdoc />
    public override string Icon => "fas fa-globe-asia";

    /// <summary>
    /// Gets or sets the maximum depth of links to follow.
    /// </summary>
    [NumberInt(1)]
    public int MaxDepth { get; set; }
        
    /// <summary>
    /// Gets or sets the minimum width of images to save.
    /// </summary>
    [NumberInt(2)]
    public int MinWidth { get; set; }
        
    /// <summary>
    /// Gets or sets the minimum height of images to save.
    /// </summary>
    [NumberInt(3)]
    public int MinHeight { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of pages to process
    /// </summary>
    [NumberInt(4)]
    public int MaxPages { get; set; } = 20;

    /// <summary>
    /// Starts the web scraping process.
    /// </summary>
    /// <param name="args">the node parameters used during execution</param>
    public override int Execute(NodeParameters args)
    {
        string textFile = args.WorkingFile;
        if (File.Exists(textFile) == false)
        {
            args.Logger?.ELog($"File '{textFile}' does not exist.");
            return -1; // Indicate failure
        }

        string[] lines = File.ReadAllLines(textFile);
        if (lines.Length > 1000)
        {
            args.Logger?.WLog("The file contains more than 1000 lines of URLs.");
        }

        lines = lines.Take(1000).Where(x => string.IsNullOrWhiteSpace(x) == false).Select(x => x.Trim()).ToArray();
        

        var allImageUrls = new List<(string, List<string>)>();
        foreach (string line in lines) // Limiting to process only 1000 lines
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            string path = string.Empty;
            if (lines.Length > 1)
            {
                Uri.TryCreate(line, UriKind.Absolute, out Uri? uri);
                if (uri != null)
                {
                    string uriPath = uri.AbsolutePath.Trim('/');
                    if (!string.IsNullOrEmpty(uriPath))
                    {
                        string[] segments = uriPath.Split('/');
                        path = segments.Last();
                    }
                }
            }

            path = string.IsNullOrWhiteSpace(path) ? args.TempPath : Path.Combine(args.TempPath, path);
            
            PageCount = 0;
            var imageUrls = ProcessUrlAndGetImageUrls(args.Logger!, line.Trim(), 0, null).GetAwaiter().GetResult();
            allImageUrls.Add((path, imageUrls));
        }

        SaveUrlsToFile(allImageUrls.SelectMany(x => x.Item2).ToList(), Path.Combine(args.TempPath, "image_urls.txt")).GetAwaiter().GetResult();
        int totalDownloaded = DownloadImages(args.Logger!, allImageUrls, args.TempPath).GetAwaiter().GetResult();
        args.Logger?.ILog($"Total images downloaded: {totalDownloaded}");
        
        return totalDownloaded > 0 ? 1 : 2;
    }
    
    /// <summary>
    /// Saves a list of URLs to a text file asynchronously.
    /// </summary>
    /// <param name="urls">The list of URLs to be saved.</param>
    /// <param name="fileName">The name of the file to save the URLs.</param>
    /// <returns>A task representing the asynchronous saving operation.</returns>
    private async Task SaveUrlsToFile(List<string> urls, string fileName)
    {
        try
        {
            await File.WriteAllLinesAsync(fileName, urls);
        }
        catch (Exception)
        {
            // Handle file writing error
        }
    }
    
    /// <summary>
    /// Downloads images asynchronously from a list of image URLs.
    /// </summary>
    /// <param name="logger">The logger implementation for logging.</param>
    /// <param name="imageUrls">The list of image URLs to download.</param>
    /// <param name="outputPath">the base path to save the images to</param>
    /// <returns>The total count of downloaded images.</returns>
    private async Task<int> DownloadImages(ILogger logger, List<(string path, List<string> urls)> imageUrls, string outputPath)
    {
        int totalDownloaded = 0;

        var downloadTasks = new List<Task>();
        foreach (var imageData in imageUrls)
        {
            foreach (var imageUrl in imageData.urls)
            {
                downloadTasks.Add(ProcessImageAsync(logger, imageUrl, Path.Combine(outputPath, imageData.path)));
            }
        }
        await Task.WhenAll(downloadTasks);

        totalDownloaded = downloadTasks.Count(task => task.IsCompletedSuccessfully);

        // if (totalDownloaded > 1)
        // {
        //     var paths = imageUrls.Select(x => x.path).Distinct().ToList();
        //     if (paths.Count > 1)
        //     {
        //         foreach(var path in paths)
        //             DeleteDuplicates(logger, path);
        //     }
        //     else
        //     {
        //         DeleteDuplicates(logger, outputPath);
        //     }
        // }

        return totalDownloaded;
    }
    
    /// <summary>
    /// Processes a URL to extract image URLs and follows links within a specified depth.
    /// </summary>
    /// <param name="logger">The logger implementation for logging.</param>
    /// <param name="url">The URL to be processed.</param>
    /// <param name="depth">The current depth of link following.</param>
    /// <param name="baseUrl">The base URL for domain comparison.</param>
    /// <returns>A list of image URLs extracted from the URL and its sub-links.</returns>
    private async Task<List<string>> ProcessUrlAndGetImageUrls(ILogger logger, string url, int depth, string? baseUrl)
    {
        var imageUrls = new List<string>();
        if (++PageCount > MaxPages)
            return imageUrls;
        
        if (string.IsNullOrEmpty(baseUrl))
        {
            Uri.TryCreate(url, UriKind.Absolute, out Uri? uri);
            baseUrl = uri?.GetLeftPart(UriPartial.Path); // Retrieve URL up to path level
        }
    
        // Ensure the base URL doesn't contain query parameters
        if (!string.IsNullOrEmpty(baseUrl) && baseUrl.Contains('?'))
        {
            baseUrl = baseUrl[..baseUrl.IndexOf('?')];
        }

        if (baseUrl == null)
            return imageUrls;

        baseUrl = baseUrl.TrimEnd('/');

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string htmlContent = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var currentImageUrls = doc.DocumentNode.SelectNodes("//img[@src]")?
                    .Select(node => node.Attributes["src"]?.Value)
                    .Where(i => string.IsNullOrWhiteSpace(i) == false && IsImageUrl(i))
                    .ToList() ?? new ();
                
                var dataSrc = doc.DocumentNode.SelectNodes("//*[@data-src]")?
                    .Select(node =>node.Attributes["data-src"]?.Value)
                    .Where(i => string.IsNullOrWhiteSpace(i) == false && IsImageUrl(i))
                    .ToList() ?? new ();
                currentImageUrls.AddRange(dataSrc);
                
                var hrefSrc = doc.DocumentNode.SelectNodes("//a[@href]")?
                    .Select(node =>node.Attributes["href"]?.Value)
                    .Where(i => string.IsNullOrWhiteSpace(i) == false && IsImageUrl(i))
                    .ToList() ?? new ();

                currentImageUrls.AddRange(hrefSrc);

                if (currentImageUrls?.Any() == true)
                {
                    foreach (var imageUrl in currentImageUrls)
                    {
                        if (imageUrl == null || savedImages.Contains(imageUrl)) 
                            continue;
                        imageUrls.Add(imageUrl);
                        savedImages.Add(imageUrl);
                    }
                }

                var links = doc.DocumentNode.SelectNodes("//a[@href]")
                    ?.Select(a => a.Attributes["href"].Value)
                    .Where(a => string.IsNullOrWhiteSpace(a) == false && IsSameDomain(a, baseUrl))
                    .ToList();

                if (links != null && depth < MaxDepth)
                {
                    foreach (var link in links)
                    {
                        if (string.IsNullOrWhiteSpace(link) || link.StartsWith("#"))
                            continue;

                        // Convert relative URLs to absolute URLs
                        var absoluteLink = ConvertToAbsoluteUrl(baseUrl, link);

                        if (baseUrl == absoluteLink)
                            continue;

                        if (absoluteLink.StartsWith(baseUrl) == false)
                            continue;

                        try
                        {
                            var urlsFromLink = await ProcessUrlAndGetImageUrls(logger, absoluteLink, depth + 1, baseUrl);
                            imageUrls.AddRange(urlsFromLink);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }
            else
            {
                logger.WLog($"Failed to download images from URL: {url}. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.ELog($"Error processing URL: {url}. {ex.Message}");
        }

        return imageUrls;
    }
    


    /// <summary>
    /// Converts a relative URL to an absolute URL based on the base URL.
    /// </summary>
    /// <param name="baseUrl">The base URL used for domain comparison.</param>
    /// <param name="link">The URL to be converted to an absolute URL.</param>
    /// <returns>The absolute URL.</returns>
    private string ConvertToAbsoluteUrl(string baseUrl, string link)
    {
        if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
        {
            // Absolute URL
            return link;
        }
        else if (link.StartsWith("/"))
        {
            // Root-relative URL
            if (!baseUrl.EndsWith("/"))
            {
                // Ensure the base URL ends with a "/"
                baseUrl += "/";
            }

            // Combine the base URL and root-relative link manually
            return $"{baseUrl.TrimEnd('/')}/{link.TrimStart('/')}";
        }
        else
        {
            // Path-relative URL
            var baseUri = new Uri(baseUrl);
            var combinedUri = new Uri(baseUri, link);
            return combinedUri.ToString();
        }
    }
    
    /// <summary>
    /// Checks if the provided URL is a valid image URL based on common image extensions.
    /// </summary>
    /// <param name="imageUrl">The URL to be checked.</param>
    /// <returns>True if the URL ends with common image extensions; otherwise, false.</returns>
    private bool IsImageUrl(string imageUrl)
    {
        if (imageUrl == null || imageUrl.EndsWith("1px.png") || imageUrl.EndsWith("svg"))
            return false;
        // Check if the URL ends with common image extensions (e.g., jpg, png, gif, etc.)
        var imageExtensionPattern = @"\.(jpg|jpeg|jpe|png|gif|bmp|webp)$";
        var regex = new Regex(imageExtensionPattern, RegexOptions.IgnoreCase);
        return regex.IsMatch(imageUrl);
    }
    

    /// <summary>
    /// Checks if a URL is from the same domain as the specified base URL.
    /// </summary>
    /// <param name="url">The URL to be checked.</param>
    /// <param name="baseUrl">The base URL used for comparison.</param>
    /// <returns>True if the URL is from the same domain; otherwise, false.</returns>
    private bool IsSameDomain(string url, string baseUrl)
    {
        if (url?.StartsWith("#") == true)
            return false;
        
        if (string.IsNullOrEmpty(url) || url.ToLowerInvariant().StartsWith("http") == false)
            return true;
        
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) &&
            Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri))
        {
            return uri.Host == baseUri.Host;
        }
        else
        {
            // Handling case when URLs are not in a valid format
            return false;
        }
    }

    /// <summary>
    /// Saves an image from the specified URL if it meets the minimum width and height criteria.
    /// </summary>
    /// <param name="logger">The logger implementation for logging.</param>
    /// <param name="imageUrl">The URL of the image to be saved.</param>
    /// <param name="outputPath">the base path to save the images to</param>
    /// <returns>true if the image downloaded and the resolution was acceptable</returns>
    private async Task<bool> ProcessImageAsync(ILogger logger, string imageUrl, string outputPath)
    {
        try
        {
            await semaphore.WaitAsync(); // Wait for a semaphore slot

            HttpResponseMessage response = await httpClient.GetAsync(imageUrl);

            if (response.IsSuccessStatusCode)
            {
                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                using (var image = Image.Load(imageData))
                {
                    if (image.Width >= MinWidth || image.Height >= MinHeight)
                    {
                        var fileName = Path.GetFileName(imageUrl);
                        var savePath = Path.Combine(outputPath, fileName);
                        try
                        {
                            if (Directory.Exists(outputPath) == false)
                                Directory.CreateDirectory(outputPath);
                        }
                        catch (Exception)
                        {
                        }

                        await image.SaveAsync(savePath); // Save the image directly
                        savedImages.Add(imageUrl);
                        logger.ILog($"Saved image: {fileName}");
                        return true; // Signal successful download
                    }
                }
            }
            else
            {
                logger.ELog($"Failed to download image from {imageUrl}. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            logger.ELog($"Error saving image from {imageUrl}: {ex.Message}");
        }
        finally
        {
            semaphore.Release(); // Release semaphore slot
        }

        return false; // Signal unsuccessful download
    }
    
    //
    // /// <summary>
    // /// Deletes duplicate images within a directory.
    // /// </summary>
    // /// <param name="logger">the logger to write logging to</param>
    // /// <param name="directoryPath">The path to the directory containing images.</param>
    // private void DeleteDuplicates(ILogger logger, string directoryPath)
    // {
    //    
    //     Dictionary<string, List<string>> hashes = new Dictionary<string, List<string>>();
    //
    //     // Load all image files in the directory
    //     string[] imageFiles = Directory.GetFiles(directoryPath, "*.*");
    //
    //     foreach (string filePath in imageFiles)
    //     {
    //         using (var image = new MagickImage(filePath))
    //         {
    //             image.Resize(300, 300);
    //
    //             // Calculate histogram
    //             int[] histogram = CalculateHistogram(image);
    //
    //             // Convert histogram to a string for hash storage
    //             string hash = string.Join(",", histogram);
    //
    //             if (!hashes.ContainsKey(hash))
    //             {
    //                 hashes[hash] = new List<string>();
    //             }
    //
    //             hashes[hash].Add(filePath);
    //         }
    //     }
    //
    //     // Delete duplicates, keeping the largest file
    //     foreach (var kvp in hashes)
    //     {
    //         List<string> duplicateFiles = kvp.Value;
    //
    //         if (duplicateFiles.Count > 1)
    //         {
    //             string largestFile = GetLargestFile(duplicateFiles);
    //
    //             foreach (string file in duplicateFiles)
    //             {
    //                 if (file != largestFile)
    //                 {
    //                     File.Delete(file);
    //                     logger?.ILog($"Deleted duplicate: {file}");
    //                 }
    //             }
    //         }
    //     }
    // }
    //
    // // Calculate the histogram of an image
    // static int[] CalculateHistogram(MagickImage image)
    // {
    //     int[] histogram = new int[256]; // Assuming grayscale images
    //
    //     using (var pixels = image.GetPixels())
    //     {
    //         foreach (var pixel in pixels)
    //         {
    //             var grayscale = (int)(pixel.GetChannel((int)Channels.Red) * 0.3 +
    //                                   pixel.GetChannel((int)Channels.Green) * 0.59 +
    //                                   pixel.GetChannel((int)Channels.Blue) * 0.11);
    //
    //             histogram[grayscale]++;
    //         }
    //     }
    //
    //     return histogram;
    // }
    
    // private string GetLargestFile(List<string> files)
    // {
    //     long maxSize = 0;
    //     string largestFile = "";
    //
    //     foreach (string file in files)
    //     {
    //         FileInfo fileInfo = new FileInfo(file);
    //         if (fileInfo.Length > maxSize)
    //         {
    //             maxSize = fileInfo.Length;
    //             largestFile = file;
    //         }
    //     }
    //
    //     return largestFile;
    // }
}
