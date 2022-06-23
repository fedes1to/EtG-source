using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class AdvancedDraGunIntroDoer : SpecificIntroDoer
{
	private bool m_isFinished;

	private tk2dSpriteAnimator m_introDummy;

	private tk2dSpriteAnimator m_introBabyDummy;

	private tk2dSpriteAnimator m_introVfxDummy;

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
		m_introBabyDummy = base.transform.Find("IntroDummy/baby").GetComponent<tk2dSpriteAnimator>();
		m_introVfxDummy = base.transform.Find("IntroDummy/vfx").GetComponent<tk2dSpriteAnimator>();
		m_introDummy.aiAnimator = base.aiAnimator;
		m_introBabyDummy.aiAnimator = base.aiAnimator;
		m_introVfxDummy.aiAnimator = base.aiAnimator;
		m_introVfxDummy.sprite.usesOverrideMaterial = false;
		m_neck = base.transform.Find("Neck").gameObject;
		m_wings = base.transform.Find("Wings").gameObject;
		m_leftArm = base.transform.Find("LeftArm").gameObject;
		m_rightArm = base.transform.Find("RightArm").gameObject;
		m_introDummy.gameObject.SetActive(true);
		m_introBabyDummy.gameObject.SetActive(true);
		m_introVfxDummy.gameObject.SetActive(true);
		base.renderer.enabled = false;
		m_neck.SetActive(false);
		m_wings.SetActive(false);
		m_leftArm.SetActive(false);
		m_rightArm.SetActive(false);
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
		animators.Add(m_introBabyDummy);
		animators.Add(m_introVfxDummy);
		GetComponent<DragunCracktonMap>().ConvertToGold();
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		m_introDummy.Play("intro");
		m_introBabyDummy.Play("intro_baby");
		m_introVfxDummy.Play("intro_vfx");
		m_introVfxDummy.sprite.usesOverrideMaterial = false;
		while (m_introDummy.IsPlaying("intro"))
		{
			yield return null;
		}
		m_introDummy.gameObject.SetActive(false);
		m_introBabyDummy.gameObject.SetActive(false);
		m_introVfxDummy.gameObject.SetActive(false);
		base.renderer.enabled = true;
		m_neck.SetActive(true);
		m_wings.SetActive(true);
		m_leftArm.SetActive(true);
		m_rightArm.SetActive(true);
		base.aiAnimator.EndAnimation();
		GetComponent<DraGunController>().ModifyCamera(true);
		m_isFinished = true;
	}

	public override void EndIntro()
	{
		m_introDummy.gameObject.SetActive(false);
		m_introBabyDummy.gameObject.SetActive(false);
		m_introVfxDummy.gameObject.SetActive(false);
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
