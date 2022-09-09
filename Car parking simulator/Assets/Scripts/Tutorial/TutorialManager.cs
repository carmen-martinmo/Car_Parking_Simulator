using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {
  public CarController car_ref_;
  public PopupManager popup_manager_ref_;

  public PopupSkin tutorial_skin_;

  [Header("Tutorial phase one")]
  public PopupScriptable phase_one_popup_;

  [Header("Tutorial phase two")]
  public PopupScriptable phase_two_popup_;

  [Header("Tutorial phase three")]
  public PopupScriptable phase_three_popup_;

  [Header("Tutorial phase three")]
  public PopupScriptable phase_four_popup_;

  [Header("Tutorial phase three")]
  public PopupScriptable phase_five_popup_;

  bool tutorial_enabled_ = true;
  public int tutorial_phase_;

  void Start() {

    switch(tutorial_phase_) {
      case 0:
        car_ref_.OnCarOn += PhaseTwoTrigger; 
        PhaseOneTrigger();
        break;

      case 4:
        PhaseFourTrigger();
        break;

      case 5:
        PhaseFiveTrigger();
        break;
    }
  }

  void PhaseOneTrigger() {
    if (tutorial_phase_ == 0) StartCoroutine(TutorialPhaseOne());
  }


  void PhaseTwoTrigger() {
    if (tutorial_phase_ == 0) StartCoroutine(TutorialPhaseTwo());
    car_ref_.OnCarOn -= PhaseTwoTrigger;
  }

  public void PhaseThreeTrigger() {
    if (tutorial_phase_ == 0) StartCoroutine(TutorialPhaseThree());
  }

  public void PhaseFourTrigger() {
    StartCoroutine(TutorialPhaseFour());
  }

  public void PhaseFiveTrigger() {
    StartCoroutine(TutorialPhaseFive());
  }

  IEnumerator TutorialPhaseOne() {
    yield return new WaitForSecondsRealtime(1.0f);
    popup_manager_ref_.SetPopupSkin(tutorial_skin_);
    popup_manager_ref_.OpenPopup(phase_one_popup_);
  }

  IEnumerator TutorialPhaseTwo() {
    yield return new WaitForSecondsRealtime(1.0f);
    popup_manager_ref_.SetPopupSkin(tutorial_skin_);
    popup_manager_ref_.OpenPopup(phase_two_popup_);
  }

  IEnumerator TutorialPhaseThree() {
    yield return null;
    popup_manager_ref_.SetPopupSkin(tutorial_skin_);
    popup_manager_ref_.OpenPopup(phase_three_popup_);
  }

  IEnumerator TutorialPhaseFour() {
    yield return null;
    popup_manager_ref_.SetPopupSkin(tutorial_skin_);
    popup_manager_ref_.OpenPopup(phase_four_popup_);
  }

  IEnumerator TutorialPhaseFive() {
    yield return null;
    popup_manager_ref_.SetPopupSkin(tutorial_skin_);
    popup_manager_ref_.OpenPopup(phase_five_popup_);
  }
}
