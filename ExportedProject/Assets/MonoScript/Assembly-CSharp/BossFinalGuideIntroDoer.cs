using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BossFinalGuideIntroDoer : SpecificIntroDoer
{
	private AIAnimator m_topAnimator;

	private AIAnimator m_drAnimator;

	private bool m_finished;

	public override bool IsIntroFinished
	{
		get
		{
			return m_finished;
		}
	}

	public void Start()
	{
		m_topAnimator = base.aiAnimator.ChildAnimator;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.FacingDirection = -90f;
		m_topAnimator.FacingDirection = -90f;
		List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
		for (int i = 0; i < allHealthHavers.Count; i++)
		{
			if (!allHealthHavers[i].IsBoss && allHealthHavers[i].name.Contains("DrWolf", true))
			{
				ObjectVisibilityManager component = allHealthHavers[i].GetComponent<ObjectVisibilityManager>();
				component.ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
				allHealthHavers[i].specRigidbody.CollideWithOthers = true;
				allHealthHavers[i].aiActor.IsGone = false;
				allHealthHavers[i].aiActor.State = AIActor.ActorState.Normal;
				m_drAnimator = allHealthHavers[i].aiAnimator;
				animators.Add(m_drAnimator.spriteAnimator);
				break;
			}
		}
		base.aiAnimator.renderer.enabled = false;
		SpriteOutlineManager.ToggleOutlineRenderers(base.aiAnimator.sprite, false);
		base.aiAnimator.ChildAnimator.renderer.enabled = false;
		SpriteOutlineManager.ToggleOutlineRenderers(base.aiAnimator.ChildAnimator.sprite, false);
		Object.FindObjectOfType<DungeonDoorSubsidiaryBlocker>().Seal();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		m_topAnimator.PlayUntilFinished("intro");
		m_drAnimator.PlayUntilFinished("intro");
		StartCoroutine(DoIntro());
	}

	public override void EndIntro()
	{
		base.aiAnimator.renderer.enabled = true;
		SpriteOutlineManager.ToggleOutlineRenderers(base.aiAnimator.sprite, true);
		base.aiAnimator.ChildAnimator.renderer.enabled = true;
		SpriteOutlineManager.ToggleOutlineRenderers(base.aiAnimator.ChildAnimator.sprite, true);
		base.aiActor.ToggleShadowVisiblity(true);
		m_finished = true;
		StopAllCoroutines();
		m_topAnimator.EndAnimationIf("intro");
		m_drAnimator.EndAnimationIf("intro");
	}

	private IEnumerator DoIntro()
	{
		AkSoundEngine.PostEvent("Play_BOSS_cyborg_drop_01", base.gameObject);
		base.aiAnimator.renderer.enabled = true;
		SpriteOutlineManager.ToggleOutlineRenderers(base.aiAnimator.sprite, true);
		base.aiAnimator.ChildAnimator.renderer.enabled = true;
		SpriteOutlineManager.ToggleOutlineRenderers(base.aiAnimator.ChildAnimator.sprite, true);
		Vector3 startShadowPosition = base.aiActor.ShadowObject.transform.position;
		Vector3 startPosition = base.aiAnimator.transform.position;
		float elapsed2 = 0f;
		float duration2 = 1f;
		while (elapsed2 < duration2)
		{
			elapsed2 += GameManager.INVARIANT_DELTA_TIME;
			base.aiAnimator.transform.position = new Vector3(startPosition.x, startPosition.y + Mathf.Lerp(14f, 0f, elapsed2 / duration2), startPosition.z).Quantize(0.0625f);
			base.aiActor.ShadowObject.transform.position = startShadowPosition;
			yield return null;
		}
		GameManager.Instance.MainCameraController.DoScreenShake(1f, 8f, 0.25f, 0.125f, null);
		base.aiActor.ShadowObject.transform.position = startShadowPosition;
		base.aiActor.ToggleShadowVisiblity(true);
		elapsed2 = 0f;
		for (duration2 = 3f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		m_finished = true;
	}
}
