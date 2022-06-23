public class DebrisRigidbodyInterface : BraveBehaviour
{
	public bool IsWall;

	public bool IsPit;

	private void Start()
	{
		if (IsWall)
		{
			DebrisObject.SRB_Walls.Add(base.specRigidbody);
		}
		if (IsPit)
		{
			base.specRigidbody.PrimaryPixelCollider.IsTrigger = true;
			DebrisObject.SRB_Pits.Add(base.specRigidbody);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
