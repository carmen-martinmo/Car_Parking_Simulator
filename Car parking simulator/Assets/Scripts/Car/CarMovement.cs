using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;
using UnityEngine;

public class CarMovement : MonoBehaviour {
  GameController gm_instance_;
  CarController car_controller_;

  [Header("Car settings")]
  public float max_acceleration_;
  public float max_speed_time_;
  public float max_turn_angle_;
  public float max_wheel_turns_;
  public float breaking_force_;

  [Header("Fuel settings")]
  public float car_fuel_consumption_;
  float car_fuel_;
  float car_max_fuel_ = 100.0f;
  float saved_fuel_ = 100.0f;

  float reverse_multiplier_ = 1.0f;

  float current_acceleration_ = 0f;
  float current_wheel_angle_ = 0f;
  float current_turn_angle_ = 0f;
  float current_break_force_ = 0f;

  float saved_wheel_angle_ = 0f;

  float max_speed_passed_time_ = 0f;

  float wheel_correction_time_ = 1.0f;
  float wheel_correction_passed_time_ = 0.0f;

  bool hand_braking_ = false;

  public float parking_timer_ = 0.0f;

  Vector2 current_wheel_vector_;
  Vector2 last_wheel_vector_;

  public void init(CarController reference) {
    car_controller_ = reference;
  }

  void Start() {
    gm_instance_ = GameController.gm_instance_;
    car_fuel_ = 0;

    current_wheel_vector_ = Vector3.zero;
    last_wheel_vector_ = Vector3.zero;
  }

  void Update() {
    if (car_controller_.car_on()) {
      parking_timer_ += Time.deltaTime;
      float ver_axis = 0.0f;
      if (car_controller_.car_inputs().left_joystick_vertical() > 0.0f) ver_axis = car_controller_.car_inputs().left_joystick_vertical();
      ver_axis *= reverse_multiplier_;

      if (!hand_braking_)
        AccelerateCar(ver_axis);

      TurnWheels();
      UpdateWheels();

      ChangeDirection();
      HandBrake();

      if (current_acceleration_ > 0.0f) {
        if (current_wheel_vector_.magnitude == 0.0f) {
          if (current_wheel_angle_ != 0.0f) {
            CorrectWheels();

          } else {
            wheel_correction_passed_time_ = 0.0f;
          }
        } else {
          wheel_correction_passed_time_ = 0.0f;
          saved_wheel_angle_ = current_wheel_angle_;
        }
      }

      if (!gm_instance_.audio_controller_ref_.IsPlaying("Engine_sound")) {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Engine_sound");

      } else {
        Sound s = gm_instance_.audio_controller_ref_.GetSoundEffect("Engine_sound");
        s.source.pitch = Mathf.Clamp(1.0f + ver_axis, 1.0f, 2.0f);
      }

      CapCarSpeed();

    } else {
      gm_instance_.audio_controller_ref_.Stop("Engine_sound");

    }
  }

  void AccelerateCar(float ver_axis) {
    if (max_speed_passed_time_ < max_speed_time_) {
      max_speed_passed_time_ += Time.deltaTime;
    }

    float real_acceleration = max_acceleration_;
    if (ver_axis < 0.0f) real_acceleration = max_acceleration_ * 0.5f;
    current_acceleration_ = real_acceleration * ver_axis * (max_speed_passed_time_ / max_speed_time_);      
    if (car_controller_.car_inputs().left_joystick_vertical() <= 0.0f) max_speed_passed_time_ = 0.0f;

    if (ver_axis != 0.0f) {
      car_fuel_ -= ((car_fuel_consumption_ / 100.0f) * car_max_fuel_);
      car_fuel_ = Mathf.Clamp(car_fuel_, 20.0f, car_max_fuel_);
    }

    if (car_controller_.car_on()) {
      GamePad.SetVibration(0, ver_axis * 0.5f, ver_axis * 0.5f);
    }
  }

  void ChangeDirection() {
    if (car_controller_.car_inputs().right_joystick_press()) {
      reverse_multiplier_ *= -1.0f;

      if (reverse_multiplier_ < 0.0f) {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Reverse_mode");
      } else {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Directional_mode");        
      }
    }
  }

