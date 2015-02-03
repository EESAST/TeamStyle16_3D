#region

using UnityEngine;

#endregion

public class DetonatorSprayHelper : MonoBehaviour
{
	public Material firstMaterial;
//the time at which this came into existence
	private bool isReallyOn;
	public Material secondMaterial;
	private float startTime;
	public float startTimeMax = 0;
	public float startTimeMin = 0;
	private float stopTime;
	public float stopTimeMax = 10;
	public float stopTimeMin = 10;

	private void FixedUpdate()
	{
		//is the start time passed? turn emit on
		if (Time.time > startTime)
			particleEmitter.emit = isReallyOn;

		if (Time.time > stopTime)
			particleEmitter.emit = false;
	}

	private void Start()
	{
		isReallyOn = particleEmitter.emit;

		//this kind of emitter should always start off
		particleEmitter.emit = false;

		//get a random number between startTimeMin and Max
		startTime = (Random.value * (startTimeMax - startTimeMin)) + startTimeMin + Time.time;
		stopTime = (Random.value * (stopTimeMax - stopTimeMin)) + stopTimeMin + Time.time;

		//assign a random material
		renderer.material = Random.value > 0.5f ? firstMaterial : secondMaterial;
	}
}