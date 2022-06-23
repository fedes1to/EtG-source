using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursePotController : BraveBehaviour
{
	public string IdleAnim;

	public string AnimToPlayWhenExcited;

	public float TimeToCursePoint = 3f;

	public tk2dSprite CircleSprite;

	public tk2dSprite ShadowSprite;

	private tk2dSpriteAnimationClip m_idleClip;

	private tk2dSpriteAnimationClip m_activeClip;

	private List<PlayerController> m_cursedPlayers = new List<PlayerController>();

	private void Start()
	{
		m_idleClip = base.spriteAnimator.GetClipByName(IdleAnim);
		m_activeClip = base.spriteAnimator.GetClipByName(AnimToPlayWhenExcited);
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerEntered));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnExitTrigger = (SpeculativeRigidbody.OnTriggerExitDelegate)Delegate.Combine(speculativeRigidbody2.OnExitTrigger, new SpeculativeRigidbody.OnTriggerExitDelegate(HandleTriggerExited));
		MinorBreakable component = GetComponent<MinorBreakable>();
		component.OnBreak = (Action)Delegate.Combine(component.OnBreak, new Action(HandleBreak));
		if ((bool)CircleSprite)
		{
			tk2dSpriteDefinition currentSpriteDef = CircleSprite.GetCurrentSpriteDef();
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			for (int i = 0; i < currentSpriteDef.uvs.Length; i++)
			{
				vector = Vector2.Min(vector, currentSpriteDef.uvs[i]);
				vector2 = Vector2.Max(vector2, currentSpriteDef.uvs[i]);
			}
			Vector2 vector3 = (vector + vector2) / 2f;
			CircleSprite.renderer.material.SetVector("_WorldCenter", new Vector4(vector3.x, vector3.y, vector3.x - vector.x, vector3.y - vector.y));
		}
	}

	private void HandleBreak()
	{
		if ((bool)CircleSprite)
		{
			CircleSprite.transform.parent = null;
		}
		if ((bool)ShadowSprite)
		{
			ShadowSprite.transform.parent = null;
		}
		GameManager.Instance.Dungeon.StartCoroutine(HandleBreakCR());
	}

	private IEnumerator HandleBreakCR()
	{
		float elapsed = 0f;
		float duration = 0.3f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			if ((bool)CircleSprite)
			{
				CircleSprite.scale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
			}
			if ((bool)ShadowSprite)
			{
				ShadowSprite.scale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
			}
			yield return null;
		}
		if ((bool)CircleSprite)
		{
			UnityEngine.Object.Destroy(CircleSprite.gameObject);
		}
		if ((bool)ShadowSprite)
		{
			UnityEngine.Object.Destroy(ShadowSprite.gameObject);
		}
	}

	private void Update()
	{
		if (base.minorBreakable.IsBroken)
		{
			return;
		}
		if (m_cursedPlayers.Count > 0)
		{
			if (!base.spriteAnimator.IsPlaying(m_activeClip))
			{
				base.spriteAnimator.Play(m_activeClip);
			}
		}
		else if (!base.spriteAnimator.IsPlaying(m_idleClip))
		{
			base.spriteAnimator.Play(m_idleClip);
		}
		for (int i = 0; i < m_cursedPlayers.Count; i++)
		{
			DoCurse(m_cursedPlayers[i]);
		}
	}

	private void DoCurse(PlayerController targetPlayer)
	{
		if (!targetPlayer.IsGhost)
		{
			targetPlayer.CurrentCurseMeterValue += BraveTime.DeltaTime / TimeToCursePoint;
			targetPlayer.CurseIsDecaying = false;
			if (targetPlayer.CurrentCurseMeterValue > 1f)
			{
				targetPlayer.CurrentCurseMeterValue = 0f;
				StatModifier statModifier = new StatModifier();
				statModifier.amount = 1f;
				statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				statModifier.statToBoost = PlayerStats.StatType.Curse;
				targetPlayer.ownerlessStatModifiers.Add(statModifier);
				targetPlayer.stats.RecalculateStats(targetPlayer);
				base.minorBreakable.Break();
			}
		}
	}

	private void HandleTriggerExited(SpeculativeRigidbody exitRigidbody, SpeculativeRigidbody sourceSpecRigidbody)
	{
		if ((bool)exitRigidbody && (bool)exitRigidbody.gameActor && exitRigidbody.gameActor is PlayerController && m_cursedPlayers.Contains(exitRigidbody.gameActor as PlayerController))
		{
			PlayerController playerController = exitRigidbody.gameActor as PlayerController;
			playerController.CurseIsDecaying = true;
			m_cursedPlayers.Remove(playerController);
		}
	}

	private void HandleTriggerEntered(SpeculativeRigidbody enteredRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2()) == GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(enteredRigidbody.UnitCenter.ToIntVector2()) && enteredRigidbody.gameActor != null && enteredRigidbody.gameActor is PlayerController)
		{
			PlayerController playerController = enteredRigidbody.gameActor as PlayerController;
			playerController.CurseIsDecaying = false;
			m_cursedPlayers.Add(playerController);
		}
	}
}
