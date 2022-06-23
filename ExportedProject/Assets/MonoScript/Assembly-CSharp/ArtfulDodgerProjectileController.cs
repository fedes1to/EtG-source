using System;
using System.Collections.Generic;
using UnityEngine;

public class ArtfulDodgerProjectileController : MonoBehaviour
{
	[NonSerialized]
	public bool hitTarget;

	private Projectile m_projectile;

	private BounceProjModifier m_bouncer;

	private void Start()
	{
		m_projectile = GetComponent<Projectile>();
		m_projectile.OnDestruction += HandleDestruction;
		m_bouncer = GetComponent<BounceProjModifier>();
	}

	private void HandleDestruction(Projectile source)
	{
		if (!hitTarget)
		{
			List<ArtfulDodgerTargetController> componentsAbsoluteInRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor)).GetComponentsAbsoluteInRoom<ArtfulDodgerTargetController>();
			for (int i = 0; i < componentsAbsoluteInRoom.Count; i++)
			{
				if (!componentsAbsoluteInRoom[i].IsBroken)
				{
					componentsAbsoluteInRoom[i].GetComponentInChildren<tk2dSpriteAnimator>().PlayForDuration("target_miss", 3f, "target_idle");
				}
			}
		}
		else
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.WINCHESTER_SHOTS_HIT, 1f);
		}
	}

	private void Update()
	{
		int numberOfBounces = m_bouncer.numberOfBounces;
		m_projectile.ChangeTintColorShader(0f, BraveUtility.GetRainbowColor(numberOfBounces));
	}
}
