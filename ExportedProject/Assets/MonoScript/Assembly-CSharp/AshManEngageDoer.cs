using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class AshManEngageDoer : CustomEngageDoer
{
	public float FromStatueChance = 0.5f;

	public string BreakablePrefix = "Forge_Ash_Bullet";

	public float MinSpawnDelay = 2f;

	public float MaxSpawnDelay = 6f;

	private bool m_isFinished;

	private bool m_brokeEarly;

	public override bool IsFinished
	{
		get
		{
			return m_isFinished;
		}
	}

	public void Awake()
	{
		if (UnityEngine.Random.value > FromStatueChance)
		{
			m_isFinished = true;
			return;
		}
		base.aiActor.HasDonePlayerEnterCheck = true;
		base.aiActor.CollisionDamage = 0f;
	}

	public override void StartIntro()
	{
		if (m_isFinished)
		{
			return;
		}
		List<MinorBreakable> list = new List<MinorBreakable>();
		RoomHandler parentRoom = base.aiActor.ParentRoom;
		List<MinorBreakable> allMinorBreakables = StaticReferenceManager.AllMinorBreakables;
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = 0; i < allMinorBreakables.Count; i++)
		{
			MinorBreakable minorBreakable = allMinorBreakables[i];
			if (minorBreakable.name.StartsWith(BreakablePrefix))
			{
				RoomHandler absoluteRoomFromPosition = data.GetAbsoluteRoomFromPosition(minorBreakable.transform.position.IntXY(VectorConversions.Floor));
				if (absoluteRoomFromPosition == parentRoom)
				{
					list.Add(minorBreakable);
				}
			}
		}
		if (list.Count == 0)
		{
			m_isFinished = true;
			base.aiActor.invisibleUntilAwaken = false;
			base.aiActor.ToggleRenderers(true);
			base.aiAnimator.PlayDefaultAwakenedState();
			base.aiActor.State = AIActor.ActorState.Normal;
		}
		else
		{
			StartCoroutine(DoIntro(BraveUtility.RandomElement(list)));
		}
	}

	private IEnumerator DoIntro(MinorBreakable breakable)
	{
		base.aiActor.enabled = false;
		base.behaviorSpeculator.enabled = false;
		base.aiActor.ToggleRenderers(false);
		base.specRigidbody.enabled = false;
		base.aiActor.IsGone = true;
		base.specRigidbody.Initialize();
		Vector2 offset = base.specRigidbody.UnitBottomCenter - base.transform.position.XY();
		base.transform.position = breakable.specRigidbody.UnitBottomCenter - offset;
		base.specRigidbody.Reinitialize();
		yield return null;
		base.aiActor.ToggleRenderers(false);
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(false, "AshManEngageDoer");
		}
		breakable.OnBreak = (Action)Delegate.Combine(breakable.OnBreak, new Action(OnBreak));
		float delay = UnityEngine.Random.Range(MinSpawnDelay, MaxSpawnDelay);
		for (float timer = 0f; timer < delay; timer += BraveTime.DeltaTime)
		{
			if (m_brokeEarly)
			{
				break;
			}
			yield return null;
		}
		if (!m_brokeEarly)
		{
			breakable.Break();
		}
		base.aiActor.enabled = true;
		base.behaviorSpeculator.enabled = true;
		base.aiActor.ToggleRenderers(true);
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(true, "AshManEngageDoer");
		}
		base.specRigidbody.enabled = true;
		base.aiActor.IsGone = false;
		base.aiAnimator.PlayDefaultAwakenedState();
		base.aiActor.State = AIActor.ActorState.Normal;
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			if ((bool)playerController && Vector2.Distance(playerController.specRigidbody.UnitCenter, base.specRigidbody.UnitCenter) < 8f)
			{
				base.behaviorSpeculator.AttackCooldown = 0.5f;
				break;
			}
		}
		breakable.OnBreak = (Action)Delegate.Remove(breakable.OnBreak, new Action(OnBreak));
		m_isFinished = true;
		yield return new WaitForSeconds(1f);
		base.aiActor.CollisionDamage = 0.5f;
	}

	private void OnBreak()
	{
		m_brokeEarly = true;
	}
}
