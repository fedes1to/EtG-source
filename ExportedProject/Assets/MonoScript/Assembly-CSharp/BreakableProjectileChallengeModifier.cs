using System;
using Dungeonator;
using UnityEngine;

public class BreakableProjectileChallengeModifier : ChallengeModifier
{
	public bool AimAtPlayer = true;

	private AIBulletBank m_bulletBank;

	private void Start()
	{
		m_bulletBank = GetComponent<AIBulletBank>();
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		for (int i = 0; i < StaticReferenceManager.AllMinorBreakables.Count; i++)
		{
			MinorBreakable minorBreakable = StaticReferenceManager.AllMinorBreakables[i];
			if ((bool)minorBreakable && !minorBreakable.IsBroken && minorBreakable.CenterPoint.GetAbsoluteRoom() == currentRoom && !minorBreakable.IgnoredForPotShotsModifier)
			{
				minorBreakable.OnBreakContext = (Action<MinorBreakable>)Delegate.Combine(minorBreakable.OnBreakContext, new Action<MinorBreakable>(HandleBroken));
			}
		}
	}

	private void HandleBroken(MinorBreakable mb)
	{
		if (!this || Time.realtimeSinceStartup - GameManager.Instance.BestActivePlayer.RealtimeEnteredCurrentRoom < 3f || !mb)
		{
			return;
		}
		if (AimAtPlayer)
		{
			PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(mb.CenterPoint);
			if ((bool)activePlayerClosestToPoint && (activePlayerClosestToPoint.CenterPosition - mb.CenterPoint).magnitude > 1f)
			{
				FireBullet(mb.CenterPoint, activePlayerClosestToPoint.CenterPosition - mb.CenterPoint);
			}
		}
		else
		{
			FireBullet(mb.CenterPoint, UnityEngine.Random.insideUnitCircle.normalized);
		}
	}

	private void FireBullet(Vector3 shootPoint, Vector2 direction)
	{
		m_bulletBank.CreateProjectileFromBank(shootPoint, BraveMathCollege.Atan2Degrees(direction), "default");
	}

	private void OnDestroy()
	{
		if (Dungeon.IsGenerating || !GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || !GameManager.Instance.PrimaryPlayer || GameManager.Instance.PrimaryPlayer.CurrentRoom == null)
		{
			return;
		}
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		for (int i = 0; i < StaticReferenceManager.AllMinorBreakables.Count; i++)
		{
			MinorBreakable minorBreakable = StaticReferenceManager.AllMinorBreakables[i];
			if ((bool)minorBreakable && minorBreakable.CenterPoint.GetAbsoluteRoom() == currentRoom)
			{
				minorBreakable.OnBreakContext = (Action<MinorBreakable>)Delegate.Remove(minorBreakable.OnBreakContext, new Action<MinorBreakable>(HandleBroken));
			}
		}
	}
}
