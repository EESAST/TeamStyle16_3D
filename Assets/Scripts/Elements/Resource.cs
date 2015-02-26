#region

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class Resource : Element
{
	private float[] initialMaxEmission;
	private float[] initialMinEmission;
	private ParticleEmitter[] particleEmitters;

	protected override void Awake()
	{
		base.Awake();
		team = 3;
		particleEmitters = GetComponentsInChildren<ParticleEmitter>();
		initialMaxEmission = new float[particleEmitters.Length];
		initialMinEmission = new float[particleEmitters.Length];
		for (var i = 0; i < particleEmitters.Length; i++)
		{
			initialMaxEmission[i] = particleEmitters[i].maxEmission;
			initialMinEmission[i] = particleEmitters[i].minEmission;
		}
	}

	public abstract int CurrentStorage();

	protected override IEnumerator FadeOut()
	{
		var markImage = markRect.GetComponent<RawImage>();
		var c = markImage.color;
		while ((c.a *= Settings.FastAttenuation) > Mathf.Epsilon)
		{
			markImage.color = c;
			yield return new WaitForSeconds(Settings.DeltaTime);
		}
	}

	protected override int Level() { return 2; }

	protected override void OnGUI()
	{
		base.OnGUI();
		if (!MouseOver || Screen.lockCursor)
			return;
		GUILayout.BeginArea(new Rect(Input.mousePosition.x - Screen.width * 0.075f, Screen.height - Input.mousePosition.y - Screen.height * 0.04f, Screen.width * 0.15f, Screen.height * 0.08f).FitScreen(), GUI.skin.box);
		GUILayout.FlexibleSpace();
		GUILayout.Label(StorageDescription() + '：' + (CurrentStorage() > 0 ? CurrentStorage().ToString() : "枯竭"), Data.GUI.Label.SmallLeft);
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}

	protected abstract string StorageDescription();

	protected override void Update()
	{
		base.Update();
		var ratio = (float)CurrentStorage() / Data.Replay.InitialStorage[index];
		for (var i = 0; i < particleEmitters.Length; i++)
		{
			particleEmitters[i].maxEmission = initialMaxEmission[i] * ratio;
			particleEmitters[i].minEmission = initialMinEmission[i] * ratio;
		}
		if (CurrentStorage() <= 0 && tag != "Doodad")
			Destruct();
	}
}