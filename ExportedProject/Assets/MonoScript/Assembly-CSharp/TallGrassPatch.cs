using System;
using System.Collections.Generic;
using UnityEngine;

public class TallGrassPatch : MonoBehaviour
{
	internal struct EnflamedGrassData
	{
		public float fireTime;

		public bool hasEnflamedNeighbors;

		public bool HasPlayedFireOutro;

		public bool HasPlayedFireIntro;

		public float ParticleTimer;
	}

	[NonSerialized]
	public List<IntVector2> cells;

	private const int INDEX_TOP = 124;

	private const int INDEX_MIDDLE = 147;

	private const int INDEX_MIDDLE_BOTTOM = 146;

	private const int INDEX_BOTTOM = 168;

	private Dictionary<IntVector2, EnflamedGrassData> m_fireData = new Dictionary<IntVector2, EnflamedGrassData>(new IntVector2EqualityComparer());

	private ParticleSystem m_fireSystem;

	private ParticleSystem m_fireIntroSystem;

	private ParticleSystem m_fireOutroSystem;

	private List<tk2dTiledSprite> m_tiledSpritePool = new List<tk2dTiledSprite>();

	private bool m_isPlayingFireAudio;

	private GameObject m_stripPrefab;

	private void InitializeParticleSystem()
	{
		if (!(m_fireSystem != null))
		{
			GameObject gameObject = GameObject.Find("Gungeon_Fire_Main");
			if (gameObject == null)
			{
				gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Particles/Gungeon_Fire_Main_raw"), Vector3.zero, Quaternion.identity);
				gameObject.name = "Gungeon_Fire_Main";
			}
			m_fireSystem = gameObject.GetComponent<ParticleSystem>();
			GameObject gameObject2 = GameObject.Find("Gungeon_Fire_Intro");
			if (gameObject2 == null)
			{
				gameObject2 = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Particles/Gungeon_Fire_Intro_raw"), Vector3.zero, Quaternion.identity);
				gameObject2.name = "Gungeon_Fire_Intro";
			}
			m_fireIntroSystem = gameObject2.GetComponent<ParticleSystem>();
			GameObject gameObject3 = GameObject.Find("Gungeon_Fire_Outro");
			if (gameObject3 == null)
			{
				gameObject3 = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Particles/Gungeon_Fire_Outro_raw"), Vector3.zero, Quaternion.identity);
				gameObject3.name = "Gungeon_Fire_Outro";
			}
			m_fireOutroSystem = gameObject3.GetComponent<ParticleSystem>();
		}
	}

	private int GetTargetIndexForPosition(IntVector2 current)
	{
		bool flag = cells.Contains(current + IntVector2.North);
		bool flag2 = cells.Contains(current + IntVector2.South);
		bool flag3 = cells.Contains(current + IntVector2.South + IntVector2.South);
		int num = -1;
		if (flag && flag2 && flag3)
		{
			return 147;
		}
		if (flag && flag2)
		{
			return 146;
		}
		if (flag && !flag2)
		{
			return 168;
		}
		if (!flag && flag2)
		{
			return 124;
		}
		return 168;
	}

	public void IgniteCircle(Vector2 center, float radius)
	{
		for (int i = Mathf.FloorToInt(center.x - radius); i < Mathf.CeilToInt(center.x + radius); i++)
		{
			for (int j = Mathf.FloorToInt(center.y - radius); j < Mathf.CeilToInt(center.y + radius); j++)
			{
				if (Vector2.Distance(new Vector2(i, j), center) < radius)
				{
					IgniteCell(new IntVector2(i, j));
				}
			}
		}
	}

	public void IgniteCell(IntVector2 cellPosition)
	{
		if (cells.Contains(cellPosition) && !m_fireData.ContainsKey(cellPosition))
		{
			m_fireData.Add(cellPosition, default(EnflamedGrassData));
		}
	}

