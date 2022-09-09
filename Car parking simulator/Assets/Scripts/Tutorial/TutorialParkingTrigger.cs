using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialParkingTrigger : MonoBehaviour {
  public TutorialManager tutorial_manager_ref_;
  bool tutorial_enabled_ = true;

  void OnTriggerEnter(Collider other) {
    if (other.name.Equals("CarBody")) {
      if (tutorial_enabled_) { 
        tutorial_manager_ref_.PhaseThreeTrigger();
        tutorial_enabled_ = false;
      }
    }  
  }
}
