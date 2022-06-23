public class SecretHandshakeItem : PassiveItem
{
	public static int NumActive;

	private void Awake()
	{
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			NumActive++;
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		NumActive--;
		debrisObject.GetComponent<SecretHandshakeItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp)
		{
			NumActive--;
		}
		base.OnDestroy();
	}
}
