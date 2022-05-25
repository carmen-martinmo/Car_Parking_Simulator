using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalController : MonoBehaviour {

  CarController car_;
  public Collider car_collider_;
  float percentage_bounds_ = 0f;

  int text_score_ = 0;
  int text_fuel_ = 0;
  int text_collisions_ = 0;
  float text_time_ = 0.0f; 

  public int next_level_;

  bool loading_scene_ = false;

  bool calculate_score_ = false;

  public GameObject canvas_;
  GameObject canvas_instance_;

  void Start() {
    car_ = GameObject.Find("Car").GetComponent<CarController>();
    car_.OnCarOff += CheckWinCondition;

    canvas_instance_ = Instantiate(canvas_);
    canvas_instance_.SetActive(false);

    canvas_instance_.GetComponent<EndLevelCanvas>().SetScoreText(0);
    canvas_instance_.GetComponent<EndLevelCanvas>().SetFuelText(0);
    canvas_instance_.GetComponent<EndLevelCanvas>().SetCollisionsText(0);
    canvas_instance_.GetComponent<EndLevelCanvas>().SetTimeText(0.0f); 
  }

  void Update() {
    percentage_bounds_ = CheckGoalBounds(car_collider_.bounds, GetComponent<Collider>().bounds);

    if (calculate_score_) {
      calculate_score_ = false;
      StartCoroutine(FillScorePercentage((int)(percentage_bounds_ * 100.0f)));
      StartCoroutine(NextLevelCoroutine());
    }

  }

  float CheckGoalBounds(Bounds obj, Bounds region) {
    var total = 1f;

    for ( var i = 0; i < 3; i++ ) {
      var dist = obj.min[i] > region.center[i] ?
          obj.max[i] - region.max[i] :
          region.min[i] - obj.min[i];

      total *= Mathf.Clamp01(1f - dist / obj.size[i]);
    }

    return total;
  }

  public void CheckWinCondition() {
    if (percentage_bounds_ > 0.0f || car_.GetComponent<CarController>().car_movement().car_fuel() <= 0.0f) {
      if (!loading_scene_) {
        loading_scene_ = true;

        canvas_instance_.SetActive(true);
         
        StartCoroutine(FillFuelPercentage((int)(car_.GetComponent<CarController>().car_movement().car_fuel())));
        StartCoroutine(FillCollisionsNumber(car_.number_of_collisions_));
        StartCoroutine(FillTimeNumber(car_.car_movement().parking_timer_));
      }
    }
  }

  void CalculateScore(){
    
  }

  IEnumerator NextLevelCoroutine() {
    yield return new WaitForSeconds(6.0f);
    NextLevel();
  }

  IEnumerator FillScorePercentage(float limit) {
    for (int i = 0; i <= limit; i++) {
      text_score_ = i;
      canvas_instance_.GetComponent<EndLevelCanvas>().SetScoreText(text_score_);
      yield return null;
    }
  }

  IEnumerator FillFuelPercentage(float limit) {
    for (int i = 0; i <= limit; i++) {
      text_fuel_ = i;
      canvas_instance_.GetComponent<EndLevelCanvas>().SetFuelText(text_fuel_);
      yield return null;
    }
  }

  IEnumerator FillCollisionsNumber(int limit) {
    for (int i = 0; i <= limit; i++) {
      text_collisions_ = i;
      canvas_instance_.GetComponent<EndLevelCanvas>().SetCollisionsText(text_collisions_); 
      yield return null;
    }
  }

  IEnumerator FillTimeNumber(float time) {
    for (float i = 0; i < time; i += 0.5f) {
      text_time_ = i;
      canvas_instance_.GetComponent<EndLevelCanvas>().SetTimeText(text_time_); 
      yield return null;      
    }
    calculate_score_ = true;
  }

  public void NextLevel() {
    string next_level = GameController.gm_instance_.scene_names_[next_level_];
    SceneManager.LoadScene(next_level);
  }

  public void ReloadLevel() {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }


}
