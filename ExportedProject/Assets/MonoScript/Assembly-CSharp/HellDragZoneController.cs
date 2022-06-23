using System;
using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;

public class HellDragZoneController : BraveBehaviour
{
	public GameObject HoleObject;

	public GameObject HellDragVFX;

	public GameObject CryoButtonPrefab;

	private Material HoleMaterial;

	private bool m_holeIsActive;

	public string cryoAnimatorName;

	public string cryoArriveAnimation;

	public string cyroDepartAnimation;

	private tk2dSpriteAnimator cryoAnimator;

	private FsmBool m_cryoBool;

	private FsmBool m_normalBool;

	private void Start()
	{
		HoleObject.SetActive(false);
		HoleMaterial = HoleObject.GetComponent<MeshRenderer>().material;
		bool flag = GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Eevee && GameStatsManager.Instance.AllCorePastsBeaten() && !GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLINGER_UNLOCKED);
		if ((GameManager.Instance.PrimaryPlayer.characterIdentity != PlayableCharacters.Gunslinger || GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLINGER_UNLOCKED)) && (!GameManager.Instance.PrimaryPlayer.CharacterUsesRandomGuns || GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_LICH)) && GameStatsManager.Instance.AllCorePastsBeaten())
		{
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_BULLET_HELL) == 0f || flag)
			{
				SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
				speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(ProcessTriggerEntered));
			}
			else
			{
				HoleObject.SetActive(true);
				SetHoleSize(0.25f);
				m_holeIsActive = true;
				SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
				speculativeRigidbody2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(ProcessTriggerFrame));
			}
		}
		if (m_holeIsActive && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_BULLET_HELL) >= 1f)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(CryoButtonPrefab);
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = new Vector3(-0.5625f, -0.875f, 0f);
			gameObject.GetComponent<SpeculativeRigidbody>().Reinitialize();
			TalkDoerLite componentInChildren = gameObject.GetComponentInChildren<TalkDoerLite>();
			componentInChildren.GetAbsoluteParentRoom().RegisterInteractable(componentInChildren);
			componentInChildren.OnGenericFSMActionA = (Action)Delegate.Combine(componentInChildren.OnGenericFSMActionA, new Action(SwitchToCryoElevator));
			componentInChildren.OnGenericFSMActionB = (Action)Delegate.Combine(componentInChildren.OnGenericFSMActionB, new Action(RescindCryoElevator));
			m_cryoBool = componentInChildren.playmakerFsm.FsmVariables.GetFsmBool("IS_CRYO");
			m_normalBool = componentInChildren.playmakerFsm.FsmVariables.GetFsmBool("IS_NORMAL");
			m_cryoBool.Value = false;
			m_normalBool.Value = true;
			Transform transform = gameObject.transform.Find(cryoAnimatorName);
			if ((bool)transform)
			{
				cryoAnimator = transform.GetComponent<tk2dSpriteAnimator>();
			}
		}
	}

	private void RescindCryoElevator()
	{
		m_cryoBool.Value = false;
		m_normalBool.Value = true;
		if ((bool)cryoAnimator && !string.IsNullOrEmpty(cyroDepartAnimation))
		{
			cryoAnimator.Play(cyroDepartAnimation);
		}
	}

	private void SwitchToCryoElevator()
	{
		m_cryoBool.Value = true;
		m_normalBool.Value = false;
		if ((bool)cryoAnimator && !string.IsNullOrEmpty(cryoArriveAnimation))
		{
			cryoAnimator.Play(cryoArriveAnimation);
		}
	}

	private void ProcessTriggerFrame(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (m_holeIsActive)
		{
			PlayerController component = specRigidbody.GetComponent<PlayerController>();
			if ((bool)component && Vector2.Distance(component.CenterPosition, HoleObject.transform.PositionVector2()) < 2.5f)
			{
				GrabPlayer(component);
				m_holeIsActive = false;
			}
		}
	}

	private void SetHoleSize(float size)
	{
		HoleMaterial.SetFloat("_UVDistCutoff", size);
	}

	private IEnumerator LerpHoleSize(float startSize, float endSize, float duration, PlayerController targetPlayer)
	{
		float ela = 0f;
		while (ela < duration)
		{
			HoleObject.transform.position = targetPlayer.SpriteBottomCenter.XY().ToVector3ZisY();
			ela += BraveTime.DeltaTime;
			SetHoleSize(Mathf.Lerp(startSize, endSize, ela / duration));
			yield return null;
		}
	}

	private IEnumerator HandleGrabbyGrab(PlayerController grabbedPlayer)
	{
		grabbedPlayer.specRigidbody.Velocity = Vector2.zero;
		grabbedPlayer.specRigidbody.CapVelocity = true;
		grabbedPlayer.specRigidbody.MaxVelocity = Vector2.zero;
		yield return new WaitForSeconds(0.2f);
		grabbedPlayer.IsVisible = false;
		yield return new WaitForSeconds(2.3f);
		grabbedPlayer.specRigidbody.CapVelocity = false;
		Pixelator.Instance.FadeToBlack(0.5f);
		if (m_cryoBool != null && m_cryoBool.Value)
		{
			AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
			GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.HELLGEON);
			float delay = 0.6f;
			GameManager.Instance.DelayedLoadCharacterSelect(delay, true, true);
		}
		else
		{
			GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.HELLGEON);
			GameManager.Instance.DelayedLoadNextLevel(0.5f);
		}
	}

	private void GrabPlayer(PlayerController enteredPlayer)
	{
		enteredPlayer.CurrentInputState = PlayerInputState.NoInput;
		GameObject gameObject = enteredPlayer.PlayEffectOnActor(HellDragVFX, Vector3.zero);
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		component.UpdateZDepth();
		component.attachParent = null;
		component.IsPerpendicular = false;
		component.HeightOffGround = 1f;
		component.UpdateZDepth();
		component.transform.position = component.transform.position.WithX(component.transform.position.x + 0.25f);
		component.transform.position = component.transform.position.WithY((float)enteredPlayer.CurrentRoom.area.basePosition.y + 55f);
		component.usesOverrideMaterial = true;
		component.renderer.material.shader = ShaderCache.Acquire("Brave/Effects/StencilMasked");
		StartCoroutine(HandleGrabbyGrab(enteredPlayer));
	}

	private void ProcessTriggerEntered(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		Debug.Log("Hell Hole entered!");
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		HoleObject.SetActive(true);
		if ((bool)component)
		{
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
				speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Remove(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(ProcessTriggerEntered));
			}
			GrabPlayer(component);
			StartCoroutine(LerpHoleSize(0f, 0.15f, 0.3f, component));
		}
	}
}
