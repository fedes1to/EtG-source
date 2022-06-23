using System;
using UnityEngine;

public class StickyGrenadePersistentDebris : BraveBehaviour
{
	public ExplosionData explosionData;

	private PlayerController m_player;

	private Gun m_attachedGun;

	public void InitializeSelf(StickyGrenadeBuff source)
	{
		explosionData = source.explosionData;
		Projectile component = source.GetComponent<Projectile>();
		if (component.PossibleSourceGun != null)
		{
			m_attachedGun = component.PossibleSourceGun;
			m_player = component.PossibleSourceGun.CurrentOwner as PlayerController;
			Gun possibleSourceGun = component.PossibleSourceGun;
			possibleSourceGun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(possibleSourceGun.OnReloadPressed, new Action<PlayerController, Gun, bool>(ExplodeOnReload));
			if ((bool)m_player)
			{
				m_player.GunChanged += GunChanged;
			}
		}
		else if ((bool)component && (bool)component.Owner && (bool)component.Owner.CurrentGun)
		{
			m_attachedGun = component.Owner.CurrentGun;
			m_player = component.Owner as PlayerController;
			Gun currentGun = component.Owner.CurrentGun;
			currentGun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(currentGun.OnReloadPressed, new Action<PlayerController, Gun, bool>(ExplodeOnReload));
			if ((bool)m_player)
			{
				m_player.GunChanged += GunChanged;
			}
		}
	}

	private void Disconnect()
	{
		if ((bool)m_player)
		{
			m_player.GunChanged -= GunChanged;
		}
		if ((bool)m_attachedGun)
		{
			Gun attachedGun = m_attachedGun;
			attachedGun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Remove(attachedGun.OnReloadPressed, new Action<PlayerController, Gun, bool>(ExplodeOnReload));
		}
	}

	private void GunChanged(Gun arg1, Gun arg2, bool newGun)
	{
		Disconnect();
		DoEffect();
	}

	private void ExplodeOnReload(PlayerController arg1, Gun arg2, bool actual)
	{
		Disconnect();
		DoEffect();
	}

	private void DoEffect()
	{
		explosionData.force = 0f;
		if ((bool)base.sprite)
		{
			Exploder.Explode(base.sprite.WorldCenter, explosionData, Vector2.zero, null, true);
		}
		else
		{
			Exploder.Explode(base.transform.position.XY(), explosionData, Vector2.zero, null, true);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		Disconnect();
		base.OnDestroy();
	}
}
