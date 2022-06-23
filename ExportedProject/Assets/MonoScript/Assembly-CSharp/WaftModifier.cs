using UnityEngine;

[RequireComponent(typeof(AIActor))]
public class WaftModifier : BraveBehaviour
{
	public bool modifierEnabled = true;

	public float waftMagnitude = 1f;

	public float waftFrequency = 3f;

	public bool fleeWalls = true;

	private void Start()
	{
		base.gameActor.MovementModifiers += ModifyVelocity;
	}

	protected override void OnDestroy()
	{
		base.gameActor.MovementModifiers -= ModifyVelocity;
	}

	public void ModifyVelocity(ref Vector2 volundaryVel, ref Vector2 involuntaryVel)
	{
		if (modifierEnabled)
		{
			Vector2 vector = new Vector2(0f - volundaryVel.y, volundaryVel.x).normalized;
			if (volundaryVel == Vector2.zero)
			{
				vector = Vector2.right;
			}
			float num = Mathf.Sin(Time.timeSinceLevelLoad * waftFrequency) * waftMagnitude;
			Vector2 vector2 = vector * num;
			Vector2 vector3 = Vector2.zero;
			if (fleeWalls && GameManager.Instance.Dungeon.data[base.specRigidbody.UnitBottomCenter.ToIntVector2(VectorConversions.Floor)].isOccludedByTopWall)
			{
				vector3 = Vector2.up * waftMagnitude;
			}
			volundaryVel += vector2 + vector3;
		}
	}
}
