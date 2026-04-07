using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Avalonia.Threading;
using CodeMusic.Client.Models;
using CodeMusic.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CodeMusic.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly IAudioPlayer _audioPlayer;
    private System.Timers.Timer? _progressTimer;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Song> _songs = new();

    [ObservableProperty]
    private Song? _selectedSong;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _currentPosition = "00:00";

    [ObservableProperty]
    private string _totalDuration = "00:00";

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private double _volume = 50;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private string _playPauseIcon = "▶";

    public MainWindowViewModel()
    {
        _apiService = new ApiService();
        _audioPlayer = new AudioPlayer();
        
        _audioPlayer.PlaybackStateChanged += OnPlaybackStateChanged;
        _audioPlayer.PositionChanged += OnPositionChanged;
        _audioPlayer.PlaybackEnded += OnPlaybackEnded;
    }

    private void OnPlaybackStateChanged(object? sender, PlaybackStateEventArgs e)
    {
        Dispatcher.UIThread.Post(() => 
        {
            IsPlaying = e.IsPlaying;
            PlayPauseIcon = e.IsPlaying ? "⏸" : "▶";
        });
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_audioPlayer.Duration.TotalSeconds > 0)
            {
                Progress = position.TotalSeconds / _audioPlayer.Duration.TotalSeconds * 100;
                CurrentPosition = position.ToString(@"mm\:ss");
                TotalDuration = _audioPlayer.Duration.ToString(@"mm\:ss");
            }
        });
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsPlaying = false;
            Progress = 0;
            CurrentPosition = "00:00";
        });
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "搜索中...";
            var result = await _apiService.SearchAsync(SearchText);
            Songs.Clear();
            foreach (var song in result.Data)
            {
                Songs.Add(song);
            }
            StatusMessage = $"找到 {Songs.Count} 首歌曲";
        }
        catch (Exception ex)
        {
            StatusMessage = $"搜索失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadHotSongsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "加载热门歌曲...";
            var result = await _apiService.SearchAsync("热门");
            Songs.Clear();
            foreach (var song in result.Data)
            {
                Songs.Add(song);
            }
            StatusMessage = $"找到 {Songs.Count} 首歌曲";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PlaySongAsync()
    {
        if (SelectedSong == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = "获取歌曲信息...";
            
            var detail = await _apiService.GetSongDetailAsync(SelectedSong.Source, SelectedSong.SongId);
            if (detail != null && !string.IsNullOrEmpty(detail.MusicUrl))
            {
                SelectedSong.MusicUrl = detail.MusicUrl;
                _audioPlayer.Play(detail.MusicUrl);
                StatusMessage = $"正在播放: {detail.Name} - {detail.Singer}";
            }
            else
            {
                StatusMessage = "无法获取播放地址";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"播放失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void TogglePlayPause()
    {
        if (IsPlaying)
        {
            _audioPlayer.Pause();
            StatusMessage = "已暂停";
        }
        else if (SelectedSong != null)
        {
            if (!string.IsNullOrEmpty(SelectedSong.MusicUrl))
            {
                _audioPlayer.Play(SelectedSong.MusicUrl);
                StatusMessage = "继续播放";
            }
            else
            {
                _ = PlaySongAsync();
            }
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _audioPlayer.Stop();
        StatusMessage = "已停止";
        Progress = 0;
        CurrentPosition = "00:00";
    }

    [RelayCommand]
    private async Task DownloadSongAsync()
    {
        if (SelectedSong == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = "下载中...";
            
            var detail = await _apiService.GetSongDetailAsync(SelectedSong.Source, SelectedSong.SongId);
            if (detail == null || string.IsNullOrEmpty(detail.MusicUrl))
            {
                StatusMessage = "无法获取下载链接";
                return;
            }

            var musicUrl = detail.MusicUrl;
            var ext = musicUrl.Contains(".flac") ? ".flac" : ".mp3";
            var fileName = $"{detail.Name} - {detail.Singer}{ext}";
            
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var filePath = Path.Combine(downloadsPath, fileName);
            
            var data = await _apiService.DownloadAsync(musicUrl);
            await File.WriteAllBytesAsync(filePath, data);
            
            StatusMessage = $"已下载到: {filePath}";
            Process.Start(new ProcessStartInfo
            {
                FileName = downloadsPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"下载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnVolumeChanged(double value)
    {
        _audioPlayer.Volume = (float)value;
    }

    public void Cleanup()
    {
        _audioPlayer.Dispose();
    }
}