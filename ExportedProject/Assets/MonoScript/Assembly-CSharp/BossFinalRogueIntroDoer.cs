using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BossFinalRogueIntroDoer : SpecificIntroDoer
{
	private bool m_isFinished;

	public override Vector2? OverrideIntroPosition
	{
		get
		{
			GameManager.Instance.MainCameraController.OverrideZoomScale = 0.6666f;
			return GetComponent<BossFinalRogueController>().CameraPos;
		}
	}

	public override bool IsIntroFinished
	{
		get
		{
			return m_isFinished;
		}
	}

	public override Vector2? OverrideOutroPosition
	{
		get
		{
			BossFinalRogueController component = GetComponent<BossFinalRogueController>();
			component.InitCamera();
			return component.CameraPos;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		yield return TimeInvariantWait(1f);
		m_isFinished = true;
	}

	public override void EndIntro()
	{
		BossFinalRogueController component = GetComponent<BossFinalRogueController>();
		component.InitCamera();
		GameManager.Instance.MainCameraController.SetManualControl(true);
		GameManager.Instance.MainCameraController.OverrideZoomScale = 0.6666f;
	}
}
