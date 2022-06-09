using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class TutorialSign : MonoBehaviour {

  public ParticleSystem particles_;

  bool sign_broke_ = false;

  void OnTriggerEnter(Collider col) {
    if (col.transform.tag.Equals("Car")) {
      foreach (Transform t in transform) {
        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb) {
          rb.isKinematic = false;
          rb.AddForce(col.transform.forward * col.GetComponent<Rigidbody>().velocity.magnitude, ForceMode.Impulse);
        }
      }

      if (!sign_broke_) {
        particles_.Play();
        AudioSource asource = GetComponent<AudioSource>();
        asource.pitch = Random.Range(1.0f, 2.0f);
        asource.Play();

        CameraShaker.Instance.ShakeOnce(0.5f, 2.5f, 0.5f, 2.5f);
      }

      sign_broke_ = true;
      Destroy(gameObject, 3.0f);
    }
  }
}
