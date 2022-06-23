using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class YellowChamberItem : PassiveItem
{
	public float ChanceToHappen = 0.25f;

	public GameActorCharmEffect CharmEffect;

	public GameObject EraseVFX;

	private PlayerController m_player;

	private AIActor m_currentlyCharmedEnemy;

	private List<AIActor> m_enemyList = new List<AIActor>();

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.OnEnteredCombat = (Action)Delegate.Combine(player.OnEnteredCombat, new Action(OnEnteredCombat));
		}
	}

	private void OnEnteredCombat()
	{
		if ((bool)m_currentlyCharmedEnemy || !(UnityEngine.Random.value < ChanceToHappen))
		{
			return;
		}
		m_player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref m_enemyList);
		for (int i = 0; i < m_enemyList.Count; i++)
		{
			AIActor aIActor = m_enemyList[i];
			if (!aIActor || !aIActor.IsNormalEnemy || !aIActor.healthHaver || aIActor.healthHaver.IsBoss || aIActor.IsHarmlessEnemy)
			{
				m_enemyList.RemoveAt(i);
				i--;
			}
		}
		if (m_enemyList.Count > 1)
		{
			AIActor aIActor2 = m_enemyList[UnityEngine.Random.Range(0, m_enemyList.Count)];
			aIActor2.IgnoreForRoomClear = true;
			aIActor2.ParentRoom.ResetEnemyHPPercentage();
			aIActor2.ApplyEffect(CharmEffect);
			m_currentlyCharmedEnemy = aIActor2;
		}
	}

	protected override void Update()
	{
		if (m_pickedUp && (bool)m_player && (bool)m_currentlyCharmedEnemy && (m_player.CurrentRoom == null || m_player.CurrentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) <= 0))
		{
			EatCharmedEnemy();
		}
		base.Update();
	}

	private void EatCharmedEnemy()
	{
		if ((bool)m_currentlyCharmedEnemy)
		{
			if ((bool)m_currentlyCharmedEnemy.behaviorSpeculator)
			{
				m_currentlyCharmedEnemy.behaviorSpeculator.Stun(1f);
			}
			if ((bool)m_currentlyCharmedEnemy.knockbackDoer)
			{
				m_currentlyCharmedEnemy.knockbackDoer.SetImmobile(true, "YellowChamberItem");
			}
			GameObject gameObject = m_currentlyCharmedEnemy.PlayEffectOnActor(EraseVFX, new Vector3(0f, -1f, 0f), false);
			m_currentlyCharmedEnemy.StartCoroutine(DelayedDestroyEnemy(m_currentlyCharmedEnemy, gameObject.GetComponent<tk2dSpriteAnimator>()));
			m_currentlyCharmedEnemy = null;
		}
	}

	private IEnumerator DelayedDestroyEnemy(AIActor enemy, tk2dSpriteAnimator vfxAnimator)
	{
		if ((bool)vfxAnimator)
		{
			vfxAnimator.sprite.IsPerpendicular = false;
			vfxAnimator.sprite.HeightOffGround = -1f;
		}
		while ((bool)enemy && (bool)vfxAnimator && vfxAnimator.sprite.GetCurrentSpriteDef().name != "kthuliber_tentacles_010")
		{
			vfxAnimator.sprite.UpdateZDepth();
			yield return null;
		}
		if ((bool)vfxAnimator)
		{
			vfxAnimator.sprite.IsPerpendicular = true;
			vfxAnimator.sprite.HeightOffGround = 1.5f;
			vfxAnimator.sprite.UpdateZDepth();
		}
		if ((bool)enemy)
		{
			enemy.EraseFromExistence();
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<YellowChamberItem>().m_pickedUpThisRun = true;
		player.OnEnteredCombat = (Action)Delegate.Remove(player.OnEnteredCombat, new Action(OnEnteredCombat));
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			PlayerController player = m_player;
			player.OnEnteredCombat = (Action)Delegate.Remove(player.OnEnteredCombat, new Action(OnEnteredCombat));
		}
	}
}
