using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Spotlight1")]
public class DraGunSpotlight1 : Script
{
	public class ArcBullet : Bullet
	{
		private Vector2 m_target;

		private float m_t;

		public ArcBullet(Vector2 target, float t)
			: base("triangle")
		{
			m_target = target;
			m_t = t;
		}

		protected override IEnumerator Top()
		{
			Vector2 toTarget = m_target - base.Position;
			float travelTime = toTarget.magnitude / Speed * 60f - 1f;
			float magnitude = BraveUtility.RandomSign() * (1f - m_t) * 8f;
			Vector2 offset = magnitude * toTarget.Rotate(90f).normalized;
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			Vector2 lastPosition = base.Position;
			for (int i = 0; (float)i < travelTime; i++)
			{
				UpdateVelocity();
				truePosition += Velocity / 60f;
				lastPosition = base.Position;
				base.Position = truePosition + offset * Mathf.Sin((float)base.Tick / travelTime * (float)Math.PI);
				yield return Wait(1);
			}
			Vector2 v = (base.Position - lastPosition) * 60f;
			Speed = v.magnitude;
			Direction = v.ToAngle();
			base.ManualControl = false;
		}
	}

	public const int ChaseTime = 480;

	protected override IEnumerator Top()
	{
		GameManager.Instance.Dungeon.PreventPlayerLightInDarkTerrifyingRooms = true;
		DraGunController dragunController = base.BulletBank.GetComponent<DraGunController>();
		dragunController.aiActor.ParentRoom.BecomeTerrifyingDarkRoom(0.5f);
		dragunController.HandleDarkRoomEffects(true, 3f);
		yield return Wait(30);
		dragunController.SpotlightPos = base.BulletBank.aiActor.transform.position + new Vector3(4f, 1f);
		dragunController.SpotlightSpeed = 8f;
		dragunController.SpotlightSmoothTime = 0.2f;
		dragunController.SpotlightVelocity = Vector2.zero;
		dragunController.SpotlightEnabled = true;
		StartTask(UpdateSpotlightShrink());
		while (base.Tick < 480)
		{
			float dist = Vector2.Distance(BulletManager.PlayerPosition(), dragunController.SpotlightPos);
			dragunController.SpotlightSpeed = Mathf.Lerp(6f, 14f, Mathf.InverseLerp(3f, 10f, dist));
			if (dist <= dragunController.SpotlightRadius)
			{
				float t = UnityEngine.Random.value;
				float speed = Mathf.Lerp(8f, 14f, t);
				Vector2 target = ((!BraveUtility.RandomBool()) ? GetPredictedTargetPositionExact(1f, speed) : BulletManager.PlayerPosition());
				Fire(new Direction((target - base.Position).ToAngle()), new Speed(speed), new ArcBullet(target, t));
				yield return Wait(3);
			}
			yield return Wait(1);
		}
		dragunController.SpotlightEnabled = false;
		dragunController.aiActor.ParentRoom.EndTerrifyingDarkRoom(0.5f);
		dragunController.HandleDarkRoomEffects(false, 3f);
		yield return Wait(30);
		GameManager.Instance.Dungeon.PreventPlayerLightInDarkTerrifyingRooms = false;
	}

	private IEnumerator UpdateSpotlightShrink()
	{
		DraGunController dragunController = base.BulletBank.GetComponent<DraGunController>();
		int startTick = base.Tick;
		while (base.Tick < 480)
		{
			if (base.Tick - startTick < 10)
			{
				dragunController.SpotlightShrink = (float)(base.Tick - startTick) / 9f;
			}
			else if (base.Tick > 470)
			{
				int num = 480 - base.Tick - 1;
				dragunController.SpotlightShrink = (float)num / 9f;
			}
			yield return Wait(1);
		}
	}

	public override void OnForceEnded()
	{
		DraGunController component = base.BulletBank.GetComponent<DraGunController>();
		component.SpotlightEnabled = false;
		component.aiActor.ParentRoom.EndTerrifyingDarkRoom(0.5f);
		component.HandleDarkRoomEffects(false, 3f);
	}
}
