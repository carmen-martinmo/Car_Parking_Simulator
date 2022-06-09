using UnityEngine;
using System.Collections;

public class on_off_light : MonoBehaviour
{

	public Light[] lights;
	public KeyCode keyboard;

	public void TurnLights(bool value) {
		foreach (Light light in lights) {
			light.enabled = value;
		}		
	}
}

