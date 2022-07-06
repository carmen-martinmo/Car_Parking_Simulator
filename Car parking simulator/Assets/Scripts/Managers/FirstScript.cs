using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstScript : MonoBehaviour {
  void Start() {
    
  }

  void Update() {
    transform.Rotate(new Vector3(Mathf.Sin(Time.deltaTime) * 10.0f, Mathf.Sin(Time.deltaTime) * 10.0f, Mathf.Sin(Time.deltaTime) * 10.0f));
  }
}
