using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RadioManager : MonoBehaviour {
  public AudioSource radio_as_;

  public AudioClip radio_on_sfx_;
  public List<AudioClip> song_list_;

  public bool radio_started_;

  void Awake() {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  void Update() {
    if (radio_started_ && !radio_as_.isPlaying) {
      StartCoroutine(StartSong());
    }
  }

  public void StartRadio() {
    if (!radio_started_) {
      StartCoroutine(DelayRadio());
    }
  }

  void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    radio_started_ = false;
    radio_as_.Stop();
  }

  AudioClip NextSong() {
    int index = Random.Range(0, song_list_.Count);
    return song_list_[index];
  }

  IEnumerator DelayRadio() {
    yield return new WaitForSeconds(1.0f);
    radio_as_.clip = radio_on_sfx_;
    radio_as_.Play();

    StartCoroutine(StartSong());
  }

  IEnumerator StartSong() {
    yield return new WaitForSeconds(2.5f);
    radio_started_ = true;

    radio_as_.clip = NextSong();
    radio_as_.volume = 0.1f;
    radio_as_.Play();    
  }
}
