using LibVLCSharp.Shared;

namespace CodeMusic.Client.Services;

public interface IAudioPlayer
{
    event EventHandler<PlaybackStateEventArgs>? PlaybackStateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
    event EventHandler? PlaybackEnded;
    
    bool IsPlaying { get; }
    TimeSpan Duration { get; }
    TimeSpan Position { get; }
    float Volume { get; set; }
    
    void Play(string url);
    void Pause();
    void Stop();
    void Seek(TimeSpan position);
    void Dispose();
}

public class PlaybackStateEventArgs : EventArgs
{
    public bool IsPlaying { get; set; }
    public bool IsPaused { get; set; }
    public bool IsStopped { get; set; }
}

public class AudioPlayer : IAudioPlayer, IDisposable
{
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private bool _isDisposed;

    public event EventHandler<PlaybackStateEventArgs>? PlaybackStateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler? PlaybackEnded;

    public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;
    public TimeSpan Duration => TimeSpan.FromMilliseconds(_mediaPlayer?.Length ?? 0);
    public TimeSpan Position => TimeSpan.FromMilliseconds(_mediaPlayer?.Time ?? 0);

    private float _volume = 50;
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = (int)value;
            }
        }
    }

    public AudioPlayer()
    {
        Core.Initialize();
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);
        
        _mediaPlayer.Playing += (s, e) => OnPlaybackStateChanged(true, false, false);
        _mediaPlayer.Paused += (s, e) => OnPlaybackStateChanged(false, true, false);
        _mediaPlayer.Stopped += (s, e) => OnPlaybackStateChanged(false, false, true);
        _mediaPlayer.EndReached += (s, e) => PlaybackEnded?.Invoke(this, EventArgs.Empty);
        
        var timer = new System.Timers.Timer(500);
        timer.Elapsed += (s, e) =>
        {
            if (IsPlaying)
            {
                PositionChanged?.Invoke(this, Position);
            }
        };
        timer.Start();
    }

    private void OnPlaybackStateChanged(bool playing, bool paused, bool stopped)
    {
        PlaybackStateChanged?.Invoke(this, new PlaybackStateEventArgs
        {
            IsPlaying = playing,
            IsPaused = paused,
            IsStopped = stopped
        });
    }

    public void Play(string url)
    {
        if (_mediaPlayer == null || _libVLC == null) return;

        using var media = new Media(_libVLC, new Uri(url));
        _mediaPlayer.Play(media);
        _mediaPlayer.Volume = (int)_volume;
    }

    public void Pause()
    {
        _mediaPlayer?.Pause();
    }

    public void Stop()
    {
        _mediaPlayer?.Stop();
    }

    public void Seek(TimeSpan position)
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Time = (long)position.TotalMilliseconds;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _mediaPlayer?.Stop();
        _mediaPlayer?.Dispose();
        _libVLC?.Dispose();
        
        _isDisposed = true;
    }
}