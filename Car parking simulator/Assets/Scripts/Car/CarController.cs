using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using EZCameraShake;

public class CarController : MonoBehaviour {
  GameController gm_instance_;

  // Internal refs
  CarInputs car_inputs_;
  CarMovement car_movement_;
  CarCollision car_collisions_;
  public GameObject car_canvas_;

  [Header("Wheel colliders")]
  public Transform wheel_colliders_parent_;
  WheelCollider front_right_;
  WheelCollider front_left_;
  WheelCollider back_right_;
  WheelCollider back_left_;

  [Header("Wheel transform")]
  public Transform wheel_transform_parent_;
  Transform front_right_tr_;
  Transform front_left_tr_;
  Transform back_right_tr_;
  Transform back_left_tr_;

  [Header("Steering wheel transform")]
  public Transform steering_wheel_tr_;

  [Header("Car settings")]
  [Range(-1.0f, 1.0f)]
  public float car_mass_center_;
  public float engine_start_time_;
  public bool car_lights_on_;
  public float parking_timer_ = 0.0f;

  // Internal variables  
  bool can_start_engine_ = true;
  bool car_on_ = false;
  int number_of_collisions_ = 0;

  [Header("Head rotation settings")]
  [Range(-90, 90)]
  public float head_left_angle_;
  [Range(-90, 90)]
  public float head_right_angle_;
  [Tooltip("Head rotation duration in seconds.")]
  public float rotating_head_time_;

  // Head rotation internal variables
  float current_head_angle_ = 0f;
  float last_head_angle_ = 0f;
  float rotating_head_passed_time_ = 0f;

  Quaternion camera_default_rotation_;
  Vector2 rotating_head_axis_;

  // Timers
  float engine_start_passed_time_ = 0.0f;

  [Header("Car panel gameobjects")]
  public GameObject engine_speed_meter_arrow_;
  public GameObject car_speed_meter_arrow_;
  public GameObject fuel_arrow_;
  public GameObject hand_brake_light_;
  public Renderer direction_layout_;
  
  [Header("Car panel settings")]
  public Material[] direction_layout_mats_;

  [Header("Audio sources")]
  public AudioSource ignition_as_;
  public AudioSource started_as_;
  public AudioSource engine_as_;
  public AudioSource engine_off_as_;

  Rigidbody car_rb_;
  public Camera main_camera_;
  
  public delegate void CarOff();
  public event CarOff OnCarOff;

  void Awake() {
    car_inputs_ = GetComponent<CarInputs>();
    car_inputs_.init(this);

    car_movement_ = GetComponent<CarMovement>();
    car_movement_.init(this);

    car_collisions_ = transform.GetChild(0).GetComponent<CarCollision>();
    car_collisions_.init(this);

    car_rb_ = transform.GetChild(0).GetComponent<Rigidbody>();

    front_right_ = wheel_colliders_parent_.GetChild(0).GetComponent<WheelCollider>();
    front_left_ = wheel_colliders_parent_.GetChild(1).GetComponent<WheelCollider>();
    back_right_ = wheel_colliders_parent_.GetChild(2).GetComponent<WheelCollider>();
    back_left_ = wheel_colliders_parent_.GetChild(3).GetComponent<WheelCollider>();  

    front_right_tr_ = wheel_transform_parent_.GetChild(0).transform;
    front_left_tr_ = wheel_transform_parent_.GetChild(1).transform;
    back_right_tr_ = wheel_transform_parent_.GetChild(2).transform;
    back_left_tr_ = wheel_transform_parent_.GetChild(3).transform; 

    camera_default_rotation_ = main_camera_.transform.rotation;

    rotating_head_axis_ = new Vector2();
  }

  void Start() {
    gm_instance_ = GameController.gm_instance_;
    gm_instance_.car_reference_ = this;

    car_rb_.centerOfMass = new Vector3(0, car_mass_center_, 0);

    GetComponent<on_off_light>().TurnLights(car_lights_on_);
    direction_layout_.material = direction_layout_mats_[0];
  }

