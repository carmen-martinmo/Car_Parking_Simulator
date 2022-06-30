using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class CarCollision : MonoBehaviour {
  CarController car_controller_;
  bool can_collide_ = true;

  public float collision_speed_limit_;

  public ParticleSystem col_particles_;

  public void init(CarController reference) {
    car_controller_ = reference;
  }

  void OnCollisionEnter(Collision col) {
    if (GetComponent<Rigidbody>().velocity.magnitude > collision_speed_limit_) {
      if (can_collide_) {
        if (col.contacts[0].point.y > GetComponent<Collider>().bounds.min.y) { 
          car_controller_.add_collisions(1);
          StartCoroutine(DisableCollision());
        }
      }
    }
  }

  IEnumerator DisableCollision() {
    if (!GameController.gm_instance_.audio_controller_ref_.IsPlaying("Car_crash")) {
      float collision_percentage = GetComponent<Rigidbody>().velocity.magnitude / (car_controller_.car_movement().car_max_speed() * 0.1f);

      Sound s = GameController.gm_instance_.audio_controller_ref_.GetSoundEffect("Car_crash");
      s.pitch = Random.Range(1.0f, 2.0f);
      s.volume = 1.0f * collision_percentage;

      GameController.gm_instance_.audio_controller_ref_.PlaySoundEffect("Car_crash");

      CameraShaker.Instance.ShakeOnce(2.5f * collision_percentage, 2.5f * collision_percentage, 0.5f, 2.5f);
      col_particles_.Play();
    }

    can_collide_ = false;
    yield return new WaitForSeconds(0.2f);
    can_collide_ = true;
  }

}
