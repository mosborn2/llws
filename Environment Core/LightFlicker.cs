using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour 
{	
	new Light light;

	[SerializeField] float minIntensity;
	[SerializeField] float maxIntensity;
	
	void Start()
	{
		light = this.GetComponent<Light>();
	}
	
	void Update () 
	{
		light.intensity = Random.Range(minIntensity, maxIntensity);	
	}
}