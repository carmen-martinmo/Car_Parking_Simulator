using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputs : MonoBehaviour {
  bool right_joystick_press_;
  bool left_joystick_press_;

  float left_joystick_vertical_;
  float left_joystick_horizontal_;

  float right_joystick_vertical_;
  float right_joystick_horizontal_;


  void Update() {
    right_joystick_press_ = Input.GetButtonDown("Reverse");
    left_joystick_press_ = Input.GetButtonDown("CameraMode");

    left_joystick_vertical_ = Input.GetAxis("JoystickVer");
    left_joystick_horizontal_ = Input.GetAxisRaw("JoystickHead");

    right_joystick_vertical_ = Input.GetAxis("JoystickSpinVer");
    right_joystick_horizontal_ = Input.GetAxisRaw("JoystickSpinHor");
  }

  public bool right_joystick_press() {
    return right_joystick_press_;
  }

  public bool left_joystick_press() {
    return left_joystick_press_;
  }

  public float left_joystick_vertical() {
    return left_joystick_vertical_;
  }

  public float left_joystick_horizontal() {
    return left_joystick_horizontal_;
  }

  public float right_joystick_vertical() {
    return right_joystick_vertical_;
  }

  public float right_joystick_horizontal() {
    return right_joystick_horizontal_;
  }
}