using System;
using System.Collections;
using UnityEngine;

public class RatBootsItem : PassiveItem
{
	public float HoverTime = 2f;

	public float FlickerPortion = 0.5f;

	public float FlickerFrequency = 0.1f;

	public GameObject FloorVFX;

	public tk2dSpriteAnimation RatAnimationLibrary;

	private tk2dSprite m_extantFloor;

	private bool m_transformed;

	private PlayerController m_lastPlayer;

	private bool m_frameWasPartialPit;

	private bool m_invulnerable;

	private bool m_wasAboutToFallLastFrame;

	private float m_elapsedAboutToFall;

	private int m_lastFrameAboutToFall;

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnAboutToFall = (Func<bool, bool>)Delegate.Combine(player.OnAboutToFall, new Func<bool, bool>(HandleAboutToFall));
	}

	protected void EnableShader(PlayerController user)
	{
		if (!user)
		{
			return;
		}
		Material[] array = user.SetOverrideShader(ShaderCache.Acquire("Brave/Internal/RainbowChestShader"));
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == null))
			{
				array[i].SetFloat("_AllColorsToggle", 1f);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)base.Owner && (bool)m_extantFloor)
		{
			Vector2 centerPosition = base.Owner.CenterPosition;
			m_extantFloor.renderer.sharedMaterial.SetVector("_PlayerPos", new Vector4(centerPosition.x, centerPosition.y, 0f, 0f));
		}
		if (Time.timeScale <= 0f)
		{
			m_lastFrameAboutToFall = Time.frameCount;
		}
		else
		{
			if (!m_wasAboutToFallLastFrame && (bool)m_extantFloor)
			{
				SpawnManager.Despawn(m_extantFloor.gameObject);
				m_extantFloor = null;
			}
			m_wasAboutToFallLastFrame = false;
		}
		ProcessRatStatus(base.Owner);
	}

	private void ProcessRatStatus(PlayerController player, bool forceDisable = false)
	{
		bool flag = (bool)player && player.HasActiveBonusSynergy(CustomSynergyType.RESOURCEFUL_RAT) && !forceDisable;
		if (flag && !m_transformed)
		{
			m_lastPlayer = player;
			if ((bool)player)
			{
				m_transformed = true;
				player.OverrideAnimationLibrary = RatAnimationLibrary;
				player.SetOverrideShader(ShaderCache.Acquire(player.LocalShaderName));
				if (player.characterIdentity == PlayableCharacters.Eevee)
				{
					player.GetComponent<CharacterAnimationRandomizer>().AddOverrideAnimLibrary(RatAnimationLibrary);
				}
				player.PlayerIsRatTransformed = true;
				player.stats.RecalculateStats(player);
			}
		}
		else
		{
			if (!m_transformed || flag)
			{
				return;
			}
			if ((bool)m_lastPlayer)
			{
				m_lastPlayer.OverrideAnimationLibrary = null;
				m_lastPlayer.ClearOverrideShader();
				if (m_lastPlayer.characterIdentity == PlayableCharacters.Eevee)
				{
					m_lastPlayer.GetComponent<CharacterAnimationRandomizer>().RemoveOverrideAnimLibrary(RatAnimationLibrary);
				}
				m_lastPlayer.PlayerIsRatTransformed = false;
				m_lastPlayer.stats.RecalculateStats(m_lastPlayer);
				m_lastPlayer = null;
			}
			m_transformed = false;
		}
	}

	private void LateUpdate()
	{
		if ((bool)m_extantFloor)
		{
			m_extantFloor.UpdateZDepth();
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		player.OnAboutToFall = (Func<bool, bool>)Delegate.Remove(player.OnAboutToFall, new Func<bool, bool>(HandleAboutToFall));
		if (m_invulnerable)
		{
			player.healthHaver.IsVulnerable = true;
		}
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if ((bool)base.Owner)
		{
			PlayerController owner = base.Owner;
			owner.OnAboutToFall = (Func<bool, bool>)Delegate.Remove(owner.OnAboutToFall, new Func<bool, bool>(HandleAboutToFall));
			if (m_invulnerable)
			{
				base.Owner.healthHaver.IsVulnerable = true;
			}
		}
		if (m_transformed)
		{
			ProcessRatStatus(null, true);
		}
		base.OnDestroy();
	}

	private IEnumerator HandleInvulnerability()
	{
		m_invulnerable = true;
		EnableShader(base.Owner);
		while ((bool)m_extantFloor && !m_frameWasPartialPit)
		{
			if ((bool)base.Owner)
			{
				base.Owner.healthHaver.IsVulnerable = false;
			}
			yield return null;
		}
		if ((bool)base.Owner)
		{
			base.Owner.ClearOverrideShader();
		}
		if ((bool)base.Owner)
		{
			base.Owner.healthHaver.IsVulnerable = true;
		}
		m_invulnerable = false;
	}

	private bool HandleAboutToFall(bool partialPit)
	{
		if ((bool)base.Owner && base.Owner.IsFlying)
		{
			return false;
		}
		if (!partialPit && !m_invulnerable)
		{
			StartCoroutine(HandleInvulnerability());
		}
		m_frameWasPartialPit = partialPit;
		m_wasAboutToFallLastFrame = true;
		if (Time.frameCount <= m_lastFrameAboutToFall)
		{
			m_lastFrameAboutToFall = Time.frameCount - 1;
		}
		if (Time.frameCount != m_lastFrameAboutToFall + 1)
		{
			m_elapsedAboutToFall = 0f;
		}
		if (partialPit)
		{
			m_elapsedAboutToFall = 0f;
		}
		m_lastFrameAboutToFall = Time.frameCount;
		m_elapsedAboutToFall += BraveTime.DeltaTime;
		if (m_elapsedAboutToFall < HoverTime)
		{
			if (!m_extantFloor)
			{
				GameObject gameObject = SpawnManager.SpawnVFX(FloorVFX);
				gameObject.transform.parent = base.Owner.transform;
				tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
				component.PlaceAtPositionByAnchor(base.Owner.SpriteBottomCenter, tk2dBaseSprite.Anchor.MiddleCenter);
				component.IsPerpendicular = false;
				component.HeightOffGround = -2.25f;
				component.UpdateZDepth();
				m_extantFloor = component;
			}
			if (m_elapsedAboutToFall > HoverTime - FlickerPortion)
			{
				bool flag = Mathf.PingPong(m_elapsedAboutToFall - (HoverTime - FlickerPortion), FlickerFrequency * 2f) < FlickerFrequency;
				m_extantFloor.renderer.enabled = flag;
			}
			else
			{
				m_extantFloor.renderer.enabled = true;
			}
			return false;
		}
		if ((bool)m_extantFloor)
		{
			SpawnManager.Despawn(m_extantFloor.gameObject);
			m_extantFloor = null;
		}
		return true;
	}
}
