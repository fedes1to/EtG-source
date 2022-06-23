using System;
using Dungeonator;
using UnityEngine;

public class TeleporterController : DungeonPlaceableBehaviour, IPlaceConfigurable, IPlayerInteractable
{
	public GameObject teleporterIcon;

	public GameObject teleportDepartureVFX;

	public GameObject teleportArrivalVFX;

	public GameObject onetimeActivateVFX;

	public GameObject extantActiveVFX;

	public tk2dSpriteAnimator portalVFX;

	private bool m_wasJustWarpedTo;

	private RoomHandler m_room;

	private bool m_activated;

	public void Start()
	{
		IntVector2 intVector = (base.transform.position.XY() + new Vector2(0.5f, 0.5f)).ToIntVector2(VectorConversions.Floor);
		for (int i = intVector.x; i < intVector.x + placeableWidth; i++)
		{
			for (int j = intVector.y; j < intVector.y + placeableHeight; j++)
			{
				GameManager.Instance.Dungeon.data[i, j].PreventRewardSpawn = true;
			}
		}
	}

	public void Update()
	{
		if (!m_activated && GameManager.Instance.PrimaryPlayer != null && GameManager.Instance.PrimaryPlayer.CurrentRoom != null && (GameManager.Instance.PrimaryPlayer.CurrentRoom == m_room || (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.SecondaryPlayer != null && GameManager.Instance.SecondaryPlayer.CurrentRoom != null && GameManager.Instance.SecondaryPlayer.CurrentRoom == m_room)) && !m_room.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			Activate();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void SetReturnActive()
	{
		m_wasJustWarpedTo = true;
		portalVFX.gameObject.SetActive(true);
	}

	public void ClearReturnActive()
	{
		portalVFX.gameObject.SetActive(false);
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		Minimap.Instance.RegisterTeleportIcon(m_room, teleporterIcon, base.sprite.WorldCenter);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!portalVFX.gameObject.activeSelf)
		{
			return 10000f;
		}
		return Vector2.Distance(point, base.sprite.WorldCenter) / 3f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (!m_wasJustWarpedTo && m_activated && interactor.CanReturnTeleport)
		{
			portalVFX.sprite.HeightOffGround = -1f;
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		m_wasJustWarpedTo = false;
		if (m_activated)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (GameManager.Instance.AllPlayers != null)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if ((bool)playerController && playerController.IsTalking)
				{
					return;
				}
			}
		}
		if (m_activated && interactor.CanReturnTeleport)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				GameManager.Instance.AllPlayers[j].AttemptReturnTeleport(this);
			}
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	private void Activate()
	{
		m_activated = true;
		base.spriteAnimator.Play("teleport_pad_activate");
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TriggerActiveVFX));
		if (onetimeActivateVFX != null)
		{
			onetimeActivateVFX.SetActive(true);
			onetimeActivateVFX.GetComponent<tk2dSprite>().IsPerpendicular = false;
		}
	}

	private void TriggerActiveVFX(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(TriggerActiveVFX));
		extantActiveVFX.SetActive(true);
	}
}
