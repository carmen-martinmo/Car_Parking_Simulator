using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndLevelCanvas : MonoBehaviour {

  public string score_fill_text_;
  public string fuel_fill_text_;
  public string collisions_fill_text_;
  public string time_fill_text_;

  public TextMeshProUGUI score_text_;
  public TextMeshProUGUI fuel_text_;
  public TextMeshProUGUI collisions_text_;
  public TextMeshProUGUI time_text_; 

  public void SetScoreText(int score) {
    score_text_.text = score_fill_text_ + "\n" + score.ToString() + "%";
  }

  public void SetFuelText(int fuel) {
    fuel_text_.text = fuel_fill_text_ + ": " + fuel.ToString() + "%";   
  }

  public void SetCollisionsText(int col) {
    collisions_text_.text = collisions_fill_text_ + ": " + col.ToString();
  }

  public void SetTimeText(float time) {
    time_text_.text = time_fill_text_ + ": " + time.ToString("F1") + "s";   
  }
}
