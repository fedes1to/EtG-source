using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ScareEnemiesModifier : MonoBehaviour
{
	public FleePlayerData FleeData;

	public float ConeAngle = 45f;

	public bool OnlyFearDuringReload;

	private Gun m_gun;

	private List<AIActor> m_allEnemies = new List<AIActor>();

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if (!m_gun || !m_gun.CurrentOwner || !m_gun.CurrentOwner.healthHaver || !(m_gun.CurrentOwner is PlayerController))
		{
			return;
		}
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (playerController.CurrentGun != m_gun || playerController.CurrentRoom == null)
		{
			return;
		}
		FleeData.Player = playerController;
		playerController.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref m_allEnemies);
		if (m_allEnemies == null || m_allEnemies.Count == 0)
		{
			return;
		}
		float currentAngle = m_gun.CurrentAngle;
		Vector2 centerPosition = m_gun.CurrentOwner.CenterPosition;
		for (int i = 0; i < m_allEnemies.Count; i++)
		{
			AIActor aIActor = m_allEnemies[i];
			if ((bool)aIActor && (bool)aIActor.healthHaver && aIActor.IsNormalEnemy && aIActor.IsWorthShootingAt && !aIActor.healthHaver.IsBoss && !aIActor.healthHaver.IsDead && (bool)aIActor.behaviorSpeculator)
			{
				if (BraveMathCollege.AbsAngleBetween(currentAngle, BraveMathCollege.Atan2Degrees(aIActor.CenterPosition - centerPosition)) < ConeAngle)
				{
					aIActor.behaviorSpeculator.FleePlayerData = ((!OnlyFearDuringReload || m_gun.IsReloading) ? FleeData : null);
				}
				else
				{
					aIActor.behaviorSpeculator.FleePlayerData = null;
				}
			}
		}
		m_allEnemies.Clear();
	}
}
