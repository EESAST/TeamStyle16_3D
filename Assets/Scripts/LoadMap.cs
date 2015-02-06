#region

using UnityEngine;

#endregion

public class LoadMap : MonoBehaviour
{
	public Texture2D[] detailTextures;
	private Vector2 realMapSize;
	public Texture2D[] splatTextures;
	public GameObject[] treePrefabs;

	private void CreateLand()
	{
		#region Preparations

		var mapData = Data.BattleData["gamebody"]["map_info"]["types"];
		var worldSize = realMapSize * Settings.ScaleFactor;
		var resolution = Mathf.RoundToInt(Mathf.Sqrt(realMapSize.x * realMapSize.y) * Settings.Terrain.Smoothness);
		var terrainData = new TerrainData { heightmapResolution = Mathf.ClosestPowerOfTwo(resolution) + 1, size = new Vector3(worldSize.y, Settings.HeightOfLevel[2], worldSize.x), alphamapResolution = resolution, baseMapResolution = resolution };

		#endregion

		#region Set Heights

		float landArea = 0;
		var heights = new float[terrainData.heightmapHeight, terrainData.heightmapWidth];
		var mapRect = new Rect(0, 0, Data.MapSize.x, Data.MapSize.y);
		// Random bumps with Gaussian Distribution
		for (int bumpNum = Mathf.RoundToInt(realMapSize.x * realMapSize.y / 40), i = 0; i < bumpNum; ++i)
		{
			var sigma = new Vector2(Random.Range(10.0f, 20.0f), Random.Range(10.0f, 20.0f));
			var mu = new Vector2(Random.Range(0, terrainData.heightmapHeight), Random.Range(0, terrainData.heightmapWidth));
			float sx = 1.0f / (sigma.x * sigma.x), sy = 1.0f / (sigma.y * sigma.y);
			int mu_x = (int)mu.x, mu_y = (int)mu.y;
			var h = Random.Range(0.1f, 0.35f);
			for (int x = Mathf.Max(-mu_x, (int)-sigma.x * 3), xe = Mathf.Min(terrainData.heightmapHeight - mu_x, (int)(sigma.x * 3)); x < xe; ++x)
				for (int y = Mathf.Max(-mu_y, (int)-sigma.y * 3), ye = Mathf.Min(terrainData.heightmapWidth - mu_y, (int)(sigma.y * 3)); y < ye; ++y)
					heights[mu_x + x, mu_y + y] += h * Mathf.Exp(- /*(float)*/(x * x * sx + y * y * sy));
		}
		for (var x = 0; x < terrainData.heightmapHeight; x++)
			for (var y = 0; y < terrainData.heightmapWidth; y++)
			{
				var i = (float)x / (terrainData.heightmapHeight - 1) * realMapSize.x - Settings.MapSizeOffset.top;
				var j = (1 - (float)y / (terrainData.heightmapWidth - 1)) * realMapSize.y - Settings.MapSizeOffset.left;
				int i0 = Mathf.FloorToInt(i), j0 = Mathf.FloorToInt(j);
				float ul = 0, ur = 0, br = 0, bl = 0, di = i - i0, dj = j - j0;
				if (mapRect.Contains(new Vector2(i0, j0)))
					ul = mapData[i0][j0].n;
				if (mapRect.Contains(new Vector2(i0, j0 + 1)))
					ur = mapData[i0][j0 + 1].n;
				if (mapRect.Contains(new Vector2(i0 + 1, j0 + 1)))
					br = mapData[i0 + 1][j0 + 1].n;
				if (mapRect.Contains(new Vector2(i0 + 1, j0)))
					bl = mapData[i0 + 1][j0].n;
				var height = (1 - di) * (1 - dj) * ul + (1 - di) * dj * ur + di * (1 - dj) * bl + di * dj * br;
				height = Mathf.Sign(height - 0.5f) * Mathf.Pow(Mathf.Abs(height * 2 - 1), 0.25f) / 2 + 0.5f;
				landArea += height;
				heights[x, y] = Mathf.Max(height, heights[x, y]);
			}
		terrainData.SetHeights(0, 0, heights);
		landArea *= realMapSize.x * realMapSize.y / (terrainData.heightmapHeight * terrainData.heightmapWidth);

		#endregion

		#region Paint Texture

		var splatPrototypes = new SplatPrototype[splatTextures.Length];
		for (var i = 0; i < splatPrototypes.Length; i++)
		{
			var splatPrototype = new SplatPrototype { texture = splatTextures[i], tileSize = Vector2.one * Settings.ScaleFactor * 4 };
			splatPrototypes[i] = splatPrototype;
		}
		terrainData.splatPrototypes = splatPrototypes;
		var alphamapResolution = terrainData.alphamapResolution;
		var alphamaps = new float[alphamapResolution, alphamapResolution, splatPrototypes.Length];
		for (var i = 0; i < alphamapResolution; i++)
			for (var j = 0; j < alphamapResolution; j++)
			{
				var height = heights[Mathf.RoundToInt((float)i / (alphamapResolution - 1) * (terrainData.heightmapHeight - 1)), Mathf.RoundToInt((float)j / (alphamapResolution - 1) * (terrainData.heightmapWidth - 1))];
				alphamaps[i, j, 0] = height;
				alphamaps[i, j, 1] = 1 - height;
			}
		terrainData.SetAlphamaps(0, 0, alphamaps);

		#endregion

		#region Place Trees

		var treePrototypes = new TreePrototype[treePrefabs.Length];
		for (var i = 0; i < treePrototypes.Length; i++)
		{
			var treePrototype = new TreePrototype { prefab = treePrefabs[i], bendFactor = 1 };
			treePrototypes[i] = treePrototype;
		}
		terrainData.treePrototypes = treePrototypes;
		var treeInstances = new TreeInstance[Mathf.RoundToInt(landArea * Settings.Terrain.Tree.Density)];
		var range = new Vector4(Settings.MapSizeOffset.right / realMapSize.y, 1 - Settings.MapSizeOffset.left / realMapSize.y, Settings.MapSizeOffset.top / realMapSize.x, 1 - Settings.MapSizeOffset.bottom / realMapSize.x);
		for (var i = 0; i < treeInstances.Length; i++)
		{
			var treeScale = Random.Range(0.08f, 0.16f) * Settings.ScaleFactor;
			Vector3 treePosition;
			do
				treePosition = new Vector3(Random.Range(range.x, range.y), 0, Random.Range(range.z, range.w));
			while ((treePosition.y = heights[Mathf.RoundToInt(treePosition.z * (terrainData.heightmapHeight - 1)), Mathf.RoundToInt(treePosition.x * (terrainData.heightmapWidth - 1))]) < Mathf.Lerp(Settings.HeightOfLevel[1] / Settings.HeightOfLevel[2], 1, 0.6f) || Methods.Coordinates.IsOccupied(Methods.Coordinates.InternalToExternal(Vector3.Scale(treePosition, new Vector3(worldSize.y, 0, worldSize.x)))));
			var treeInstance = new TreeInstance { prototypeIndex = Random.Range(0, treePrototypes.Length), position = treePosition + Vector3.up * Settings.Terrain.Tree.VerticalPositionOffset * treeScale, color = new Color(0, 0.8f, 0, 1), lightmapColor = new Color(1, 1, 1, 1), heightScale = treeScale, widthScale = treeScale };
			treeInstances[i] = treeInstance;
		}
		terrainData.treeInstances = treeInstances;

		#endregion

		#region Paint Details

		var detailPrototypes = new DetailPrototype[detailTextures.Length];
		for (var i = 0; i < detailPrototypes.Length; i++)
		{
			var detailPrototype = new DetailPrototype { prototypeTexture = detailTextures[i], minWidth = Settings.Terrain.Detail.MinDimension, minHeight = Settings.Terrain.Detail.MinDimension, maxWidth = Settings.Terrain.Detail.MaxDimension, maxHeight = Settings.Terrain.Detail.MaxDimension, renderMode = DetailRenderMode.GrassBillboard };
			detailPrototypes[i] = detailPrototype;
		}
		terrainData.detailPrototypes = detailPrototypes;
		terrainData.SetDetailResolution(resolution, Mathf.Clamp(resolution, 8, 128));
		var detailLayers = new int[detailPrototypes.Length][,];
		for (var i = 0; i < detailPrototypes.Length; i++)
			detailLayers[i] = new int[terrainData.detailResolution, terrainData.detailResolution];
		for (var i = 0; i < terrainData.detailResolution; i++)
			for (var j = 0; j < terrainData.detailResolution; j++)
			{
				var layer = Random.Range(0, detailPrototypes.Length);
				var height = heights[Mathf.RoundToInt((float)i / (terrainData.detailResolution - 1) * (terrainData.heightmapHeight - 1)), Mathf.RoundToInt((float)j / (terrainData.detailResolution - 1) * (terrainData.heightmapWidth - 1))];
				if (height > Mathf.Lerp(Settings.HeightOfLevel[1] / Settings.HeightOfLevel[2], 1, 0.4f))
					detailLayers[layer][i, j] = 1;
			}
		for (var i = 0; i < detailPrototypes.Length; i++)
			terrainData.SetDetailLayer(0, 0, i, detailLayers[i]);
		terrainData.wavingGrassAmount = Settings.Terrain.Detail.Waving.Amount;
		terrainData.wavingGrassSpeed = Settings.Terrain.Detail.Waving.Speed;
		terrainData.wavingGrassStrength = Settings.Terrain.Detail.Waving.Strength;

		#endregion

		#region Final Settings

		var terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
		terrain.treeBillboardDistance = Settings.Terrain.Tree.BillboardDistance;
		terrain.detailObjectDistance = Settings.Terrain.Detail.MaxVisibleDistance;
		terrain.detailObjectDensity = Settings.Terrain.Detail.Density;

		#endregion
	}

