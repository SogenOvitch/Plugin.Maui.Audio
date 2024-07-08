namespace Plugin.Maui.Audio;

public partial class AudioPlayer : IAudioPlayer
{
#pragma warning disable CS0067

	public event EventHandler? PlaybackEnded;

	public event EventHandler? VolumeChanged;

	public event EventHandler? TimeChanged;

#pragma warning restore CS0067
	~AudioPlayer()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}
}
