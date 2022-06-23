using System.Collections;
using Dungeonator;
using UnityEngine;

public class PotFairyEngageDoer : CustomEngageDoer
{
	public static bool InstantSpawn;

	public GameObject[] PotPrefabs;

	private bool m_isFinished;

	private MinorBreakable m_minorBreakable;

	private bool m_hasDonePotCheck;

	public override bool IsFinished
	{
		get
		{
			return m_isFinished;
		}
	}

	public void Awake()
	{
		if (BraveUtility.RandomBool())
		{
			base.aiAnimator.IdleAnimation.Prefix = base.aiAnimator.IdleAnimation.Prefix.Replace("pink", "blue");
			for (int i = 0; i < base.aiAnimator.OtherAnimations.Count; i++)
			{
				base.aiAnimator.OtherAnimations[i].anim.Prefix = base.aiAnimator.OtherAnimations[i].anim.Prefix.Replace("pink", "blue");
			}
		}
		if (InstantSpawn)
		{
			StartIntro();
		}
	}

	public void Update()
	{
		if (!m_hasDonePotCheck)
		{
			if (!InstantSpawn && !base.aiActor.IsInReinforcementLayer)
			{
				base.specRigidbody.Initialize();
				IntVector2 intVector = base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
				RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(intVector);
				GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(BraveUtility.RandomElement(PotPrefabs), roomFromPosition, intVector - roomFromPosition.area.basePosition, true);
				m_minorBreakable = gameObject.GetComponent<MinorBreakable>();
			}
			m_hasDonePotCheck = true;
		}
		if (!base.specRigidbody || !base.specRigidbody.enabled)
		{
			return;
		}
		RoomHandler roomFromPosition2 = GameManager.Instance.Dungeon.GetRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			if (playerController.healthHaver.IsAlive && playerController.CurrentRoom != null && playerController.CurrentRoom.IsSealed && playerController.CurrentRoom != roomFromPosition2)
			{
				base.aiActor.CanDropCurrency = false;
				base.aiActor.CanDropItems = false;
				base.healthHaver.ApplyDamage(10000f, Vector2.zero, "Lonely Suicide", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
				break;
			}
		}
	}

	public override void StartIntro()
	{
		if (!m_isFinished)
		{
			StartCoroutine(DoIntro());
		}
	}

	private IEnumerator DoIntro()
	{
		base.aiActor.enabled = false;
		base.behaviorSpeculator.enabled = false;
		base.aiActor.ToggleRenderers(false);
		base.specRigidbody.enabled = false;
		base.aiActor.IgnoreForRoomClear = true;
		base.aiActor.IsGone = true;
		base.aiActor.ToggleRenderers(false);
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(false, "PotFairyEngageDoer");
		}
		if ((bool)m_minorBreakable)
		{
			yield return null;
		}
		while ((bool)m_minorBreakable && !m_minorBreakable.IsBroken)
		{
			if (!base.aiActor.ParentRoom.IsSealed || base.healthHaver.IsDead)
			{
				Object.Destroy(base.gameObject);
				yield break;
			}
			base.aiActor.ToggleRenderers(false);
			if ((bool)base.aiShooter)
			{
				base.aiShooter.ToggleGunAndHandRenderers(false, "PotFairyEngageDoer");
			}
			yield return null;
		}
		base.aiActor.enabled = true;
		base.behaviorSpeculator.enabled = true;
		base.specRigidbody.enabled = true;
		base.aiActor.IsGone = false;
		base.aiActor.IgnoreForRoomClear = false;
		base.aiActor.ToggleRenderers(true);
		base.aiAnimator.PlayDefaultAwakenedState();
		base.aiActor.State = AIActor.ActorState.Normal;
		m_isFinished = true;
		int playerMask = CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.PlayerHitBox);
		base.aiActor.specRigidbody.AddCollisionLayerIgnoreOverride(playerMask);
		while (base.aiAnimator.IsPlaying("awaken"))
		{
			if ((bool)base.aiShooter)
			{
				base.aiShooter.ToggleGunAndHandRenderers(false, "PotFairyEngageDoer");
			}
			yield return null;
		}
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(true, "PotFairyEngageDoer");
		}
		yield return new WaitForSeconds(0.5f);
		base.aiActor.specRigidbody.RemoveCollisionLayerIgnoreOverride(playerMask);
	}
}
