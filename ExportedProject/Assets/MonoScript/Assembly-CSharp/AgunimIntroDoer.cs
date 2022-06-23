using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class AgunimIntroDoer : SpecificIntroDoer
{
	public GameObject cameraPoint;

	private int m_cachedCameraMinY;

	private bool m_isMotionRestricted;

	public override Vector2? OverrideOutroPosition
	{
		get
		{
			if (!this)
			{
				ModifyCamera(false);
			}
			else
			{
				ModifyCamera(true);
			}
			return null;
		}
	}

	public void Start()
	{
		base.aiActor.ParentRoom.Entered += PlayerEnteredRoom;
		base.aiActor.healthHaver.OnPreDeath += OnPreDeath;
	}

	public void Update()
	{
		m_cachedCameraMinY = PhysicsEngine.UnitToPixel(BraveUtility.ViewportToWorldpoint(new Vector2(0.5f, 0f), ViewportType.Camera).y);
	}

	protected override void OnDestroy()
	{
		RestrictMotion(false);
		ModifyCamera(false);
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		RoomHandler parentRoom = base.aiActor.ParentRoom;
		if (parentRoom == null)
		{
			return;
		}
		List<TorchController> componentsInRoom = parentRoom.GetComponentsInRoom<TorchController>();
		for (int i = 0; i < componentsInRoom.Count; i++)
		{
			TorchController torchController = componentsInRoom[i];
			if ((bool)torchController && (bool)torchController.specRigidbody)
			{
				torchController.specRigidbody.CollideWithOthers = false;
			}
		}
	}

	private void PlayerMovementRestrictor(SpeculativeRigidbody playerSpecRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (validLocation && (pixelOffset - prevPixelOffset).y < 0)
		{
			int num = playerSpecRigidbody.PixelColliders[0].MinY + pixelOffset.y;
			if (num < m_cachedCameraMinY + 20)
			{
				validLocation = false;
			}
		}
	}

	private void PlayerEnteredRoom(PlayerController playerController)
	{
		RestrictMotion(true);
		BulletPastRoomController[] array = UnityEngine.Object.FindObjectsOfType<BulletPastRoomController>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].RoomIdentifier == BulletPastRoomController.BulletRoomCategory.ROOM_C)
			{
				StartCoroutine(array[i].HandleAgunimIntro(base.transform));
				break;
			}
		}
	}

	private void OnPreDeath(Vector2 finalDirection)
	{
		RestrictMotion(false);
	}

	private void RestrictMotion(bool value)
	{
		if (m_isMotionRestricted == value)
		{
			return;
		}
		if (value)
		{
			if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach)
			{
				return;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				SpeculativeRigidbody speculativeRigidbody = GameManager.Instance.AllPlayers[i].specRigidbody;
				speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
			}
		}
		else
		{
			if (!GameManager.HasInstance || GameManager.IsReturningToBreach)
			{
				return;
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController)
				{
					SpeculativeRigidbody speculativeRigidbody2 = playerController.specRigidbody;
					speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
				}
			}
		}
		m_isMotionRestricted = value;
	}

	private void ModifyCamera(bool value)
	{
		if (GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel && !GameManager.IsReturningToBreach)
		{
			if (value)
			{
				CameraController mainCameraController = GameManager.Instance.MainCameraController;
				mainCameraController.LockToRoom = true;
				mainCameraController.PreventAimLook = true;
				mainCameraController.AddFocusPoint(cameraPoint.gameObject);
			}
			else
			{
				CameraController mainCameraController2 = GameManager.Instance.MainCameraController;
				mainCameraController2.LockToRoom = false;
				mainCameraController2.PreventAimLook = false;
				mainCameraController2.RemoveFocusPoint(cameraPoint.gameObject);
			}
		}
	}
}
