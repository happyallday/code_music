using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using CodeMusic.Server.Models;

namespace CodeMusic.Server.Services;

public interface IMusicApiService
{
    Task<SearchResult> SearchAsync(string keyword);
    Task<Song?> GetSongDetailAsync(string songId, string source);
    Task<byte[]> DownloadAsync(string url);
}

public class MusicApiService : IMusicApiService
{
    private readonly HttpClient _httpClient;
    private const string QQMusicApiUrl = "https://www.hhlqilongzhu.cn/api/dg_qqmusic_SQ.php";
    private const string NetEaseApiUrl = "https://www.hhlqilongzhu.cn/api/dg_wyymusic.php";

    public MusicApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<SearchResult> SearchAsync(string keyword)
    {
        var result = new SearchResult();

        try
        {
            var qqResult = await SearchQQMusicAsync(keyword);
            result.Data.AddRange(qqResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"QQ音乐搜索失败: {ex.Message}");
        }

        try
        {
            var neResult = await SearchNetEaseAsync(keyword);
            result.Data.AddRange(neResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"网易云搜索失败: {ex.Message}");
        }

        for (int i = 0; i < result.Data.Count; i++)
        {
            result.Data[i].Id = i + 1;
        }

        result.Code = result.Data.Count > 0 ? 200 : 404;
        return result;
    }

    private async Task<List<Song>> SearchQQMusicAsync(string keyword)
    {
        var songs = new List<Song>();
        var url = $"{QQMusicApiUrl}?msg={HttpUtility.UrlEncode(keyword)}&type=json";
        
        var response = await _httpClient.GetStringAsync(url);
        var jsonDoc = JsonDocument.Parse(response);
        var root = jsonDoc.RootElement;
        
        if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() == 200)
        {
            if (jsonDoc.RootElement.TryGetProperty("data", out var data))
            {
                var song = new Song
                {
                    Name = data.GetProperty("song_name").GetString() ?? "",
                    Singer = data.GetProperty("song_singer").GetString() ?? "",
                    CoverUrl = data.TryGetProperty("cover", out var cover) ? cover.GetString() ?? "" : "",
                    MusicUrl = data.TryGetProperty("music_url", out var musicUrl) ? musicUrl.GetString() ?? "" : "",
                    Lyric = data.TryGetProperty("lyric", out var lyric) ? lyric.GetString() ?? "" : "",
                    Quality = data.TryGetProperty("quality", out var quality) ? quality.GetString() ?? "" : "",
                    Source = "QQMusic",
                    SongId = data.TryGetProperty("songmid", out var songmid) ? songmid.GetString() ?? "" : ""
                };
                songs.Add(song);
            }
        }
        
        return songs;
    }

    private async Task<List<Song>> SearchNetEaseAsync(string keyword)
    {
        var songs = new List<Song>();
        var url = $"{NetEaseApiUrl}?gm={HttpUtility.UrlEncode(keyword)}&type=text&br=1";
        
        var response = await _httpClient.GetStringAsync(url);
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        int startIndex = songs.Count;
        
        foreach (var line in lines.Take(10))
        {
            var match = Regex.Match(line, @"(\d+)\.(.+?)☆(.+)");
            if (match.Success)
            {
                songs.Add(new Song
                {
                    Name = match.Groups[2].Value.Trim(),
                    Singer = match.Groups[3].Value.Trim(),
                    Source = "NetEase",
                    SongId = match.Groups[1].Value
                });
            }
        }
        
        return songs;
    }

    public async Task<Song?> GetSongDetailAsync(string songId, string source)
    {
        try
        {
            if (source == "QQMusic")
            {
                var url = $"{QQMusicApiUrl}?songmid={songId}&type=json";
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() == 200)
                {
                    if (root.TryGetProperty("data", out var data))
                    {
                        return new Song
                        {
                            Name = data.GetProperty("song_name").GetString() ?? "",
                            Singer = data.GetProperty("song_singer").GetString() ?? "",
                            CoverUrl = data.TryGetProperty("cover", out var cover) ? cover.GetString() ?? "" : "",
                            MusicUrl = data.TryGetProperty("music_url", out var musicUrl) ? musicUrl.GetString() ?? "" : "",
                            Lyric = data.TryGetProperty("lyric", out var lyric) ? lyric.GetString() ?? "" : "",
                            Quality = data.TryGetProperty("quality", out var quality) ? quality.GetString() ?? "" : "",
                            Source = "QQMusic",
                            SongId = songId
                        };
                    }
                }
            }
            else if (source == "NetEase")
            {
                var url = $"{NetEaseApiUrl}?n={songId}&type=text&br=1";
                var response = await _httpClient.GetStringAsync(url);
                
                var match = Regex.Match(response, @"(.+?)☆(.+)");
                if (match.Success)
                {
                    var song = new Song
                    {
                        Name = match.Groups[1].Value.Trim(),
                        Singer = match.Groups[2].Value.Trim(),
                        Source = "NetEase",
                        SongId = songId
                    };
                    
                    var detailUrl = $"{NetEaseApiUrl}?n={songId}&type=json&br=1";
                    var detailResponse = await _httpClient.GetStringAsync(detailUrl);
                    var detailDoc = JsonDocument.Parse(detailResponse);
                    
                    if (detailDoc.RootElement.TryGetProperty("data", out var data))
                    {
                        song.MusicUrl = data.TryGetProperty("music_url", out var musicUrl) ? musicUrl.GetString() ?? "" : "";
                        song.CoverUrl = data.TryGetProperty("cover", out var cover) ? cover.GetString() ?? "" : "";
                    }
                    
                    return song;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取歌曲详情失败: {ex.Message}");
        }
        
        return null;
    }

    public async Task<byte[]> DownloadAsync(string url)
    {
        return await _httpClient.GetByteArrayAsync(url);
    }
}