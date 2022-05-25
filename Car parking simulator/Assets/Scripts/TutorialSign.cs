using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSign : MonoBehaviour {
  void OnTriggerEnter(Collider col) {
    if (col.transform.tag.Equals("Car")) {
      foreach (Transform t in transform) {
        Rigidbody rb = t.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        rb.AddForce(col.transform.forward * col.GetComponent<Rigidbody>().velocity.magnitude, ForceMode.Impulse);
      }

      Destroy(gameObject, 3.0f);
    }
  }
}
