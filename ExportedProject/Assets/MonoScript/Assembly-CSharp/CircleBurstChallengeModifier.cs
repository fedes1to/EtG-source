using System;
using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class CircleBurstChallengeModifier : ChallengeModifier
{
	public BulletScriptSelector BulletScript;

	public GameObject tellVFX;

	public GameObject ChainLightningVFX;

	public float NearRadius = 5f;

	public float FarRadius = 9f;

	public float StartDelay = 3f;

	public float TimeBetweenWaves = 10f;

	private RoomHandler m_room;

	[NonSerialized]
	public bool Preprocessed;

	private float m_waveTimer = 5f;

	private List<ChainLightningModifier> m_clms = new List<ChainLightningModifier>();

	private ChainLightningModifier m_firstCLM;

	private ChainLightningModifier m_lastCLM;

	public override bool IsValid(RoomHandler room)
	{
		if (room.Cells.Count < 150 || room.area.IsProceduralRoom)
		{
			return false;
		}
		return base.IsValid(room);
	}

	private IEnumerator Start()
	{
		m_room = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		m_waveTimer = StartDelay;
		yield return null;
		if (!ChallengeManager.Instance)
		{
			yield break;
		}
		for (int i = 0; i < ChallengeManager.Instance.ActiveChallenges.Count; i++)
		{
			if (ChallengeManager.Instance.ActiveChallenges[i] is FloorShockwaveChallengeModifier)
			{
				float num = TimeBetweenWaves;
				if (!Preprocessed)
				{
					FloorShockwaveChallengeModifier floorShockwaveChallengeModifier = ChallengeManager.Instance.ActiveChallenges[i] as FloorShockwaveChallengeModifier;
					float num2 = Mathf.Max(floorShockwaveChallengeModifier.TimeBetweenGaze, TimeBetweenWaves);
					num = (floorShockwaveChallengeModifier.TimeBetweenGaze = (TimeBetweenWaves = num2 * 1.25f));
					Preprocessed = true;
					floorShockwaveChallengeModifier.Preprocessed = true;
				}
				m_waveTimer = num * 0.75f;
			}
		}
	}

	public static IntVector2? GetAppropriateSpawnPointForChallengeBurst(RoomHandler room, float tooCloseRadius, float tooFarRadius)
	{
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				float num = Vector2.Distance(c.ToCenterVector2(), GameManager.Instance.AllPlayers[i].CenterPosition);
				if (num < tooCloseRadius || num > tooFarRadius)
				{
					return false;
				}
			}
			return true;
		};
		return room.GetRandomAvailableCell(IntVector2.One, CellTypes.FLOOR | CellTypes.PIT, true, cellValidator);
	}

	private void Update()
	{
		m_waveTimer -= BraveTime.DeltaTime;
		if (m_waveTimer <= 0f)
		{
			Cleanup();
			IntVector2? appropriateSpawnPointForChallengeBurst = GetAppropriateSpawnPointForChallengeBurst(m_room, NearRadius, FarRadius);
			if (appropriateSpawnPointForChallengeBurst.HasValue)
			{
				m_waveTimer = TimeBetweenWaves;
				SpawnBulletScript(null, BulletScript, GetComponent<AIBulletBank>(), appropriateSpawnPointForChallengeBurst.Value.ToCenterVector2(), StringTableManager.GetEnemiesString("#TRAP"));
			}
		}
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		for (int num = m_clms.Count - 1; num >= 0; num--)
		{
			ChainLightningModifier chainLightningModifier = m_clms[num];
			if ((bool)chainLightningModifier)
			{
				chainLightningModifier.ForcedLinkProjectile = null;
				if ((bool)chainLightningModifier.projectile)
				{
					chainLightningModifier.projectile.ForceDestruction();
				}
			}
		}
		m_clms.Clear();
	}

	private void SpawnBulletScript(AIActor aiActor, BulletScriptSelector bulletScript, AIBulletBank bank, Vector2 position, string ownerName)
	{
		StartCoroutine(HandleSpawnBulletScript(aiActor, bulletScript, bank, position, ownerName));
	}

	private IEnumerator HandleSpawnBulletScript(AIActor aiActor, BulletScriptSelector bulletScript, AIBulletBank bank, Vector2 position, string ownerName)
	{
		m_firstCLM = null;
		m_lastCLM = null;
		if ((bool)tellVFX)
		{
			GameObject instanceVFX = SpawnManager.SpawnVFX(tellVFX, position, Quaternion.identity);
			tk2dBaseSprite instanceSprite = instanceVFX.GetComponent<tk2dBaseSprite>();
			while ((bool)instanceVFX && instanceVFX.activeSelf)
			{
				if ((bool)instanceSprite)
				{
					instanceSprite.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.MiddleCenter);
				}
				yield return null;
			}
		}
		m_firstCLM = null;
		m_lastCLM = null;
		SpawnManager.SpawnBulletScript(aiActor, position, bank, bulletScript, ownerName, null, null, false, OnBulletCreated);
		m_firstCLM.ForcedLinkProjectile = m_lastCLM.projectile;
		m_lastCLM.BackLinkProjectile = m_firstCLM.projectile;
	}

	private void OnBulletCreated(Bullet b, Projectile p)
	{
		ChainLightningModifier orAddComponent = p.gameObject.GetOrAddComponent<ChainLightningModifier>();
		orAddComponent.DamagesPlayers = true;
		orAddComponent.DamagesEnemies = false;
		orAddComponent.RequiresSameProjectileClass = true;
		orAddComponent.LinkVFXPrefab = ChainLightningVFX;
		orAddComponent.damageTypes = CoreDamageTypes.Electric;
		orAddComponent.maximumLinkDistance = 100f;
		orAddComponent.damagePerHit = 0.5f;
		orAddComponent.damageCooldown = 1f;
		orAddComponent.UsesDispersalParticles = false;
		orAddComponent.UseForcedLinkProjectile = true;
		if (m_lastCLM != null)
		{
			orAddComponent.ForcedLinkProjectile = m_lastCLM.projectile;
			m_lastCLM.BackLinkProjectile = orAddComponent.projectile;
		}
		if (m_firstCLM == null)
		{
			m_firstCLM = orAddComponent;
		}
		m_lastCLM = orAddComponent;
		m_clms.Add(orAddComponent);
	}
}
