using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class AuraSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool TriggeredOnReload;

	public float AuraRadius = 5f;

	public bool HasOverrideDuration;

	public float OverrideDuration = 0.05f;

	public bool DoPoison;

	public GameActorHealthEffect PoisonEffect;

	public bool DoFreeze;

	public GameActorFreezeEffect FreezeEffect;

	public bool DoBurn;

	public GameActorFireEffect FireEffect;

	public bool DoCharm;

	public GameActorCharmEffect CharmEffect;

	public bool DoSlow;

	public GameActorSpeedEffect SpeedEffect;

	public bool DoStun;

	public float StunDuration = 1f;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReload));
	}

	private void HandleReload(PlayerController sourcePlayer, Gun arg2, bool arg3)
	{
		if ((bool)sourcePlayer && sourcePlayer.HasActiveBonusSynergy(RequiredSynergy) && TriggeredOnReload)
		{
			StartCoroutine(HandleReloadTrigger());
		}
	}

	private IEnumerator HandleReloadTrigger()
	{
		float elapsed = 0f;
		while ((bool)m_gun && m_gun.IsReloading && (!HasOverrideDuration || elapsed < OverrideDuration) && m_gun.enabled && (bool)m_gun.CurrentOwner && !Dungeon.IsGenerating)
		{
			elapsed += BraveTime.DeltaTime;
			PlayerController playerOwner = m_gun.CurrentOwner as PlayerController;
			if (!playerOwner || playerOwner.CurrentRoom == null)
			{
				break;
			}
			playerOwner.CurrentRoom.ApplyActionToNearbyEnemies(playerOwner.CenterPosition, AuraRadius, ProcessEnemy);
			yield return null;
		}
	}

	private void ProcessEnemy(AIActor enemy, float distance)
	{
		if (DoPoison)
		{
			enemy.ApplyEffect(PoisonEffect);
		}
		if (DoFreeze)
		{
			enemy.ApplyEffect(FreezeEffect, BraveTime.DeltaTime);
		}
		if (DoBurn)
		{
			enemy.ApplyEffect(FireEffect);
		}
		if (DoCharm)
		{
			enemy.ApplyEffect(CharmEffect);
		}
		if (DoSlow)
		{
			enemy.ApplyEffect(SpeedEffect);
		}
		if (DoStun && (bool)enemy.behaviorSpeculator)
		{
			if (enemy.behaviorSpeculator.IsStunned)
			{
				enemy.behaviorSpeculator.UpdateStun(StunDuration);
			}
			else
			{
				enemy.behaviorSpeculator.Stun(StunDuration);
			}
		}
	}
}
