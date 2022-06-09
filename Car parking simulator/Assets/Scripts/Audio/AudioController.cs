
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
public class AudioController : MonoBehaviour {
	[Header("Audio Settings")]
	bool is_music_on_;
  bool are_sound_effects_on_;

  [Header("Sounds")]
  public List<Sound> sound_effects_;
  public List<Sound> music_songs_;

  [Header("Mixers")]
  public AudioMixerGroup music_audio_mixer_group_;
  public AudioMixerGroup effects_audio_mixer_group_;
  public AudioMixer audio_mixer_;


	void Awake() {
		foreach (Sound s in sound_effects_) {
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.outputAudioMixerGroup = effects_audio_mixer_group_;
			s.source.clip = s.clip;
			s.source.loop = s.loop;
		}

		foreach (Sound s in music_songs_) {
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.outputAudioMixerGroup = music_audio_mixer_group_;
			s.source.clip = s.clip;
			s.source.loop = s.loop;
		}
	}

	void Start() {
		is_music_on_ = true;
  	are_sound_effects_on_ = true;		
	}

	public void PlaySong(string sound) {
		Sound s = GetSong(sound);
		if (s == null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		if (is_music_on_) {
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.Play();
		}
	}

	public void PlaySoundEffect(string sound) {
		Sound s = GetSoundEffect(sound);
		if (s == null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		if (are_sound_effects_on_) {
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.Play();
		}
	}

	public void Stop(string sound) {
		Sound s = GetSound(sound);//Array.Find(sounds, item => item.name == sound);
		if (s == null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.Stop();
	}

	public bool IsPlaying(string sound) {
		Sound s = GetSound(sound);
		if (s == null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return false;
		}

		return s.source.isPlaying;
	}

	public Sound GetSoundEffect(string sound) {
		return sound_effects_.Find(item => item.name == sound);
	}

	public Sound GetSong(string sound) {
		return music_songs_.Find(item => item.name == sound);
	}

	public Sound GetSound(string sound) {
		Sound s = GetSoundEffect(sound);
		if (s == null) {
			s = GetSong(sound);
		}
		return s;
	}

	public void SetMusicOn(bool activate) {
    is_music_on_ = activate;
		float volume_value = activate ? 0.0f : -80.0f;
		music_audio_mixer_group_.audioMixer.SetFloat("MusicVolume", volume_value);

		if (activate) {
			if (!IsPlaying("Music")) {
				PlaySong("Music");
      }
    }
  }

  public void SetSoundEffectsOn(bool activate) {
    are_sound_effects_on_ = activate;
		float volume_value = activate ? 0.0f : -80.0f;
		effects_audio_mixer_group_.audioMixer.SetFloat("EffectsVolume", volume_value);
	}

  public bool IsMusicOn() {
    return is_music_on_;
  }

  public bool AreSoundEffectsOn() {
    return are_sound_effects_on_;
  }
}