  void Update() {
    parking_timer_ += Time.deltaTime;

    StartCarEngine();
    RotateHead();
    UpdateSpeedMeterArrow();
  }

  void RotateHead() {
    rotating_head_axis_.x = car_inputs_.right_joystick_horizontal();
    rotating_head_axis_.y = car_inputs_.right_joystick_vertical();   
    float hor_axis = rotating_head_axis_.x;

    float dot_result = Mathf.Abs(Vector2.Dot(-rotating_head_axis_.normalized, Vector2.right));
    if (dot_result > 0.9f) {
      if (hor_axis < 0.0f) {
        if (rotating_head_passed_time_ < rotating_head_time_) {
          rotating_head_passed_time_ += Time.deltaTime;
          current_head_angle_ = Mathf.Lerp(0.0f, head_right_angle_, rotating_head_passed_time_ / rotating_head_time_);         
        }
      } else if (hor_axis > 0.0f) {
        if (rotating_head_passed_time_ < rotating_head_time_) {
          rotating_head_passed_time_ += Time.deltaTime;
          current_head_angle_ = Mathf.Lerp(0.0f, head_left_angle_, rotating_head_passed_time_ / rotating_head_time_);       
        }   
      } else {
        rotating_head_passed_time_ = 0.0f;
        last_head_angle_ = current_head_angle_;
        current_head_angle_ = 0.0f;
      }

    } else {
      rotating_head_passed_time_ = 0.0f;
      last_head_angle_ = current_head_angle_;
      current_head_angle_ = 0.0f;
    }

    if (last_head_angle_ != 0.0f) {
      current_head_angle_ = Mathf.Lerp(last_head_angle_, 0.0f, Time.deltaTime * 100.0f);
      if (Mathf.Abs(current_head_angle_) * 0.3f > 0.0f) last_head_angle_ = 0.0f;
    }

    Transform camera_tr = main_camera_.transform;
    Quaternion head = camera_tr.rotation;
    head = Quaternion.Euler(camera_default_rotation_.eulerAngles.x, current_head_angle_, camera_default_rotation_.eulerAngles.z);
    camera_tr.localRotation = head; 
  }

  void StartCarEngine() {
    if (car_inputs().right_joystick_vertical() == -1.0f) {
      if (can_start_engine_) {
        if (engine_start_passed_time_ < engine_start_time_) {
          engine_start_passed_time_ += Time.deltaTime;
          
          GamePad.SetVibration(0, car_inputs().right_joystick_vertical() * -0.3f, car_inputs().right_joystick_vertical() * -0.3f);
          if (!car_on_) {
            if (!gm_instance_.audio_controller_ref_.IsPlaying("Car_ignition")) {
              gm_instance_.audio_controller_ref_.PlaySoundEffect("Car_ignition");

            }
          }
        } else {
          can_start_engine_ = false;
          car_on_ = !car_on_;
          engine_start_passed_time_ = 0.0f;

          if (car_on_) {
            gm_instance_.audio_controller_ref_.Stop("Car_ignition");
            gm_instance_.audio_controller_ref_.PlaySoundEffect("Car_started");

            StartCoroutine(CarOnVibration());
            StartCoroutine(FuelMeterFill());

            GameController.gm_instance_.radio_manager_ref_.StartRadio();
            CameraShaker.Instance.ShakeOnce(0.1f, 5.5f, 0.5f, 2.5f);
          } else {
            gm_instance_.audio_controller_ref_.PlaySoundEffect("Engine_off");
            CameraShaker.Instance.ShakeOnce(0.1f, 2.0f, 0.5f, 1.5f);

            StartCoroutine(CarOffVibration());
            OnCarOff();
          }
        }
      }
    } else {
      engine_start_passed_time_ = 0.0f;
      can_start_engine_ = true;
      gm_instance_.audio_controller_ref_.Stop("Car_ignition");

      if (!car_on_) {
        GamePad.SetVibration(0, 0.0f, 0.0f);
      }
    }
  }

