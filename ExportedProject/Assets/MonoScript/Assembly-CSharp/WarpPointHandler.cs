using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class WarpPointHandler : BraveBehaviour
{
	public enum WarpTargetType
	{
		WARP_A,
		WARP_B,
		WARP_C,
		WARP_D,
		WARP_E,
		WARP_F,
		WARP_G,
		WARP_H,
		WARP_I
	}

	[NonSerialized]
	public bool DISABLED_TEMPORARILY;

	public WarpTargetType warpTarget;

	public bool OnlyReceiver;

	public MajorBreakable OptionalCover;

	public Vector2 AdditionalSpawnOffset;

	[NonSerialized]
	public Vector2 spawnOffset = Vector2.zero;

	[NonSerialized]
	public bool ManuallyAssigned;

	public Func<PlayerController, float> OnPreWarp;

	public Func<PlayerController, float> OnWarping;

	public Func<PlayerController, float> OnWarpDone;

	private WarpPointHandler m_targetWarper;

	private static bool m_justWarped;

	public void SetTarget(WarpPointHandler target)
	{
		warpTarget = (WarpTargetType)(-1);
		ManuallyAssigned = true;
		m_targetWarper = target;
	}

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (!ManuallyAssigned)
		{
			TryAcquirePairedWarp();
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerEntered));
	}

	private void TryAcquirePairedWarp()
	{
		WarpPointHandler[] array = UnityEngine.Object.FindObjectsOfType<WarpPointHandler>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].warpTarget == warpTarget && this != array[i])
			{
				m_targetWarper = array[i];
				break;
			}
		}
	}

	public Vector2 GetTargetPoint()
	{
		Vector2 vector = ((!ManuallyAssigned) ? m_targetWarper.specRigidbody.UnitCenter : m_targetWarper.specRigidbody.UnitBottomCenter);
		return vector + new Vector2(-0.5f, (!ManuallyAssigned) ? 0f : (-0.125f)) + spawnOffset + m_targetWarper.AdditionalSpawnOffset;
	}

	private void HandleTriggerEntered(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (m_justWarped || OnlyReceiver || DISABLED_TEMPORARILY)
		{
			return;
		}
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if (component != null)
		{
			if (m_targetWarper == null)
			{
				TryAcquirePairedWarp();
			}
			if (!(m_targetWarper == null))
			{
				Pixelator.Instance.StartCoroutine(HandleWarpCooldown(component));
			}
		}
	}

	private IEnumerator HandleWarpCooldown(PlayerController player)
	{
		m_justWarped = true;
		if (OnPreWarp != null)
		{
			float additionalDelay2 = OnPreWarp(player);
			if (additionalDelay2 > 0f)
			{
				yield return new WaitForSeconds(additionalDelay2);
			}
		}
		Pixelator.Instance.FadeToBlack(0.1f);
		yield return new WaitForSeconds(0.1f);
		player.SetInputOverride("arbitrary warp");
		if (OnWarping != null)
		{
			float additionalDelay3 = OnWarping(player);
			if (additionalDelay3 > 0f)
			{
				yield return new WaitForSeconds(additionalDelay3);
			}
		}
		if ((bool)OptionalCover)
		{
			OptionalCover.Break(-Vector2.up);
		}
		if ((bool)m_targetWarper.OptionalCover)
		{
			m_targetWarper.OptionalCover.Break(-Vector2.up);
		}
		Vector2 targetPoint = ((!ManuallyAssigned) ? m_targetWarper.specRigidbody.UnitCenter : m_targetWarper.specRigidbody.UnitBottomCenter);
		targetPoint = targetPoint + new Vector2(-0.5f, (!ManuallyAssigned) ? 0f : (-0.125f)) + spawnOffset + m_targetWarper.AdditionalSpawnOffset;
		Vector3 prevPlayerPosition = player.transform.position;
		player.WarpToPoint(targetPoint);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
			if ((bool)otherPlayer && otherPlayer.healthHaver.IsAlive)
			{
				otherPlayer.ReuniteWithOtherPlayer(player);
			}
		}
		GameManager.Instance.MainCameraController.ForceToPlayerPosition(player, prevPlayerPosition);
		if (OnWarpDone != null)
		{
			float additionalDelay = OnWarpDone(player);
			Pixelator.Instance.FadeToBlack(additionalDelay + 0.1f, true);
			if (additionalDelay > 0f)
			{
				yield return new WaitForSeconds(additionalDelay);
			}
		}
		else
		{
			Pixelator.Instance.FadeToBlack(0.1f, true);
		}
		player.ClearInputOverride("arbitrary warp");
		yield return new WaitForSeconds(0.05f);
		m_justWarped = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
