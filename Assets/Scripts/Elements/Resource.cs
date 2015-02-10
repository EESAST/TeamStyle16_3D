#region

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class Resource : Element
{
	private float[] initialMaxEmission;
	private float[] initialMinEmission;
	protected int initialStorage;
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

	protected abstract float CurrentStorage();

	protected override IEnumerator FadeOut()
	{
		var markImage = markRect.GetComponent<RawImage>();
		var c = markImage.color;
		while ((c.a *= 0.8f) > Mathf.Epsilon)
		{
			markImage.color = c;
			yield return new WaitForSeconds(0.04f);
		}
	}

	protected override int Level() { return 2; }

	protected override void OnGUI()
	{
		base.OnGUI();
		if (!MouseOver || Screen.lockCursor)
			return;
		GUILayout.BeginArea(new Rect(Input.mousePosition.x - Screen.width * 0.06f, Screen.height - Input.mousePosition.y - Screen.height * 0.04f, Screen.width * 0.12f, Screen.height * 0.08f), GUI.skin.box);
		GUILayout.FlexibleSpace();
		GUILayout.Label(StorageDescription() + '：' + Mathf.RoundToInt(CurrentStorage()), Data.GUI.Label.SmallLeft);
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}

	protected abstract string StorageDescription();

	protected override void Update()
	{
		base.Update();
		var ratio = CurrentStorage() / initialStorage;
		for (var i = 0; i < particleEmitters.Length; i++)
		{
			particleEmitters[i].maxEmission = initialMaxEmission[i] * ratio;
			particleEmitters[i].minEmission = initialMinEmission[i] * ratio;
		}
		if (CurrentStorage() <= 0 && !isDead)
			Destruct();
	}
}