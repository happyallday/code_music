namespace CodeMusic.Server.Models;

public class Song
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Singer { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public string MusicUrl { get; set; } = string.Empty;
    public string Lyric { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SongId { get; set; } = string.Empty;
}

public class SearchResult
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Song> Data { get; set; } = new();
}

public class QQMusicSearchResponse
{
    public int Code { get; set; }
    public QQMusicData? Data { get; set; }
}

public class QQMusicData
{
    public string Song_name { get; set; } = string.Empty;
    public string Song_singer { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string Cover { get; set; } = string.Empty;
    public string Music_url { get; set; } = string.Empty;
    public string Lyric { get; set; } = string.Empty;
}

public class NetEaseSearchItem
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Singer { get; set; } = string.Empty;
}