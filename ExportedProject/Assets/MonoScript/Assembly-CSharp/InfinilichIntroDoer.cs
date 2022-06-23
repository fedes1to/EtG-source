using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class InfinilichIntroDoer : SpecificIntroDoer
{
	[Header("Shell Sucking")]
	public float radius = 15f;

	public float gravityForce = 200f;

	public float destroyRadius = 1f;

	private bool m_isFinished;

	private tk2dBaseSprite m_shadowSprite;

	private bool m_isWorldModified;

	private EndTimesNebulaController m_endTimesNebulaController;

	private float m_radiusSquared;

	public override Vector2? OverrideOutroPosition
	{
		get
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.controllerCamera.isTransitioning = false;
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

	public void Awake()
	{
		GetComponentInChildren<BulletLimbController>().HideBullets = true;
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		if (absoluteRoom != null)
		{
			absoluteRoom.AdditionalRoomState = RoomHandler.CustomRoomState.LICH_PHASE_THREE;
		}
	}

	protected override void OnDestroy()
	{
		ModifyWorld(false);
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.PlayUntilCancelled("preintro");
		m_shadowSprite = base.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		m_shadowSprite.color = m_shadowSprite.color.WithAlpha(0f);
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		Minimap.Instance.TemporarilyPreventMinimap = false;
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		GetComponentInChildren<BulletLimbController>().HideBullets = false;
		base.aiAnimator.PlayUntilCancelled("intro");
		m_radiusSquared = radius * radius;
		while (base.aiAnimator.IsPlaying("intro"))
		{
			float clipProgress = Mathf.InverseLerp(0.3f, 1f, base.aiAnimator.CurrentClipProgress);
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(clipProgress);
			for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
			{
				AdjustDebrisVelocity(StaticReferenceManager.AllDebris[i]);
			}
			yield return null;
		}
		base.aiAnimator.EndAnimationIf("intro");
		m_isFinished = true;
	}

	public override void EndIntro()
	{
		StopAllCoroutines();
		base.aiAnimator.EndAnimationIf("preintro");
		base.aiAnimator.EndAnimationIf("intro");
		GetComponentInChildren<BulletLimbController>().HideBullets = false;
		AkSoundEngine.PostEvent("Play_MUS_Lich_Phase_03", base.gameObject);
	}

	public void ModifyWorld(bool value)
	{
		if (!GameManager.HasInstance || value == m_isWorldModified)
		{
			return;
		}
		if (value)
		{
			if (!m_endTimesNebulaController)
			{
				m_endTimesNebulaController = Object.FindObjectOfType<EndTimesNebulaController>();
			}
			if ((bool)m_endTimesNebulaController)
			{
				m_endTimesNebulaController.BecomeActive();
			}
		}
		else if ((bool)m_endTimesNebulaController)
		{
			m_endTimesNebulaController.BecomeInactive(false);
		}
		m_isWorldModified = value;
	}

	private bool AdjustDebrisVelocity(DebrisObject debris)
	{
		if (debris.IsPickupObject)
		{
			return false;
		}
		if (debris.GetComponent<BlackHoleDoer>() != null)
		{
			return false;
		}
		if (!debris.name.Contains("shell", true))
		{
			return false;
		}
		Vector2 a = debris.sprite.WorldCenter - base.specRigidbody.UnitCenter;
		float num = Vector2.SqrMagnitude(a);
		if (num > m_radiusSquared)
		{
			return false;
		}
		float num2 = Mathf.Sqrt(num);
		if (num2 < destroyRadius)
		{
			Object.Destroy(debris.gameObject);
			return true;
		}
		Vector2 frameAccelerationForRigidbody = GetFrameAccelerationForRigidbody(debris.sprite.WorldCenter, num2, gravityForce);
		float num3 = Mathf.Clamp(GameManager.INVARIANT_DELTA_TIME, 0f, 0.02f);
		if (debris.HasBeenTriggered)
		{
			debris.ApplyVelocity(frameAccelerationForRigidbody * num3);
		}
		else if (num2 < radius / 2f)
		{
			debris.Trigger(frameAccelerationForRigidbody * num3, 0.5f);
		}
		return true;
	}

	private Vector2 GetFrameAccelerationForRigidbody(Vector2 unitCenter, float currentDistance, float g)
	{
		float num = Mathf.Clamp01(1f - currentDistance / radius);
		float num2 = g * num * num;
		return (base.specRigidbody.UnitCenter - unitCenter).normalized * num2;
	}
}
