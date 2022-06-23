using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class DemonWallIntroDoer : SpecificIntroDoer
{
	public string preIntro;

	public override Vector2? OverrideOutroPosition
	{
		get
		{
			return GetComponent<DemonWallController>().CameraPos;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void OnCameraIntro()
	{
		base.aiAnimator.PlayUntilCancelled(preIntro);
	}

	public override void OnCleanup()
	{
		base.aiAnimator.EndAnimation();
	}

	public override void EndIntro()
	{
		GetComponent<DemonWallController>().ModifyCamera(true);
	}
}
