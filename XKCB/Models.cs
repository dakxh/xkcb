using System.Collections.Generic;
using System.Text.Json; // <-- Added this
using System.Text.Json.Serialization;

namespace XKCB
{
    public class MediaItem
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        // The API sends a Number, so we capture the raw JSON element...
        [JsonPropertyName("year")]
        public JsonElement RawYear { get; set; }

        // ...and safely convert it to a String for the XAML UI behind the scenes!
        [JsonIgnore]
        public string Year => RawYear.ValueKind != JsonValueKind.Undefined ? RawYear.ToString() : string.Empty;

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("poster_url")]
        public string? PosterUrl { get; set; }
    }

    public class CatalogResponse
    {
        [JsonPropertyName("data")]
        public List<MediaItem> Data { get; set; } = new List<MediaItem>();

        [JsonPropertyName("next_cursor")]
        public int? NextCursor { get; set; }
    }

    // Represents a single 4K/1080p variation of a movie
    public class MovieSource
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("variation_name")] public string? VariationName { get; set; }
        [JsonPropertyName("quality")] public string? Quality { get; set; }
        [JsonPropertyName("is_imax")] public int IsImax { get; set; }
        [JsonPropertyName("is_hdr")] public int IsHdr { get; set; }
    }

    // Represents an episode of a TV Series
    public class Episode
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("episode_number")] public int EpisodeNumber { get; set; }
        [JsonPropertyName("episode_name")] public string? EpisodeName { get; set; }
        [JsonPropertyName("duration")] public string? Duration { get; set; }
        [JsonPropertyName("quality")] public string? Quality { get; set; }
    }

    // Represents a Season containing Episodes
    public class Season
    {
        [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
        [JsonPropertyName("episodes")] public List<Episode> Episodes { get; set; } = new List<Episode>();
    }

    // The full Details payload
    public class MediaDetails
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }

        [JsonPropertyName("year")] public JsonElement RawYear { get; set; }
        [JsonIgnore] public string Year => RawYear.ValueKind != JsonValueKind.Undefined ? RawYear.ToString() : string.Empty;

        [JsonPropertyName("rating")] public double Rating { get; set; }
        [JsonPropertyName("poster_url")] public string? PosterUrl { get; set; }
        [JsonPropertyName("backdrop_url")] public string? BackdropUrl { get; set; }

        [JsonPropertyName("sources")] public List<MovieSource>? Sources { get; set; }
        [JsonPropertyName("seasons")] public List<Season>? Seasons { get; set; }
    }

    public class WatchResponse
    {
        [JsonPropertyName("hls_manifest_url")]
        public string? HlsManifestUrl { get; set; }

        [JsonPropertyName("timeline_thumbnails_url")]
        public string? TimelineThumbnailsUrl { get; set; }

        // The worker returns these as stringified JSON arrays, 
        // but since we are just passing them to mpv, we can grab the raw properties if needed later.
        // For right now, we just need the manifest URL to start playback!
    }
}