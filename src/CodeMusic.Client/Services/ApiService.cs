using System.Net.Http.Json;
using CodeMusic.Client.Models;

namespace CodeMusic.Client.Services;

public interface IApiService
{
    Task<SearchResult> SearchAsync(string keyword);
    Task<Song?> GetSongDetailAsync(string source, string songId);
    Task<byte[]> DownloadAsync(string url);
    Task<byte[]> DownloadSongAsync(string source, string songId);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5000/api";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<SearchResult> SearchAsync(string keyword)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/songs/search?q={Uri.EscapeDataString(keyword)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SearchResult>() ?? new SearchResult();
    }

    public async Task<Song?> GetSongDetailAsync(string source, string songId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/songs/{source}/{songId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Song>();
        }
        return null;
    }

    public async Task<byte[]> DownloadAsync(string url)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/songs/download?url={Uri.EscapeDataString(url)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]> DownloadSongAsync(string source, string songId)
    {
        var song = await GetSongDetailAsync(source, songId);
        if (song == null || string.IsNullOrEmpty(song.MusicUrl))
        {
            throw new Exception("无法获取歌曲URL");
        }
        return await DownloadAsync(song.MusicUrl);
    }
}