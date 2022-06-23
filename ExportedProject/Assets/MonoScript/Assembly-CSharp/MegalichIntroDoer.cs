using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class MegalichIntroDoer : SpecificIntroDoer
{
	public GameObject head;

	public ScreenShakeSettings screenShake;

	private bool m_isFinished;

	private bool m_isCameraModified;

	public override Vector2? OverrideOutroPosition
	{
		get
		{
			ModifyCamera(true);
			BlockPitTiles(true);
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

	public void Start()
	{
		base.aiAnimator.SetBaseAnim("blank");
		base.spriteAnimator.Play("blank");
	}

	protected override void OnDestroy()
	{
		ModifyCamera(false);
		BlockPitTiles(false);
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		base.spriteAnimator.Play("blank");
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.ClearBaseAnim();
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		base.aiAnimator.PlayUntilCancelled("intro");
		float elapsed = 0f;
		for (float duration = 1f; elapsed < duration; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		GameManager.Instance.MainCameraController.DoContinuousScreenShake(screenShake, this);
		while (base.aiAnimator.IsPlaying("intro"))
		{
			yield return null;
		}
		base.aiAnimator.EndAnimationIf("intro");
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		m_isFinished = true;
	}

	public override void EndIntro()
	{
		StopAllCoroutines();
		base.aiAnimator.EndAnimationIf("intro");
		GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		AkSoundEngine.PostEvent("Play_MUS_lich_phase_02", base.gameObject);
	}

	public void ModifyCamera(bool value)
	{
		if (GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel && !GameManager.IsReturningToBreach && m_isCameraModified != value)
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			if (value)
			{
				mainCameraController.OverrideZoomScale = 0.75f;
				mainCameraController.LockToRoom = true;
				mainCameraController.AddFocusPoint(head);
				mainCameraController.controllerCamera.isTransitioning = false;
			}
			else
			{
				mainCameraController.SetZoomScaleImmediate(1f);
				mainCameraController.LockToRoom = false;
				mainCameraController.RemoveFocusPoint(head);
			}
			m_isCameraModified = value;
		}
	}

	public void BlockPitTiles(bool value)
	{
		if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach || GameManager.Instance.Dungeon == null)
		{
			return;
		}
		IntVector2 basePosition = base.aiActor.ParentRoom.area.basePosition;
		IntVector2 intVector = base.aiActor.ParentRoom.area.basePosition + base.aiActor.ParentRoom.area.dimensions - IntVector2.One;
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = basePosition.x; i <= intVector.x; i++)
		{
			for (int j = basePosition.y; j <= intVector.y; j++)
			{
				CellData cellData = data[i, j];
				if (cellData != null && cellData.type == CellType.PIT)
				{
					cellData.IsPlayerInaccessible = value;
				}
			}
		}
	}
}
