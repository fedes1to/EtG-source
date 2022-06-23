using UnityEngine;

public interface IAutoAimTarget
{
	bool IsValid { get; }

	Vector2 AimCenter { get; }

	Vector2 Velocity { get; }

	bool IgnoreForSuperDuperAutoAim { get; }

	float MinDistForSuperDuperAutoAim { get; }
}
