using System;
using System.Collections;
using UnityEngine;

public class NightclubCrowdController : MonoBehaviour
{
	public Transform[] Dancers;

	public Action OnPanic;

	public tk2dBaseSprite[] DanceFloors;

	private bool m_departed;

	private IEnumerator Start()
	{
		IntVector2 minPos = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY()).area.basePosition;
		IntVector2 maxPos = minPos + new IntVector2(42, 42);
		Vector2 primaryCenter = GameManager.Instance.PrimaryPlayer.CenterPosition;
		while (primaryCenter.x > (float)maxPos.x || primaryCenter.y > (float)maxPos.y)
		{
			primaryCenter = GameManager.Instance.PrimaryPlayer.CenterPosition;
			yield return null;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].PostProcessProjectile += NightclubCrowdController_PostProcessProjectile;
			GameManager.Instance.AllPlayers[i].PostProcessBeam += NightclubCrowdController_PostProcessBeam;
		}
	}

	private void NightclubCrowdController_PostProcessBeam(BeamController obj)
	{
		Panic();
	}

	private void NightclubCrowdController_PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		Panic();
	}

	public void Panic()
	{
		if (!m_departed)
		{
			if (OnPanic != null)
			{
				OnPanic();
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				GameManager.Instance.AllPlayers[i].PostProcessProjectile -= NightclubCrowdController_PostProcessProjectile;
				GameManager.Instance.AllPlayers[i].PostProcessBeam -= NightclubCrowdController_PostProcessBeam;
			}
			m_departed = true;
			StartCoroutine(HandlePanic());
			StartCoroutine(HandleDanceFloors());
		}
	}

	private IEnumerator HandleDanceFloors()
	{
		float ela = 0f;
		float cachedEmissivePower = DanceFloors[0].renderer.material.GetFloat("_EmissivePower");
		while (ela < 0.5f)
		{
			bool wasBelow = ela < 0.2f;
			ela += BraveTime.DeltaTime;
			if (ela > 0.2f && wasBelow)
			{
				for (int i = 0; i < DanceFloors.Length; i++)
				{
					DanceFloors[i].renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutEmissive");
				}
			}
			if (ela < 0.2f)
			{
				if (ela % 0.1f < 0.05f)
				{
					for (int j = 0; j < DanceFloors.Length; j++)
					{
						DanceFloors[j].usesOverrideMaterial = true;
						DanceFloors[j].renderer.material.SetFloat("_EmissivePower", 0f);
					}
				}
				else
				{
					for (int k = 0; k < DanceFloors.Length; k++)
					{
						DanceFloors[k].renderer.material.SetFloat("_EmissivePower", cachedEmissivePower);
					}
				}
			}
			else
			{
				for (int l = 0; l < DanceFloors.Length; l++)
				{
					DanceFloors[l].spriteAnimator.Stop();
					DanceFloors[l].renderer.material.SetFloat("_EmissivePower", Mathf.Lerp(cachedEmissivePower, 0f, (ela - 0.2f) / 0.3f));
				}
			}
			yield return null;
		}
		for (int m = 0; m < DanceFloors.Length; m++)
		{
			DanceFloors[m].renderer.material.SetFloat("_EmissivePower", 0f);
		}
	}

	private IEnumerator HandlePanic()
	{
		Vector3 exitLocation = base.transform.position.Quantize(0.0625f);
		float[] targetXCoords = new float[Dancers.Length];
		bool[] hasReachedCenter = new bool[Dancers.Length];
		for (int i = 0; i < Dancers.Length; i++)
		{
			hasReachedCenter[i] = false;
			targetXCoords[i] = UnityEngine.Random.Range(exitLocation.x - 1.25f, exitLocation.x);
		}
		while (true)
		{
			bool allDancersDestroyed = true;
			for (int j = 0; j < Dancers.Length; j++)
			{
				if (!Dancers[j])
				{
					continue;
				}
				allDancersDestroyed = false;
				if (hasReachedCenter[j])
				{
					Dancers[j].position = Dancers[j].position + Vector3.down * 10f * BraveTime.DeltaTime;
					if (Dancers[j].position.y < exitLocation.y - 10f)
					{
						UnityEngine.Object.Destroy(Dancers[j].gameObject);
					}
				}
				else
				{
					Dancers[j].position = Vector3.MoveTowards(Dancers[j].position, exitLocation.WithX(targetXCoords[j]), 10f * BraveTime.DeltaTime);
					if (Vector2.Distance(Dancers[j].PositionVector2(), exitLocation.WithX(targetXCoords[j]).XY()) < 1f)
					{
						hasReachedCenter[j] = true;
					}
				}
			}
			if (allDancersDestroyed)
			{
				break;
			}
			yield return null;
		}
	}
}
