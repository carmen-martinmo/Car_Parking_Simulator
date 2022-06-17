using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;
using UnityEngine;
using UnityEngine.XR;

public class CarMovement : MonoBehaviour {
  GameController gm_instance_;
  CarController car_controller_;

  [Header("Car settings")]

  [Header("Acceleration")]
  [Tooltip("Used for the directional mode of the car. Must be a positive number.")]
  public float max_acceleration_;
  [Tooltip("Used for the reverse mode of the car. Must be a negative number.")]
  public float min_acceleration_;
  [Tooltip("Time needed to reach maximum acceleration with the trigger fully hold.")]
  public float max_acceleration_time_;

  [Header("Braking")]
  [Tooltip("Max brake force used with the hand brake or the brake pedal.")]
  public float max_brake_force_;
  [Tooltip("Passive brake force applied when you are not accelerating.")]
  public float max_passive_brake_force_;

  [Header("Wheels")]
  public float wheel_correction_time_;
  [Tooltip("Max turning angle of the wheels for each side.")]
  public float max_turn_angle_;
  [Tooltip("Number of turns needed from default position (0 degrees) to any side.")]
  public float max_steering_wheel_turns_;
  [Tooltip("Acceleration curve for the car.")]
  public AnimationCurve acceleration_curve_;

  [Header("Fuel settings")]
  [Tooltip("Fuel consumption percentage each frame. (This number gets divided by 100.0f before the fuel consumption operation).")]
  public float car_fuel_consumption_;
  float car_fuel_;
  float car_max_fuel_ = 100.0f;

  //Internal acceleration values
  float current_acceleration_ = 0f;
  float max_acceleration_passed_time_ = 0f;
  float max_acceleration_time_backup_;
  float last_max_acceleration_time_;
  float reverse_multiplier_ = 1.0f;

  //Internal wheel control values
  float current_wheel_angle_ = 0f;
  float wheel_correction_time_backup_;
  float last_wheel_correction_time_;
  float wheel_correction_passed_time_ = 0.0f;
  float current_turn_angle_ = 0f;
  float saved_wheel_angle_ = 0f;

  Vector2 current_wheel_vector_;
  Vector2 last_wheel_vector_;

  //Internal brake values
  float current_brake_force_ = 0f;
  bool hand_braking_ = false;

  public void init(CarController reference) {
    car_controller_ = reference;
  }

  void Start() {
    gm_instance_ = GameController.gm_instance_;
    car_fuel_ = 0;

    current_wheel_vector_ = Vector3.zero;
    last_wheel_vector_ = Vector3.zero;

    wheel_correction_time_backup_ = wheel_correction_time_;
    max_acceleration_time_backup_ = max_acceleration_time_;
  }

  void Update() {
    if (car_controller_.car_on()) {
      float acceleration_axis = 0.0f;
      float brake_axis = 0.0f;

      if (car_controller_.car_inputs().accelerate_car_axis() > 0.0f) acceleration_axis = car_controller_.car_inputs().accelerate_car_axis();
      if (car_controller_.car_inputs().brake_car_axis() < 0.0f) brake_axis = car_controller_.car_inputs().brake_car_axis();

      if (!hand_braking_) { 
        AccelerateCar(acceleration_axis);
        BrakeCar(brake_axis);
      }

      TurnWheels();
      UpdateWheels();
      CorrectWheels();
      LimitCarSpeed();

      if (!gm_instance_.audio_controller_ref_.IsPlaying("Engine_sound")) {
        gm_instance_.audio_controller_ref_.PlaySoundEffect("Engine_sound");

      } else {
        Sound s = gm_instance_.audio_controller_ref_.GetSoundEffect("Engine_sound");
        s.source.pitch = Mathf.Clamp(1.0f + acceleration_axis, 1.0f, 2.0f);
      }

    } else {
      gm_instance_.audio_controller_ref_.Stop("Engine_sound");

    }
  }

  void AccelerateCar(float acceleration_axis) {
    last_max_acceleration_time_ = max_acceleration_time_backup_ * (Mathf.Abs(acceleration_axis) + 0.01f);

    if (last_max_acceleration_time_ != max_acceleration_time_) {
      float percentage_passed = max_acceleration_passed_time_ / max_acceleration_time_;

      if (last_max_acceleration_time_ < max_acceleration_time_) max_acceleration_passed_time_ = last_max_acceleration_time_ * percentage_passed;
      max_acceleration_time_ = last_max_acceleration_time_;
    }

    if (max_acceleration_passed_time_ < max_acceleration_time_) {
      max_acceleration_passed_time_ += Time.deltaTime;
    }

    float real_acceleration = max_acceleration_;
    if (acceleration_axis < 0.0f) real_acceleration = max_acceleration_ * 0.5f;
    current_acceleration_ = real_acceleration * acceleration_curve_.Evaluate((max_acceleration_passed_time_ / max_acceleration_time_) * acceleration_axis);  
    current_acceleration_ = Mathf.Clamp(current_acceleration_ * reverse_multiplier_, min_acceleration_, max_acceleration_);

    if (car_controller_.car_inputs().accelerate_car_axis() <= 0.0f) max_acceleration_passed_time_ = 0.0f;

    if (acceleration_axis != 0.0f) {
      car_fuel_ -= ((car_fuel_consumption_ / 100.0f) * car_max_fuel_);
      car_fuel_ = Mathf.Clamp(car_fuel_, 20.0f, car_max_fuel_);
    }
  }

