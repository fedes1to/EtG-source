using System;
using UnityEngine;

public class FriendlyFireChallengeModifier : ChallengeModifier
{
	private void Start()
	{
		GameManager.PVP_ENABLED = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].PostProcessProjectile += ModifyProjectile;
		}
	}

	private void ModifyProjectile(Projectile proj, float somethin)
	{
		if (!proj || proj.TreatedAsNonProjectileForChallenge)
		{
			return;
		}
		tk2dBaseSprite componentInChildren = proj.GetComponentInChildren<tk2dBaseSprite>();
		Renderer componentInChildren2 = proj.GetComponentInChildren<Renderer>();
		if ((bool)componentInChildren && !componentInChildren.GetComponent<TrailController>() && (bool)componentInChildren2 && componentInChildren2.enabled)
		{
			BounceProjModifier bounceProjModifier = proj.GetComponent<BounceProjModifier>();
			if (!bounceProjModifier)
			{
				bounceProjModifier = proj.gameObject.AddComponent<BounceProjModifier>();
				bounceProjModifier.numberOfBounces = 1;
				bounceProjModifier.onlyBounceOffTiles = true;
			}
			BounceProjModifier bounceProjModifier2 = bounceProjModifier;
			bounceProjModifier2.OnBounceContext = (Action<BounceProjModifier, SpeculativeRigidbody>)Delegate.Combine(bounceProjModifier2.OnBounceContext, new Action<BounceProjModifier, SpeculativeRigidbody>(OnFirstBounce));
		}
	}

	private void OnFirstBounce(BounceProjModifier mod, SpeculativeRigidbody otherRigidbody)
	{
		if (!mod)
		{
			return;
		}
		mod.OnBounceContext = (Action<BounceProjModifier, SpeculativeRigidbody>)Delegate.Remove(mod.OnBounceContext, new Action<BounceProjModifier, SpeculativeRigidbody>(OnFirstBounce));
		Projectile component = mod.GetComponent<Projectile>();
		if (!component)
		{
			return;
		}
		if ((bool)otherRigidbody && (bool)otherRigidbody.minorBreakable)
		{
			component.DieInAir();
			return;
		}
		component.MakeLookLikeEnemyBullet(false);
		component.baseData.speed = Mathf.Min(component.baseData.speed, 10f);
		component.Speed = Mathf.Min(component.Speed, 10f);
		component.allowSelfShooting = true;
		component.ForcePlayerBlankable = true;
		if ((bool)component.Shooter)
		{
			component.specRigidbody.DeregisterSpecificCollisionException(component.Shooter);
			component.specRigidbody.RegisterGhostCollisionException(component.Shooter);
		}
	}

	private void OnDestroy()
	{
		GameManager.PVP_ENABLED = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].PostProcessProjectile -= ModifyProjectile;
		}
	}
}
