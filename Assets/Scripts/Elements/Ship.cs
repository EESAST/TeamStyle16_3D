#region

using UnityEngine;

#endregion

public abstract class Ship : Unit
{
	private ParticleEmitter waterSplash;

	protected override void Activate()
	{
		base.Activate();
		waterSplash.emit = true;
	}

	protected override void Awake()
	{
		base.Awake();
		var splashTransform = (Instantiate(Resources.Load("Water Surface Splash")) as GameObject).transform;
		splashTransform.parent = transform;
		splashTransform.localPosition = Vector3.Scale(Center(), new Vector3(1, 0, 1));
		waterSplash = splashTransform.particleEmitter;
	}

	protected override void Deactivate()
	{
		base.Deactivate();
		waterSplash.emit = false;
	}

	protected override int Level() { return 1; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Ship")) as GameObject).GetComponent<RectTransform>(); }
}