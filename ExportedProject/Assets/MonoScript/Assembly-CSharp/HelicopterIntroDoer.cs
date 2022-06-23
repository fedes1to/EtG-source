using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class HelicopterIntroDoer : SpecificIntroDoer
{
	private bool m_isFinished;

	public bool IsCameraModified { get; set; }

	public override bool IsIntroFinished
	{
		get
		{
			return m_isFinished && base.IsIntroFinished;
		}
	}

	public override Vector2? OverrideOutroPosition
	{
		get
		{
			ModifyCamera(true);
			return null;
		}
	}

	protected override void OnDestroy()
	{
		ModifyCamera(false);
		base.OnDestroy();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		base.StartIntro(animators);
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		TextBoxManager.TIME_INVARIANT = true;
		yield return StartCoroutine(GetComponent<VoiceOverer>().HandleIntroVO());
		TextBoxManager.TIME_INVARIANT = false;
		m_isFinished = true;
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		mainCameraController.SetZoomScaleImmediate(0.75f);
		AkSoundEngine.PostEvent("Play_boss_helicopter_loop_01", base.gameObject);
		AkSoundEngine.PostEvent("Play_State_Volume_Lower_01", base.gameObject);
		base.aiActor.ParentRoom.CompletelyPreventLeaving = true;
	}

	public void ModifyCamera(bool value)
	{
		if (GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel && !GameManager.IsReturningToBreach && IsCameraModified != value)
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			if (value)
			{
				mainCameraController.OverrideZoomScale = 0.75f;
				mainCameraController.LockToRoom = true;
				mainCameraController.controllerCamera.isTransitioning = false;
			}
			else
			{
				mainCameraController.SetZoomScaleImmediate(1f);
				mainCameraController.LockToRoom = false;
				AkSoundEngine.PostEvent("Stop_State_Volume_Lower_01", base.gameObject);
			}
			IsCameraModified = value;
		}
	}
}
