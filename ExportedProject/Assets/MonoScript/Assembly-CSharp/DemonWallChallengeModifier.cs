using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DemonWallChallengeModifier : ChallengeModifier
{
	[EnemyIdentifier]
	public string SniperGuyGuid;

	public float SniperCooldown = 2.4f;

	private AIActor m_sniper1;

	private AIActor m_sniper2;

	private IEnumerator Start()
	{
		RoomHandler room = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		AIActor m_boss = null;
		List<AIActor> roomEnemies = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < roomEnemies.Count; i++)
		{
			if ((bool)roomEnemies[i] && (bool)roomEnemies[i].healthHaver && roomEnemies[i].healthHaver.IsBoss)
			{
				m_boss = roomEnemies[i];
			}
		}
		yield return null;
		while (Time.timeScale == 0f)
		{
			yield return null;
		}
		yield return null;
		AIActor prefabEnemy = EnemyDatabase.GetOrLoadByGuid(SniperGuyGuid);
		IntVector2 spawnPosition = room.area.basePosition;
		m_sniper1 = AIActor.Spawn(prefabEnemy, spawnPosition + new IntVector2(4, 78), room, true);
		m_sniper2 = AIActor.Spawn(prefabEnemy, spawnPosition + new IntVector2(20, 78), room, true);
		m_sniper1.transform.position = m_sniper1.transform.position + new Vector3(0f, 0.25f, 0f);
		m_sniper2.transform.position = m_sniper2.transform.position + new Vector3(0f, 0.25f, 0f);
		m_sniper1.specRigidbody.Reinitialize();
		m_sniper2.specRigidbody.Reinitialize();
		m_sniper1.healthHaver.PreventAllDamage = true;
		m_sniper2.healthHaver.PreventAllDamage = true;
		m_sniper1.knockbackDoer.knockbackMultiplier = 0f;
		m_sniper2.knockbackDoer.knockbackMultiplier = 0f;
		m_sniper1.MovementSpeed = 0f;
		m_sniper2.MovementSpeed = 0f;
		ShootGunBehavior sgb2 = m_sniper1.behaviorSpeculator.AttackBehaviors[0] as ShootGunBehavior;
		sgb2.Range = 400f;
		sgb2.Cooldown = SniperCooldown;
		sgb2 = m_sniper2.behaviorSpeculator.AttackBehaviors[0] as ShootGunBehavior;
		sgb2.Range = 400f;
		sgb2.Cooldown = SniperCooldown;
		yield return null;
		m_sniper1.aiShooter.CurrentGun.CustomLaserSightDistance = 90f;
		m_sniper2.aiShooter.CurrentGun.CustomLaserSightDistance = 90f;
		m_sniper1.aiShooter.CurrentGun.CustomLaserSightHeight = 7f;
		m_sniper2.aiShooter.CurrentGun.CustomLaserSightHeight = 7f;
		m_sniper1.sprite.HeightOffGround = 7f;
		m_sniper2.sprite.HeightOffGround = 7f;
		m_sniper1.transform.parent = m_boss.transform.Find("sniper1");
		m_sniper2.transform.parent = m_boss.transform.Find("sniper2");
	}

	private void LateUpdate()
	{
		if ((bool)m_sniper1)
		{
			m_sniper1.sprite.UpdateZDepth();
		}
		if ((bool)m_sniper2)
		{
			m_sniper2.sprite.UpdateZDepth();
		}
	}
}
