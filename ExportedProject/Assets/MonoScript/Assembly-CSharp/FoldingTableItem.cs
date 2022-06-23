using Dungeonator;
using UnityEngine;

public class FoldingTableItem : PlayerItem
{
	public FlippableCover TableToSpawn;

	public override bool CanBeUsed(PlayerController user)
	{
		if (!user || user.InExitCell || user.CurrentRoom == null)
		{
			return false;
		}
		Vector2 nearbyPoint = user.CenterPosition + (user.unadjustedAimPoint.XY() - user.CenterPosition).normalized;
		if (!user.CurrentRoom.GetNearestAvailableCell(nearbyPoint, IntVector2.One, CellTypes.FLOOR).HasValue)
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		AkSoundEngine.PostEvent("Play_ITM_Folding_Table_Use_01", base.gameObject);
		Vector2 nearbyPoint = user.CenterPosition + (user.unadjustedAimPoint.XY() - user.CenterPosition).normalized;
		GameObject gameObject = Object.Instantiate(position: user.CurrentRoom.GetNearestAvailableCell(nearbyPoint, IntVector2.One, CellTypes.FLOOR).Value.ToVector2(), original: TableToSpawn.gameObject, rotation: Quaternion.identity);
		SpeculativeRigidbody componentInChildren = gameObject.GetComponentInChildren<SpeculativeRigidbody>();
		FlippableCover component = gameObject.GetComponent<FlippableCover>();
		component.transform.position.XY().GetAbsoluteRoom().RegisterInteractable(component);
		component.ConfigureOnPlacement(component.transform.position.XY().GetAbsoluteRoom());
		componentInChildren.Initialize();
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(componentInChildren);
	}
}
