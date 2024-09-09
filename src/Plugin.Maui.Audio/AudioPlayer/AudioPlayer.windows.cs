using Windows.Media.Core;
using Windows.Media.Playback;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	bool isDisposed = false;
	MediaPlayer? player;

	public double CurrentPosition => GetPlaybackSession()?.Position.TotalSeconds ?? 0;

	public double Duration => GetPlaybackSession()?.NaturalDuration.TotalSeconds ?? 0;

	public double Volume
	{
		get => player?.Volume ?? 0;
		set => SetVolume(value, Balance);
	}

	public double Balance
	{
		get => player?.AudioBalance ?? 0;
		set => SetVolume(Volume, value);
	}

	public double Speed => GetPlaybackSession()?.PlaybackRate ?? 0;

	public void SetSpeed(double speed)
	{
		var playbackSession = GetPlaybackSession();

		if (playbackSession != null)
		{
			playbackSession.PlaybackRate = Math.Clamp(speed, MinimumSpeed, MaximumSpeed);
		}
	}

	MediaPlaybackSession? GetPlaybackSession()
	{
		try
		{
			return player?.PlaybackSession;
		}
		catch
		{
			return null;
		}
	}

	public double MinimumSpeed => 0;

	public double MaximumSpeed => 8;

	public bool CanSetSpeed => true;

	public bool IsPlaying => GetPlaybackSession()?.PlaybackState == MediaPlaybackState.Playing; //might need to expand

	public bool Loop
	{
		get => player?.IsLoopingEnabled ?? false;
		set { if (player != null) { player.IsLoopingEnabled = value; } }
	}

	public bool CanSeek => GetPlaybackSession()?.CanSeek ?? false;

	public AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions)
	{
		player = CreatePlayer();

		if (player is null)
		{
			throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
		}
		player.CommandManager.IsEnabled = false;

		player.Source = MediaSource.CreateFromStream(audioStream?.AsRandomAccessStream(), string.Empty);
		player.MediaEnded += OnPlaybackEnded;
		SetSpeed(1.0);
	}

	public AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions)
	{
		player = CreatePlayer();

		if (player is null)
		{
			throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
		}
		player.CommandManager.IsEnabled = false;

		player.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/" + fileName));
		player.MediaEnded += OnPlaybackEnded;
		SetSpeed(1.0);
	}

	void OnPlaybackEnded(MediaPlayer sender, object args)
	{
		PlaybackEnded?.Invoke(sender, EventArgs.Empty);
	}

	public void Play()
	{
		var playbackSession = GetPlaybackSession();

		if (player?.Source is null || playbackSession is null)
		{
			return;
		}

		if (playbackSession.PlaybackState == MediaPlaybackState.Playing)
		{
			Pause();
			Seek(0);
		}

		player.Play();
	}

	public void Pause()
	{
		player?.Pause();
	}

	public void Stop()
	{
		Pause();
		Seek(0);
		PlaybackEnded?.Invoke(this, EventArgs.Empty);
	}

	public void Seek(double position)
	{
		var playbackSession = GetPlaybackSession();

		if (playbackSession is null)
		{
			return;
		}

		if (playbackSession.CanSeek)
		{
			playbackSession.Position = TimeSpan.FromSeconds(position);
		}
	}

	void SetVolume(double volume, double balance)
	{
		if (isDisposed || player == null)
		{
			return;
		}

		player.Volume = Math.Clamp(volume, 0, 1);
		player.AudioBalance = Math.Clamp(balance, -1, 1);
	}

	MediaPlayer CreatePlayer()
	{
		return new MediaPlayer() { AutoPlay = false, IsLoopingEnabled = false };
	}

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}

		if (disposing && player != null)
		{
			Pause();

			player.MediaEnded -= OnPlaybackEnded;
			player.Dispose();
			player = null;
		}

		isDisposed = true;
	}
}
