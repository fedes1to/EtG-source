using UnityEngine;

public interface IPlayerOrbital
{
	void Reinitialize();

	Transform GetTransform();

	void ToggleRenderer(bool visible);

	int GetOrbitalTier();

	void SetOrbitalTier(int tier);

	int GetOrbitalTierIndex();

	void SetOrbitalTierIndex(int tierIndex);

	float GetOrbitalRadius();

	float GetOrbitalRotationalSpeed();
}
