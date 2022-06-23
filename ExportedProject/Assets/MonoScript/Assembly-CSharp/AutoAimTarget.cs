using Dungeonator;
using UnityEngine;

public class AutoAimTarget : BraveBehaviour, IAutoAimTarget
{
	public bool ForceUseTransform;

	public bool IgnoreForSuperAutoAim;

	public float MinDistForSuperAutoAim;

	private RoomHandler parentRoom;

	public bool IsValid
	{
		get
		{
			if (!this)
			{
				return false;
			}
			if ((bool)base.specRigidbody && !ForceUseTransform)
			{
				if (!base.specRigidbody.enabled)
				{
					return false;
				}
				if (base.specRigidbody.GetPixelCollider(ColliderType.HitBox) == null)
				{
					return false;
				}
				return true;
			}
			return base.enabled && base.gameObject.activeSelf;
		}
	}

	public Vector2 AimCenter
	{
		get
		{
			if ((bool)base.specRigidbody && !ForceUseTransform)
			{
				return base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			return base.transform.position.XY();
		}
	}

	public Vector2 Velocity
	{
		get
		{
			if ((bool)base.specRigidbody)
			{
				return base.specRigidbody.Velocity;
			}
			return Vector2.zero;
		}
	}

	public bool IgnoreForSuperDuperAutoAim
	{
		get
		{
			return IgnoreForSuperAutoAim;
		}
	}

	public float MinDistForSuperDuperAutoAim
	{
		get
		{
			return MinDistForSuperAutoAim;
		}
	}

	public void Start()
	{
		Vector2 aimCenter = AimCenter;
		parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(aimCenter.ToIntVector2(VectorConversions.Floor));
		parentRoom.RegisterAutoAimTarget(this);
	}

	protected override void OnDestroy()
	{
		if (parentRoom != null)
		{
			parentRoom.DeregisterAutoAimTarget(this);
		}
		base.OnDestroy();
	}
}