  //MOVE TO FEEL SCRIPT
  void UpdateSpeedMeterArrow() {
    RectTransform engine_arrow_t = engine_speed_meter_arrow_.GetComponent<RectTransform>();
    RectTransform car_arrow_t = car_speed_meter_arrow_.GetComponent<RectTransform>();
    RectTransform fuel_arrow_t = fuel_arrow_.GetComponent<RectTransform>();

    float engine_rotation_value = (Mathf.Abs(car_movement_.current_acceleration()) / car_movement_.max_acceleration_) * 2.0f - 1.0f;
    float car_rotation_value = (car_rb_.velocity.magnitude / (car_movement_.max_acceleration_ / 10.0f)) * 2.0f - 1.0f;
    float fuel_rotation_value = (car_movement_.car_fuel() / car_movement_.car_max_fuel()) * 2.0f - 1.0f;

    Vector3 rotation = engine_arrow_t.transform.parent.transform.rotation.eulerAngles;

    engine_arrow_t.rotation = Quaternion.Euler(rotation.x, rotation.y, -90.0f * engine_rotation_value);
    car_arrow_t.rotation = Quaternion.Euler(rotation.x, rotation.y, -90.0f * car_rotation_value);
    fuel_arrow_t.rotation = Quaternion.Euler(rotation.x, rotation.y, -70.0f * fuel_rotation_value);

    hand_brake_light_.SetActive(car_movement_.hand_braking());
  }

  //MOVE TO FEEL SCRIPT
  IEnumerator FuelMeterFill() {
    for (int i = 0; i < car_movement_.car_max_fuel(); i++) {
      car_movement_.set_car_fuel(i);
      yield return null;
    }
  }

  //MOVE TO FEEL SCRIPT
  IEnumerator CarOnVibration() {
    car_on_ = false;
    GamePad.SetVibration(0, 0.8f, 0.8f);
    yield return new WaitForSeconds(1.0f);
    GamePad.SetVibration(0, 0.0f, 0.0f);
    car_on_ = true;
  }

  //MOVE TO FEEL SCRIPT
  IEnumerator CarOffVibration() {
    GamePad.SetVibration(0, 0.6f, 0.6f);
    yield return new WaitForSeconds(1.0f);
    GamePad.SetVibration(0, 0.0f, 0.0f);
  }

  #region getters
  //-------------- GETTERS --------------//
  public CarInputs car_inputs() {
    return car_inputs_;
  }

  public CarMovement car_movement() {
    return car_movement_;
  }

  public Rigidbody car_rb() {
    return car_rb_;
  }

  public WheelCollider front_right() {
    return front_right_;
  }

  public WheelCollider front_left() {
    return front_left_;
  }

  public WheelCollider back_right() {
    return back_right_;
  }

  public WheelCollider back_left() {
    return back_left_;
  }

  public Transform front_right_tr() {
    return front_right_tr_;
  }

  public Transform front_left_tr() {
    return front_left_tr_;
  }

  public Transform back_right_tr() {
    return back_right_tr_;
  }

  public Transform back_left_tr() {
    return back_left_tr_;
  }

  public bool car_on() {
    return car_on_;
  }

  public int number_of_collisions() {
    return number_of_collisions_;
  }

  public Vector2 rotating_head_axis() {
    return rotating_head_axis_;
  }

  public bool car_lights_on() {
    return car_lights_on_;
  }
  //-------------------------------------//
  #endregion

  #region setters
  //-------------- SETTERS --------------//
  public void add_collisions(int value) {
    number_of_collisions_ += value;
  }

  public void set_car_lights(bool value) {
    car_lights_on_ = value;
    GetComponent<on_off_light>().TurnLights(car_lights_on_);
  }
  //-------------------------------------//
  #endregion
}
