using Microsoft.AspNetCore.Mvc;
using CodeMusic.Server.Models;
using CodeMusic.Server.Services;

namespace CodeMusic.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    private readonly IMusicApiService _musicService;

    public SongsController(IMusicApiService musicService)
    {
        _musicService = musicService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<SearchResult>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("搜索关键词不能为空");

        var result = await _musicService.SearchAsync(q);
        return Ok(result);
    }

    [HttpGet("{source}/{songId}")]
    public async Task<ActionResult<Song>> GetDetail(string source, string songId)
    {
        var song = await _musicService.GetSongDetailAsync(songId, source);
        if (song == null)
            return NotFound("歌曲不存在");

        return Ok(song);
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest("URL不能为空");

        try
        {
            var data = await _musicService.DownloadAsync(url);
            return File(data, "audio/mpeg", "song.mp3");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"下载失败: {ex.Message}");
        }
    }

    [HttpGet("hot")]
    public async Task<ActionResult<SearchResult>> GetHotSongs()
    {
        var hotKeywords = new[] { "周杰伦", "邓紫棋", "林俊杰", "五月天", "告五人" };
        var random = new Random();
        var keyword = hotKeywords[random.Next(hotKeywords.Length)];

        var result = await _musicService.SearchAsync(keyword);
        return Ok(result);
    }
}