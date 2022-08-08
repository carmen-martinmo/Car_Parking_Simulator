using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupAudioManager : MonoBehaviour {
  [SerializeField]
  AudioSource popup_sounds_source_;

  [SerializeField]
  AudioSource popup_music_source_;

  [SerializeField]
  AudioSource popup_text_source_;

  public void ConfigureAudioSources(PopupSkin setting) {
    popup_music_source_.clip = setting.popup_background_music_;
    popup_text_source_.clip = setting.popup_text_sound_;

    popup_music_source_.volume = setting.popup_music_volume_;
    popup_sounds_source_.volume = setting.popup_sfx_volume_;
  }

  public void StartMusic() {
    if (popup_music_source_.clip != null) popup_music_source_.Play();
  }

  public void StopMusic() {
    if (popup_music_source_.clip != null) popup_music_source_.Stop();
  }

  public void PlaySFX(AudioClip sfx_clip = null) {
    if (sfx_clip != null) {
      popup_sounds_source_.clip = sfx_clip;
      popup_sounds_source_.Play();
    }
  }

  public void PlayTextSFX(AudioClip sfx_clip = null) {
    if (sfx_clip != null) {
      popup_text_source_.clip = sfx_clip;
      popup_text_source_.Play();
    }
  }
 }
