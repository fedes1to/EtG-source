using UnityEngine;

public class EvolutionaryGunController : MonoBehaviour
{
	[PickupIdentifier]
	public int EvoStage01ID = -1;

	[PickupIdentifier]
	public int EvoStage02ID = -1;

	[PickupIdentifier]
	public int EvoStage03ID = -1;

	[PickupIdentifier]
	public int EvoStage04ID = -1;

	[PickupIdentifier]
	public int EvoStage05ID = -1;

	[PickupIdentifier]
	public int EvoStage06ID = -1;

	private Gun m_gun;
}
