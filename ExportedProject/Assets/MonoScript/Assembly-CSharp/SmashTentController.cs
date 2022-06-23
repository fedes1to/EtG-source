using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class SmashTentController : MonoBehaviour, IPlaceConfigurable
{
	public TalkDoerLite DrGreet;

	public TalkDoerLite DrSmash;

	public tk2dBaseSprite TableBottleSprite;

	public Transform PlayerStandPoint;

	public GameObject BottleSmashVFX;

	[NonSerialized]
	public bool IsProcessing;

	[NonSerialized]
	private bool HasSmashed;

	private PlayerController m_targetPlayer;

	public void DoSmash()
	{
		PlayerController talkingPlayer = DrGreet.TalkingPlayer;
		DrGreet.GetDungeonFSM().Fsm.SuppressGlobalTransitions = true;
		DrSmash.GetDungeonFSM().Fsm.SuppressGlobalTransitions = true;
		StartCoroutine(HandleSmash(talkingPlayer));
	}

	private IEnumerator WaitForPlayerPosition(PlayerController targetPlayer)
	{
		Vector2 targetPosition = PlayerStandPoint.PositionVector2();
		Vector2 currentPosition = targetPlayer.CenterPosition;
		targetPlayer.ForceMoveToPoint(targetPosition, 0.5f);
		float ela = 0f;
		bool doDillywop = false;
		while (!currentPosition.Approximately(targetPosition))
		{
			ela += BraveTime.DeltaTime;
			if (ela > 3f)
			{
				break;
			}
			currentPosition = targetPlayer.CenterPosition;
			yield return null;
			if (!targetPlayer.usingForcedInput || Vector2.Distance(currentPosition, targetPosition) < 0.125f)
			{
				targetPlayer.usingForcedInput = false;
				doDillywop = true;
				break;
			}
		}
		if (ela > 3f || doDillywop)
		{
			Vector2 vector = targetPlayer.transform.position.XY() - targetPlayer.CenterPosition;
			targetPlayer.WarpToPoint(targetPosition + vector);
		}
		targetPlayer.ForceStaticFaceDirection(Vector2.right);
	}

	private IEnumerator HandleSmash(PlayerController targetPlayer)
	{
		IsProcessing = true;
		targetPlayer.SetInputOverride("smash test");
		m_targetPlayer = targetPlayer;
		yield return StartCoroutine(WaitForPlayerPosition(targetPlayer));
		DrGreet.aiAnimator.PlayUntilCancelled("drgreet_diagnose");
		yield return new WaitForSeconds(1f);
		tk2dSpriteAnimator spriteAnimator = DrSmash.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleSmashEvent));
		DrSmash.aiAnimator.PlayUntilFinished("drsmash_smash");
		while (!HasSmashed)
		{
			yield return null;
		}
		DrGreet.aiAnimator.PlayForDuration("drgreet_cure", 3f);
		while (DrSmash.aiAnimator.IsPlaying("drsmash_smash"))
		{
			yield return null;
		}
		DrSmash.aiAnimator.PlayForDuration("drsmash_clap", 3f);
		tk2dSpriteAnimator spriteAnimator2 = DrSmash.spriteAnimator;
		spriteAnimator2.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(spriteAnimator2.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleSmashEvent));
		targetPlayer.ClearInputOverride("smash test");
		IsProcessing = false;
		while (DrSmash.aiAnimator.IsPlaying("drsmash_clap"))
		{
			yield return null;
		}
		DrGreet.GetDungeonFSM().Fsm.SuppressGlobalTransitions = false;
		DrSmash.GetDungeonFSM().Fsm.SuppressGlobalTransitions = false;
		DrSmash.transform.position = DrSmash.transform.position + new Vector3(1.375f, 0f, 0f);
		DrSmash.specRigidbody.Reinitialize();
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(DrSmash.specRigidbody);
	}

	private IEnumerator HandleHearts(PlayerController targetPlayer)
	{
		GameUIRoot.Instance.ShowCoreUI(string.Empty);
		float duration = 4.5f;
		int halfHeartsToHeal = Mathf.RoundToInt((targetPlayer.healthHaver.GetMaxHealth() - targetPlayer.healthHaver.GetCurrentHealth()) / 0.5f);
		Debug.Log(halfHeartsToHeal + " to heal!");
		float timeStep = duration / (float)halfHeartsToHeal;
		while (targetPlayer.healthHaver.GetCurrentHealth() < targetPlayer.healthHaver.GetMaxHealth())
		{
			targetPlayer.healthHaver.ApplyHealing(0.5f);
			yield return new WaitForSeconds(timeStep);
		}
	}

	private void HandleSmashEvent(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2, int arg3)
	{
		tk2dSpriteAnimationFrame frame = arg2.GetFrame(arg3);
		if (frame.eventInfo == "grabbottle")
		{
			TableBottleSprite.gameObject.SetActive(false);
		}
		else if (frame.eventInfo == "smash")
		{
			UnityEngine.Object.Instantiate(BottleSmashVFX, DrSmash.transform.position, Quaternion.identity);
			m_targetPlayer.PlayFairyEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_Fairy_Fly") as GameObject, Vector3.zero, 4.5f, true);
			StartCoroutine(HandleHearts(m_targetPlayer));
			HasSmashed = true;
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		if (room.connectedRooms.Count == 1)
		{
			room.ShouldAttemptProceduralLock = true;
			room.AttemptProceduralLockChance = Mathf.Max(room.AttemptProceduralLockChance, UnityEngine.Random.Range(0.3f, 0.5f));
		}
	}
}
