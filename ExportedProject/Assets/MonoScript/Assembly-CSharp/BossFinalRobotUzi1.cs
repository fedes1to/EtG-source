using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRobot/Uzi1")]
public class BossFinalRobotUzi1 : Script
{
	private const float NumBullets = 70f;

	private float NarrowAngle = 60f;

	private float NarrowAngleChance = 0.5f;

	protected override IEnumerator Top()
	{
		bool offhand = false;
		for (int i = 0; (float)i < 70f; i++)
		{
			Fire(direction: new Direction((!(Random.value < NarrowAngleChance)) ? Random.Range(65f, 295f) : (GetAimDirection("left hand shoot point") + Random.Range(0f - NarrowAngle, NarrowAngle))), offset: new Offset("left hand shoot point"), speed: new Speed(12f));
			Fire(direction: new Direction((!(Random.value < NarrowAngleChance)) ? Random.Range(-115f, 115f) : (GetAimDirection("right hand shoot point") + Random.Range(0f - NarrowAngle, NarrowAngle))), offset: new Offset("right hand shoot point"), speed: new Speed(12f));
			if (i % 3 == 0)
			{
				string transform = ((!offhand) ? "left" : "right") + " hand shoot point";
				Fire(direction: new Direction(GetAimDirection(transform)), offset: new Offset(transform), speed: new Speed(12f));
				offhand = !offhand;
			}
			yield return Wait(4);
		}
	}
}
