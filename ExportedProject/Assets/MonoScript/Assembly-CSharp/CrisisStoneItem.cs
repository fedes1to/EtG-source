using System;
using System.Collections;
using UnityEngine;

public class CrisisStoneItem : PassiveItem
{
	public string ReloadAudioEvent;

	public VFXPool ImpactVFX;

	public GameObject WallVFX;

	private bool m_hasPlayedAudioForOutOfAmmo;

	protected override void Update()
	{
		base.Update();
		if ((bool)m_owner && (bool)m_owner.CurrentGun)
		{
			if (m_owner.CurrentGun.ClipShotsRemaining == 0 && !m_hasPlayedAudioForOutOfAmmo)
			{
				m_hasPlayedAudioForOutOfAmmo = true;
				AkSoundEngine.PostEvent(ReloadAudioEvent, base.gameObject);
			}
			else if (m_hasPlayedAudioForOutOfAmmo && m_owner.CurrentGun.ClipShotsRemaining > 0)
			{
				m_hasPlayedAudioForOutOfAmmo = false;
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		m_owner = player;
		HealthHaver obj = player.healthHaver;
		obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Combine(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(HandleDamageModification));
		SpeculativeRigidbody speculativeRigidbody = player.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		player.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Combine(player.OnReloadedGun, new Action<PlayerController, Gun>(HandleReloadedGun));
	}

	private void HandleReloadedGun(PlayerController sourcePlayer, Gun sourceGun)
	{
		if (!sourceGun || !sourceGun.IsHeroSword)
		{
			sourcePlayer.StartCoroutine(HandleWallVFX(sourcePlayer, sourceGun));
			AkSoundEngine.PostEvent("Play_ITM_Crisis_Stone_Shield_01", base.gameObject);
		}
	}

	private IEnumerator HandleWallVFX(PlayerController sourcePlayer, Gun sourceGun)
	{
		GameObject instanceVFX = sourcePlayer.PlayEffectOnActor(WallVFX, new Vector3(0f, -0.5f, 0f));
		float reloadTime = sourceGun.AdjustedReloadTime;
		while ((bool)sourceGun && sourceGun.IsReloading && sourceGun.ClipShotsRemaining == 0)
		{
			reloadTime -= BraveTime.DeltaTime;
			if (reloadTime < 0.15f)
			{
				break;
			}
			yield return null;
		}
		SpawnManager.Despawn(instanceVFX);
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if ((bool)m_owner && (bool)m_owner.CurrentGun && m_owner.CurrentGun.IsReloading && m_owner.CurrentGun.ClipShotsRemaining == 0 && !m_owner.CurrentGun.IsHeroSword && (bool)rigidbodyCollision.OtherRigidbody)
		{
			Projectile component = rigidbodyCollision.OtherRigidbody.GetComponent<Projectile>();
			if ((bool)component)
			{
				ImpactVFX.SpawnAtPosition(rigidbodyCollision.Contact);
				AkSoundEngine.PostEvent("Play_ITM_Crisis_Stone_Impact_01", base.gameObject);
			}
		}
	}

	private void HandleDamageModification(HealthHaver source, HealthHaver.ModifyDamageEventArgs args)
	{
		if (args != EventArgs.Empty && !(args.ModifiedDamage <= 0f) && source.IsVulnerable && (bool)m_owner && (bool)m_owner.CurrentGun && m_owner.CurrentGun.IsReloading && m_owner.CurrentGun.ClipShotsRemaining == 0 && !m_owner.CurrentGun.IsHeroSword)
		{
			args.ModifiedDamage = 0f;
		}
	}

	protected override void DisableEffect(PlayerController disablingPlayer)
	{
		if ((bool)disablingPlayer)
		{
			HealthHaver obj = disablingPlayer.healthHaver;
			obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Remove(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(HandleDamageModification));
		}
		if ((bool)disablingPlayer)
		{
			SpeculativeRigidbody speculativeRigidbody = disablingPlayer.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		}
		if ((bool)disablingPlayer)
		{
			disablingPlayer.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Remove(disablingPlayer.OnReloadedGun, new Action<PlayerController, Gun>(HandleReloadedGun));
		}
		base.DisableEffect(disablingPlayer);
	}
}
