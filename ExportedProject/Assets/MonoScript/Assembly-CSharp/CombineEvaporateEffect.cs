using System;
using System.Collections;
using UnityEngine;

public class CombineEvaporateEffect : MonoBehaviour
{
	public GameObject ParticleSystemToSpawn;

	public Shader FallbackShader;

	private void Start()
	{
		Projectile component = GetComponent<Projectile>();
		component.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(component.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
	}

	private void HandleHitEnemy(Projectile proj, SpeculativeRigidbody hitRigidbody, bool fatal)
	{
		if (fatal)
		{
			AIActor aiActor = hitRigidbody.aiActor;
			if ((bool)aiActor && aiActor.IsNormalEnemy && (!aiActor.healthHaver || !aiActor.healthHaver.IsBoss))
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleEnemyDeath(aiActor, proj.LastVelocity));
			}
		}
	}

	private IEnumerator HandleEnemyDeath(AIActor target, Vector2 motionDirection)
	{
		target.EraseFromExistenceWithRewards();
		Transform copyTransform = CreateEmptySprite(target);
		tk2dSprite copySprite = copyTransform.GetComponentInChildren<tk2dSprite>();
		GameObject gameObject = UnityEngine.Object.Instantiate(ParticleSystemToSpawn, copySprite.WorldCenter.ToVector3ZisY(), Quaternion.identity);
		ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
		gameObject.transform.parent = copyTransform;
		if ((bool)copySprite)
		{
			gameObject.transform.position = copySprite.WorldCenter;
			Bounds bounds = copySprite.GetBounds();
			ParticleSystem.ShapeModule shape = component.shape;
			shape.scale = new Vector3(bounds.extents.x * 2f, bounds.extents.y * 2f, 0.125f);
		}
		float elapsed = 0f;
		float duration = 2.5f;
		copySprite.renderer.material.DisableKeyword("TINTING_OFF");
		copySprite.renderer.material.EnableKeyword("TINTING_ON");
		copySprite.renderer.material.DisableKeyword("EMISSIVE_OFF");
		copySprite.renderer.material.EnableKeyword("EMISSIVE_ON");
		copySprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
		copySprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
		copySprite.renderer.material.SetFloat("_EmissiveThresholdSensitivity", 5f);
		copySprite.renderer.material.SetFloat("_EmissiveColorPower", 1f);
		int emId = Shader.PropertyToID("_EmissivePower");
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			copySprite.renderer.material.SetFloat(emId, Mathf.Lerp(1f, 10f, t));
			copySprite.renderer.material.SetFloat("_BurnAmount", t);
			copyTransform.position += motionDirection.ToVector3ZisY().normalized * BraveTime.DeltaTime * 1f;
			yield return null;
		}
		UnityEngine.Object.Destroy(copyTransform.gameObject);
	}

	private Transform CreateEmptySprite(AIActor target)
	{
		GameObject gameObject = new GameObject("suck image");
		gameObject.layer = target.gameObject.layer;
		tk2dSprite tk2dSprite2 = gameObject.AddComponent<tk2dSprite>();
		gameObject.transform.parent = SpawnManager.Instance.VFX;
		tk2dSprite2.SetSprite(target.sprite.Collection, target.sprite.spriteId);
		tk2dSprite2.transform.position = target.sprite.transform.position;
		GameObject gameObject2 = new GameObject("image parent");
		gameObject2.transform.position = tk2dSprite2.WorldCenter;
		tk2dSprite2.transform.parent = gameObject2.transform;
		tk2dSprite2.usesOverrideMaterial = true;
		bool flag = false;
		if (target.optionalPalette != null)
		{
			flag = true;
			tk2dSprite2.renderer.material.SetTexture("_PaletteTex", target.optionalPalette);
		}
		if (tk2dSprite2.renderer.material.shader.name.Contains("ColorEmissive"))
		{
		}
		return gameObject2.transform;
	}
}
