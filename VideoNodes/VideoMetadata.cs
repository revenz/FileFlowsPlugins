using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileFlows.VideoNodes;

internal class VideoMetadata
{
    public string Title { get; set; }

    public string Subtitle { get; set; }
    public string Description { get; set; }
    public int Year { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string ArtJpeg { get; set; }
    public int? Season { get; set; }
    public int? Episode { get; set; }

    private List<string> _Actors = new ();
    public List<string> Actors { get => _Actors; set { _Actors = value ?? new(); } }

    private List<string> _Directors = new();
    public List<string> Directors { get => _Directors; set { _Directors = value ?? new(); } }

    private List<string> _Writers = new();
    public List<string> Writers { get => _Writers; set { _Writers = value ?? new(); } }

    private List<string> _Producers = new();
    public List<string> Producers { get => _Producers; set { _Producers = value ?? new(); } }

    private List<string> _Genres = new();
    public List<string> Genres { get => _Genres; set { _Genres = value ?? new(); } }

    public static VideoMetadata Convert(object source)
    {
        try
        {
            string json = JsonSerializer.Serialize(source);
            var result = JsonSerializer.Deserialize<VideoMetadata>(json);
            return result ?? new ();
        }
        catch (Exception)
        {
            return new (); 
        }
    }
}
