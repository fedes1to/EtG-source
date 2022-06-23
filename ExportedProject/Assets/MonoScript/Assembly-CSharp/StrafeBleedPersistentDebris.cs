using System;
using UnityEngine;

public class StrafeBleedPersistentDebris : BraveBehaviour
{
	public ExplosionData explosionData;

	public float CascadeTime = 3f;

	private Gun m_attachedGun;

	private bool m_initialized;

	private float m_elapsed;

	public void InitializeSelf(StrafeBleedBuff source)
	{
		m_initialized = true;
		explosionData = source.explosionData;
		Projectile component = source.GetComponent<Projectile>();
		if (component.PossibleSourceGun != null)
		{
			m_attachedGun = component.PossibleSourceGun;
			Gun possibleSourceGun = component.PossibleSourceGun;
			possibleSourceGun.OnFinishAttack = (Action<PlayerController, Gun>)Delegate.Combine(possibleSourceGun.OnFinishAttack, new Action<PlayerController, Gun>(HandleCeaseAttack));
		}
		else if ((bool)component && (bool)component.Owner && (bool)component.Owner.CurrentGun)
		{
			m_attachedGun = component.Owner.CurrentGun;
			Gun currentGun = component.Owner.CurrentGun;
			currentGun.OnFinishAttack = (Action<PlayerController, Gun>)Delegate.Combine(currentGun.OnFinishAttack, new Action<PlayerController, Gun>(HandleCeaseAttack));
		}
	}

	private void HandleCeaseAttack(PlayerController arg1, Gun arg2)
	{
		DoEffect();
		Disconnect();
	}

	private void Disconnect()
	{
		m_initialized = false;
		if ((bool)m_attachedGun)
		{
			Gun attachedGun = m_attachedGun;
			attachedGun.OnFinishAttack = (Action<PlayerController, Gun>)Delegate.Remove(attachedGun.OnFinishAttack, new Action<PlayerController, Gun>(HandleCeaseAttack));
		}
	}

	private void Update()
	{
		if (m_initialized)
		{
			m_elapsed += BraveTime.DeltaTime;
			if (m_elapsed > CascadeTime)
			{
				DoEffect();
				Disconnect();
			}
		}
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
