using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ArtfulDodgerGunController : MonoBehaviour
{
	private Gun m_gun;

	private Projectile m_lastProjectile;

	private RoomHandler m_startRoom;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun.PostProcessProjectile, new Action<Projectile>(NotifyFired));
	}

	private void NotifyFired(Projectile obj)
	{
		m_lastProjectile = obj;
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.WINCHESTER_SHOTS_FIRED, 1f);
	}

	private void Start()
	{
		m_startRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		m_gun.DoubleWideLaserSight = true;
		List<ArtfulDodgerRoomController> componentsAbsoluteInRoom = m_startRoom.GetComponentsAbsoluteInRoom<ArtfulDodgerRoomController>();
		if (componentsAbsoluteInRoom != null && componentsAbsoluteInRoom.Count > 0)
		{
			componentsAbsoluteInRoom[0].gamePlayingPlayer = m_gun.CurrentOwner as PlayerController;
		}
	}

	private IEnumerator HandleDelayedReward()
	{
		float elapsed = 5f;
		while ((bool)m_lastProjectile && elapsed > 0f)
		{
			elapsed -= BraveTime.DeltaTime;
			yield return null;
		}
		m_startRoom.GetComponentsAbsoluteInRoom<ArtfulDodgerRoomController>()[0].DoHandleReward();
	}

	private void Update()
	{
		if ((bool)m_gun && (bool)m_gun.CurrentOwner)
		{
			(m_gun.CurrentOwner as PlayerController).HighAccuracyAimMode = true;
		}
		if (m_gun.ammo == 0)
		{
			if (m_gun.CurrentOwner is PlayerController)
			{
				PlayerController playerController = m_gun.CurrentOwner as PlayerController;
				playerController.HighAccuracyAimMode = false;
				playerController.SuppressThisClick = true;
				playerController.inventory.DestroyGun(m_gun);
				playerController.StartCoroutine(HandleDelayedReward());
			}
		}
		else if (m_gun.CurrentOwner != null && m_gun.CurrentOwner is PlayerController)
		{
			PlayerController playerController2 = m_gun.CurrentOwner as PlayerController;
			if (playerController2.CurrentRoom != m_startRoom)
			{
				playerController2.HighAccuracyAimMode = false;
				playerController2.SuppressThisClick = true;
				playerController2.inventory.DestroyGun(m_gun);
				playerController2.StartCoroutine(HandleDelayedReward());
			}
		}
	}
}