  void TurnWheels() {
    current_wheel_vector_.x = car_controller_.car_inputs().right_joystick_horizontal();
    current_wheel_vector_.y = car_controller_.car_inputs().right_joystick_vertical();

    if (last_wheel_vector_ != current_wheel_vector_) {

      float angle = Vector2.SignedAngle(last_wheel_vector_, current_wheel_vector_);
      float variation = (angle * (max_turn_angle_ / (max_wheel_turns_ * 360.0f)));

      if (current_wheel_angle_ + variation > -max_turn_angle_ && current_wheel_angle_ + variation < max_turn_angle_) {
        Quaternion steering = car_controller_.steering_wheel_tr_.rotation;
        steering *= Quaternion.Euler(0, angle, 0);
        car_controller_.steering_wheel_tr_.rotation = steering; 

        current_wheel_angle_ += variation; 
      }
      
      last_wheel_vector_ = current_wheel_vector_;
    }
  }

  void CorrectWheels() {
    if (wheel_correction_passed_time_ < wheel_correction_time_) {
      wheel_correction_passed_time_ += Time.deltaTime;

      current_wheel_angle_ = Mathf.Lerp(saved_wheel_angle_, 0.0f, wheel_correction_passed_time_ / wheel_correction_time_);

      Quaternion steering = car_controller_.steering_wheel_tr_.rotation;
      steering *= Quaternion.Euler(0, current_wheel_angle_, 0);
      car_controller_.steering_wheel_tr_.rotation = steering;
    }
  }

  void HandBrake() {
    if (car_controller_.car_inputs().left_joystick_press()) {
      hand_braking_ = !hand_braking_;

      if (hand_braking_) {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Handbrake_on");
      } else {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Handbrake_off");
      }
    }

    if (hand_braking_) {
      current_break_force_ = 100000f;
    }
  }

  void UpdateWheels() {
    float ver_axis_raw = Input.GetAxisRaw("JoystickVer");

    car_controller_.back_right().motorTorque = current_acceleration_;
    car_controller_.back_left().motorTorque = current_acceleration_;

    if (!hand_braking_) {
      car_controller_.front_right().brakeTorque = current_break_force_;
      car_controller_.front_left().brakeTorque = current_break_force_;
    }
    
    car_controller_.back_right().brakeTorque = current_break_force_;
    car_controller_.back_left().brakeTorque = current_break_force_;

    if (ver_axis_raw <= 0.0f) current_break_force_ += 300f;
    else current_break_force_ = 0.0f;
    current_break_force_ = Mathf.Clamp(current_break_force_, 0.0f, breaking_force_);

    car_controller_.front_left().steerAngle = current_turn_angle_;
    car_controller_.front_right().steerAngle = current_turn_angle_;

    current_turn_angle_ = current_wheel_angle_;
    UpdateWheel(car_controller_.front_right(), car_controller_.front_right_tr());
    UpdateWheel(car_controller_.front_left(), car_controller_.front_left_tr());
    UpdateWheel(car_controller_.back_right(), car_controller_.back_right_tr());
    UpdateWheel(car_controller_.back_left(), car_controller_.back_left_tr()); 
  }

  void UpdateWheel(WheelCollider collider, Transform tr) {
    Vector3 position;
    Quaternion rotation;

    collider.GetWorldPose(out position, out rotation);
    tr.position = position;
    tr.rotation = rotation;
  }

  void CapCarSpeed() {
    if (car_controller_.car_rb().velocity.magnitude > (max_acceleration_ / 10.0f)) {
      car_controller_.car_rb().velocity = Vector3.ClampMagnitude(car_controller_.car_rb().velocity, max_acceleration_ / 10.0f);
    }
    else if (car_controller_.car_rb().velocity.magnitude < -(max_acceleration_ / 10.0f)) { 
      car_controller_.car_rb().velocity = Vector3.ClampMagnitude(car_controller_.car_rb().velocity, max_acceleration_ / 10.0f);
    }
  }

  public float current_acceleration() {
    return current_acceleration_;
  }

  public float car_fuel() {
    return car_fuel_;
  }

  public void set_car_fuel(float fuel) {
    car_fuel_ = fuel;
  }

  public float car_max_fuel() {
    return car_max_fuel_;
  }

  public float reverse_multiplier() {
    return reverse_multiplier_;
  }

  public bool hand_braking() {
    return hand_braking_;
  }
}
