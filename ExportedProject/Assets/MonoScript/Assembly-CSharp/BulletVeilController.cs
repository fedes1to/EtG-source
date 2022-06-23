using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class BulletVeilController : BraveBehaviour
{
	public tk2dSpriteAnimator VeilAnimator;

	public string OpenVeilAnimName;

	public string CloseVeilAnimName;

	public BulletCurtainParticleController[] ParticleControllers;

	public GameObject DepartureVFX;

	public GameObject ArrivalVFX;

	private bool m_isOpen;

	private bool m_hasWarped;

	private RoomHandler m_parentRoom;

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		m_parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		WarpPointHandler wph = GetComponent<WarpPointHandler>();
		wph.OnPreWarp = (Func<PlayerController, float>)Delegate.Combine(wph.OnPreWarp, new Func<PlayerController, float>(OnPreWarp));
		wph.OnWarping = (Func<PlayerController, float>)Delegate.Combine(wph.OnWarping, new Func<PlayerController, float>(OnWarping));
		wph.OnWarpDone = (Func<PlayerController, float>)Delegate.Combine(wph.OnWarpDone, new Func<PlayerController, float>(OnWarpDone));
		UnityEngine.Object.Instantiate(BraveResources.Load("temp programmer art/Madness/EndTimes"), new Vector3(-1000f, 0f, 0f), Quaternion.identity);
	}

	private float OnPreWarp(PlayerController p)
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(p);
			if ((bool)otherPlayer && otherPlayer.IsGhost)
			{
				otherPlayer.ResurrectFromBossKill();
			}
		}
		p.IsOnFire = false;
		p.CurrentFireMeterValue = 0f;
		p.CurrentPoisonMeterValue = 0f;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer2 = GameManager.Instance.GetOtherPlayer(p);
			if ((bool)otherPlayer2)
			{
				otherPlayer2.IsOnFire = false;
				otherPlayer2.CurrentFireMeterValue = 0f;
				otherPlayer2.CurrentPoisonMeterValue = 0f;
			}
		}
		p.specRigidbody.Velocity = Vector2.zero;
		p.SetInputOverride("bullet veil");
		p.ToggleHandRenderers(false, "bullet veil");
		p.ToggleGunRenderers(false, "bullet veil");
		p.ToggleShadowVisiblity(false);
		p.ForceMoveToPoint(p.CenterPosition + Vector2.up * 3f, 0f, 0.5f);
		if (DepartureVFX != null)
		{
			SpawnManager.SpawnVFX(DepartureVFX, p.CenterPosition.ToVector3ZisY(), Quaternion.identity);
		}
		Minimap.Instance.ToggleMinimap(false);
		Minimap.Instance.TemporarilyPreventMinimap = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		Pixelator.Instance.FadeToBlack(0.25f, false, 0.25f);
		return 0.5f;
	}

	private void HandleDoorwayAnimationComplete(tk2dSpriteAnimator anim, tk2dSpriteAnimationClip clip)
	{
		anim.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(anim.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(HandleDoorwayAnimationComplete));
		anim.transform.parent.GetComponent<PlayerController>().IsVisible = false;
	}

	private void Update()
	{
		if (!m_isOpen && !m_hasWarped)
		{
			PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(base.specRigidbody.UnitCenter);
			if (activePlayerClosestToPoint != null && activePlayerClosestToPoint.CurrentRoom == m_parentRoom && Vector2.Distance(base.specRigidbody.UnitCenter, activePlayerClosestToPoint.CenterPosition) < 6f)
			{
				m_isOpen = true;
				VeilAnimator.Play(OpenVeilAnimName);
				StartCoroutine(HandleVeilParticles(false));
			}
		}
		else if (m_isOpen && !m_hasWarped)
		{
			PlayerController activePlayerClosestToPoint2 = GameManager.Instance.GetActivePlayerClosestToPoint(base.specRigidbody.UnitCenter);
			if (activePlayerClosestToPoint2 != null && activePlayerClosestToPoint2.CurrentRoom == m_parentRoom && Vector2.Distance(base.specRigidbody.UnitCenter, activePlayerClosestToPoint2.CenterPosition) > 6f)
			{
				m_isOpen = false;
				StartCoroutine(HandleVeilParticles(true));
			}
		}
	}

	private IEnumerator HandleVeilParticles(bool reverse)
	{
		AkSoundEngine.PostEvent("Play_OBJ_shells_shower_01", base.gameObject);
		float ela = 0f;
		float duration = ((!reverse) ? 0.5f : 1.5f);
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			float t = ela / duration;
			if (reverse)
			{
				t = 1f - t;
			}
			for (int i = 0; i < ParticleControllers.Length; i++)
			{
				if ((bool)ParticleControllers[i])
				{
					ParticleControllers[i].LocalYMax = Mathf.Lerp(0f, 2.5f, t);
				}
			}
			yield return null;
		}
	}

	private void ActivateEndTimes()
	{
		Minimap.Instance.ToggleMinimap(false);
		GameManager.Instance.Dungeon.IsEndTimes = true;
		Minimap.Instance.TemporarilyPreventMinimap = true;
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].CurrentInputState = PlayerInputState.FoyerInputOnly;
		}
		EndTimesNebulaController endTimesNebulaController = UnityEngine.Object.FindObjectOfType<EndTimesNebulaController>();
		endTimesNebulaController.BecomeActive();
		Pixelator.Instance.DoOcclusionLayer = false;
	}

	private float OnWarping(PlayerController player)
	{
		player.ClearInputOverride("bullet veil");
		player.ToggleShadowVisiblity(true);
		player.ToggleHandRenderers(true, "bullet veil");
		player.ToggleGunRenderers(true, "bullet veil");
		player.IsVisible = true;
		GetComponent<WarpPointHandler>().OnWarping = null;
		ActivateEndTimes();
		TimeTubeCreditsController.AcquireTunnelInstanceInAdvance();
		GameManager.Instance.DungeonMusicController.SwitchToEndTimesMusic();
		return 0.5f;
	}

	private float OnWarpDone(PlayerController player)
	{
		AkSoundEngine.PostEvent("State_ENV_Dimension_01", base.gameObject);
		Pixelator.Instance.FadeToBlack(0.25f, true, 0.1f);
		GetComponent<WarpPointHandler>().OnWarpDone = null;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].DoSpinfallSpawn(0.1f * (float)(i + 1));
		}
		if (ArrivalVFX != null)
		{
			SpawnManager.SpawnVFX(ArrivalVFX, player.CenterPosition.ToVector3ZisY(), Quaternion.identity);
		}
		return 0.4f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
