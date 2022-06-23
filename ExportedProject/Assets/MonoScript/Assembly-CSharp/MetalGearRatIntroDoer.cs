using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class MetalGearRatIntroDoer : SpecificIntroDoer
{
	public GameObject head;

	private bool m_initialized;

	private GameObject m_introDummy;

	private tk2dSpriteAnimator m_introLightsDummy;

	private bool m_isFinished;

	private bool m_isCameraModified;

	private bool m_musicStarted;

	public override Vector2? OverrideOutroPosition
	{
		get
		{
			ModifyCamera(true);
			return null;
		}
	}

	public override bool IsIntroFinished
	{
		get
		{
			return m_isFinished;
		}
	}

	private void Awake()
	{
		m_introDummy = base.transform.Find("intro dummy").gameObject;
		m_introLightsDummy = base.transform.Find("intro dummy lights").GetComponent<tk2dSpriteAnimator>();
	}

	protected override void OnDestroy()
	{
		ModifyCamera(false);
		base.OnDestroy();
	}

	public void Init()
	{
		if (!m_initialized)
		{
			GameManager.Instance.Dungeon.OverrideAmbientLight = true;
			GameManager.Instance.Dungeon.OverrideAmbientColor = base.aiActor.ParentRoom.area.runtimePrototypeData.customAmbient * 0.1f;
			base.aiActor.ParentRoom.BecomeTerrifyingDarkRoom(0f, 0.1f, 1f, "Play_Empty_Event_01");
			AkSoundEngine.PostEvent("Play_BOSS_RatMech_Ambience_01", base.gameObject);
			GameManager.Instance.Dungeon.OverrideAmbientLight = true;
			GameManager.Instance.Dungeon.OverrideAmbientColor = base.aiActor.ParentRoom.area.runtimePrototypeData.customAmbient * 0.1f;
			base.aiAnimator.PlayUntilCancelled("blank");
			base.aiAnimator.ChildAnimator.PlayUntilCancelled("blank");
			base.aiAnimator.ChildAnimator.ChildAnimator.PlayUntilCancelled("blank");
			m_introDummy.SetActive(true);
			GameManager.Instance.StartCoroutine(MusicStopperCR());
			m_initialized = true;
		}
	}

	private IEnumerator MusicStopperCR()
	{
		for (int i = 0; i < 3; i++)
		{
			if ((bool)this && !m_musicStarted)
			{
				AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
			}
			yield return null;
		}
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		base.PlayerWalkedIn(player, animators);
		Init();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		animators.Add(base.aiAnimator.ChildAnimator.ChildAnimator.spriteAnimator);
		base.aiAnimator.ChildAnimator.ChildAnimator.spriteAnimator.enabled = false;
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		m_introDummy.SetActive(true);
		m_introLightsDummy.gameObject.SetActive(true);
		m_introLightsDummy.Play("intro_light");
		float elapsed3 = 0f;
		float duration3;
		for (duration3 = m_introLightsDummy.CurrentClip.BaseClipLength; elapsed3 < duration3; elapsed3 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		Material material = m_introDummy.GetComponent<Renderer>().material;
		elapsed3 = 0f;
		duration3 = 1f;
		while (elapsed3 < duration3)
		{
			yield return null;
			elapsed3 += GameManager.INVARIANT_DELTA_TIME;
			material.SetFloat("_ValueMinimum", elapsed3 / duration3 * 0.5f);
		}
		m_introDummy.SetActive(false);
		m_introLightsDummy.gameObject.SetActive(false);
		base.aiAnimator.EndAnimationIf("blank");
		base.aiAnimator.ChildAnimator.EndAnimationIf("blank");
		base.aiAnimator.ChildAnimator.ChildAnimator.EndAnimationIf("blank");
		base.aiAnimator.PlayUntilCancelled("intro");
		GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_Rat_Theme_02", base.gameObject);
		m_musicStarted = true;
		elapsed3 = 0f;
		for (duration3 = base.aiAnimator.CurrentClipLength; elapsed3 < duration3; elapsed3 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		base.aiAnimator.EndAnimationIf("intro");
		m_isFinished = true;
	}

	public override void OnCleanup()
	{
		base.aiAnimator.ChildAnimator.ChildAnimator.spriteAnimator.enabled = true;
		base.OnCleanup();
	}

	public override void EndIntro()
	{
		StopAllCoroutines();
		base.aiAnimator.EndAnimationIf("intro");
		m_introDummy.SetActive(false);
		m_introLightsDummy.gameObject.SetActive(false);
		base.aiAnimator.EndAnimationIf("blank");
		base.aiAnimator.ChildAnimator.EndAnimationIf("blank");
		base.aiAnimator.ChildAnimator.ChildAnimator.EndAnimationIf("blank");
		GameManager.Instance.Dungeon.OverrideAmbientLight = false;
		base.aiActor.ParentRoom.EndTerrifyingDarkRoom(1f, 0.1f, 1f, "Play_Empty_Event_01");
		GameManager.Instance.Dungeon.OverrideAmbientLight = false;
		if (!m_musicStarted)
		{
			GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_Rat_Theme_02", base.gameObject);
			m_musicStarted = true;
		}
	}

	public void ModifyCamera(bool value)
	{
		if (GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel && !GameManager.IsReturningToBreach && m_isCameraModified != value)
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			if (value)
			{
				mainCameraController.OverrideZoomScale = 0.66f;
				mainCameraController.LockToRoom = true;
				mainCameraController.AddFocusPoint(head);
				mainCameraController.controllerCamera.isTransitioning = false;
				Projectile.SetGlobalProjectileDepth(4f);
				BasicBeamController.SetGlobalBeamHeight(4f);
			}
			else
			{
				mainCameraController.SetZoomScaleImmediate(1f);
				mainCameraController.LockToRoom = false;
				mainCameraController.RemoveFocusPoint(head);
				Projectile.ResetGlobalProjectileDepth();
				BasicBeamController.ResetGlobalBeamHeight();
			}
			m_isCameraModified = value;
		}
	}
}