  void BrakeCar(float brake_axis) {
    current_brake_force_ = Mathf.Lerp(0.0f, max_brake_force_, Mathf.Abs(brake_axis));
    if (car_controller_.car_inputs().accelerate_car_axis() == 0.0f) current_brake_force_ += max_passive_brake_force_;
  }

  void TurnWheels() {
    current_wheel_vector_.x = car_controller_.car_inputs().left_joystick_horizontal();
    current_wheel_vector_.y = car_controller_.car_inputs().left_joystick_vertical();

    if (last_wheel_vector_ != current_wheel_vector_) {

      float angle = Vector2.SignedAngle(last_wheel_vector_, current_wheel_vector_);
      float variation = (angle * (max_turn_angle_ / (max_steering_wheel_turns_ * 360.0f)));

      if (current_wheel_angle_ + variation > -max_turn_angle_ && current_wheel_angle_ + variation < max_turn_angle_) {
        Quaternion steering = car_controller_.steering_wheel_tr_.rotation;
        steering *= Quaternion.Euler(0, angle, 0);
        car_controller_.steering_wheel_tr_.rotation = steering; 

        current_wheel_angle_ += variation; 
      }
      
      last_wheel_vector_ = current_wheel_vector_;
    }
  }

  //Wheel auto correction while the player is driving, to keep the wheels pointing forward
  void CorrectWheels() {
    if (current_acceleration_ != 0.0f) {
      if (current_wheel_vector_.magnitude == 0.0f) {
          
          last_wheel_correction_time_ = (max_acceleration_ * wheel_correction_time_backup_) / Mathf.Abs(current_acceleration_);

          //Updating the correction passed time percentage to it's new value
          //(this is needed because if the target time changes, the passed time no longer corresponds to the same percentage,
          //so we need to adapt the passed time to keep the same percentage of passed time.)
          if (last_wheel_correction_time_ != wheel_correction_time_) {
            float percentage_passed = wheel_correction_passed_time_ / wheel_correction_time_;

            wheel_correction_time_ = last_wheel_correction_time_;
            wheel_correction_passed_time_ = wheel_correction_time_ * percentage_passed;
          }

          if (wheel_correction_passed_time_ < wheel_correction_time_) {
            wheel_correction_passed_time_ += Time.deltaTime;

            current_wheel_angle_ = Mathf.Lerp(saved_wheel_angle_, 0.0f, wheel_correction_passed_time_ / wheel_correction_time_);

            Quaternion steering = car_controller_.steering_wheel_tr_.rotation;
            steering *= Quaternion.Euler(0, current_wheel_angle_ * (car_controller_.car_inputs().accelerate_car_axis() * 0.2f), 0);
            car_controller_.steering_wheel_tr_.rotation = steering;
          }

      } else {
        wheel_correction_passed_time_ = 0.0f;
        saved_wheel_angle_ = current_wheel_angle_;
      }
    } else {
      wheel_correction_passed_time_ = 0.0f;
      saved_wheel_angle_ = current_wheel_angle_;
    }

  }

  void UpdateWheels() {
    car_controller_.back_right().motorTorque = current_acceleration_;
    car_controller_.back_left().motorTorque = current_acceleration_;

    car_controller_.front_right().brakeTorque = current_brake_force_;
    car_controller_.front_left().brakeTorque = current_brake_force_;
    
    car_controller_.back_right().brakeTorque = current_brake_force_;
    car_controller_.back_left().brakeTorque = current_brake_force_;

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

  void LimitCarSpeed() {
    if (car_controller_.car_rb().velocity.magnitude > (max_acceleration_ / 10.0f)) {
      car_controller_.car_rb().velocity = Vector3.ClampMagnitude(car_controller_.car_rb().velocity, max_acceleration_ / 10.0f);
    }
    else if (car_controller_.car_rb().velocity.magnitude < -(max_acceleration_ / 10.0f)) { 
      car_controller_.car_rb().velocity = Vector3.ClampMagnitude(car_controller_.car_rb().velocity, max_acceleration_ / 10.0f);
    }
  }

  #region getters
  //-------------- GETTERS --------------//
  public float current_acceleration() {
    return current_acceleration_;
  }

  public float car_fuel() {
    return car_fuel_;
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

  public float current_brake_force() {
    return current_brake_force_;
  }
  //-------------------------------------//
  #endregion

  #region setters
  //-------------- SETTERS --------------//
  public void set_car_fuel(float fuel) {
    car_fuel_ = fuel;
  }

  public void set_reverse_multiplier(float value) {
    reverse_multiplier_ = value;
  }

  public void set_hand_braking(bool value) {
    hand_braking_ = value;
  }

  public void set_current_brake_force(float value) {
    current_brake_force_ = value;
  }
  //-------------------------------------//
  #endregion
}
