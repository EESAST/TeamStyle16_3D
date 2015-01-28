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
		var fragments = new ArrayList();
		var count = GetComponentsInChildren<MeshFilter>().Sum(meshFilter => meshFilter.mesh.triangles.Length);
		var threshold = 300f * RelativeSize / count;
		count = 0;
		foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
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
						vertices = Methods.Array.Add(meshFilter.transform.TransformPoints(new[] { mesh.vertices[subMeshTriangles[j + 0]], mesh.vertices[subMeshTriangles[j + 1]], mesh.vertices[subMeshTriangles[j + 2]], mesh.vertices[subMeshTriangles[j + 0]] - mesh.normals[subMeshTriangles[j + 0]] * 0.1f * RelativeSize, mesh.vertices[subMeshTriangles[j + 1]] - mesh.normals[subMeshTriangles[j + 1]] * 0.1f * RelativeSize, mesh.vertices[subMeshTriangles[j + 2]] - mesh.normals[subMeshTriangles[j + 2]] * 0.1f * RelativeSize }, out center), -center),
						uv = new[] { mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]], mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]] },
						triangles = new[] { 0, 2, 3, 2, 5, 3, 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5, 2, 0, 1, 5, 4, 3 }
					};
					fragmentedMesh.RecalculateNormals();
					fragmentedMesh.CalculateTangents();
					var fragment = new GameObject
					{
						layer = LayerMask.NameToLayer("Fragment")
					};
					fragment.transform.position = center;
					fragment.AddComponent<MeshRenderer>().material = material;
					fragment.AddComponent<MeshFilter>().sharedMesh = fragmentedMesh;
					var meshCollider = fragment.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = fragmentedMesh;
					meshCollider.convex = true;
					var rigidBody = fragment.AddComponent<Rigidbody>();
					rigidBody.isKinematic = true;
					rigidBody.SetDensity(Mathf.Pow(12f / RelativeSize, 3));
					fragment.transform.parent = dummy.transform;
					var smokeTrail = Instantiate(Resources.Load("Smoke Trail")) as GameObject;
					smokeTrail.transform.SetParent(fragment.transform, false);
					fragments.Add(fragment);
					if (count++ % 5 == 0)
						yield return null;
				}
			}
		}
		foreach (GameObject fragment in fragments)
			fragment.AddComponent<FragmentManager>();
		Instantiate(Resources.Load("Detonator"), transform.TransformPoint(Center()) , Quaternion.identity);
		Destroy(gameObject);
	}

	protected override void UpdateInfo()
	{
		base.UpdateInfo();
		team = Mathf.RoundToInt(_info["team"].n);
	}
}