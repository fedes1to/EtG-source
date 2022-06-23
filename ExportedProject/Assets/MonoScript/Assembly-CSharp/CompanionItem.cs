using System;
using Dungeonator;
using UnityEngine;

public class CompanionItem : PassiveItem
{
	[EnemyIdentifier]
	public string CompanionGuid;

	public bool UsesAlternatePastPrefab;

	[EnemyIdentifier]
	[ShowInInspectorIf("UsesAlternatePastPrefab", false)]
	public string CompanionPastGuid;

	public CompanionTransformSynergy[] Synergies;

	[NonSerialized]
	public bool PreventRespawnOnFloorLoad;

	[Header("For Pig Synergy")]
	public bool HasGunTransformationSacrificeSynergy;

	[ShowInInspectorIf("HasGunTransformationSacrificeSynergy", false)]
	public CustomSynergyType GunTransformationSacrificeSynergy;

	[PickupIdentifier]
	[ShowInInspectorIf("HasGunTransformationSacrificeSynergy", false)]
	public int SacrificeGunID;

	[ShowInInspectorIf("HasGunTransformationSacrificeSynergy", false)]
	public float SacrificeGunDuration = 30f;

	[NonSerialized]
	public bool BabyGoodMimicOrbitalOverridden;

	[NonSerialized]
	public PlayerOrbitalItem OverridePlayerOrbitalItem;

	private int m_lastActiveSynergyTransformation = -1;

	private GameObject m_extantCompanion;

	public GameObject ExtantCompanion
	{
		get
		{
			return m_extantCompanion;
		}
	}

	private void CreateCompanion(PlayerController owner)
	{
		if (PreventRespawnOnFloorLoad)
		{
			return;
		}
		if (BabyGoodMimicOrbitalOverridden)
		{
			GameObject gameObject = (m_extantCompanion = PlayerOrbitalItem.CreateOrbital(owner, (!OverridePlayerOrbitalItem.OrbitalFollowerPrefab) ? OverridePlayerOrbitalItem.OrbitalPrefab.gameObject : OverridePlayerOrbitalItem.OrbitalFollowerPrefab.gameObject, OverridePlayerOrbitalItem.OrbitalFollowerPrefab));
			return;
		}
		string guid = CompanionGuid;
		m_lastActiveSynergyTransformation = -1;
		if (UsesAlternatePastPrefab && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
		{
			guid = CompanionPastGuid;
		}
		else if (Synergies.Length > 0)
		{
			for (int i = 0; i < Synergies.Length; i++)
			{
				if (owner.HasActiveBonusSynergy(Synergies[i].RequiredSynergy))
				{
					guid = Synergies[i].SynergyCompanionGuid;
					m_lastActiveSynergyTransformation = i;
				}
			}
		}
		AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(guid);
		Vector3 position = owner.transform.position;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			position += new Vector3(1.125f, -0.3125f, 0f);
		}
		GameObject gameObject2 = (m_extantCompanion = UnityEngine.Object.Instantiate(orLoadByGuid.gameObject, position, Quaternion.identity));
		CompanionController orAddComponent = m_extantCompanion.GetOrAddComponent<CompanionController>();
		orAddComponent.Initialize(owner);
		if ((bool)orAddComponent.specRigidbody)
		{
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(orAddComponent.specRigidbody);
		}
		if (orAddComponent.companionID == CompanionController.CompanionIdentifier.BABY_GOOD_MIMIC)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_GOT_BABY_MIMIC, true);
		}
	}

	public void ForceCompanionRegeneration(PlayerController owner, Vector2? overridePosition)
	{
		bool flag = false;
		Vector2 vector = Vector2.zero;
		if ((bool)m_extantCompanion)
		{
			flag = true;
			vector = m_extantCompanion.transform.position.XY();
		}
		if (overridePosition.HasValue)
		{
			flag = true;
			vector = overridePosition.Value;
		}
		DestroyCompanion();
		CreateCompanion(owner);
		if ((bool)m_extantCompanion && flag)
		{
			m_extantCompanion.transform.position = vector.ToVector3ZisY();
			SpeculativeRigidbody component = m_extantCompanion.GetComponent<SpeculativeRigidbody>();
			if ((bool)component)
			{
				component.Reinitialize();
			}
		}
	}

	public void ForceDisconnectCompanion()
	{
		m_extantCompanion = null;
	}

	private void DestroyCompanion()
	{
		if ((bool)m_extantCompanion)
		{
			UnityEngine.Object.Destroy(m_extantCompanion);
			m_extantCompanion = null;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Dungeon.IsGenerating || !m_owner || Synergies.Length <= 0 || (UsesAlternatePastPrefab && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST))
		{
			return;
		}
		bool flag = false;
		for (int num = Synergies.Length - 1; num >= 0; num--)
		{
			if (m_owner.HasActiveBonusSynergy(Synergies[num].RequiredSynergy))
			{
				if (m_lastActiveSynergyTransformation != num)
				{
					DestroyCompanion();
					CreateCompanion(m_owner);
				}
				flag = true;
				break;
			}
		}
		if (!flag && m_lastActiveSynergyTransformation != -1)
		{
			DestroyCompanion();
			CreateCompanion(m_owner);
		}
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		CreateCompanion(player);
	}

	private void HandleNewFloor(PlayerController obj)
	{
		DestroyCompanion();
		if (!PreventRespawnOnFloorLoad)
		{
			CreateCompanion(obj);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DestroyCompanion();
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			PlayerController owner = m_owner;
			owner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(owner.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		}
		DestroyCompanion();
		base.OnDestroy();
	}
}
