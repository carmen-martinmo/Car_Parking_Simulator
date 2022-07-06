using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
  public static GameController gm_instance_ = null;

  public RadioManager radio_manager_ref_;
  public AudioController audio_controller_ref_;

  public CarController car_reference_;

  public string[] scene_names_;

  public float reset_time_;
  float reset_timer_;

  void Awake() {
    DontDestroyOnLoad(gameObject);

    radio_manager_ref_ = GetComponent<RadioManager>();
    audio_controller_ref_ = GetComponent<AudioController>();
  }

  private void Update() {
    if (car_reference_ != null) ResetLevel();
  }

  void Start() {
    if (gm_instance_ == null) gm_instance_ = this;
    reset_timer_ = 0.0f;

    StartGame();
  }

  public void StartGame() {
    if (SceneManager.GetActiveScene().name.Equals("MenuScene")) {
      SceneManager.LoadScene("Tutorial_level_1");
    }
  }

  void ResetLevel() {
    if (car_reference_.car_inputs().reset_button()) {
      if (reset_timer_ < reset_time_) reset_timer_ += Time.deltaTime;
      else SceneManager.LoadScene(SceneManager.GetActiveScene().name);

      float percentage = reset_timer_ / reset_time_;
      Transform reset_bar = car_reference_.car_canvas_.transform.Find("ResetBar");
      reset_bar.GetComponent<Slider>().value = percentage;

    } else {
      reset_timer_ = 0.0f;
      Transform reset_bar = car_reference_.car_canvas_.transform.Find("ResetBar"); //Optimizar esta llamada
      reset_bar.GetComponent<Slider>().value = 0.0f;

    }
  }
}
