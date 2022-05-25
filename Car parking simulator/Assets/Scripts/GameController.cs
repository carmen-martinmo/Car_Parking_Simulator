using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
  public static GameController gm_instance_ = null;

  public RadioManager radio_manager_ref_;
  public AudioController audio_controller_ref_;

  public string[] scene_names_;

  void Awake() {
    DontDestroyOnLoad(gameObject);

    radio_manager_ref_ = GetComponent<RadioManager>();
    audio_controller_ref_ = GetComponent<AudioController>();
  }

  void Start() {
    if (gm_instance_ == null) gm_instance_ = this;

    if (SceneManager.GetActiveScene().name.Equals("MenuScene")) {
      SceneManager.LoadScene("Tutorial_level");
    }
  }
}
