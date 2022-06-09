using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class CarCollision : MonoBehaviour {
  CarController car_controller_;
  bool can_collide_ = true;

  public ParticleSystem col_particles_;

  public void init(CarController reference) {
    car_controller_ = reference;
  }

  void OnCollisionEnter(Collision col) {
    if (GetComponent<Rigidbody>().velocity.magnitude > 1.0f) {
      if (can_collide_) {
        car_controller_.add_collisions(1);
        StartCoroutine(DisableCollision());
      }
    }
  }

  IEnumerator DisableCollision() {
    if (!GameController.gm_instance_.audio_controller_ref_.IsPlaying("Car_crash")) {
      Sound s = GameController.gm_instance_.audio_controller_ref_.GetSoundEffect("Car_crash");
      s.pitch = Random.Range(1.0f, 2.0f);
      GameController.gm_instance_.audio_controller_ref_.PlaySoundEffect("Car_crash");

      CameraShaker.Instance.ShakeOnce(2.5f, 2.5f, 0.5f, 2.5f);
      col_particles_.Play();
    }

    can_collide_ = false;
    yield return new WaitForSeconds(0.2f);
    can_collide_ = true;
  }

}
