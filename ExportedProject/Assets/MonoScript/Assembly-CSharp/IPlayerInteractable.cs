using UnityEngine;

public interface IPlayerInteractable
{
	float GetDistanceToPoint(Vector2 point);

	void OnEnteredRange(PlayerController interactor);

	void OnExitRange(PlayerController interactor);

	void Interact(PlayerController interactor);

	string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped);

	float GetOverrideMaxDistance();
}
