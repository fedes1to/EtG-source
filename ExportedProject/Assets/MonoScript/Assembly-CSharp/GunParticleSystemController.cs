using System;
using UnityEngine;

public class GunParticleSystemController : MonoBehaviour
{
	public ParticleSystem TargetSystem;

	public bool DoesParticlesOnFire;

	public int MinParticlesOnFire = 10;

	public int MaxParticlesOnFire = 10;

	public bool DoesParticlesOnReload;

	public int MinParticlesOnReload = 20;

	public int MaxParticlesOnReload = 20;

	private Gun m_gun;

	private Vector3 m_localPositionOffset;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		if ((bool)TargetSystem)
		{
			m_localPositionOffset = TargetSystem.transform.localPosition;
		}
	}

	private void Start()
	{
		m_gun = GetComponent<Gun>();
		if (DoesParticlesOnFire)
		{
			Gun gun = m_gun;
			gun.OnPostFired = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnPostFired, new Action<PlayerController, Gun>(HandlePostFired));
		}
		if (DoesParticlesOnReload)
		{
			Gun gun2 = m_gun;
			gun2.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun2.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReload));
		}
	}

	private void LateUpdate()
	{
		if ((bool)TargetSystem)
		{
			if (m_gun.GetSprite().FlipY)
			{
				TargetSystem.transform.localPosition = m_localPositionOffset.WithY(m_localPositionOffset.y * -1f);
			}
			else
			{
				TargetSystem.transform.localPosition = m_localPositionOffset;
			}
		}
	}

	private void HandleReload(PlayerController arg1, Gun arg2, bool arg3)
	{
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.MEDIUM)
		{
			TargetSystem.Emit(UnityEngine.Random.Range(MinParticlesOnReload, MaxParticlesOnReload + 1));
		}
	}

	private void HandlePostFired(PlayerController arg1, Gun arg2)
	{
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.MEDIUM)
		{
			TargetSystem.Emit(UnityEngine.Random.Range(MinParticlesOnFire, MaxParticlesOnFire + 1));
		}
	}

	private void OnEnable()
	{
		if (DoesParticlesOnFire)
		{
			Gun gun = m_gun;
			gun.OnPostFired = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnPostFired, new Action<PlayerController, Gun>(HandlePostFired));
		}
		if (DoesParticlesOnReload)
		{
			Gun gun2 = m_gun;
			gun2.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun2.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReload));
		}
	}

	private void OnDisable()
	{
		Gun gun = m_gun;
		gun.OnPostFired = (Action<PlayerController, Gun>)Delegate.Remove(gun.OnPostFired, new Action<PlayerController, Gun>(HandlePostFired));
		Gun gun2 = m_gun;
		gun2.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Remove(gun2.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReload));
	}

	private void OnDestroy()
	{
		Gun gun = m_gun;
		gun.OnPostFired = (Action<PlayerController, Gun>)Delegate.Remove(gun.OnPostFired, new Action<PlayerController, Gun>(HandlePostFired));
		Gun gun2 = m_gun;
		gun2.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Remove(gun2.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReload));
	}
}
