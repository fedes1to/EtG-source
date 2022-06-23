using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class MirrorController : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public MirrorDweller PlayerReflection;

	public MirrorDweller CoopPlayerReflection;

	public MirrorDweller ChestReflection;

	public tk2dBaseSprite ChestSprite;

	public tk2dBaseSprite MirrorSprite;

	public GameObject ShatterSystem;

	public float CURSE_EXPOSED = 3f;

	private void Start()
	{
		PlayerReflection.TargetPlayer = GameManager.Instance.PrimaryPlayer;
		PlayerReflection.MirrorSprite = MirrorSprite;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			CoopPlayerReflection.TargetPlayer = GameManager.Instance.SecondaryPlayer;
			CoopPlayerReflection.MirrorSprite = MirrorSprite;
		}
		else
		{
			CoopPlayerReflection.gameObject.SetActive(false);
		}
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		Chest chest = GameManager.Instance.RewardManager.GenerationSpawnRewardChestAt(base.transform.position.IntXY() + new IntVector2(0, -2) - absoluteRoom.area.basePosition, absoluteRoom);
		chest.PreventFuse = true;
		SpriteOutlineManager.RemoveOutlineFromSprite(chest.sprite);
		Transform transform = chest.gameObject.transform.Find("Shadow");
		if ((bool)transform)
		{
			chest.ShadowSprite = transform.GetComponent<tk2dSprite>();
		}
		chest.IsMirrorChest = true;
		chest.ConfigureOnPlacement(GetAbsoluteParentRoom());
		if ((bool)chest.majorBreakable)
		{
			chest.majorBreakable.TemporarilyInvulnerable = true;
		}
		ChestSprite = chest.sprite;
		ChestSprite.renderer.enabled = false;
		ChestReflection.TargetSprite = ChestSprite;
		ChestReflection.MirrorSprite = MirrorSprite;
		SpeculativeRigidbody speculativeRigidbody = MirrorSprite.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollisionWithMirror));
		MinorBreakable componentInChildren = GetComponentInChildren<MinorBreakable>();
		componentInChildren.OnlyBrokenByCode = true;
		componentInChildren.heightOffGround = 4f;
	}

	private void HandleRigidbodyCollisionWithMirror(CollisionData rigidbodyCollision)
	{
		if ((bool)rigidbodyCollision.OtherRigidbody.projectile)
		{
			GetAbsoluteParentRoom().DeregisterInteractable(this);
			if (rigidbodyCollision.OtherRigidbody.projectile.Owner is PlayerController)
			{
				StartCoroutine(HandleShatter(rigidbodyCollision.OtherRigidbody.projectile.Owner as PlayerController, true));
			}
			else
			{
				StartCoroutine(HandleShatter(GameManager.Instance.PrimaryPlayer, true));
			}
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		Bounds bounds = ChestSprite.GetBounds();
		bounds.SetMinMax(bounds.min + ChestSprite.transform.position, bounds.max + ChestSprite.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public void OnEnteredRange(PlayerController interactor)
	{
	}

	public void OnExitRange(PlayerController interactor)
	{
		MirrorDweller[] componentsInChildren = ChestReflection.GetComponentsInChildren<MirrorDweller>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].UsesOverrideTintColor)
			{
				componentsInChildren[i].renderer.enabled = false;
			}
		}
	}

	public void Interact(PlayerController interactor)
	{
		ChestSprite.GetComponent<Chest>().ForceOpen(interactor);
		MirrorDweller[] componentsInChildren = ChestReflection.GetComponentsInChildren<MirrorDweller>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].UsesOverrideTintColor)
			{
				componentsInChildren[i].renderer.enabled = false;
			}
		}
		GetAbsoluteParentRoom().DeregisterInteractable(this);
		StartCoroutine(HandleShatter(interactor));
		for (int j = 0; j < interactor.passiveItems.Count && !(interactor.passiveItems[j] is YellowChamberItem); j++)
		{
		}
	}

	private IEnumerator HandleShatter(PlayerController interactor, bool skipInitialWait = false)
	{
		if (!skipInitialWait)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if ((bool)this)
		{
			AkSoundEngine.PostEvent("Play_OBJ_crystal_shatter_01", base.gameObject);
			AkSoundEngine.PostEvent("Play_OBJ_pot_shatter_01", base.gameObject);
			AkSoundEngine.PostEvent("Play_OBJ_glass_shatter_01", base.gameObject);
		}
		StatModifier curse = new StatModifier
		{
			statToBoost = PlayerStats.StatType.Curse,
			amount = CURSE_EXPOSED,
			modifyType = StatModifier.ModifyMethod.ADDITIVE
		};
		if (!interactor)
		{
			interactor = GameManager.Instance.PrimaryPlayer;
		}
		if ((bool)interactor)
		{
			interactor.ownerlessStatModifiers.Add(curse);
			interactor.stats.RecalculateStats(interactor);
		}
		MinorBreakable childBreakable = GetComponentInChildren<MinorBreakable>();
		if ((bool)childBreakable)
		{
			childBreakable.Break();
			while ((bool)childBreakable)
			{
				yield return null;
			}
		}
		tk2dSpriteAnimator eyeBall = GetComponentInChildren<tk2dSpriteAnimator>();
		if ((bool)eyeBall)
		{
			eyeBall.Play("haunted_mirror_eye");
		}
		if ((bool)ShatterSystem)
		{
			ShatterSystem.SetActive(true);
		}
		yield return new WaitForSeconds(2.5f);
		if ((bool)ShatterSystem)
		{
			ShatterSystem.GetComponent<ParticleSystem>().Pause(false);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		room.OptionalDoorTopDecorable = ResourceCache.Acquire("Global Prefabs/Purple_Lantern") as GameObject;
		if (!room.IsOnCriticalPath && room.connectedRooms.Count == 1)
		{
			room.ShouldAttemptProceduralLock = true;
			room.AttemptProceduralLockChance = Mathf.Max(room.AttemptProceduralLockChance, UnityEngine.Random.Range(0.3f, 0.5f));
		}
	}
}
