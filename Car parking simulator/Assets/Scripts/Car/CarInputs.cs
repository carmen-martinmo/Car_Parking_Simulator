using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class CarInputs : MonoBehaviour {
  GameController gm_instance_;
  CarController car_controller_;

  bool right_joystick_press_;
  bool left_joystick_press_;

  float left_joystick_vertical_;
  float left_joystick_horizontal_;

  float right_joystick_vertical_;
  float right_joystick_horizontal_;

  float accelerate_car_axis_;
  float brake_car_axis_;

  float accelerate_car_raw_axis_;

  bool car_lights_button_;
  bool reset_button_;

  public void init(CarController reference) {
    car_controller_ = reference;
  }

  private void Start() {
    gm_instance_ = GameController.gm_instance_;
  }

  void Update() {
    float reverse_multiplier = car_controller_.car_movement().reverse_multiplier();
    ChangeDirection(reverse_multiplier);

    bool hand_braking = car_controller_.car_movement().hand_braking();
    HandBrake(hand_braking);

    bool car_lights_state = car_controller_.car_lights_on();
    TurnCarLights(car_lights_state);
  }

  public void TurnCarLights(bool car_lights_state) {
    car_controller_.set_car_lights(car_lights_button_);
  }

  public void ChangeDirection(float reverse_multiplier) {
    if (right_joystick_press_) {
      reverse_multiplier *= -1.0f;
      car_controller_.car_movement().set_reverse_multiplier(reverse_multiplier);

      if (reverse_multiplier < 0.0f) {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Reverse_mode");
        car_controller_.direction_layout_.material = car_controller_.direction_layout_mats_[1];
      }
      else {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Directional_mode");
        car_controller_.direction_layout_.material = car_controller_.direction_layout_mats_[0];
      }

      right_joystick_press_ = false;
    }
  }

  public void HandBrake(bool hand_braking) {
    if (left_joystick_press_) {
      hand_braking = !hand_braking;
      car_controller_.car_movement().set_hand_braking(hand_braking);

      if (hand_braking) {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Handbrake_on");
      }
      else {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Handbrake_off");
      }

      left_joystick_press_ = false;
    }

    if (hand_braking) {
      car_controller_.car_movement().set_current_brake_force(100000f);
    }
  }

  // GET INPUTS
  public void RightJoystickPress(InputAction.CallbackContext context) {
    right_joystick_press_ = !right_joystick_press_;
  }

  public void LeftJoystickPress(InputAction.CallbackContext context) {
    left_joystick_press_ = !left_joystick_press_;
  }
  public void LeftJoystickVertical(InputAction.CallbackContext context) {
    left_joystick_vertical_ = context.ReadValue<float>();
  }

  public void LeftJoystickHorizontal(InputAction.CallbackContext context) {
    left_joystick_horizontal_ = context.ReadValue<float>();
  }
  public void RightJoystickVertical(InputAction.CallbackContext context) {
    right_joystick_vertical_ = context.ReadValue<float>();
  }

  public void RightJoystickHorizontal(InputAction.CallbackContext context) {
    right_joystick_horizontal_ = context.ReadValue<float>();
  }

  public void RightTrigger(InputAction.CallbackContext context) {
    accelerate_car_axis_ = context.ReadValue<float>();

    if (accelerate_car_axis_ > 0.0f) accelerate_car_raw_axis_ = 1.0f;
    else accelerate_car_raw_axis_ = 0.0f;
  }

  public void LeftTrigger(InputAction.CallbackContext context) {
    brake_car_axis_ = context.ReadValue<float>();
  }

  public void AButtonInput() {
    car_lights_button_ = !car_lights_button_;
  }

  public void BButtonInput(InputAction.CallbackContext context) {
    if (context.canceled) reset_button_ = false;
    else reset_button_ = true;
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

  public float accelerate_car_axis() {
    return accelerate_car_axis_;
  }

  public float brake_car_axis() {
    return brake_car_axis_;
  }

  public float accelerate_car_raw_axis () {
    return accelerate_car_raw_axis_;
  }

  public bool reset_button() {
    return reset_button_;
  }
}