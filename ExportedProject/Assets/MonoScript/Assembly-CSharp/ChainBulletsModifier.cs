using System;
using Dungeonator;
using UnityEngine;

public class ChainBulletsModifier : MonoBehaviour
{
	public int GuaranteedNumberOfChains = 1;

	public float ChanceToContinueChaining;

	public bool BounceRandomlyOnNoTarget = true;

	public float EnemyDetectRadius = -1f;

	private int m_numBounces;

	private void Start()
	{
		Projectile component = GetComponent<Projectile>();
		if ((bool)component)
		{
			component.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(component.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		}
	}

	private void HandleHitEnemy(Projectile arg1, SpeculativeRigidbody arg2, bool arg3)
	{
		DoChain(arg1, arg2);
	}

	private void DoChain(Projectile proj, SpeculativeRigidbody enemy)
	{
		if (m_numBounces >= GuaranteedNumberOfChains && !(UnityEngine.Random.value < ChanceToContinueChaining))
		{
			return;
		}
		m_numBounces++;
		if (BounceRandomlyOnNoTarget)
		{
			PierceProjModifier orAddComponent = proj.gameObject.GetOrAddComponent<PierceProjModifier>();
			orAddComponent.penetratesBreakables = true;
			orAddComponent.penetration++;
		}
		Vector2 dirVec = UnityEngine.Random.insideUnitCircle;
		if ((bool)enemy.aiActor)
		{
			AIActor closestToPosition = BraveUtility.GetClosestToPosition(enemy.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All), enemy.UnitCenter, null, EnemyDetectRadius, enemy.aiActor);
			if ((bool)closestToPosition)
			{
				if (!BounceRandomlyOnNoTarget)
				{
					PierceProjModifier orAddComponent2 = proj.gameObject.GetOrAddComponent<PierceProjModifier>();
					orAddComponent2.penetratesBreakables = true;
					orAddComponent2.penetration++;
				}
				dirVec = closestToPosition.CenterPosition - proj.transform.position.XY();
				if (!BounceRandomlyOnNoTarget)
				{
					proj.SendInDirection(dirVec, false);
				}
			}
		}
		if (BounceRandomlyOnNoTarget)
		{
			proj.SendInDirection(dirVec, false);
		}
	}
}
