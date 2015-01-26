#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class CompoundObjectController : FlashingController
{
	// GUI controls offset
	private readonly int ox = -220;
	private readonly int oy = 20;
	private readonly string[] shaderNames = { "Diffuse", "Specular", "VertexLit", "Bumped Specular" };
	private int currentShaderID;
	// Cached list of child objects
	private List<GameObject> objects;
	// Cached transform component
	private Transform tr;
	// 
	private void AddObject()
	{
		var primitiveType = (PrimitiveType)Random.Range(0, 4);
		var newObject = GameObject.CreatePrimitive(primitiveType);
		var newObjectTransform = newObject.GetComponent<Transform>();
		newObjectTransform.parent = tr;
		newObjectTransform.localPosition = Random.insideUnitSphere * 2f;
		objects.Add(newObject);

		// Reinitialize highlighting materials, because child objects has changed
		h.ReinitMaterials();
	}

	// 
	private void ChangeMaterial()
	{
		if (objects.Count < 1)
			AddObject();

		currentShaderID++;
		if (currentShaderID >= shaderNames.Length)
			currentShaderID = 0;

		foreach (var obj in objects)
		{
			var renderer = obj.GetComponent<Renderer>();
			var newShader = Shader.Find(shaderNames[currentShaderID]);
			renderer.material = new Material(newShader);
		}

		// Reinitialize highlightable materials, because material(s) has changed
		h.ReinitMaterials();
	}

	// 
	private void ChangeShader()
	{
		if (objects.Count < 1)
			AddObject();

		currentShaderID++;
		if (currentShaderID >= shaderNames.Length)
			currentShaderID = 0;

		foreach (var obj in objects)
		{
			var renderer = obj.GetComponent<Renderer>();
			var newShader = Shader.Find(shaderNames[currentShaderID]);
			renderer.material.shader = newShader;
		}

		// Reinitialize highlightable materials, because shader(s) has changed
		h.ReinitMaterials();
	}

	// 
	private void OnGUI()
	{
		float newX = Screen.width + ox;
		GUI.Label(new Rect(newX, oy, 500, 100), "Compound object controls:");
		if (GUI.Button(new Rect(newX, oy + 30, 200, 30), "Add Random Primitive"))
			AddObject();
		if (GUI.Button(new Rect(newX, oy + 70, 200, 30), "Change Material"))
			ChangeMaterial();
		if (GUI.Button(new Rect(newX, oy + 110, 200, 30), "Change Shader"))
			ChangeShader();
		if (GUI.Button(new Rect(newX, oy + 150, 200, 30), "Remove Object"))
			RemoveObject();
	}

	// 
	private void RemoveObject()
	{
		if (objects.Count < 1)
			return;

		var toRemove = objects[objects.Count - 1];
		objects.Remove(toRemove);
		Destroy(toRemove);

		// Reinitialize highlighting materials, because child objects has changed
		h.ReinitMaterials();
	}

	// 
	private new void Start()
	{
		base.Start();

		tr = GetComponent<Transform>();
		objects = new List<GameObject>();
		StartCoroutine(DelayFlashing());
	}
}