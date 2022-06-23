using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class MicroCuccoController : BraveBehaviour
{
	public float Speed = 8f;

	public float Damage = 4f;

	private PlayerController m_owner;

	private AIActor m_target;

	public void Initialize(PlayerController owner)
	{
		m_owner = owner;
		StartCoroutine(FindTarget());
	}

	private void Update()
	{
		if (!m_target)
		{
			return;
		}
		Vector2 vector = m_target.CenterPosition - base.sprite.WorldCenter;
		if (vector.x > 0f)
		{
			if (!base.spriteAnimator.IsPlaying("attack_right"))
			{
				base.spriteAnimator.Play("attack_right");
			}
		}
		else if (!base.spriteAnimator.IsPlaying("attack_left"))
		{
			base.spriteAnimator.Play("attack_left");
		}
		if (vector.magnitude < 0.5f)
		{
			float num = 1f;
			if (PassiveItem.IsFlagSetAtAll(typeof(BattleStandardItem)) && (bool)m_owner && (bool)m_owner.CurrentGun && m_owner.CurrentGun.IsLuteCompanionBuff)
			{
				num = BattleStandardItem.BattleStandardCompanionDamageMultiplier;
			}
			m_target.healthHaver.ApplyDamage(num * Damage, Vector2.zero, "Cucco");
			Object.Destroy(base.gameObject);
		}
		else
		{
			base.transform.position = base.transform.position + (vector.normalized * Speed * BraveTime.DeltaTime).ToVector3ZUp();
		}
	}

	private IEnumerator FindTarget()
	{
		while ((bool)m_owner)
		{
			List<AIActor> activeEnemies = m_owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null)
			{
				float num = float.MaxValue;
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					AIActor aIActor = activeEnemies[i];
					if ((bool)aIActor && (bool)aIActor.healthHaver && !aIActor.healthHaver.IsDead && !aIActor.IsGone && (bool)aIActor.specRigidbody)
					{
						float num2 = Vector2.Distance(aIActor.specRigidbody.GetUnitCenter(ColliderType.HitBox), base.sprite.WorldCenter);
						if (num2 < num)
						{
							m_target = aIActor;
							num = num2;
						}
					}
				}
				if (m_target == null)
				{
					break;
				}
			}
			yield return new WaitForSeconds(1f);
		}
		Object.Destroy(base.gameObject);
	}
}
