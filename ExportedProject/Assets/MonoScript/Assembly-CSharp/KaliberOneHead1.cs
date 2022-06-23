using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Kaliber/OneHead1")]
public class KaliberOneHead1 : Script
{
	private const int NumBullets = 60;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 60; i++)
		{
			AkSoundEngine.PostEvent("Play_WPN_earthwormgun_shot_01", base.BulletBank.gameObject);
			Vector2 offset;
			float angle;
			GetOffset(out offset, out angle, true);
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle + (float)Random.Range(-20, 20)), new Speed(7f));
			GetOffset(out offset, out angle, false);
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle + (float)Random.Range(-20, 20)), new Speed(7f));
			yield return Wait(3);
		}
	}

	private void GetOffset(out Vector2 offset, out float angle, bool left)
	{
		int num = base.Tick % 40;
		GetFrameOffset(num / 5, out offset, left);
		if (left)
		{
			if (num <= 25)
			{
				angle = Mathf.Lerp(70f, 290f, (float)num / 25f);
			}
			else
			{
				angle = Mathf.Lerp(290f, 70f, (float)(num - 25) / 15f);
			}
		}
		else if (num <= 25)
		{
			angle = Mathf.Lerp(-110f, 110f, (float)num / 25f);
		}
		else
		{
			angle = Mathf.Lerp(110f, -110f, (float)(num - 25) / 15f);
		}
	}

	private void GetFrameOffset(int frame, out Vector2 offset, bool left)
	{
		IntVector2 pixel = IntVector2.Zero;
		if (left)
		{
			switch (frame)
			{
			case 0:
				pixel = new IntVector2(13, 30);
				break;
			case 1:
				pixel = new IntVector2(12, 28);
				break;
			case 2:
				pixel = new IntVector2(13, 20);
				break;
			case 3:
				pixel = new IntVector2(12, 12);
				break;
			case 4:
				pixel = new IntVector2(11, 4);
				break;
			case 5:
				pixel = new IntVector2(11, 3);
				break;
			case 6:
				pixel = new IntVector2(9, 4);
				break;
			case 7:
				pixel = new IntVector2(11, 12);
				break;
			}
		}
		else
		{
			switch (frame)
			{
			case 0:
				pixel = new IntVector2(59, 3);
				break;
			case 1:
				pixel = new IntVector2(61, 4);
				break;
			case 2:
				pixel = new IntVector2(62, 11);
				break;
			case 3:
				pixel = new IntVector2(61, 20);
				break;
			case 4:
				pixel = new IntVector2(60, 28);
				break;
			case 5:
				pixel = new IntVector2(58, 31);
				break;
			case 6:
				pixel = new IntVector2(60, 28);
				break;
			case 7:
				pixel = new IntVector2(61, 21);
				break;
			}
		}
		pixel -= new IntVector2(36, 13);
		offset = PhysicsEngine.PixelToUnit(pixel);
	}
}
