using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class AlarmMushroom : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public GameObject TriggerVFX;

	public GameObject DestroyVFX;

	private bool m_triggered;

	private RoomHandler m_room;

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerCollision));
	}

	private void HandleTriggerCollision(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!m_triggered)
		{
			PlayerController component = specRigidbody.GetComponent<PlayerController>();
			if ((bool)component)
			{
				StartCoroutine(Trigger());
			}
		}
	}

	private IEnumerator Trigger()
	{
		if (!m_triggered)
		{
			base.spriteAnimator.Play("alarm_mushroom_alarm");
			m_triggered = true;
			if ((bool)TriggerVFX)
			{
				SpawnManager.SpawnVFX(TriggerVFX, base.specRigidbody.UnitCenter + new Vector2(0f, 2f), Quaternion.identity);
			}
			yield return new WaitForSeconds(1f);
			RobotDaveIdea targetIdea = ((!GameManager.Instance.Dungeon.UsesCustomFloorIdea) ? GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultProceduralIdea : GameManager.Instance.Dungeon.FloorIdea);
			DungeonPlaceable backupEnemyPlaceable = targetIdea.ValidEasyEnemyPlaceables[UnityEngine.Random.Range(0, targetIdea.ValidEasyEnemyPlaceables.Length)];
			DungeonPlaceableVariant variant = backupEnemyPlaceable.SelectFromTiersFull();
			AIActor selectedEnemy = variant.GetOrLoadPlaceableObject.GetComponent<AIActor>();
			if ((bool)selectedEnemy)
			{
				AIActor aIActor = AIActor.Spawn(selectedEnemy, base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor) + new IntVector2(0, 2), m_room, true);
				aIActor.HandleReinforcementFallIntoRoom();
			}
			yield return new WaitForSeconds(1f);
			DestroyMushroom();
		}
	}

	private void DestroyMushroom()
	{
		if ((bool)DestroyVFX)
		{
			SpawnManager.SpawnVFX(DestroyVFX, base.specRigidbody.UnitCenter, Quaternion.identity);
		}
		base.spriteAnimator.PlayAndDestroyObject("alarm_mushroom_break");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
	}
}
