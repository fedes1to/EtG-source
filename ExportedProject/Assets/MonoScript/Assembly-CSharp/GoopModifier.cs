using System;
using UnityEngine;

public class GoopModifier : BraveBehaviour
{
	public GoopDefinition goopDefinition;

	public bool IsSynergyContingent;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool SpawnGoopInFlight;

	[ShowInInspectorIf("SpawnGoopInFlight", true)]
	public float InFlightSpawnFrequency = 0.05f;

	[ShowInInspectorIf("SpawnGoopInFlight", true)]
	public float InFlightSpawnRadius = 1f;

	public bool SpawnGoopOnCollision;

	[ShowInInspectorIf("SpawnGoopOnCollision", true)]
	public bool OnlyGoopOnEnemyCollision;

	[ShowInInspectorIf("SpawnGoopOnCollision", true)]
	public float CollisionSpawnRadius = 3f;

	public bool SpawnAtBeamEnd;

	[ShowInInspectorIf("SpawnAtBeamEnd", true)]
	public float BeamEndRadius = 1f;

	public Vector2 spawnOffset = new Vector2(0f, -0.5f);

	public bool UsesInitialDelay;

	public float InitialDelay = 0.25f;

	private float m_totalElapsed;

	[NonSerialized]
	public bool SynergyViable;

	private DeadlyDeadlyGoopManager m_manager;

	private float elapsed;

	public DeadlyDeadlyGoopManager Manager
	{
		get
		{
			if (m_manager == null)
			{
				m_manager = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinition);
			}
			return m_manager;
		}
	}

	public void Start()
	{
		if (IsSynergyContingent)
		{
			if (!base.projectile || !base.projectile.PossibleSourceGun || !(base.projectile.PossibleSourceGun.CurrentOwner is PlayerController))
			{
				base.enabled = false;
				return;
			}
			PlayerController playerController = base.projectile.PossibleSourceGun.CurrentOwner as PlayerController;
			if (!playerController.HasActiveBonusSynergy(RequiredSynergy))
			{
				base.enabled = false;
				return;
			}
			SynergyViable = true;
		}
		if ((bool)GetComponent<BeamController>())
		{
			base.enabled = false;
		}
	}

	public void Update()
	{
		if (m_manager == null)
		{
			m_manager = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinition);
		}
		if (IsSynergyContingent && (bool)base.projectile && base.projectile.OverrideMotionModule != null && base.projectile.OverrideMotionModule is OrbitProjectileMotionModule)
		{
			base.enabled = false;
		}
		else
		{
			if (!SpawnGoopInFlight)
			{
				return;
			}
			elapsed += BraveTime.DeltaTime;
			m_totalElapsed += BraveTime.DeltaTime;
			if ((!UsesInitialDelay || m_totalElapsed > InitialDelay) && elapsed >= InFlightSpawnFrequency)
			{
				elapsed -= InFlightSpawnFrequency;
				Vector2 vector = base.projectile.sprite.WorldCenter + spawnOffset - base.projectile.transform.position.XY();
				m_manager.AddGoopLine(base.projectile.sprite.WorldCenter + spawnOffset, base.projectile.LastPosition.XY() + vector, InFlightSpawnRadius);
				if (goopDefinition.CanBeFrozen && (base.projectile.damageTypes | CoreDamageTypes.Ice) == base.projectile.damageTypes)
				{
					Manager.FreezeGoopCircle(base.projectile.sprite.WorldCenter, InFlightSpawnRadius);
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void SpawnCollisionGoop(Vector2 pos)
	{
		if ((!IsSynergyContingent || SynergyViable) && SpawnGoopOnCollision)
		{
			Manager.TimedAddGoopCircle(pos, CollisionSpawnRadius);
			if (goopDefinition.CanBeFrozen && (base.projectile.damageTypes | CoreDamageTypes.Ice) == base.projectile.damageTypes)
			{
				Manager.FreezeGoopCircle(pos, CollisionSpawnRadius);
			}
		}
	}

	public void SpawnCollisionGoop(CollisionData lcr)
	{
		if ((!IsSynergyContingent || SynergyViable) && SpawnGoopOnCollision)
		{
			Manager.TimedAddGoopCircle(lcr.Contact, CollisionSpawnRadius);
			if (goopDefinition.CanBeFrozen && (base.projectile.damageTypes | CoreDamageTypes.Ice) == base.projectile.damageTypes)
			{
				Manager.FreezeGoopCircle(lcr.Contact, CollisionSpawnRadius);
			}
		}
	}
}