	private void CreateOcean()
	{
		var ocean = Instantiate(Resources.Load("Ocean")) as GameObject;
		ocean.transform.position = Methods.Coordinates.ExternalToInternal(realMapSize / 2 - new Vector2(Settings.MapSizeOffset.top, Settings.MapSizeOffset.left), 1);
		ocean.transform.localScale = new Vector3(realMapSize.y, 0, realMapSize.x) * Settings.ScaleFactor / 100;
		var oceanMaterial = ocean.GetComponent<WaterBase>().sharedMaterial;
		oceanMaterial.SetColor("_BaseColor", Settings.Ocean.RefractionColor);
		oceanMaterial.SetColor("_ReflectionColor", Settings.Ocean.ReflectionColor);
	}

	private void LoadEntities()
	{
		var mapElements = Data.BattleData["gamebody"]["map_info"]["elements"];
		for (var i = 0; i < mapElements.Count; i++)
		{
			var info = mapElements[i];
			var entityType = info["__class__"].str;
			((Instantiate(Resources.Load(entityType + '/' + entityType)) as GameObject).GetComponent(entityType) as Entity).Info = info;
		}
	}

	private void Start()
	{
		realMapSize = Data.MapSize + new Vector2(Settings.MapSizeOffset.vertical, Settings.MapSizeOffset.horizontal) - Vector2.one;
		LoadEntities();
		CreateLand();
		CreateOcean();
	}
}