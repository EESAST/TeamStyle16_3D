#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class Interceptor : MonoBehaviour
{
	private const float relativeSize = 0.4f;
	private AudioSource dummyAudio;
	private Transform[] missiles;
	private Carrier owner;
	public IEnumerator returnTrip;
	private Transform seat;
	private bool shallResumeAudio;
	private bool shallResumeDummyAudio;
	private ParticleSystem[] trails;

	private IEnumerator AdjustOrientation(Vector3 targetOrientation)
	{
		var targetRotation = Quaternion.LookRotation(targetOrientation);
		while (Quaternion.Angle(transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Settings.Interceptor.AngularCorrectionRate * Time.deltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
	}

	public IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		++owner.movingInterceptorsLeft;
		var localTarget = Vector3.forward * Settings.DimensionScaleFactor / 3;
		foreach (var trail in trails)
			trail.Play();
		audio.clip = Resources.Load<AudioClip>("Sounds/Interceptor_Launching");
		if (Data.GamePaused)
			shallResumeAudio = true;
		else
			audio.Play();
		while (Vector3.Distance(transform.localPosition = Vector3.MoveTowards(transform.localPosition, localTarget, Settings.Interceptor.Speed * Time.deltaTime), localTarget) > Settings.DimensionalTolerance)
			yield return null;
		transform.parent.DetachChildren();
		var target = targetPosition + ((transform.position + Vector3.up * (Settings.Map.HeightOfLevel[3] - Settings.Map.HeightOfLevel[1]) * 0.8f - targetPosition).normalized * 2.5f + Random.insideUnitSphere) * Settings.DimensionScaleFactor;
		while ((target - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Settings.Interceptor.Speed)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target - transform.position), Time.deltaTime * Settings.Interceptor.AngularCorrectionRate);
			transform.Translate(Vector3.forward * Settings.Interceptor.Speed * Time.deltaTime);
			yield return null;
		}
		yield return StartCoroutine(AdjustOrientation(targetPosition - transform.position));
		--owner.movingInterceptorsLeft;
	}

	private void Awake()
	{
		Delegates.GameStateChanged += OnGameStateChanged;
		owner = transform.GetComponentInParent<Carrier>();
		seat = transform.parent;
		missiles = new[] { transform.Find("Airframe/LSP"), transform.Find("Airframe/RSP") };
		trails = GetComponentsInChildren<ParticleSystem>();
		gameObject.AddComponent<AudioSource>();
		audio.dopplerLevel = 0;
		audio.maxDistance = Settings.Audio.MaxAudioDistance;
		audio.rolloffMode = AudioRolloffMode.Linear;
		audio.volume = Settings.Audio.Volume.Unit;
	}

	private static Vector3 Center() { return new Vector3(0.00f, 0.12f, 0.04f); }

	private IEnumerator Explode()
	{
		var dummy = Instantiate(Resources.Load("Dummy"), transform.TransformPoint(Center()), Quaternion.identity) as GameObject;
		dummyAudio = dummy.audio;
		var meshFilters = GetComponentsInChildren<MeshFilter>();
		var threshold = 5 * relativeSize / Mathf.Pow(meshFilters.Sum(meshFilter => meshFilter.mesh.triangles.Length), 0.6f);
		var count = 0;
		var thickness = Settings.Fragment.ThicknessPerUnitSize * relativeSize;
		var desiredAverageMass = Mathf.Pow(relativeSize * Settings.DimensionScaleFactor * 0.12f, 3);
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
					var fragmentedMesh = new Mesh { vertices = Methods.Array.Add(meshFilter.transform.TransformPoints(new[] { mesh.vertices[subMeshTriangles[j + 0]], mesh.vertices[subMeshTriangles[j + 1]], mesh.vertices[subMeshTriangles[j + 2]], mesh.vertices[subMeshTriangles[j + 0]] - mesh.normals[subMeshTriangles[j + 0]] * thickness, mesh.vertices[subMeshTriangles[j + 1]] - mesh.normals[subMeshTriangles[j + 1]] * thickness, mesh.vertices[subMeshTriangles[j + 2]] - mesh.normals[subMeshTriangles[j + 2]] * thickness }, out center), -center), uv = new[] { mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]], mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]] }, triangles = new[] { 0, 2, 3, 2, 5, 3, 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5, 2, 0, 1, 5, 4, 3 } };
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
					if (++count % 10 == 0)
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
		dummyAudio.maxDistance = Settings.Audio.MaxAudioDistance;
		dummyAudio.volume = Settings.Audio.Volume.Death1;
		dummyAudio.clip = Resources.Load<AudioClip>("Sounds/Death_1");
		if (Data.GamePaused)
			shallResumeDummyAudio = true;
		else
			dummyAudio.Play();
		var detonator = Instantiate(Resources.Load("Detonator_Death"), transform.TransformPoint(Center()), Quaternion.identity) as GameObject;
		detonator.GetComponent<Detonator>().size = relativeSize * Settings.DimensionScaleFactor;
		detonator.GetComponent<DetonatorForce>().power = Mathf.Pow(relativeSize, 2.5f) * Mathf.Pow(Settings.DimensionScaleFactor, 3);
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.collider.enabled = meshRenderer.enabled = false;
		Destroy(dummy, Settings.Fragment.MaxLifeSpan * 2);
		while (dummyAudio.isPlaying || Data.GamePaused)
			yield return null;
		Destroy(gameObject);
	}

	public void FireAtPosition(Vector3 targetPosition)
	{
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(owner, targetPosition, BombManager.Level.Small);
	}

	public void FireAtUnitBase(UnitBase targetUnitBase)
	{
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(owner, targetUnitBase, BombManager.Level.Small);
	}

	public void ForceDestruct()
	{
		StopCoroutine(returnTrip);
		StartCoroutine(Explode());
	}

	private void OnDestroy() { Delegates.GameStateChanged -= OnGameStateChanged; }

	private void OnGameStateChanged()
	{
		if (Data.GamePaused)
		{
			if (audio.isPlaying)
			{
				audio.Pause();
				shallResumeAudio = true;
			}
			if (dummyAudio && dummyAudio.isPlaying)
			{
				dummyAudio.Pause();
				shallResumeDummyAudio = true;
			}
		}
		else
		{
			if (shallResumeAudio)
			{
				audio.Play();
				shallResumeAudio = false;
			}
			if (shallResumeDummyAudio)
			{
				dummyAudio.Play();
				shallResumeDummyAudio = false;
			}
		}
	}

	public IEnumerator Return()
	{
		++owner.movingInterceptorsLeft;
		audio.clip = Resources.Load<AudioClip>("Sounds/Interceptor_Returning");
		if (Data.GamePaused)
			shallResumeAudio = true;
		else
			audio.Play();
		while ((seat.position - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Settings.Interceptor.Speed)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(seat.position - transform.position), Time.deltaTime * Settings.Interceptor.AngularCorrectionRate);
			transform.Translate(Vector3.forward * Settings.Interceptor.Speed * Time.deltaTime);
			yield return null;
		}
		transform.parent = seat;
		foreach (var trail in trails)
			trail.Stop();
		while (Quaternion.Angle(transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.identity, Settings.Interceptor.AngularCorrectionRate * Time.deltaTime), Quaternion.identity) > Settings.AngularTolerance || Vector3.Distance(transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Settings.Interceptor.Speed * Time.deltaTime), Vector3.zero) > Settings.DimensionalTolerance)
			yield return null;
		--owner.movingInterceptorsLeft;
	}
}