	private EnflamedGrassData DoParticleAtPosition(IntVector2 worldPos, EnflamedGrassData fireData)
	{
		if (m_fireSystem != null && fireData.ParticleTimer <= 0f)
		{
			bool flag = cells.Contains(worldPos + IntVector2.South);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					if (!flag && j == 0)
					{
						continue;
					}
					float num = UnityEngine.Random.Range(1f, 1.5f);
					float num2 = UnityEngine.Random.Range(0.75f, 1f);
					Vector2 vector = worldPos.ToVector3() + new Vector3(0.33f + 0.33f * (float)i, 0.33f + 0.33f * (float)j, 0f);
					vector += UnityEngine.Random.insideUnitCircle / 5f;
					if (!fireData.HasPlayedFireOutro)
					{
						if (!fireData.HasPlayedFireOutro && fireData.fireTime > 3f && m_fireOutroSystem != null)
						{
							num = num2;
							ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
							emitParams.position = vector;
							emitParams.velocity = Vector3.zero;
							emitParams.startSize = m_fireSystem.startSize;
							emitParams.startLifetime = num2;
							emitParams.startColor = m_fireSystem.startColor;
							emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
							ParticleSystem.EmitParams emitParams2 = emitParams;
							m_fireOutroSystem.Emit(emitParams2, 1);
							if (i == 1 && j == 1)
							{
								fireData.HasPlayedFireOutro = true;
							}
						}
						else if (!fireData.HasPlayedFireIntro && m_fireIntroSystem != null)
						{
							num = UnityEngine.Random.Range(0.75f, 1f);
							ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
							emitParams.position = vector;
							emitParams.velocity = Vector3.zero;
							emitParams.startSize = m_fireSystem.startSize;
							emitParams.startLifetime = num;
							emitParams.startColor = m_fireSystem.startColor;
							emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
							ParticleSystem.EmitParams emitParams3 = emitParams;
							m_fireIntroSystem.Emit(emitParams3, 1);
							if (i == 1 && j == 1)
							{
								fireData.HasPlayedFireIntro = true;
							}
						}
						else if (UnityEngine.Random.value < 0.5f)
						{
							ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
							emitParams.position = vector;
							emitParams.velocity = Vector3.zero;
							emitParams.startSize = m_fireSystem.startSize;
							emitParams.startLifetime = num;
							emitParams.startColor = m_fireSystem.startColor;
							emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
							ParticleSystem.EmitParams emitParams4 = emitParams;
							m_fireSystem.Emit(emitParams4, 1);
						}
					}
					if (i == 1 && j == 1)
					{
						fireData.ParticleTimer = num - 0.125f;
					}
				}
			}
		}
		return fireData;
	}

	private void LateUpdate()
	{
		bool flag = false;
		for (int i = 0; i < cells.Count; i++)
		{
			if (m_fireData.ContainsKey(cells[i]))
			{
				EnflamedGrassData fireData = m_fireData[cells[i]];
				fireData.fireTime += BraveTime.DeltaTime;
				fireData.ParticleTimer -= BraveTime.DeltaTime;
				if (!m_fireData[cells[i]].hasEnflamedNeighbors && m_fireData[cells[i]].fireTime > 0.1f)
				{
					IgniteCell(cells[i] + IntVector2.North);
					IgniteCell(cells[i] + IntVector2.East);
					IgniteCell(cells[i] + IntVector2.South);
					IgniteCell(cells[i] + IntVector2.West);
					fireData.hasEnflamedNeighbors = true;
				}
				if (fireData.HasPlayedFireOutro && fireData.ParticleTimer <= 0f)
				{
					RemovePosition(cells[i]);
					i--;
				}
				else
				{
					fireData = DoParticleAtPosition(cells[i], fireData);
					m_fireData[cells[i]] = fireData;
				}
			}
		}
		if (flag && !m_isPlayingFireAudio)
		{
			m_isPlayingFireAudio = true;
			AkSoundEngine.PostEvent("Play_ENV_oilfire_ignite_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
	}

	private void RemovePosition(IntVector2 pos)
	{
		if (cells.Contains(pos))
		{
			cells.Remove(pos);
			BuildPatch();
		}
	}

	public void BuildPatch()
	{
		int num;
		for (num = 0; num < m_tiledSpritePool.Count; num++)
		{
			SpawnManager.Despawn(m_tiledSpritePool[num].gameObject);
			m_tiledSpritePool.RemoveAt(num);
			num--;
		}
		if (m_stripPrefab == null)
		{
			m_stripPrefab = (GameObject)BraveResources.Load("Global Prefabs/TallGrassStrip");
		}
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		for (int i = 0; i < cells.Count; i++)
		{
			IntVector2 intVector = cells[i];
			if (hashSet.Contains(intVector))
			{
				continue;
			}
			hashSet.Add(intVector);
			int num2 = 1;
			int targetIndexForPosition = GetTargetIndexForPosition(intVector);
			IntVector2 intVector2 = intVector;
			while (true)
			{
				intVector2 += IntVector2.Right;
				if (!hashSet.Contains(intVector2) && cells.Contains(intVector2) && targetIndexForPosition == GetTargetIndexForPosition(intVector2))
				{
					num2++;
					hashSet.Add(intVector2);
					continue;
				}
				break;
			}
			GameObject gameObject = SpawnManager.SpawnVFX(m_stripPrefab);
			tk2dTiledSprite component = gameObject.GetComponent<tk2dTiledSprite>();
			component.SetSprite(GameManager.Instance.Dungeon.tileIndices.dungeonCollection, targetIndexForPosition);
			component.dimensions = new Vector2(16 * num2, 16f);
			gameObject.transform.position = new Vector3(intVector.x, intVector.y, intVector.y);
			m_tiledSpritePool.Add(component);
			switch (targetIndexForPosition)
			{
			case 168:
				component.HeightOffGround = -2f;
				component.IsPerpendicular = true;
				component.transform.position += new Vector3(0f, 0.6875f, 0f);
				break;
			case 124:
				component.IsPerpendicular = true;
				break;
			default:
				component.IsPerpendicular = false;
				break;
			}
			component.UpdateZDepth();
		}
		if (!StaticReferenceManager.AllGrasses.Contains(this))
		{
			StaticReferenceManager.AllGrasses.Add(this);
		}
		InitializeParticleSystem();
	}
}
