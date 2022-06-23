using UnityEngine;

public class InfiniteMinecartZone : DungeonPlaceableBehaviour
{
	public static bool InInfiniteMinecartZone;

	public int RegionWidth = 10;

	public int RegionHeight = 3;

	private int m_remainingLoops = 10;

	private bool processed;

	public void Start()
	{
	}

	private void Update()
	{
		if (IsPlayerInRegion() && m_remainingLoops > 0)
		{
			InInfiniteMinecartZone = true;
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			if (!processed && primaryPlayer.IsInMinecart)
			{
				ParticleSystem componentInChildren = primaryPlayer.currentMineCart.Sparks_A.GetComponentInChildren<ParticleSystem>();
				ParticleSystem componentInChildren2 = primaryPlayer.currentMineCart.Sparks_B.GetComponentInChildren<ParticleSystem>();
				componentInChildren.simulationSpace = ParticleSystemSimulationSpace.Local;
				componentInChildren2.simulationSpace = ParticleSystemSimulationSpace.Local;
			}
			IntVector2 intVector = base.transform.position.IntXY();
			IntRect intRect = new IntRect(intVector.x, intVector.y, RegionWidth, RegionHeight);
			if (!(primaryPlayer.CenterPosition.x > (float)intVector.x + (float)RegionWidth * 0.75f) || !primaryPlayer.IsInMinecart)
			{
				return;
			}
			m_remainingLoops--;
			Vector2 vector = GameManager.Instance.MainCameraController.transform.position.XY() - primaryPlayer.currentMineCart.transform.position.XY();
			PathMover component = primaryPlayer.currentMineCart.GetComponent<PathMover>();
			Vector2 vector2 = component.transform.position.XY();
			component.WarpToNearestPoint(intVector.ToVector2() + new Vector2(0f, (float)RegionHeight / 2f));
			Vector2 vector3 = component.transform.position.XY() - vector2;
			for (int i = 0; i < primaryPlayer.orbitals.Count; i++)
			{
				primaryPlayer.orbitals[i].GetTransform().position = primaryPlayer.orbitals[i].GetTransform().position + vector3.ToVector3ZisY();
				if (primaryPlayer.orbitals[i] is PlayerOrbital)
				{
					(primaryPlayer.orbitals[i] as PlayerOrbital).ReinitializeWithDelta(vector3);
				}
				else
				{
					primaryPlayer.orbitals[i].Reinitialize();
				}
			}
			for (int j = 0; j < primaryPlayer.trailOrbitals.Count; j++)
			{
				primaryPlayer.trailOrbitals[j].transform.position = primaryPlayer.trailOrbitals[j].transform.position + vector3.ToVector3ZisY();
				primaryPlayer.trailOrbitals[j].specRigidbody.Reinitialize();
			}
			primaryPlayer.currentMineCart.ForceUpdatePositions();
			GameManager.Instance.MainCameraController.transform.position = (primaryPlayer.currentMineCart.transform.position.XY() + vector).ToVector3ZUp(GameManager.Instance.MainCameraController.transform.position.z);
		}
		else
		{
			InInfiniteMinecartZone = false;
		}
	}

	private bool IsPlayerInRegion()
	{
		IntVector2 intVector = base.transform.position.IntXY();
		IntRect intRect = new IntRect(intVector.x, intVector.y, RegionWidth, RegionHeight);
		return intRect.Contains(GameManager.Instance.PrimaryPlayer.CenterPosition);
	}
}
