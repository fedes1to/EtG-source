using System.Collections;
using Dungeonator;
using UnityEngine;

public class InstantProjectileEffectModifier : BraveBehaviour
{
	public bool DoesWhiteFlash;

	public float RoomDamageRadius = 10f;

	public VFXPool AdditionalVFX;

	public bool DoesAdditionalScreenShake;

	[ShowInInspectorIf("DoesAdditionalScreenShake", false)]
	public ScreenShakeSettings AdditionalScreenShake;

	public bool DoesRadialProjectileModule;

	[ShowInInspectorIf("DoesRadialProjectileModule", false)]
	public RadialBurstInterface RadialModule;

	private IEnumerator Start()
	{
		yield return null;
		if (DoesWhiteFlash)
		{
			Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.1f);
		}
		RoomHandler currentRoom = base.transform.position.GetAbsoluteRoom();
		currentRoom.ApplyActionToNearbyEnemies(base.transform.position.XY(), RoomDamageRadius, delegate(AIActor a, float b)
		{
			if ((bool)a && a.IsNormalEnemy && (bool)a.healthHaver)
			{
				a.healthHaver.ApplyDamage(base.projectile.ModifiedDamage, Vector2.zero, "projectile", base.projectile.damageTypes);
			}
		});
		AdditionalVFX.SpawnAtPosition(base.transform.position.XY());
		if (DoesAdditionalScreenShake)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(AdditionalScreenShake, base.transform.position.XY(), true);
		}
		if (DoesRadialProjectileModule && base.projectile.Owner is PlayerController)
		{
			RadialModule.DoBurst(base.projectile.Owner as PlayerController);
		}
		base.projectile.DieInAir();
	}
}
