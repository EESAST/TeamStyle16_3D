#region

using System.Collections;
using System.Linq;
using GameStatics;
using UnityEngine;

#endregion

public abstract class Unit : Entity
{
	protected override void Destruct()
	{
		base.Destruct();
		StartCoroutine(Explode());
	}

	private IEnumerator Explode()
	{
		var dummy = new GameObject(name);
		var meshFilters = GetComponentsInChildren<MeshFilter>();
		var threshold = 3 * RelativeSize / Mathf.Pow(meshFilters.Sum(meshFilter => meshFilter.mesh.triangles.Length), 0.6f);
		var count = 0;
		var thickness = Settings.Fragment.ThicknessPerUnitSize * RelativeSize * Settings.ScaleFactor;
		var desiredAverageMass = Mathf.Pow(RelativeSize * Settings.ScaleFactor * 0.12f, 3);
		var totalMass = 0f;
		foreach (var meshFilter in meshFilters)
		{
			var mesh = meshFilter.mesh;
			for (var i = 0; i < mesh.subMeshCount; i++)
			{
				var subMeshTriangles = mesh.GetTriangles(i);
				var material = meshFilter.GetComponent<MeshRenderer>().sharedMaterials[i];
				for (var j = 0; j < subMeshTriangles.Length; j += 3)
				{
					if (Random.Range(0, 1f) > threshold)
						continue;
					Vector3 center;
					var fragmentedMesh = new Mesh
					{
						vertices = Methods.Array.Add(meshFilter.transform.TransformPoints(new[] { mesh.vertices[subMeshTriangles[j + 0]], mesh.vertices[subMeshTriangles[j + 1]], mesh.vertices[subMeshTriangles[j + 2]], mesh.vertices[subMeshTriangles[j + 0]] - mesh.normals[subMeshTriangles[j + 0]] * thickness, mesh.vertices[subMeshTriangles[j + 1]] - mesh.normals[subMeshTriangles[j + 1]] * thickness, mesh.vertices[subMeshTriangles[j + 2]] - mesh.normals[subMeshTriangles[j + 2]] * thickness }, out center), -center),
						uv = new[] { mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]], mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]] },
						triangles = new[] { 0, 2, 3, 2, 5, 3, 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5, 2, 0, 1, 5, 4, 3 }
					};
					fragmentedMesh.RecalculateNormals();
					fragmentedMesh.CalculateTangents();
					var fragment = Instantiate(Resources.Load("Fragment"), center, Quaternion.identity) as GameObject;
					fragment.GetComponent<MeshCollider>().sharedMesh = fragment.GetComponent<MeshFilter>().sharedMesh = fragmentedMesh;
					fragment.GetComponent<MeshRenderer>().material = material;
					fragment.rigidbody.SetDensity(1000);
					totalMass += fragment.rigidbody.mass;
					fragment.transform.parent = dummy.transform;
					var smokeTrail = fragment.GetComponentInChildren<ParticleEmitter>();
					smokeTrail.maxSize = smokeTrail.minSize = thickness * 3;
					if (count++ % 5 == 0)
						yield return null;
				}
			}
		}
		var ratio = desiredAverageMass * count / totalMass;
		foreach (var fragmentManager in dummy.GetComponentsInChildren<FragmentManager>())
		{
			fragmentManager.rigidbody.mass *= ratio;
			fragmentManager.enabled = true;
		}
		var detonator = Instantiate(Resources.Load("Detonator"), transform.TransformPoint(Center()), Quaternion.identity) as GameObject;
		detonator.GetComponent<Detonator>().size = RelativeSize * Settings.ScaleFactor;
		detonator.GetComponent<DetonatorForce>().power = Mathf.Pow(RelativeSize, 2.5f) * Mathf.Pow(Settings.ScaleFactor, 3);
		Destroy(gameObject);
	}

	protected override void UpdateInfo()
	{
		base.UpdateInfo();
		team = Mathf.RoundToInt(_info["team"].n);
	}
}