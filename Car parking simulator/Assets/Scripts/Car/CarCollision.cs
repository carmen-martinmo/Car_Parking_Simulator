using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollision : MonoBehaviour {
  CarController car_controller_;
  bool can_collide_ = true;

  public void init(CarController reference) {
    car_controller_ = reference;
  }

  void OnCollisionEnter(Collision col) {
    if (GetComponent<Rigidbody>().velocity.magnitude > 1.0f) {
      if (can_collide_) {
        car_controller_.number_of_collisions_++;
        StartCoroutine(DisableCollision());
      }
    }
  }

  IEnumerator DisableCollision() {
    can_collide_ = false;
    yield return new WaitForSeconds(0.2f);
    can_collide_ = true;
  }

}
