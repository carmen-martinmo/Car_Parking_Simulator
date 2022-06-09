using UnityEngine;
using System.Collections;

public class brake_light : MonoBehaviour
{

	public Light[] lights;
	public KeyCode keyboard;
	public KeyCode keyboard2;


	void Update () {
		foreach (Light light in lights) {
			if (GetComponent<CarController>().car_movement().current_acceleration() < 0.0f) {
				light.enabled = true;
			} else {
				light.enabled = false;
			}
		}
	}
}

