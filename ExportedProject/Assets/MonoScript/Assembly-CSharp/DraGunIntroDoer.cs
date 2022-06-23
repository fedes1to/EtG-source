using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class DraGunIntroDoer : SpecificIntroDoer
{
	private bool m_isFinished;

	private tk2dSpriteAnimator m_introDummy;

	private tk2dSpriteAnimator m_transitionDummy;

	private tk2dSpriteAnimator m_deathDummy;

	private GameObject m_neck;

	private GameObject m_wings;

	private GameObject m_leftArm;

	private GameObject m_rightArm;

	public override Vector2? OverrideIntroPosition
	{
		get
		{
			return base.specRigidbody.UnitCenter + new Vector2(0f, 4f);
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
		base.aiActor.IgnoreForRoomClear = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override IntVector2 OverrideExitBasePosition(DungeonData.Direction directionToWalk, IntVector2 exitBaseCenter)
	{
		return exitBaseCenter + new IntVector2(0, DraGunRoomPlaceable.HallHeight);
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		m_introDummy = base.transform.Find("IntroDummy").GetComponent<tk2dSpriteAnimator>();
		m_transitionDummy = base.transform.Find("TransitionDummy").GetComponent<tk2dSpriteAnimator>();
		m_deathDummy = base.transform.Find("DeathDummy").GetComponent<tk2dSpriteAnimator>();
		m_introDummy.aiAnimator = base.aiAnimator;
		m_transitionDummy.aiAnimator = base.aiAnimator;
		m_deathDummy.aiAnimator = base.aiAnimator;
		m_neck = base.transform.Find("Neck").gameObject;
		m_wings = base.transform.Find("Wings").gameObject;
		m_leftArm = base.transform.Find("LeftArm").gameObject;
		m_rightArm = base.transform.Find("RightArm").gameObject;
		m_introDummy.gameObject.SetActive(true);
		m_transitionDummy.gameObject.SetActive(false);
		base.renderer.enabled = false;
		m_neck.SetActive(false);
		m_wings.SetActive(false);
		m_leftArm.SetActive(false);
		m_rightArm.SetActive(false);
		base.aiActor.IgnoreForRoomClear = false;
		base.aiActor.ParentRoom.SealRoom();
		StartCoroutine(RunEmbers());
	}

	private IEnumerator RunEmbers()
	{
		DraGunRoomPlaceable emberDoer = base.aiActor.ParentRoom.GetComponentsAbsoluteInRoom<DraGunRoomPlaceable>()[0];
		emberDoer.UseInvariantTime = true;
		while (!m_isFinished)
		{
			yield return null;
		}
		emberDoer.UseInvariantTime = false;
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		animators.Add(m_introDummy);
		animators.Add(m_transitionDummy);
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		m_introDummy.Play("intro");
		while (m_introDummy.IsPlaying("intro"))
		{
			yield return null;
		}
		m_introDummy.gameObject.SetActive(false);
		m_transitionDummy.gameObject.SetActive(true);
		m_transitionDummy.Play("roar");
		while (m_transitionDummy.IsPlaying("roar"))
		{
			yield return null;
		}
		m_transitionDummy.Play("idle");
		GetComponent<DraGunController>().ModifyCamera(true);
		m_isFinished = true;
	}

	public override void EndIntro()
	{
		m_introDummy.gameObject.SetActive(false);
		m_transitionDummy.gameObject.SetActive(false);
		base.renderer.enabled = true;
		m_neck.SetActive(true);
		m_wings.SetActive(true);
		m_leftArm.SetActive(true);
		m_rightArm.SetActive(true);
		base.aiAnimator.EndAnimation();
		DraGunController component = GetComponent<DraGunController>();
		component.ModifyCamera(true);
		component.BlockPitTiles(true);
		component.HasDoneIntro = true;
		m_isFinished = true;
	}
}
