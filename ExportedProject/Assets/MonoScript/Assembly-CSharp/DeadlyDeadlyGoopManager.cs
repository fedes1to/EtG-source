using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DeadlyDeadlyGoopManager : MonoBehaviour
{
	private class GoopPositionData
	{
		public IntVector2 goopPosition;

		public GoopPositionData[] neighborGoopData;

		public bool IsOnFire;

		public bool IsElectrified;

		public bool IsFrozen;

		public bool HasPlayedFireIntro;

		public bool HasPlayedFireOutro;

		public bool lifespanOverridden;

		public int lastSourceID = -1;

		public int frameGooped = -1;

		public float remainingLifespan;

		public float remainingFreezeTimer;

		public int hasBeenFrozen;

		public bool unfrozeLastFrame;

		public bool eternal;

		public bool selfIgnites;

		public float remainingTimeTilSelfIgnition;

		public float remainingElectrifiedTime;

		public float remainingElecTimer;

		public uint elecTriggerSemaphore;

		public float remainingFireTimer;

		public float totalOnFireTime;

		public float totalFrozenTime;

		public int baseIndex = -1;

		public int NeighborsAsInt;

		public int NeighborsAsIntFuckDiagonals;

		public int GoopUpdateBin;

		public bool SupportsAmbientVFX
		{
			get
			{
				return NeighborsAsInt == 255 && remainingLifespan > 2f;
			}
		}

		public bool HasOnFireNeighbor
		{
			get
			{
				for (int i = 0; i < 8; i++)
				{
					if (neighborGoopData[i] != null && neighborGoopData[i].IsOnFire)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasFrozenNeighbor
		{
			get
			{
				for (int i = 0; i < 8; i++)
				{
					if (neighborGoopData[i] != null && neighborGoopData[i].IsFrozen)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasNonFrozenNeighbor
		{
			get
			{
				for (int i = 0; i < 8; i++)
				{
					if (neighborGoopData[i] == null || (!neighborGoopData[i].IsFrozen && !neighborGoopData[i].unfrozeLastFrame))
					{
						return true;
					}
				}
				return false;
			}
		}

		public GoopPositionData(IntVector2 position, Dictionary<IntVector2, GoopPositionData> goopData, float lifespan)
		{
			goopPosition = position;
			neighborGoopData = new GoopPositionData[8];
			remainingLifespan = lifespan;
			IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
			for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
			{
				IntVector2 key = position + cardinalsAndOrdinals[i];
				GoopPositionData value;
				if (goopData.TryGetValue(key, out value))
				{
					value.neighborGoopData[(i + 4) % 8] = this;
					neighborGoopData[i] = goopData[key];
					value.SetNeighborGoop((i + 4) % 8, true);
					SetNeighborGoop(i, true);
				}
			}
			GoopUpdateBin = UnityEngine.Random.Range(0, 4);
		}

		public void SetNeighborGoop(int index, bool value)
		{
			int num = 1 << 7 - index;
			if (value)
			{
				NeighborsAsInt |= num;
			}
			else
			{
				NeighborsAsInt &= ~num;
			}
			NeighborsAsIntFuckDiagonals = NeighborsAsInt & 0xAA;
		}
	}

	private enum VertexColorRebuildResult
	{
		ALL_OK,
		ELECTRIFIED
	}

	public static bool DrawDebugLines = false;

	public static Dictionary<IntVector2, DeadlyDeadlyGoopManager> allGoopPositionMap = new Dictionary<IntVector2, DeadlyDeadlyGoopManager>(new IntVector2EqualityComparer());

	public static List<Tuple<Vector2, float>> m_goopExceptions = new List<Tuple<Vector2, float>>();

	private static bool m_DoGoopSpawnSplashes = true;

	public static float GOOP_GRID_SIZE = 0.25f;

	public GoopDefinition goopDefinition;

	public float goopDepth = 1.5f;

	private HashSet<IntVector2> m_goopedPositions = new HashSet<IntVector2>();

	private Dictionary<IntVector2, GoopPositionData> m_goopedCells = new Dictionary<IntVector2, GoopPositionData>(new IntVector2EqualityComparer());

	private Dictionary<int, Vector2> m_uvMap;

	private Dictionary<GameActor, float> m_containedActors = new Dictionary<GameActor, float>();

	private List<Vector2> m_centerUVOptions = new List<Vector2>();

	private bool[,] m_dirtyFlags;

	private bool[,] m_colorDirtyFlags;

	private MeshRenderer[,] m_mrs;

	private Mesh[,] m_meshes;

	private Vector3[] m_vertexArray;

	private Vector2[] m_uvArray;

	private Vector2[] m_uv2Array;

	private Color32[] m_colorArray;

	private int[] m_triangleArray;

	private List<IntVector2> m_removalPositions = new List<IntVector2>();

	private int CHUNK_SIZE = 5;

	private bool m_isPlayingFireAudio;

	private bool m_isPlayingAcidAudio;

	private Shader m_shader;

	private Texture2D m_texture;

	private Texture2D m_worldTexture;

	private static int MainTexPropertyID = -1;

	private static int WorldTexPropertyID = -1;

	private static int OpaquenessMultiplyPropertyID = -1;

	private static int BrightnessMultiplyPropertyID = -1;

	private static int TintColorPropertyID = -1;

	private uint m_lastElecSemaphore;

	private ParticleSystem m_fireSystem;

	private ParticleSystem m_fireIntroSystem;

	private ParticleSystem m_fireOutroSystem;

	private ParticleSystem m_elecSystem;

	private int m_currentUpdateBin;

	private CircularBuffer<float> m_deltaTimes = new CircularBuffer<float>(4);

	private const bool c_CULL_MESHES = true;

	private const int UPDATE_EVERY_N_FRAMES = 3;

	public Color ElecColor0 = new Color(1f, 1f, 1f, 1f);

	public Color ElecColor2 = new Color(1f, 1f, 10f, 1f);

	public float divFactor = 8.7f;

	public float tFactor = 4.2f;

	private GameObject m_genericSplashPrefab;

	private static BitArray2D m_pointsArray = new BitArray2D(true);

	private static IntVector2 s_goopPointCenter = new IntVector2(0, 0);

	private static float s_goopPointRadius;

	private static float s_goopPointRadiusSquare;

	public static void ClearPerLevelData()
	{
		StaticReferenceManager.AllGoops.Clear();
		allGoopPositionMap.Clear();
		m_goopExceptions.Clear();
	}

	public static void ReinitializeData()
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = StaticReferenceManager.AllGoops[i];
			deadlyDeadlyGoopManager.ReinitializeArrays();
		}
	}

	public static void ForceClearGoopsInCell(IntVector2 cellPos)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = StaticReferenceManager.AllGoops[i];
			IntVector2 intVector = (cellPos.ToVector2() / GOOP_GRID_SIZE).ToIntVector2();
			for (int j = intVector.x; (float)j < (float)intVector.x + 1f / GOOP_GRID_SIZE; j++)
			{
				for (int k = intVector.y; (float)k < (float)intVector.y + 1f / GOOP_GRID_SIZE; k++)
				{
					deadlyDeadlyGoopManager.RemoveGoopedPosition(new IntVector2(j, k));
				}
			}
		}
	}

	public static int CountGoopsInRadius(Vector2 centerPosition, float radius)
	{
		int num = 0;
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = StaticReferenceManager.AllGoops[i];
			num += deadlyDeadlyGoopManager.CountGoopCircle(centerPosition, radius);
		}
		return num;
	}

	public static void DelayedClearGoopsInRadius(Vector2 centerPosition, float radius)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = StaticReferenceManager.AllGoops[i];
			deadlyDeadlyGoopManager.RemoveGoopCircle(centerPosition, radius);
		}
	}

	public static void FreezeGoopsCircle(Vector2 position, float radius)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			StaticReferenceManager.AllGoops[i].FreezeGoopCircle(position, radius);
		}
	}

	public static void IgniteGoopsCircle(Vector2 position, float radius)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			StaticReferenceManager.AllGoops[i].IgniteGoopCircle(position, radius);
		}
		for (int j = 0; j < StaticReferenceManager.AllGrasses.Count; j++)
		{
			StaticReferenceManager.AllGrasses[j].IgniteCircle(position, radius);
		}
	}

	public static void IgniteGoopsLine(Vector2 p1, Vector2 p2, float radius)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			StaticReferenceManager.AllGoops[i].IgniteGoopLine(p1, p2, radius);
		}
	}

	public void IgniteGoopLine(Vector2 p1, Vector2 p2, float radius)
	{
		float num = 0f;
		for (float num2 = Vector2.Distance(p2, p1); num < num2 + radius; num += radius)
		{
			IgniteGoopCircle(p1 + (p2 - p1).normalized * num, radius);
		}
	}

	public static void ElectrifyGoopsLine(Vector2 p1, Vector2 p2, float radius)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			StaticReferenceManager.AllGoops[i].ElectrifyGoopLine(p1, p2, radius);
		}
	}

	public static void FreezeGoopsLine(Vector2 p1, Vector2 p2, float radius)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			StaticReferenceManager.AllGoops[i].FreezeGoopLine(p1, p2, radius);
		}
	}

	public void ElectrifyGoopLine(Vector2 p1, Vector2 p2, float radius)
	{
		float num = 0f;
		for (float num2 = Vector2.Distance(p2, p1); num < num2 + radius; num += radius)
		{
			ElectrifyGoopCircle(p1 + (p2 - p1).normalized * num, radius);
		}
	}

	public void FreezeGoopLine(Vector2 p1, Vector2 p2, float radius)
	{
		float num = 0f;
		for (float num2 = Vector2.Distance(p2, p1); num < num2 + radius; num += radius)
		{
			FreezeGoopCircle(p1 + (p2 - p1).normalized * num, radius);
		}
	}

	public static DeadlyDeadlyGoopManager GetGoopManagerForGoopType(GoopDefinition goopDef)
	{
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			if ((bool)StaticReferenceManager.AllGoops[i] && StaticReferenceManager.AllGoops[i].goopDefinition == goopDef)
			{
				return StaticReferenceManager.AllGoops[i];
			}
		}
		GameObject gameObject = new GameObject("goop_" + goopDef.name);
		DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = gameObject.AddComponent<DeadlyDeadlyGoopManager>();
		deadlyDeadlyGoopManager.SetTexture(goopDef.goopTexture, goopDef.worldTexture);
		deadlyDeadlyGoopManager.goopDefinition = goopDef;
		deadlyDeadlyGoopManager.InitialzeUV2IfNecessary();
		StaticReferenceManager.AllGoops.Add(deadlyDeadlyGoopManager);
		deadlyDeadlyGoopManager.InitializeParticleSystems();
		return deadlyDeadlyGoopManager;
	}

	public static int RegisterUngoopableCircle(Vector2 center, float radius)
	{
		float second = radius * radius;
		m_goopExceptions.Add(Tuple.Create(center, second));
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			if ((bool)StaticReferenceManager.AllGoops[i])
			{
				StaticReferenceManager.AllGoops[i].RemoveGoopCircle(center, radius);
			}
		}
		return m_goopExceptions.Count - 1;
	}

	public static void UpdateUngoopableCircle(int id, Vector2 center, float radius)
	{
		if (id < 0 || id >= m_goopExceptions.Count)
		{
			return;
		}
		m_goopExceptions[id].First = center;
		m_goopExceptions[id].Second = radius * radius;
		for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
		{
			if ((bool)StaticReferenceManager.AllGoops[i])
			{
				StaticReferenceManager.AllGoops[i].RemoveGoopCircle(center, radius);
			}
		}
	}

	public static void DeregisterUngoopableCircle(int id)
	{
		if (m_goopExceptions != null && id < m_goopExceptions.Count && id >= 0)
		{
			m_goopExceptions[id] = null;
		}
	}

	private static void InitializePropertyIDs()
	{
		if (TintColorPropertyID == -1)
		{
			TintColorPropertyID = Shader.PropertyToID("_TintColor");
			MainTexPropertyID = Shader.PropertyToID("_MainTex");
			WorldTexPropertyID = Shader.PropertyToID("_WorldTex");
			OpaquenessMultiplyPropertyID = Shader.PropertyToID("_OpaquenessMultiply");
			BrightnessMultiplyPropertyID = Shader.PropertyToID("_BrightnessMultiply");
		}
	}

	public void SetTexture(Texture2D goopTexture, Texture2D worldTexture)
	{
		m_texture = goopTexture;
		m_worldTexture = worldTexture;
		for (int i = 0; i < m_mrs.GetLength(0); i++)
		{
			for (int j = 0; j < m_mrs.GetLength(1); j++)
			{
				if (m_mrs[i, j] != null && (bool)m_mrs[i, j])
				{
					m_mrs[i, j].material.SetTexture(MainTexPropertyID, goopTexture);
					m_mrs[i, j].material.SetTexture(WorldTexPropertyID, worldTexture);
				}
			}
		}
	}

	public void Awake()
	{
		InitializePropertyIDs();
		ConstructUVMap();
		int num = Mathf.RoundToInt(4f * ((float)CHUNK_SIZE / GOOP_GRID_SIZE) * ((float)CHUNK_SIZE / GOOP_GRID_SIZE));
		m_vertexArray = new Vector3[num];
		m_uvArray = new Vector2[num];
		m_uv2Array = new Vector2[num];
		for (int i = 0; i < num; i++)
		{
			m_uv2Array[i] = Vector2.zero;
		}
		m_colorArray = new Color32[num];
		m_triangleArray = new int[num / 4 * 6];
		int num2 = Mathf.CeilToInt((float)GameManager.Instance.Dungeon.Width / (float)CHUNK_SIZE);
		int num3 = Mathf.CeilToInt((float)GameManager.Instance.Dungeon.Height / (float)CHUNK_SIZE);
		m_mrs = new MeshRenderer[num2, num3];
		m_meshes = new Mesh[num2, num3];
		m_dirtyFlags = new bool[num2, num3];
		m_colorDirtyFlags = new bool[num2, num3];
		m_shader = ShaderCache.Acquire("Brave/GoopShader");
	}

	public void ReinitializeArrays()
	{
		int rows = Mathf.CeilToInt((float)GameManager.Instance.Dungeon.Width / (float)CHUNK_SIZE);
		int cols = Mathf.CeilToInt((float)GameManager.Instance.Dungeon.Height / (float)CHUNK_SIZE);
		m_mrs = BraveUtility.MultidimensionalArrayResize(m_mrs, rows, cols);
		m_meshes = BraveUtility.MultidimensionalArrayResize(m_meshes, rows, cols);
		m_dirtyFlags = BraveUtility.MultidimensionalArrayResize(m_dirtyFlags, rows, cols);
		m_colorDirtyFlags = BraveUtility.MultidimensionalArrayResize(m_colorDirtyFlags, rows, cols);
	}

	private Mesh GetChunkMesh(int chunkX, int chunkY)
	{
		if (m_meshes[chunkX, chunkY] != null)
		{
			return m_meshes[chunkX, chunkY];
		}
		GameObject gameObject = new GameObject(string.Format("goop_{0}_chunk_{1}_{2}", goopDefinition.name, chunkX, chunkY));
		gameObject.transform.position = new Vector3(chunkX * CHUNK_SIZE, chunkY * CHUNK_SIZE, (float)(chunkY * CHUNK_SIZE) + goopDepth);
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		Mesh mesh = new Mesh();
		mesh.MarkDynamic();
		meshFilter.mesh = mesh;
		Material material = new Material(m_shader);
		if (m_texture != null)
		{
			material.SetTexture(MainTexPropertyID, m_texture);
			material.SetTexture(WorldTexPropertyID, m_worldTexture);
		}
		if (goopDefinition.isOily)
		{
			material.SetFloat("_OilGoop", 1f);
		}
		if (goopDefinition.usesOverrideOpaqueness)
		{
			material.SetFloat(OpaquenessMultiplyPropertyID, goopDefinition.overrideOpaqueness);
		}
		meshRenderer.material = material;
		m_mrs[chunkX, chunkY] = meshRenderer;
		m_meshes[chunkX, chunkY] = mesh;
		int num = chunkX * Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num2 = num + Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num3 = chunkY * Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num4 = num3 + Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		IntVector2 goopPos = default(IntVector2);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				goopPos.x = i;
				goopPos.y = j;
				InitMesh(goopPos, chunkX, chunkY);
			}
		}
		mesh.vertices = m_vertexArray;
		mesh.triangles = m_triangleArray;
		return mesh;
	}

	private void ConstructUVMap()
	{
		m_uvMap = new Dictionary<int, Vector2>();
		m_uvMap.Add(62, new Vector2(0f, 0f));
		m_uvMap.Add(191, new Vector2(0.375f, 0f));
		m_uvMap.Add(254, new Vector2(0.375f, 0f));
		m_uvMap.Add(124, new Vector2(0.5f, 0f));
		m_uvMap.Add(31, new Vector2(0.625f, 0f));
		m_uvMap.Add(241, new Vector2(0.75f, 0f));
		m_uvMap.Add(199, new Vector2(0.875f, 0f));
		m_uvMap.Add(14, new Vector2(0f, 0.125f));
		m_uvMap.Add(143, new Vector2(0.125f, 0.125f));
		m_uvMap.Add(238, new Vector2(0.25f, 0.125f));
		m_uvMap.Add(239, new Vector2(0.375f, 0f));
		m_uvMap.Add(221, new Vector2(0.5f, 0.125f));
		m_uvMap.Add(119, new Vector2(0.625f, 0.125f));
		m_uvMap.Add(56, new Vector2(0f, 0.25f));
		m_uvMap.Add(187, new Vector2(0.125f, 0.25f));
		m_uvMap.Add(248, new Vector2(0.25f, 0.25f));
		m_uvMap.Add(251, new Vector2(0.375f, 0f));
		m_uvMap.Add(60, new Vector2(0.5f, 0.25f));
		m_uvMap.Add(30, new Vector2(0.625f, 0.25f));
		m_uvMap.Add(225, new Vector2(0.75f, 0.25f));
		m_uvMap.Add(195, new Vector2(0.875f, 0.25f));
		m_uvMap.Add(0, new Vector2(0f, 0.375f));
		m_uvMap.Add(131, new Vector2(0.125f, 0.375f));
		m_uvMap.Add(224, new Vector2(0.25f, 0.375f));
		m_uvMap.Add(227, new Vector2(0.375f, 0.375f));
		m_uvMap.Add(126, new Vector2(0.5f, 0.375f));
		m_uvMap.Add(63, new Vector2(0.625f, 0.375f));
		m_uvMap.Add(243, new Vector2(0.75f, 0.375f));
		m_uvMap.Add(231, new Vector2(0.875f, 0.375f));
		m_uvMap.Add(253, new Vector2(0f, 0.5f));
		m_uvMap.Add(223, new Vector2(0.125f, 0.5f));
		m_uvMap.Add(127, new Vector2(0.25f, 0.5f));
		m_uvMap.Add(247, new Vector2(0.375f, 0.5f));
		m_uvMap.Add(249, new Vector2(0.5f, 0.5f));
		m_uvMap.Add(207, new Vector2(0.625f, 0.5f));
		m_uvMap.Add(252, new Vector2(0.75f, 0.5f));
		m_uvMap.Add(159, new Vector2(0.875f, 0.5f));
		m_uvMap.Add(68, new Vector2(0.25f, 0.125f));
		m_uvMap.Add(102, new Vector2(0.25f, 0.125f));
		m_uvMap.Add(204, new Vector2(0.25f, 0.125f));
		m_uvMap.Add(17, new Vector2(0.25f, 0.125f));
		m_uvMap.Add(51, new Vector2(0.25f, 0.125f));
		m_uvMap.Add(153, new Vector2(0.25f, 0.125f));
		m_uvMap.Add(16, new Vector2(0f, 0.625f));
		m_uvMap.Add(4, new Vector2(0.125f, 0.625f));
		m_uvMap.Add(64, new Vector2(0.25f, 0.625f));
		m_uvMap.Add(1, new Vector2(0.375f, 0.625f));
		m_uvMap.Add(240, new Vector2(0.5f, 0.625f));
		m_uvMap.Add(135, new Vector2(0.625f, 0.625f));
		m_uvMap.Add(120, new Vector2(0.75f, 0.625f));
		m_uvMap.Add(15, new Vector2(0.875f, 0.625f));
		m_uvMap.Add(-1, new Vector2(0f, 0.375f));
		m_centerUVOptions.Add(new Vector2(0.375f, 0f));
		m_centerUVOptions.Add(new Vector2(0.375f, 0f));
		m_centerUVOptions.Add(new Vector2(0.375f, 0f));
		m_centerUVOptions.Add(new Vector2(0.375f, 0f));
		m_centerUVOptions.Add(new Vector2(0.375f, 0f));
		m_centerUVOptions.Add(new Vector2(0f, 0.875f));
		m_centerUVOptions.Add(new Vector2(0.125f, 0.875f));
		m_centerUVOptions.Add(new Vector2(0.25f, 0.875f));
		m_centerUVOptions.Add(new Vector2(0.375f, 0.875f));
	}

	public void ProcessProjectile(Projectile p)
	{
		for (int i = 0; i < goopDefinition.goopDamageTypeInteractions.Count; i++)
		{
			GoopDefinition.GoopDamageTypeInteraction goopDamageTypeInteraction = goopDefinition.goopDamageTypeInteractions[i];
			bool flag = goopDamageTypeInteraction.damageType == CoreDamageTypes.Ice && p.AppliesFreeze;
			if (((p.damageTypes & goopDamageTypeInteraction.damageType) == goopDamageTypeInteraction.damageType || flag) && IsPositionInGoop(p.specRigidbody.UnitCenter))
			{
				if (goopDamageTypeInteraction.ignitionMode == GoopDefinition.GoopDamageTypeInteraction.GoopIgnitionMode.IGNITE)
				{
					IgniteGoopCircle(p.specRigidbody.UnitCenter, 1f);
				}
				else if (goopDamageTypeInteraction.ignitionMode != GoopDefinition.GoopDamageTypeInteraction.GoopIgnitionMode.DOUSE)
				{
				}
				if (goopDamageTypeInteraction.electrifiesGoop)
				{
					IntVector2 cellIndex = (p.specRigidbody.UnitCenter / GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
					AkSoundEngine.PostEvent("Play_ENV_puddle_zap_01", GameManager.Instance.PrimaryPlayer.gameObject);
					GameManager.Instance.Dungeon.StartCoroutine(HandleRecursiveElectrification(cellIndex));
				}
				if (goopDamageTypeInteraction.freezesGoop)
				{
					FreezeGoopCircle(p.specRigidbody.UnitCenter, 1f);
				}
			}
		}
	}

	private void ElectrifyCell(IntVector2 cellIndex)
	{
		if (goopDefinition.CanBeElectrified && m_goopedCells.ContainsKey(cellIndex) && m_goopedCells[cellIndex] != null && !m_goopedCells[cellIndex].IsFrozen && !(m_goopedCells[cellIndex].remainingLifespan < goopDefinition.fadePeriod))
		{
			if (!m_goopedCells[cellIndex].IsElectrified)
			{
				m_goopedCells[cellIndex].IsElectrified = true;
				m_goopedCells[cellIndex].remainingElecTimer = 0f;
			}
			m_goopedCells[cellIndex].remainingElectrifiedTime = goopDefinition.electrifiedTime;
		}
	}

	private IEnumerator HandleRecursiveElectrification(IntVector2 cellIndex)
	{
		if (!goopDefinition.CanBeElectrified || m_goopedCells[cellIndex].IsFrozen || m_goopedCells[cellIndex].remainingLifespan < goopDefinition.fadePeriod)
		{
			yield break;
		}
		Queue<IntVector2> m_positionsToElectrify = new Queue<IntVector2>();
		m_positionsToElectrify.Enqueue(cellIndex);
		m_lastElecSemaphore++;
		uint currentSemaphore = m_lastElecSemaphore;
		m_goopedCells[cellIndex].elecTriggerSemaphore = currentSemaphore;
		int enumeratorCounter = 0;
		while (m_positionsToElectrify.Count > 0)
		{
			IntVector2 currentPos = m_positionsToElectrify.Dequeue();
			if (!m_goopedCells.ContainsKey(currentPos) || m_goopedCells[currentPos] == null)
			{
				continue;
			}
			ElectrifyCell(currentPos);
			for (int i = 0; i < 8; i++)
			{
				GoopPositionData goopPositionData = m_goopedCells[currentPos].neighborGoopData[i];
				if (goopPositionData != null && goopPositionData.elecTriggerSemaphore < currentSemaphore && (!goopPositionData.IsElectrified || goopPositionData.remainingElectrifiedTime < goopDefinition.electrifiedTime - 0.01f))
				{
					goopPositionData.elecTriggerSemaphore = currentSemaphore;
					m_positionsToElectrify.Enqueue(goopPositionData.goopPosition);
				}
			}
			enumeratorCounter++;
			if (enumeratorCounter > 200)
			{
				yield return null;
				enumeratorCounter = 0;
			}
		}
	}

	private void FreezeCell(IntVector2 cellIndex)
	{
		if (goopDefinition.CanBeFrozen)
		{
			GoopPositionData goopPositionData = m_goopedCells[cellIndex];
			goopPositionData.IsFrozen = true;
			goopPositionData.remainingFreezeTimer = goopDefinition.freezeLifespan;
		}
	}

	public void ElectrifyGoopCircle(Vector2 center, float radius)
	{
		if (!goopDefinition.CanBeElectrified)
		{
			return;
		}
		int num = Mathf.CeilToInt((center.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.FloorToInt((center.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.CeilToInt((center.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.FloorToInt((center.y + radius) / GOOP_GRID_SIZE);
		Vector2 a = center / GOOP_GRID_SIZE;
		float num5 = radius / GOOP_GRID_SIZE;
		for (int i = num; i < num2; i++)
		{
			bool flag = false;
			for (int j = num3; j < num4; j++)
			{
				IntVector2 intVector = new IntVector2(i, j);
				if (m_goopedCells.ContainsKey(intVector) && Vector2.Distance(a, intVector.ToVector2()) <= num5)
				{
					flag = true;
					GameManager.Instance.Dungeon.StartCoroutine(HandleRecursiveElectrification(intVector));
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	public void FreezeGoopCircle(Vector2 center, float radius)
	{
		if (!goopDefinition.CanBeFrozen)
		{
			return;
		}
		int num = Mathf.CeilToInt((center.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.FloorToInt((center.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.CeilToInt((center.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.FloorToInt((center.y + radius) / GOOP_GRID_SIZE);
		Vector2 a = center / GOOP_GRID_SIZE;
		float num5 = radius / GOOP_GRID_SIZE;
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				IntVector2 intVector = new IntVector2(i, j);
				if (m_goopedCells.ContainsKey(intVector) && Vector2.Distance(a, intVector.ToVector2()) <= num5)
				{
					FreezeCell(intVector);
				}
			}
		}
	}

	private void IgniteCell(IntVector2 cellIndex)
	{
		if (goopDefinition.CanBeIgnited)
		{
			GoopPositionData goopPositionData = m_goopedCells[cellIndex];
			goopPositionData.IsOnFire = true;
			if (goopDefinition.ignitionChangesLifetime)
			{
				goopPositionData.remainingLifespan = Mathf.Min(m_goopedCells[cellIndex].remainingLifespan, goopDefinition.ignitedLifetime);
				goopPositionData.lifespanOverridden = true;
			}
		}
	}

	public void IgniteGoopCircle(Vector2 center, float radius)
	{
		if (!goopDefinition.CanBeIgnited)
		{
			return;
		}
		int num = Mathf.CeilToInt((center.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.FloorToInt((center.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.CeilToInt((center.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.FloorToInt((center.y + radius) / GOOP_GRID_SIZE);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				IntVector2 intVector = new IntVector2(i, j);
				if (m_goopedCells.ContainsKey(intVector))
				{
					IgniteCell(intVector);
				}
			}
		}
	}

	public bool ProcessGameActor(GameActor actor)
	{
		if (IsPositionInGoop(actor.specRigidbody.UnitCenter))
		{
			IntVector2 intVector = (actor.specRigidbody.UnitCenter / GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
			PlayerController playerController = actor as PlayerController;
			if ((bool)playerController && goopDefinition.playerStepsChangeLifetime && playerController.IsGrounded && !playerController.IsSlidingOverSurface)
			{
				for (int i = -2; i <= 2; i++)
				{
					for (int j = -2; j <= 2; j++)
					{
						if ((float)(Mathf.Abs(i) + Mathf.Abs(j)) > 3.5f)
						{
							continue;
						}
						IntVector2 key = new IntVector2(intVector.x + i, intVector.y + j);
						if (m_goopedCells.ContainsKey(key))
						{
							GoopPositionData goopPositionData = m_goopedCells[key];
							if (goopPositionData.remainingLifespan > goopDefinition.playerStepsLifetime)
							{
								goopPositionData.remainingLifespan = goopDefinition.playerStepsLifetime;
							}
						}
					}
				}
			}
			if (actor.IsFlying && !m_goopedCells[intVector].IsOnFire)
			{
				return false;
			}
			if (actor is PlayerController)
			{
				PlayerController playerController2 = actor as PlayerController;
				if ((bool)playerController2.CurrentGun && playerController2.CurrentGun.gunName == "Mermaid Gun")
				{
					return false;
				}
			}
			if (!m_containedActors.ContainsKey(actor))
			{
				m_containedActors.Add(actor, 0f);
				InitialGoopEffect(actor);
			}
			else
			{
				m_containedActors[actor] += BraveTime.DeltaTime;
			}
			DoTimelessGoopEffect(actor, intVector);
			if (actor is AIActor)
			{
				DoGoopEffect(actor, intVector);
			}
			else if (actor is PlayerController)
			{
				PlayerController playerController3 = actor as PlayerController;
				if (goopDefinition.damagesPlayers && playerController3.spriteAnimator.QueryGroundedFrame())
				{
					if (playerController3.CurrentPoisonMeterValue >= 1f)
					{
						DoGoopEffect(actor, intVector);
						playerController3.CurrentPoisonMeterValue -= 1f;
					}
					playerController3.IncreasePoison(BraveTime.DeltaTime / goopDefinition.delayBeforeDamageToPlayers);
				}
				if (goopDefinition.DrainsAmmo && playerController3.spriteAnimator.QueryGroundedFrame())
				{
					if (playerController3.CurrentDrainMeterValue >= 1f)
					{
						playerController3.inventory.HandleAmmoDrain(goopDefinition.PercentAmmoDrainPerSecond * BraveTime.DeltaTime);
					}
					else
					{
						playerController3.CurrentDrainMeterValue += BraveTime.DeltaTime / goopDefinition.delayBeforeDamageToPlayers;
					}
				}
			}
			return true;
		}
		if (m_containedActors.ContainsKey(actor))
		{
			m_containedActors.Remove(actor);
			EndGoopEffect(actor);
		}
		return false;
	}

	public bool IsPositionOnFire(Vector2 position)
	{
		IntVector2 key = (position / GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
		GoopPositionData value;
		if (m_goopedCells.TryGetValue(key, out value) && value.remainingLifespan > goopDefinition.fadePeriod)
		{
			return value.IsOnFire;
		}
		return false;
	}

	public bool IsPositionFrozen(Vector2 position)
	{
		IntVector2 key = (position / GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
		GoopPositionData value;
		if (m_goopedCells.TryGetValue(key, out value) && value.remainingLifespan > goopDefinition.fadePeriod)
		{
			return value.IsFrozen;
		}
		return false;
	}

	public bool IsPositionInGoop(Vector2 position)
	{
		IntVector2 key = (position / GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
		GoopPositionData value;
		if (m_goopedCells.TryGetValue(key, out value) && value.remainingLifespan > goopDefinition.fadePeriod)
		{
			return true;
		}
		return false;
	}

	public void InitialGoopEffect(GameActor actor)
	{
		if (goopDefinition.AppliesSpeedModifier)
		{
			actor.ApplyEffect(goopDefinition.SpeedModifierEffect);
		}
	}

	public void DoTimelessGoopEffect(GameActor actor, IntVector2 goopPosition)
	{
		float num = 0f;
		CoreDamageTypes coreDamageTypes = CoreDamageTypes.None;
		if (m_goopedCells[goopPosition].IsOnFire)
		{
			if (actor is PlayerController && !goopDefinition.UsesGreenFire)
			{
				PlayerController playerController = actor as PlayerController;
				if (playerController.IsGrounded && !playerController.IsSlidingOverSurface && (bool)playerController.healthHaver && playerController.healthHaver.IsVulnerable)
				{
					playerController.IsOnFire = true;
					playerController.CurrentFireMeterValue += BraveTime.DeltaTime * 0.5f;
				}
			}
			else if (actor is AIActor)
			{
				if (goopDefinition.fireBurnsEnemies)
				{
					if (actor.GetResistanceForEffectType(EffectResistanceType.Fire) < 1f)
					{
						num += goopDefinition.fireDamagePerSecondToEnemies * BraveTime.DeltaTime;
					}
					(actor as AIActor).ApplyEffect(goopDefinition.fireEffect);
				}
				else
				{
					num += goopDefinition.fireDamagePerSecondToEnemies * BraveTime.DeltaTime;
				}
			}
			coreDamageTypes |= CoreDamageTypes.Fire;
		}
		if (m_goopedCells[goopPosition].IsElectrified)
		{
			if (actor is PlayerController)
			{
				num = Mathf.Max(num, goopDefinition.electrifiedDamageToPlayer);
			}
			else if (actor is AIActor)
			{
				num += goopDefinition.electrifiedDamagePerSecondToEnemies * BraveTime.DeltaTime;
			}
			coreDamageTypes |= CoreDamageTypes.Electric;
		}
		if (goopDefinition.AppliesSpeedModifierContinuously)
		{
			actor.ApplyEffect(goopDefinition.SpeedModifierEffect);
		}
		if (goopDefinition.AppliesDamageOverTime)
		{
			actor.ApplyEffect(goopDefinition.HealthModifierEffect);
		}
		if (actor is AIActor && (actor as AIActor).IsNormalEnemy && goopDefinition.AppliesCharm)
		{
			actor.ApplyEffect(goopDefinition.CharmModifierEffect);
		}
		if (actor is AIActor && (actor as AIActor).IsNormalEnemy && goopDefinition.AppliesCheese)
		{
			AIActor aIActor = actor as AIActor;
			if (!aIActor.IsGone && aIActor.HasBeenEngaged)
			{
				actor.ApplyEffect(goopDefinition.CheeseModifierEffect, BraveTime.DeltaTime * goopDefinition.CheeseModifierEffect.CheeseAmount);
			}
		}
		if (num > 0f)
		{
			actor.healthHaver.ApplyDamage(num, Vector2.zero, StringTableManager.GetEnemiesString("#GOOP"), coreDamageTypes, DamageCategory.Environment);
		}
	}

	public void DoGoopEffect(GameActor actor, IntVector2 goopPosition)
	{
		float num = 0f;
		if (goopDefinition.damagesPlayers && actor is PlayerController)
		{
			num = goopDefinition.damageToPlayers;
		}
		else if (goopDefinition.damagesEnemies && actor is AIActor)
		{
			num = goopDefinition.damagePerSecondtoEnemies * BraveTime.DeltaTime;
		}
		if (num > 0f)
		{
			actor.healthHaver.ApplyDamage(num, Vector2.zero, StringTableManager.GetEnemiesString("#GOOP"), goopDefinition.damageTypes, DamageCategory.Environment, true);
		}
	}

	public void EndGoopEffect(GameActor actor)
	{
		if (goopDefinition.AppliesSpeedModifier)
		{
			actor.RemoveEffect(goopDefinition.SpeedModifierEffect);
		}
	}

	private void SetColorDirty(IntVector2 goopPosition)
	{
		IntVector2 intVector = (goopPosition.ToVector2() * GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
		int num = Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num2 = Mathf.FloorToInt((float)intVector.x / (float)CHUNK_SIZE);
		int num3 = Mathf.FloorToInt((float)intVector.y / (float)CHUNK_SIZE);
		bool flag = num2 > 0 && goopPosition.x % num == 0;
		bool flag2 = num2 < m_colorDirtyFlags.GetLength(0) - 1 && goopPosition.x % num == num - 1;
		bool flag3 = num3 > 0 && goopPosition.y % num == 0;
		bool flag4 = num3 < m_colorDirtyFlags.GetLength(1) - 1 && goopPosition.y % num == num - 1;
		m_colorDirtyFlags[num2, num3] = true;
		if (flag)
		{
			m_colorDirtyFlags[num2 - 1, num3] = true;
		}
		if (flag2)
		{
			m_colorDirtyFlags[num2 + 1, num3] = true;
		}
		if (flag3)
		{
			m_colorDirtyFlags[num2, num3 - 1] = true;
		}
		if (flag4)
		{
			m_colorDirtyFlags[num2, num3 + 1] = true;
		}
		if (flag && flag3)
		{
			m_colorDirtyFlags[num2 - 1, num3 - 1] = true;
		}
		if (flag && flag4)
		{
			m_colorDirtyFlags[num2 - 1, num3 + 1] = true;
		}
		if (flag2 && flag3)
		{
			m_colorDirtyFlags[num2 + 1, num3 - 1] = true;
		}
		if (flag2 && flag4)
		{
			m_colorDirtyFlags[num2 + 1, num3 + 1] = true;
		}
	}

	private void SetDirty(IntVector2 goopPosition)
	{
		int num = (int)((float)goopPosition.x * GOOP_GRID_SIZE);
		int num2 = (int)((float)goopPosition.y * GOOP_GRID_SIZE);
		int num3 = Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num4 = Mathf.FloorToInt((float)num / (float)CHUNK_SIZE);
		int num5 = Mathf.FloorToInt((float)num2 / (float)CHUNK_SIZE);
		if (num4 >= 0 && num4 < m_dirtyFlags.GetLength(0) && num5 >= 0 && num5 < m_dirtyFlags.GetLength(1))
		{
			bool flag = num4 > 0 && goopPosition.x % num3 == 0;
			bool flag2 = num4 < m_dirtyFlags.GetLength(0) - 1 && goopPosition.x % num3 == num3 - 1;
			bool flag3 = num5 > 0 && goopPosition.y % num3 == 0;
			bool flag4 = num5 < m_dirtyFlags.GetLength(1) - 1 && goopPosition.y % num3 == num3 - 1;
			m_dirtyFlags[num4, num5] = true;
			if (flag)
			{
				m_dirtyFlags[num4 - 1, num5] = true;
			}
			if (flag2)
			{
				m_dirtyFlags[num4 + 1, num5] = true;
			}
			if (flag3)
			{
				m_dirtyFlags[num4, num5 - 1] = true;
			}
			if (flag4)
			{
				m_dirtyFlags[num4, num5 + 1] = true;
			}
			if (flag && flag3)
			{
				m_dirtyFlags[num4 - 1, num5 - 1] = true;
			}
			if (flag && flag4)
			{
				m_dirtyFlags[num4 - 1, num5 + 1] = true;
			}
			if (flag2 && flag3)
			{
				m_dirtyFlags[num4 + 1, num5 - 1] = true;
			}
			if (flag2 && flag4)
			{
				m_dirtyFlags[num4 + 1, num5 + 1] = true;
			}
		}
	}

	private void InitialzeUV2IfNecessary()
	{
		if (goopDefinition.usesWorldTextureByDefault)
		{
			int num = Mathf.RoundToInt(4f * ((float)CHUNK_SIZE / GOOP_GRID_SIZE) * ((float)CHUNK_SIZE / GOOP_GRID_SIZE));
			for (int i = 0; i < num; i++)
			{
				m_uv2Array[i] = Vector2.right;
			}
		}
	}

	private void InitializeParticleSystems()
	{
		string text = ((!goopDefinition.UsesGreenFire) ? "Gungeon_Fire_Main" : "Gungeon_Fire_Main_Green");
		string text2 = ((!goopDefinition.UsesGreenFire) ? "Gungeon_Fire_Intro" : "Gungeon_Fire_Intro_Green");
		string text3 = ((!goopDefinition.UsesGreenFire) ? "Gungeon_Fire_Outro" : "Gungeon_Fire_Outro_Green");
		GameObject gameObject = GameObject.Find(text);
		if (gameObject == null)
		{
			string path = ((!goopDefinition.UsesGreenFire) ? "Particles/Gungeon_Fire_Main_raw" : "Particles/Gungeon_Fire_Main_green");
			gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load(path), Vector3.zero, Quaternion.identity);
			gameObject.name = text;
		}
		m_fireSystem = gameObject.GetComponent<ParticleSystem>();
		GameObject gameObject2 = GameObject.Find(text2);
		if (gameObject2 == null)
		{
			string path2 = ((!goopDefinition.UsesGreenFire) ? "Particles/Gungeon_Fire_Intro_raw" : "Particles/Gungeon_Fire_Intro_green");
			gameObject2 = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load(path2), Vector3.zero, Quaternion.identity);
			gameObject2.name = text2;
		}
		m_fireIntroSystem = gameObject2.GetComponent<ParticleSystem>();
		GameObject gameObject3 = GameObject.Find(text3);
		if (gameObject3 == null)
		{
			string path3 = ((!goopDefinition.UsesGreenFire) ? "Particles/Gungeon_Fire_Outro_raw" : "Particles/Gungeon_Fire_Outro_green");
			gameObject3 = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load(path3), Vector3.zero, Quaternion.identity);
			gameObject3.name = text3;
		}
		m_fireOutroSystem = gameObject3.GetComponent<ParticleSystem>();
		GameObject gameObject4 = GameObject.Find("Gungeon_Elec");
		if (gameObject4 == null)
		{
			gameObject4 = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Particles/Gungeon_Elec_raw"), Vector3.zero, Quaternion.identity);
			gameObject4.name = "Gungeon_Elec";
		}
		m_elecSystem = gameObject4.GetComponent<ParticleSystem>();
	}

	private void LateUpdate()
	{
		if (Time.timeScale <= 0f || GameManager.Instance.IsPaused)
		{
			return;
		}
		m_removalPositions.Clear();
		bool flag = false;
		bool flag2 = false;
		m_currentUpdateBin = (m_currentUpdateBin + 1) % 4;
		m_deltaTimes.Enqueue(BraveTime.DeltaTime);
		float num = 0f;
		for (int i = 0; i < m_deltaTimes.Count; i++)
		{
			num += m_deltaTimes[i];
		}
		foreach (IntVector2 goopedPosition in m_goopedPositions)
		{
			GoopPositionData goopPositionData = m_goopedCells[goopedPosition];
			if (goopPositionData.GoopUpdateBin != m_currentUpdateBin)
			{
				continue;
			}
			goopPositionData.unfrozeLastFrame = false;
			if (goopDefinition.usesAmbientGoopFX && goopPositionData.remainingLifespan > 0f && UnityEngine.Random.value < goopDefinition.ambientGoopFXChance && goopPositionData.SupportsAmbientVFX)
			{
				Vector3 position = goopedPosition.ToVector3(goopedPosition.y) * GOOP_GRID_SIZE;
				goopDefinition.ambientGoopFX.SpawnAtPosition(position);
			}
			if (!goopPositionData.IsOnFire && !goopPositionData.IsElectrified && !goopDefinition.usesLifespan && !goopPositionData.lifespanOverridden && !goopPositionData.selfIgnites)
			{
				continue;
			}
			if (goopPositionData.selfIgnites)
			{
				if (goopPositionData.remainingTimeTilSelfIgnition <= 0f)
				{
					goopPositionData.selfIgnites = false;
					IgniteCell(goopedPosition);
				}
				else
				{
					goopPositionData.remainingTimeTilSelfIgnition -= num;
				}
			}
			if (goopPositionData.remainingLifespan > 0f)
			{
				if (!goopPositionData.IsFrozen)
				{
					goopPositionData.remainingLifespan -= num;
				}
				else
				{
					goopPositionData.remainingFreezeTimer -= num;
					if (goopPositionData.remainingFreezeTimer <= 0f)
					{
						goopPositionData.hasBeenFrozen = 1;
						goopPositionData.remainingLifespan = Mathf.Min(goopPositionData.remainingLifespan, goopDefinition.fadePeriod);
						goopPositionData.remainingLifespan -= num;
					}
				}
				if (goopDefinition.usesAcidAudio)
				{
					flag2 = true;
				}
				if (goopPositionData.remainingLifespan < goopDefinition.fadePeriod && goopPositionData.IsElectrified)
				{
					goopPositionData.remainingLifespan = goopDefinition.fadePeriod;
				}
				if (goopPositionData.remainingLifespan < goopDefinition.fadePeriod || goopPositionData.remainingLifespan <= 0f)
				{
					SetDirty(goopedPosition);
					goopPositionData.IsOnFire = false;
					goopPositionData.IsElectrified = false;
					goopPositionData.HasPlayedFireIntro = false;
					goopPositionData.HasPlayedFireOutro = false;
					if (goopPositionData.remainingLifespan <= 0f)
					{
						m_removalPositions.Add(goopedPosition);
						continue;
					}
				}
				if (goopPositionData.IsElectrified)
				{
					goopPositionData.remainingElectrifiedTime -= num;
					goopPositionData.remainingElecTimer -= num;
					if (goopPositionData.remainingElectrifiedTime <= 0f)
					{
						goopPositionData.IsElectrified = false;
						goopPositionData.remainingElectrifiedTime = 0f;
					}
					if (goopPositionData.IsElectrified && m_elecSystem != null && goopPositionData.remainingElecTimer <= 0f && goopedPosition.x % 2 == 0 && goopedPosition.y % 2 == 0)
					{
						Vector3 vector = goopedPosition.ToVector3(goopedPosition.y) * GOOP_GRID_SIZE + new Vector3(UnityEngine.Random.Range(0.125f, 0.375f), UnityEngine.Random.Range(0.125f, 0.375f), 0.125f).Quantize(0.0625f);
						float num2 = UnityEngine.Random.Range(0.75f, 1.5f);
						if (UnityEngine.Random.value < 0.1f)
						{
							ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
							emitParams.position = vector;
							emitParams.velocity = Vector3.zero;
							emitParams.startSize = m_fireSystem.startSize;
							emitParams.startLifetime = num2;
							emitParams.startColor = m_fireSystem.startColor;
							emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
							ParticleSystem.EmitParams emitParams2 = emitParams;
							m_elecSystem.Emit(emitParams2, 1);
							if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
							{
								int num3 = ((GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.MEDIUM) ? 10 : 4);
								GlobalSparksDoer.DoRandomParticleBurst(num3, vector + new Vector3(-0.625f, -0.625f, 0f), vector + new Vector3(0.625f, 0.625f, 0f), Vector3.up, 120f, 0.5f);
							}
						}
						goopPositionData.remainingElecTimer = num2 - 0.1f;
					}
				}
				if (goopPositionData.IsFrozen)
				{
					if (goopPositionData.totalOnFireTime < 0.5f || goopPositionData.remainingLifespan < goopDefinition.fadePeriod)
					{
						SetColorDirty(goopedPosition);
					}
					goopPositionData.totalOnFireTime += num;
					if (goopPositionData.totalOnFireTime >= goopDefinition.freezeSpreadTime)
					{
						for (int j = 0; j < goopPositionData.neighborGoopData.Length; j++)
						{
							if (goopPositionData.neighborGoopData[j] != null && !goopPositionData.neighborGoopData[j].IsFrozen && goopPositionData.neighborGoopData[j].hasBeenFrozen == 0)
							{
								if (UnityEngine.Random.value < 0.2f)
								{
									FreezeCell(goopPositionData.neighborGoopData[j].goopPosition);
								}
								else
								{
									goopPositionData.totalFrozenTime = 0f;
								}
							}
						}
					}
				}
				if (goopPositionData.IsOnFire)
				{
					flag = true;
					SetColorDirty(goopedPosition);
					goopPositionData.remainingFireTimer -= num;
					goopPositionData.totalOnFireTime += num;
					if (goopPositionData.totalOnFireTime >= goopDefinition.igniteSpreadTime)
					{
						for (int k = 0; k < goopPositionData.neighborGoopData.Length; k++)
						{
							if (goopPositionData.neighborGoopData[k] != null && !goopPositionData.neighborGoopData[k].IsOnFire)
							{
								if (UnityEngine.Random.value < 0.2f)
								{
									IgniteCell(goopPositionData.neighborGoopData[k].goopPosition);
								}
								else
								{
									goopPositionData.totalOnFireTime = 0f;
								}
							}
						}
					}
				}
				if (!(m_fireSystem != null) || !goopPositionData.IsOnFire || !(goopPositionData.remainingFireTimer <= 0f) || goopedPosition.x % 2 != 0 || goopedPosition.y % 2 != 0)
				{
					continue;
				}
				Vector3 vector2 = goopedPosition.ToVector3(goopedPosition.y) * GOOP_GRID_SIZE + new Vector3(UnityEngine.Random.Range(0.125f, 0.375f), UnityEngine.Random.Range(0.125f, 0.375f), 0.125f).Quantize(0.0625f);
				float num4 = UnityEngine.Random.Range(1f, 1.5f);
				float num5 = UnityEngine.Random.Range(0.75f, 1f);
				if (!goopPositionData.HasPlayedFireOutro)
				{
					if (!goopPositionData.HasPlayedFireOutro && goopPositionData.remainingLifespan <= num5 + goopDefinition.fadePeriod && m_fireOutroSystem != null)
					{
						num4 = num5;
						ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
						emitParams.position = vector2;
						emitParams.velocity = Vector3.zero;
						emitParams.startSize = m_fireSystem.startSize;
						emitParams.startLifetime = num5;
						emitParams.startColor = m_fireSystem.startColor;
						emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
						ParticleSystem.EmitParams emitParams3 = emitParams;
						m_fireOutroSystem.Emit(emitParams3, 1);
						goopPositionData.HasPlayedFireOutro = true;
					}
					else if (!goopPositionData.HasPlayedFireIntro && m_fireIntroSystem != null)
					{
						num4 = UnityEngine.Random.Range(0.75f, 1f);
						ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
						emitParams.position = vector2;
						emitParams.velocity = Vector3.zero;
						emitParams.startSize = m_fireSystem.startSize;
						emitParams.startLifetime = num4;
						emitParams.startColor = m_fireSystem.startColor;
						emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
						ParticleSystem.EmitParams emitParams4 = emitParams;
						m_fireIntroSystem.Emit(emitParams4, 1);
						goopPositionData.HasPlayedFireIntro = true;
					}
					else
					{
						if (UnityEngine.Random.value < 0.5f)
						{
							ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
							emitParams.position = vector2;
							emitParams.velocity = Vector3.zero;
							emitParams.startSize = m_fireSystem.startSize;
							emitParams.startLifetime = num4;
							emitParams.startColor = m_fireSystem.startColor;
							emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
							ParticleSystem.EmitParams emitParams5 = emitParams;
							m_fireSystem.Emit(emitParams5, 1);
						}
						GlobalSparksDoer.DoRandomParticleBurst(UnityEngine.Random.Range(3, 6), vector2, vector2, Vector3.up * 2f, 30f, 1f, null, UnityEngine.Random.Range(0.5f, 1f), (!goopDefinition.UsesGreenFire) ? Color.red : Color.green);
					}
				}
				goopPositionData.remainingFireTimer = num4 - 0.125f;
			}
			else
			{
				m_removalPositions.Add(goopedPosition);
			}
		}
		if (flag && !m_isPlayingFireAudio)
		{
			m_isPlayingFireAudio = true;
			AkSoundEngine.PostEvent("Play_ENV_oilfire_ignite_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
		else if (!flag && m_isPlayingFireAudio)
		{
			m_isPlayingFireAudio = false;
			AkSoundEngine.PostEvent("Stop_ENV_oilfire_loop_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
		if (flag2 && !m_isPlayingAcidAudio)
		{
			m_isPlayingAcidAudio = true;
			AkSoundEngine.PostEvent("Play_ENV_acidsizzle_loop_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
		else if (!flag2 && m_isPlayingAcidAudio)
		{
			m_isPlayingAcidAudio = false;
			AkSoundEngine.PostEvent("Stop_ENV_acidsizzle_loop_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
		RemoveGoopedPosition(m_removalPositions);
		for (int l = 0; l < m_dirtyFlags.GetLength(0); l++)
		{
			for (int m = 0; m < m_dirtyFlags.GetLength(1); m++)
			{
				if (m_dirtyFlags[l, m])
				{
					int num6 = (m * m_dirtyFlags.GetLength(0) + l) % 3;
					if (num6 == Time.frameCount % 3)
					{
						bool flag3 = HasGoopedPositionCountForChunk(l, m);
						if (flag3)
						{
							RebuildMeshUvsAndColors(l, m);
						}
						m_dirtyFlags[l, m] = false;
						m_colorDirtyFlags[l, m] = false;
						if (m_meshes[l, m] != null && !flag3)
						{
							UnityEngine.Object.Destroy(m_mrs[l, m].gameObject);
							UnityEngine.Object.Destroy(m_meshes[l, m]);
							m_mrs[l, m] = null;
							m_meshes[l, m] = null;
						}
					}
				}
				else
				{
					if (!m_colorDirtyFlags[l, m])
					{
						continue;
					}
					int num7 = (m * m_dirtyFlags.GetLength(0) + l) % 3;
					if (num7 == Time.frameCount % 3)
					{
						bool flag4 = HasGoopedPositionCountForChunk(l, m);
						if (flag4)
						{
							RebuildMeshColors(l, m);
						}
						m_colorDirtyFlags[l, m] = false;
						if (m_meshes[l, m] != null && !flag4)
						{
							UnityEngine.Object.Destroy(m_mrs[l, m].gameObject);
							UnityEngine.Object.Destroy(m_meshes[l, m]);
							m_mrs[l, m] = null;
							m_meshes[l, m] = null;
						}
					}
				}
			}
		}
	}

	private bool HasGoopedPositionCountForChunk(int chunkX, int chunkY)
	{
		int num = Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		IntVector2 intVector = new IntVector2(chunkX * num, chunkY * num);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if (m_goopedPositions.Contains(intVector + new IntVector2(i, j)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void RebuildMeshUvsAndColors(int chunkX, int chunkY)
	{
		Mesh chunkMesh = GetChunkMesh(chunkX, chunkY);
		for (int i = 0; i < m_colorArray.Length; i++)
		{
			m_colorArray[i] = new Color32(0, 0, 0, 0);
		}
		int num = chunkX * Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num2 = num + Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num3 = chunkY * Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num4 = num3 + Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		IntVector2 intVector = default(IntVector2);
		for (int j = num; j < num2; j++)
		{
			for (int k = num3; k < num4; k++)
			{
				intVector.x = j;
				intVector.y = k;
				GoopPositionData value;
				if (m_goopedCells.TryGetValue(intVector, out value) && value.remainingLifespan > 0f)
				{
					if (value.baseIndex < 0)
					{
						value.baseIndex = GetGoopBaseIndex(intVector, chunkX, chunkY);
					}
					AssignUvsAndColors(value, intVector, chunkX, chunkY);
				}
			}
		}
		chunkMesh.uv = m_uvArray;
		chunkMesh.uv2 = m_uv2Array;
		chunkMesh.colors32 = m_colorArray;
	}

	private void RebuildMeshColors(int chunkX, int chunkY)
	{
		Mesh chunkMesh = GetChunkMesh(chunkX, chunkY);
		for (int i = 0; i < m_colorArray.Length; i++)
		{
			m_colorArray[i] = new Color32(0, 0, 0, 0);
		}
		int num = chunkX * Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num2 = num + Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num3 = chunkY * Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		int num4 = num3 + Mathf.RoundToInt((float)CHUNK_SIZE / GOOP_GRID_SIZE);
		VertexColorRebuildResult b = VertexColorRebuildResult.ALL_OK;
		foreach (IntVector2 goopedPosition in m_goopedPositions)
		{
			GoopPositionData goopPositionData = m_goopedCells[goopedPosition];
			if (!(goopPositionData.remainingLifespan < 0f) && goopedPosition.x >= num && goopedPosition.x < num2 && goopedPosition.y >= num3 && goopedPosition.y < num4)
			{
				if (goopPositionData.baseIndex < 0)
				{
					goopPositionData.baseIndex = GetGoopBaseIndex(goopedPosition, chunkX, chunkY);
				}
				if (goopDefinition.CanBeFrozen)
				{
					int num5 = (goopPositionData.IsFrozen ? 1 : 0);
					m_uv2Array[goopPositionData.baseIndex] = new Vector2(num5, 0f);
					m_uv2Array[goopPositionData.baseIndex + 1] = new Vector2(num5, 0f);
					m_uv2Array[goopPositionData.baseIndex + 2] = new Vector2(num5, 0f);
					m_uv2Array[goopPositionData.baseIndex + 3] = new Vector2(num5, 0f);
				}
				VertexColorRebuildResult a = AssignVertexColors(goopPositionData, goopedPosition, chunkX, chunkY);
				b = (VertexColorRebuildResult)Mathf.Max((int)a, (int)b);
			}
		}
		if (goopDefinition.CanBeFrozen)
		{
			chunkMesh.uv2 = m_uv2Array;
		}
		chunkMesh.colors32 = m_colorArray;
	}

	private void PostprocessRebuildResult(int chunkX, int chunkY, VertexColorRebuildResult rr)
	{
		MeshRenderer meshRenderer = m_mrs[chunkX, chunkY];
		Material sharedMaterial = meshRenderer.sharedMaterial;
		switch (rr)
		{
		case VertexColorRebuildResult.ALL_OK:
		{
			float value = ((!goopDefinition.usesOverrideOpaqueness) ? 0.5f : goopDefinition.overrideOpaqueness);
			sharedMaterial.SetFloat(OpaquenessMultiplyPropertyID, value);
			sharedMaterial.SetFloat(BrightnessMultiplyPropertyID, 1f);
			break;
		}
		case VertexColorRebuildResult.ELECTRIFIED:
			sharedMaterial.SetFloat(OpaquenessMultiplyPropertyID, 1f);
			sharedMaterial.SetFloat(BrightnessMultiplyPropertyID, 5f);
			break;
		}
	}

	private void InitMesh(IntVector2 goopPos, int chunkX, int chunkY)
	{
		int num = chunkX * CHUNK_SIZE;
		int num2 = chunkY * CHUNK_SIZE;
		int num3 = (int)((float)num / GOOP_GRID_SIZE + 0.5f);
		int num4 = (int)((float)num2 / GOOP_GRID_SIZE + 0.5f);
		int num5 = goopPos.x - num3;
		int num6 = goopPos.y - num4;
		int num7 = num6 * (4 * (int)(1f / GOOP_GRID_SIZE)) * CHUNK_SIZE + num5 * 4;
		bool flag = false;
		IntVector2 intVector = new IntVector2((int)((float)goopPos.x * GOOP_GRID_SIZE), (int)((float)goopPos.y * GOOP_GRID_SIZE));
		if (GameManager.Instance.Dungeon.data.CheckInBounds(intVector))
		{
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			flag = cellData != null && !cellData.forceDisallowGoop && cellData.IsLowerFaceWall();
		}
		if (flag)
		{
			float num8 = (float)goopPos.x * GOOP_GRID_SIZE;
			float num9 = (float)goopPos.y * GOOP_GRID_SIZE;
			float num10 = Mathf.Floor(num9) - num9 % 1f;
			Vector3 vector = new Vector3(num8 - (float)num, num9 - (float)num2, num10 - (float)num2);
			m_vertexArray[num7] = vector;
			m_vertexArray[num7 + 1] = vector + new Vector3(GOOP_GRID_SIZE, 0f, 0f);
			m_vertexArray[num7 + 2] = vector + new Vector3(0f, GOOP_GRID_SIZE, 0f - GOOP_GRID_SIZE);
			m_vertexArray[num7 + 3] = vector + new Vector3(GOOP_GRID_SIZE, GOOP_GRID_SIZE, 0f - GOOP_GRID_SIZE);
		}
		else
		{
			Vector3 vector2 = new Vector3((float)goopPos.x * GOOP_GRID_SIZE - (float)num, (float)goopPos.y * GOOP_GRID_SIZE - (float)num2, (float)goopPos.y * GOOP_GRID_SIZE - (float)num2);
			m_vertexArray[num7] = vector2;
			m_vertexArray[num7 + 1] = vector2 + new Vector3(GOOP_GRID_SIZE, 0f, 0f);
			m_vertexArray[num7 + 2] = vector2 + new Vector3(0f, GOOP_GRID_SIZE, GOOP_GRID_SIZE);
			m_vertexArray[num7 + 3] = vector2 + new Vector3(GOOP_GRID_SIZE, GOOP_GRID_SIZE, GOOP_GRID_SIZE);
		}
		int num11 = num7 / 4 * 6;
		m_triangleArray[num11] = num7;
		m_triangleArray[num11 + 1] = num7 + 1;
		m_triangleArray[num11 + 2] = num7 + 2;
		m_triangleArray[num11 + 3] = num7 + 3;
		m_triangleArray[num11 + 4] = num7 + 2;
		m_triangleArray[num11 + 5] = num7 + 1;
	}

	private int GetGoopBaseIndex(IntVector2 goopPos, int chunkX, int chunkY)
	{
		int num = chunkX * CHUNK_SIZE;
		int num2 = chunkY * CHUNK_SIZE;
		int num3 = (int)((float)num / GOOP_GRID_SIZE + 0.5f);
		int num4 = (int)((float)num2 / GOOP_GRID_SIZE + 0.5f);
		int num5 = goopPos.x - num3;
		int num6 = goopPos.y - num4;
		return num6 * (4 * (int)(1f / GOOP_GRID_SIZE)) * CHUNK_SIZE + num5 * 4;
	}

	private void AssignUvsAndColors(GoopPositionData goopData, IntVector2 goopPos, int chunkX, int chunkY)
	{
		Vector2 vector = (m_uvMap.ContainsKey(goopData.NeighborsAsInt) ? m_uvMap[goopData.NeighborsAsInt] : ((!m_uvMap.ContainsKey(goopData.NeighborsAsIntFuckDiagonals)) ? m_uvMap[-1] : m_uvMap[goopData.NeighborsAsIntFuckDiagonals]));
		if (goopData.NeighborsAsInt == 255)
		{
			vector = m_centerUVOptions[Mathf.FloorToInt((float)m_centerUVOptions.Count * goopPos.GetHashedRandomValue())];
		}
		m_uvArray[goopData.baseIndex] = vector;
		m_uvArray[goopData.baseIndex + 1] = vector + new Vector2(0.125f, 0f);
		m_uvArray[goopData.baseIndex + 2] = vector + new Vector2(0f, 0.125f);
		m_uvArray[goopData.baseIndex + 3] = vector + new Vector2(0.125f, 0.125f);
		if (goopDefinition.CanBeFrozen)
		{
			int num = (goopData.IsFrozen ? 1 : 0);
			m_uv2Array[goopData.baseIndex] = new Vector2(num, 0f);
			m_uv2Array[goopData.baseIndex + 1] = new Vector2(num, 0f);
			m_uv2Array[goopData.baseIndex + 2] = new Vector2(num, 0f);
			m_uv2Array[goopData.baseIndex + 3] = new Vector2(num, 0f);
		}
		AssignVertexColors(goopData, goopPos, chunkX, chunkY);
	}

	private VertexColorRebuildResult AssignVertexColors(GoopPositionData goopData, IntVector2 goopPos, int chunkX, int chunkY)
	{
		VertexColorRebuildResult result = VertexColorRebuildResult.ALL_OK;
		bool flag = false;
		Color32 color = goopDefinition.baseColor32;
		Color32 color2 = color;
		Color32 color3 = color;
		Color32 color4 = color;
		if (goopData.IsOnFire)
		{
			color = goopDefinition.fireColor32;
		}
		else if (goopData.HasOnFireNeighbor)
		{
			flag = true;
			for (int i = 0; i < 8; i++)
			{
				if (goopData.neighborGoopData[i] != null && goopData.neighborGoopData[i].IsOnFire)
				{
					switch (i)
					{
					case 0:
						color3 = goopDefinition.igniteColor32;
						color4 = goopDefinition.igniteColor32;
						break;
					case 1:
						color4 = goopDefinition.igniteColor32;
						break;
					case 2:
						color4 = goopDefinition.igniteColor32;
						color2 = goopDefinition.igniteColor32;
						break;
					case 3:
						color2 = goopDefinition.igniteColor32;
						break;
					case 4:
						color2 = goopDefinition.igniteColor32;
						color = goopDefinition.igniteColor32;
						break;
					case 5:
						color = goopDefinition.igniteColor32;
						break;
					case 6:
						color = goopDefinition.igniteColor32;
						color3 = goopDefinition.igniteColor32;
						break;
					case 7:
						color3 = goopDefinition.igniteColor32;
						break;
					}
				}
			}
		}
		else if (goopData.IsFrozen)
		{
			color = goopDefinition.frozenColor32;
		}
		else if (goopData.HasFrozenNeighbor)
		{
			flag = true;
			for (int j = 0; j < 8; j++)
			{
				if (goopData.neighborGoopData[j] != null && goopData.neighborGoopData[j].IsFrozen)
				{
					switch (j)
					{
					case 0:
						m_uv2Array[goopData.baseIndex + 2] = new Vector2(0.5f, 0f);
						color3 = goopDefinition.prefreezeColor32;
						m_uv2Array[goopData.baseIndex + 3] = new Vector2(0.5f, 0f);
						color4 = goopDefinition.prefreezeColor32;
						break;
					case 1:
						m_uv2Array[goopData.baseIndex + 3] = new Vector2(0.5f, 0f);
						color4 = goopDefinition.prefreezeColor32;
						break;
					case 2:
						m_uv2Array[goopData.baseIndex + 3] = new Vector2(0.5f, 0f);
						color4 = goopDefinition.prefreezeColor32;
						m_uv2Array[goopData.baseIndex + 1] = new Vector2(0.5f, 0f);
						color2 = goopDefinition.prefreezeColor32;
						break;
					case 3:
						m_uv2Array[goopData.baseIndex + 1] = new Vector2(0.5f, 0f);
						color2 = goopDefinition.prefreezeColor32;
						break;
					case 4:
						m_uv2Array[goopData.baseIndex + 1] = new Vector2(0.5f, 0f);
						color2 = goopDefinition.prefreezeColor32;
						m_uv2Array[goopData.baseIndex] = new Vector2(0.5f, 0f);
						color = goopDefinition.prefreezeColor32;
						break;
					case 5:
						m_uv2Array[goopData.baseIndex] = new Vector2(0.5f, 0f);
						color = goopDefinition.prefreezeColor32;
						break;
					case 6:
						m_uv2Array[goopData.baseIndex] = new Vector2(0.5f, 0f);
						color = goopDefinition.prefreezeColor32;
						m_uv2Array[goopData.baseIndex + 2] = new Vector2(0.5f, 0f);
						color3 = goopDefinition.prefreezeColor32;
						break;
					case 7:
						m_uv2Array[goopData.baseIndex + 2] = new Vector2(0.5f, 0f);
						color3 = goopDefinition.prefreezeColor32;
						break;
					}
				}
			}
		}
		if (goopData.remainingLifespan < goopDefinition.fadePeriod)
		{
			color = Color32.Lerp(goopDefinition.fadeColor32, color, goopData.remainingLifespan / goopDefinition.fadePeriod);
			if (flag)
			{
				color2 = Color32.Lerp(goopDefinition.fadeColor32, color2, goopData.remainingLifespan / goopDefinition.fadePeriod);
				color3 = Color32.Lerp(goopDefinition.fadeColor32, color3, goopData.remainingLifespan / goopDefinition.fadePeriod);
				color4 = Color32.Lerp(goopDefinition.fadeColor32, color4, goopData.remainingLifespan / goopDefinition.fadePeriod);
			}
		}
		if (flag)
		{
			m_colorArray[goopData.baseIndex] = color;
			m_colorArray[goopData.baseIndex + 1] = color2;
			m_colorArray[goopData.baseIndex + 2] = color3;
			m_colorArray[goopData.baseIndex + 3] = color4;
		}
		else
		{
			m_colorArray[goopData.baseIndex] = color;
			m_colorArray[goopData.baseIndex + 1] = color;
			m_colorArray[goopData.baseIndex + 2] = color;
			m_colorArray[goopData.baseIndex + 3] = color;
		}
		return result;
	}

	private void RemoveGoopedPosition(IntVector2 entry)
	{
		IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
		for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
		{
			IntVector2 key = entry + cardinalsAndOrdinals[i];
			GoopPositionData value;
			if (m_goopedCells.TryGetValue(key, out value))
			{
				value.neighborGoopData[(i + 4) % 8] = null;
				value.SetNeighborGoop((i + 4) % 8, false);
			}
		}
		m_goopedPositions.Remove(entry);
		m_goopedCells.Remove(entry);
		allGoopPositionMap.Remove(entry);
		SetDirty(entry);
	}

	private void RemoveGoopedPosition(List<IntVector2> entries)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			IntVector2 entry = entries[i];
			RemoveGoopedPosition(entry);
		}
	}

	public int CountGoopCircle(Vector2 center, float radius)
	{
		if (m_goopedCells == null || m_goopedCells.Count == 0)
		{
			return 0;
		}
		int num = Mathf.FloorToInt((center.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.CeilToInt((center.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.FloorToInt((center.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.CeilToInt((center.y + radius) / GOOP_GRID_SIZE);
		Vector2 b = center / GOOP_GRID_SIZE;
		float num5 = radius / GOOP_GRID_SIZE;
		int num6 = 0;
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				IntVector2 key = new IntVector2(i, j);
				if (Vector2.Distance(key.ToVector2(), b) < num5 && m_goopedCells.ContainsKey(key))
				{
					num6++;
				}
			}
		}
		return num6;
	}

	public void RemoveGoopCircle(Vector2 center, float radius)
	{
		int num = Mathf.FloorToInt((center.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.CeilToInt((center.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.FloorToInt((center.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.CeilToInt((center.y + radius) / GOOP_GRID_SIZE);
		Vector2 b = center / GOOP_GRID_SIZE;
		float num5 = radius / GOOP_GRID_SIZE;
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				if (Vector2.Distance(new Vector2(i, j), b) < num5)
				{
					RemoveGoopedPosition(new IntVector2(i, j));
				}
			}
		}
	}

	private void AddGoopedPosition(IntVector2 pos, float radiusFraction = 0f, bool suppressSplashes = false, int sourceId = -1, int sourceFrameCount = -1)
	{
		if (GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		Vector2 vector = pos.ToVector2() * GOOP_GRID_SIZE;
		Vector2 vector2 = vector + new Vector2(GOOP_GRID_SIZE, GOOP_GRID_SIZE) * 0.5f;
		for (int i = 0; i < m_goopExceptions.Count; i++)
		{
			if (m_goopExceptions[i] != null)
			{
				Vector2 first = m_goopExceptions[i].First;
				float second = m_goopExceptions[i].Second;
				if ((first - vector2).sqrMagnitude < second)
				{
					return;
				}
			}
		}
		if (!m_goopedCells.ContainsKey(pos))
		{
			IntVector2 intVector = vector.ToIntVector2(VectorConversions.Floor);
			if (!GameManager.Instance.Dungeon.data.CheckInBounds(intVector))
			{
				return;
			}
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			if (cellData == null || (cellData != null && cellData.forceDisallowGoop) || (cellData.cellVisualData.absorbsDebris && goopDefinition.CanBeFrozen))
			{
				return;
			}
			if (goopDefinition.CanBeFrozen)
			{
				GameManager.Instance.Dungeon.data.SolidifyLavaInCell(intVector);
			}
			bool flag = cellData.IsLowerFaceWall();
			if (flag && pos.GetHashedRandomValue() > 0.75f)
			{
				flag = false;
			}
			if (cellData.type != CellType.FLOOR && !flag && !cellData.forceAllowGoop)
			{
				return;
			}
			bool flag2 = false;
			int num = ((sourceFrameCount == -1) ? Time.frameCount : sourceFrameCount);
			DeadlyDeadlyGoopManager value;
			if (allGoopPositionMap.TryGetValue(pos, out value))
			{
				GoopPositionData goopPositionData = value.m_goopedCells[pos];
				if (goopPositionData.frameGooped > num || goopPositionData.eternal)
				{
					return;
				}
				if (goopPositionData.IsOnFire)
				{
					flag2 = true;
				}
				value.RemoveGoopedPosition(pos);
			}
			GoopPositionData goopPositionData2 = new GoopPositionData(pos, m_goopedCells, goopDefinition.GetLifespan(radiusFraction));
			goopPositionData2.frameGooped = ((sourceFrameCount == -1) ? Time.frameCount : sourceFrameCount);
			goopPositionData2.lastSourceID = sourceId;
			if (!suppressSplashes && m_DoGoopSpawnSplashes && UnityEngine.Random.value < 0.02f)
			{
				if (m_genericSplashPrefab == null)
				{
					m_genericSplashPrefab = ResourceCache.Acquire("Global VFX/Generic_Goop_Splash") as GameObject;
				}
				GameObject gameObject = SpawnManager.SpawnVFX(m_genericSplashPrefab, vector.ToVector3ZUp(vector.y), Quaternion.identity);
				gameObject.GetComponent<tk2dBaseSprite>().usesOverrideMaterial = true;
				gameObject.GetComponent<Renderer>().material.SetColor(TintColorPropertyID, goopDefinition.baseColor32);
			}
			goopPositionData2.eternal = goopDefinition.eternal;
			goopPositionData2.selfIgnites = goopDefinition.SelfIgnites;
			goopPositionData2.remainingTimeTilSelfIgnition = goopDefinition.selfIgniteDelay;
			m_goopedPositions.Add(pos);
			m_goopedCells.Add(pos, goopPositionData2);
			allGoopPositionMap.Add(pos, this);
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector);
			absoluteRoomFromPosition.RegisterGoopManagerInRoom(this);
			if (cellData.OnCellGooped != null)
			{
				cellData.OnCellGooped(cellData);
			}
			if (cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Ice)
			{
				FreezeCell(pos);
			}
			if (flag2 && goopDefinition.CanBeIgnited)
			{
				IgniteCell(pos);
			}
			SetDirty(pos);
			return;
		}
		if (m_goopedCells[pos].remainingLifespan < goopDefinition.fadePeriod)
		{
			SetDirty(pos);
		}
		if (m_goopedCells[pos].IsOnFire && goopDefinition.ignitionChangesLifetime)
		{
			if (m_goopedCells[pos].remainingLifespan > 0f)
			{
				m_goopedCells[pos].remainingLifespan = goopDefinition.ignitedLifetime;
			}
		}
		else
		{
			if (!suppressSplashes && m_DoGoopSpawnSplashes && (m_goopedCells[pos].lastSourceID < 0 || m_goopedCells[pos].lastSourceID != sourceId) && UnityEngine.Random.value < 0.001f)
			{
				if (m_genericSplashPrefab == null)
				{
					m_genericSplashPrefab = ResourceCache.Acquire("Global VFX/Generic_Goop_Splash") as GameObject;
				}
				GameObject gameObject2 = SpawnManager.SpawnVFX(m_genericSplashPrefab, vector.ToVector3ZUp(vector.y), Quaternion.identity);
				gameObject2.GetComponent<tk2dBaseSprite>().usesOverrideMaterial = true;
				gameObject2.GetComponent<Renderer>().material.SetColor(TintColorPropertyID, goopDefinition.baseColor32);
			}
			m_goopedCells[pos].remainingLifespan = Mathf.Max(m_goopedCells[pos].remainingLifespan, goopDefinition.GetLifespan(radiusFraction));
			m_goopedCells[pos].lifespanOverridden = true;
			m_goopedCells[pos].HasPlayedFireOutro = false;
			m_goopedCells[pos].hasBeenFrozen = 0;
		}
		m_goopedCells[pos].lastSourceID = sourceId;
	}

	public void TimedAddGoopArc(Vector2 origin, float radius, float arcDegrees, Vector2 direction, float duration = 0.5f, AnimationCurve goopCurve = null)
	{
		StartCoroutine(TimedAddGoopArc_CR(origin, radius, arcDegrees, direction, duration, goopCurve));
	}

	private IEnumerator TimedAddGoopArc_CR(Vector2 origin, float radius, float arcDegrees, Vector2 direction, float duration, AnimationCurve goopCurve)
	{
		float elapsed = 0f;
		float m_lastRadius = 0f;
		int sourceFrameCount = Time.frameCount;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			if (goopCurve != null)
			{
				t = Mathf.Clamp01(goopCurve.Evaluate(t));
			}
			float currentRadius = Mathf.Lerp(0.5f, radius, t).Quantize(GOOP_GRID_SIZE);
			if (m_lastRadius != currentRadius)
			{
				m_lastRadius = currentRadius;
				float num = -1f * (arcDegrees / 2f);
				float f = 2f * currentRadius * (float)Math.PI * (arcDegrees / 360f);
				int num2 = Mathf.CeilToInt(f);
				for (int i = 0; i < num2 + 1; i++)
				{
					float z = num + arcDegrees * (float)i / (float)num2;
					Vector2 center = origin + (Quaternion.Euler(0f, 0f, z) * (direction * currentRadius).ToVector3ZUp()).XY();
					AddGoopCircle(center, 0.5f, -1, false, sourceFrameCount);
				}
			}
			yield return null;
		}
	}

	public void TimedAddGoopCircle(Vector2 center, float radius, float duration = 0.5f, bool suppressSplashes = false)
	{
		StartCoroutine(TimedAddGoopCircle_CR(center, radius, duration, suppressSplashes));
	}

	private IEnumerator TimedAddGoopCircle_CR(Vector2 center, float radius, float duration, bool suppressSplashes = false)
	{
		float elapsed = 0f;
		float m_lastRadius = 0f;
		int sourceID = UnityEngine.Random.Range(1, 1000);
		int sourceFrameCount = Time.frameCount;
		float previousRadius = 0f;
		while (elapsed < duration && !GameManager.Instance.IsLoadingLevel)
		{
			elapsed += BraveTime.DeltaTime;
			float t2 = elapsed / duration;
			t2 = 1f - (t2 - 1f) * (t2 - 1f);
			float currentRadius = Mathf.Lerp(0.5f, radius, t2).Quantize(GOOP_GRID_SIZE);
			if (m_lastRadius != currentRadius)
			{
				m_lastRadius = currentRadius;
				AddGoopRing(center, previousRadius, currentRadius, sourceID, suppressSplashes, sourceFrameCount);
			}
			previousRadius = Mathf.Max(0f, currentRadius - 1f);
			yield return null;
		}
	}

	public void AddGoopCircle(Vector2 center, float radius, int sourceID = -1, bool suppressSplashes = false, int sourceFrameCount = -1)
	{
		int num = Mathf.FloorToInt((center.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.CeilToInt((center.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.FloorToInt((center.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.CeilToInt((center.y + radius) / GOOP_GRID_SIZE);
		Vector2 b = center / GOOP_GRID_SIZE;
		float num5 = radius / GOOP_GRID_SIZE;
		suppressSplashes = suppressSplashes || radius < 1f;
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				IntVector2 pos = new IntVector2(i, j);
				float num6 = Vector2.Distance(new Vector2(i, j), b);
				if (num6 < num5)
				{
					float t = num6 / num5;
					if (num6 < GOOP_GRID_SIZE * 2f)
					{
						t = 0f;
					}
					t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
					AddGoopedPosition(pos, t, suppressSplashes, sourceID, sourceFrameCount);
				}
			}
		}
	}

	public void AddGoopRing(Vector2 center, float minRadius, float maxRadius, int sourceID = -1, bool suppressSplashes = false, int sourceFrameCount = -1)
	{
		int num = Mathf.FloorToInt((center.x - maxRadius) / GOOP_GRID_SIZE);
		int num2 = Mathf.CeilToInt((center.x + maxRadius) / GOOP_GRID_SIZE);
		int num3 = Mathf.FloorToInt((center.y - maxRadius) / GOOP_GRID_SIZE);
		int num4 = Mathf.CeilToInt((center.y + maxRadius) / GOOP_GRID_SIZE);
		Vector2 b = center / GOOP_GRID_SIZE;
		float num5 = minRadius / GOOP_GRID_SIZE;
		float num6 = maxRadius / GOOP_GRID_SIZE;
		suppressSplashes = suppressSplashes || num6 < 1f;
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				IntVector2 pos = new IntVector2(i, j);
				float num7 = Vector2.Distance(new Vector2(i, j), b);
				if (num7 >= num5 && num7 <= num6)
				{
					float t = num7 / num6;
					if (num7 < GOOP_GRID_SIZE * 2f)
					{
						t = 0f;
					}
					t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
					AddGoopedPosition(pos, t, suppressSplashes, sourceID, sourceFrameCount);
				}
			}
		}
	}

	public void TimedAddGoopLine(Vector2 p1, Vector2 p2, float radius, float duration)
	{
		StartCoroutine(TimedAddGoopLine_CR(p1, p2, radius, duration));
	}

	private IEnumerator TimedAddGoopLine_CR(Vector2 p1, Vector2 p2, float radius, float duration)
	{
		float elapsed = 0f;
		Vector2 lastEnd = p1;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			Vector2 currentEnd = Vector2.Lerp(p1, p2, elapsed / duration);
			float curDist = Vector2.Distance(currentEnd, lastEnd);
			int steps = Mathf.CeilToInt(curDist / radius);
			for (int i = 0; i < steps; i++)
			{
				Vector2 center = lastEnd + (currentEnd - lastEnd) * (((float)i + 1f) / (float)steps);
				TimedAddGoopCircle(center, radius);
			}
			lastEnd = currentEnd;
			yield return null;
		}
	}

	public void AddGoopLine(Vector2 p1, Vector2 p2, float radius)
	{
		Vector2 vector = Vector2.Min(p1, p2);
		Vector2 vector2 = Vector2.Max(p1, p2);
		int num = Mathf.FloorToInt((vector.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.CeilToInt((vector2.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.FloorToInt((vector.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.CeilToInt((vector2.y + radius) / GOOP_GRID_SIZE);
		Vector2 vector3 = p1 / GOOP_GRID_SIZE;
		Vector2 vector4 = p2 / GOOP_GRID_SIZE;
		float num5 = radius / GOOP_GRID_SIZE;
		float num6 = num5 * num5;
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				IntVector2 pos = new IntVector2(i, j);
				float num7 = (float)pos.x - vector3.x;
				float num8 = (float)pos.y - vector3.y;
				float num9 = vector4.x - vector3.x;
				float num10 = vector4.y - vector3.y;
				float num11 = num7 * num9 + num8 * num10;
				float num12 = num9 * num9 + num10 * num10;
				float num13 = -1f;
				if (num12 != 0f)
				{
					num13 = num11 / num12;
				}
				float num14;
				float num15;
				if (num13 < 0f)
				{
					num14 = vector3.x;
					num15 = vector3.y;
				}
				else if (num13 > 1f)
				{
					num14 = vector4.x;
					num15 = vector4.y;
				}
				else
				{
					num14 = vector3.x + num13 * num9;
					num15 = vector3.y + num13 * num10;
				}
				float x = (float)pos.x - num14;
				float y = (float)pos.y - num15;
				float sqrMagnitude = new Vector2(x, y).sqrMagnitude;
				if (sqrMagnitude < num6)
				{
					float num16 = Mathf.Sqrt(sqrMagnitude);
					float t = num16 / num5;
					if (num16 < GOOP_GRID_SIZE * 2f)
					{
						t = 0f;
					}
					t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
					AddGoopedPosition(pos, t);
				}
			}
		}
	}

	public void TimedAddGoopRect(Vector2 min, Vector2 max, float duration)
	{
		StartCoroutine(TimedAddGoopRect_CR(min, max, duration));
	}

	public IEnumerator TimedAddGoopRect_CR(Vector2 min, Vector2 max, float duration)
	{
		float elapsed = 0f;
		float lastT = 0f;
		while (elapsed < duration && !GameManager.Instance.IsLoadingLevel)
		{
			elapsed += BraveTime.DeltaTime;
			float t2 = elapsed / duration;
			t2 = 1f - (t2 - 1f) * (t2 - 1f);
			int minGoopX = Mathf.FloorToInt(min.x / GOOP_GRID_SIZE);
			float maxGoopX = Mathf.Lerp(minGoopX, Mathf.CeilToInt(max.x / GOOP_GRID_SIZE), t2);
			int minGoopY = Mathf.FloorToInt(min.y / GOOP_GRID_SIZE);
			float maxGoopY = Mathf.Lerp(minGoopY, Mathf.CeilToInt(max.y / GOOP_GRID_SIZE), t2);
			float lastMaxX = Mathf.Lerp(minGoopX, Mathf.CeilToInt(max.x / GOOP_GRID_SIZE), lastT);
			float lastMaxY = Mathf.Lerp(minGoopY, Mathf.CeilToInt(max.y / GOOP_GRID_SIZE), lastT);
			for (int i = minGoopX; (float)i < maxGoopX; i++)
			{
				for (int j = minGoopY; (float)j < maxGoopY; j++)
				{
					if ((float)i > lastMaxX || (float)j > lastMaxY)
					{
						IntVector2 pos = new IntVector2(i, j);
						AddGoopedPosition(pos);
					}
				}
			}
			lastT = t2;
			yield return null;
		}
	}

	public void AddGoopRect(Vector2 min, Vector2 max)
	{
		int num = Mathf.FloorToInt(min.x / GOOP_GRID_SIZE);
		int num2 = Mathf.CeilToInt(max.x / GOOP_GRID_SIZE);
		int num3 = Mathf.FloorToInt(min.y / GOOP_GRID_SIZE);
		int num4 = Mathf.CeilToInt(max.y / GOOP_GRID_SIZE);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				IntVector2 pos = new IntVector2(i, j);
				AddGoopedPosition(pos);
			}
		}
	}

	public void AddGoopPoints(List<Vector2> points, float radius, Vector2 excludeCenter, float excludeRadius)
	{
		Vector2 lhs = Vector2Extensions.max;
		Vector2 lhs2 = Vector2Extensions.min;
		for (int i = 0; i < points.Count; i++)
		{
			lhs = Vector2.Min(lhs, points[i]);
			lhs2 = Vector2.Max(lhs2, points[i]);
		}
		int num = Mathf.FloorToInt((lhs.x - radius) / GOOP_GRID_SIZE);
		int num2 = Mathf.CeilToInt((lhs2.x + radius) / GOOP_GRID_SIZE);
		int num3 = Mathf.FloorToInt((lhs.y - radius) / GOOP_GRID_SIZE);
		int num4 = Mathf.CeilToInt((lhs2.y + radius) / GOOP_GRID_SIZE);
		int num5 = num2 - num + 1;
		int num6 = num4 - num3 + 1;
		int radius2 = Mathf.RoundToInt(radius / GOOP_GRID_SIZE);
		s_goopPointRadius = radius / GOOP_GRID_SIZE;
		s_goopPointRadiusSquare = s_goopPointRadius * s_goopPointRadius;
		m_pointsArray.ReinitializeWithDefault(num5, num6, false, 1f);
		for (int j = 0; j < points.Count; j++)
		{
			s_goopPointCenter.x = (int)(points[j].x / GOOP_GRID_SIZE) - num;
			s_goopPointCenter.y = (int)(points[j].y / GOOP_GRID_SIZE) - num3;
			m_pointsArray.SetCircle(s_goopPointCenter.x, s_goopPointCenter.y, radius2, true, GetRadiusFraction);
		}
		int x = (int)(excludeCenter.x / GOOP_GRID_SIZE) - num;
		int y = (int)(excludeCenter.y / GOOP_GRID_SIZE) - num3;
		int radius3 = Mathf.RoundToInt(excludeRadius / GOOP_GRID_SIZE);
		m_pointsArray.SetCircle(x, y, radius3, false, GetRadiusFraction);
		for (int k = 0; k < num5; k++)
		{
			for (int l = 0; l < num6; l++)
			{
				if (m_pointsArray[k, l])
				{
					AddGoopedPosition(new IntVector2(num + k, num3 + l), m_pointsArray.GetFloat(k, l));
				}
			}
		}
	}

	private static float GetRadiusFraction(int x, int y, bool value, float currentFloatValue)
	{
		if (!value)
		{
			return currentFloatValue;
		}
		float num = s_goopPointCenter.x - x;
		float num2 = s_goopPointCenter.y - y;
		float num3 = num * num + num2 * num2;
		if (num3 < s_goopPointRadiusSquare)
		{
			float num4 = Mathf.Sqrt(num3);
			float t = num4 / s_goopPointRadius;
			if (num4 < 0.5f)
			{
				t = 0f;
			}
			t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
			return Mathf.Min(t, currentFloatValue);
		}
		return currentFloatValue;
	}
}
