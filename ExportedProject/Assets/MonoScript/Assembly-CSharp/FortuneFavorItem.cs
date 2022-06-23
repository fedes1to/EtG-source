using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class FortuneFavorItem : PlayerItem
{
	public float pushRadius = 4f;

	public float secondRadius = 6f;

	public float finalRadius = 8f;

	public float pushStrength = 10f;

	public float duration = 5f;

	public GameObject sparkOctantVFX;

	protected override void DoEffect(PlayerController user)
	{
		StartCoroutine(HandleShield(user));
		AkSoundEngine.PostEvent("Play_OBJ_fortune_shield_01", base.gameObject);
	}

	private IEnumerator HandleShield(PlayerController user)
	{
		base.IsCurrentlyActive = true;
		m_activeElapsed = 0f;
		m_activeDuration = duration;
		float innerRadiusSqrDistance = pushRadius * pushRadius;
		float outerRadiusSqrDistance = secondRadius * secondRadius;
		float finalRadiusSqrDistance = finalRadius * finalRadius;
		float pushStrengthRadians = pushStrength * ((float)Math.PI / 180f);
		List<Projectile> ensnaredProjectiles = new List<Projectile>();
		List<Vector2> initialDirections = new List<Vector2>();
		GameObject[] octantVFX = new GameObject[8];
		while (m_activeElapsed < m_activeDuration)
		{
			Vector2 playerCenter = user.CenterPosition;
			ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
			for (int i = 0; i < allProjectiles.Count; i++)
			{
				Projectile projectile = allProjectiles[i];
				if (!(projectile.Owner != user) || projectile.Owner is PlayerController)
				{
					continue;
				}
				Vector2 worldCenter = projectile.sprite.WorldCenter;
				Vector2 vector = worldCenter - playerCenter;
				float num = Vector2.SqrMagnitude(vector);
				if (num < innerRadiusSqrDistance && !ensnaredProjectiles.Contains(projectile))
				{
					projectile.RemoveBulletScriptControl();
					ensnaredProjectiles.Add(projectile);
					initialDirections.Add(projectile.Direction);
					int num2 = BraveMathCollege.VectorToOctant(vector);
					if (octantVFX[num2] == null)
					{
						octantVFX[num2] = user.PlayEffectOnActor(sparkOctantVFX, Vector3.zero, true, true);
						octantVFX[num2].transform.rotation = Quaternion.Euler(0f, 0f, -45 + -45 * num2);
					}
				}
			}
			for (int j = 0; j < ensnaredProjectiles.Count; j++)
			{
				Projectile projectile2 = ensnaredProjectiles[j];
				if (!projectile2)
				{
					ensnaredProjectiles.RemoveAt(j);
					initialDirections.RemoveAt(j);
					j--;
					continue;
				}
				Vector2 worldCenter2 = projectile2.sprite.WorldCenter;
				Vector2 vector2 = playerCenter - worldCenter2;
				float num3 = Vector2.SqrMagnitude(vector2);
				if (num3 > finalRadiusSqrDistance)
				{
					ensnaredProjectiles.RemoveAt(j);
					initialDirections.RemoveAt(j);
					j--;
					continue;
				}
				if (num3 > outerRadiusSqrDistance)
				{
					projectile2.Direction = Vector3.RotateTowards(projectile2.Direction, initialDirections[j], pushStrengthRadians * BraveTime.DeltaTime * 0.5f, 0f).XY().normalized;
					continue;
				}
				Vector2 vector3 = vector2 * -1f;
				float num4 = 1f;
				if (num3 / innerRadiusSqrDistance < 0.75f)
				{
					num4 = 3f;
				}
				vector3 = ((vector3.normalized + initialDirections[j].normalized) / 2f).normalized;
				projectile2.Direction = Vector3.RotateTowards(projectile2.Direction, vector3, pushStrengthRadians * BraveTime.DeltaTime * num4, 0f).XY().normalized;
			}
			for (int k = 0; k < 8; k++)
			{
				if (octantVFX[k] != null && !octantVFX[k])
				{
					octantVFX[k] = null;
				}
			}
			yield return null;
		}
		base.IsCurrentlyActive = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
