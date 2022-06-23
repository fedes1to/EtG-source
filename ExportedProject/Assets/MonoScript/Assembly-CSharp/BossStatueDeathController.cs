using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStatueDeathController : BraveBehaviour
{
	public float deathFlashInterval = 0.1f;

	public List<GameObject> explosionVfx;

	public float explosionMidDelay = 0.3f;

	public int explosionCount = 10;

	public Transform bigExplosionTransform;

	public GameObject bigExplosionVfx;

	public List<GameObject> debrisObjects;

	public int debrisCount;

	public int debrisMinForce = 5;

	public int debrisMaxForce = 5;

	public float debrisAngleVariance = 15f;

	private BossStatueController m_statueController;

	private bool m_isReallyDead;

	public void Start()
	{
		m_statueController = GetComponent<BossStatueController>();
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
	}

	protected override void OnDestroy()
	{
		if ((bool)this)
		{
			Object.Destroy(base.transform.parent.gameObject);
		}
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		StartCoroutine(OnDeathAnimationCR());
	}

	private IEnumerator OnDeathAnimationCR()
	{
		PixelCollider collider = base.specRigidbody.HitboxPixelCollider;
		while (!m_statueController.IsGrounded)
		{
			m_statueController.State = BossStatueController.StatueState.StandStill;
			yield return null;
		}
		string deathAnim = m_statueController.CurrentLevel.deathAnim;
		base.spriteAnimator.Play(deathAnim);
		tk2dSpriteAnimationClip deathClip = base.spriteAnimator.CurrentClip;
		float explosionsDelay = Mathf.Max(0f, (float)(deathClip.frames.Length - 1) / deathClip.fps - explosionMidDelay * (float)explosionCount);
		yield return new WaitForSeconds(explosionsDelay);
		StartCoroutine(DeathFlashCR());
		for (int i = 0; i < explosionCount; i++)
		{
			Vector2 minPos = collider.UnitBottomLeft;
			Vector2 maxPos = collider.UnitTopRight;
			float yStep = (maxPos.y - minPos.y) / (float)explosionCount;
			GameObject vfxPrefab = BraveUtility.RandomElement(explosionVfx);
			Vector2 pos = BraveUtility.RandomVector2(minPos.WithY(minPos.y + yStep * (float)i), maxPos.WithY(minPos.y + yStep * (float)(i + 1)), new Vector2(0.4f, 0f));
			GameObject vfxObj = SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			yield return new WaitForSeconds(explosionMidDelay);
		}
		if (base.spriteAnimator.IsPlaying(deathAnim))
		{
			while (base.spriteAnimator.IsPlaying(deathAnim) && base.spriteAnimator.CurrentFrame < deathClip.frames.Length - 1)
			{
				yield return null;
			}
		}
		if ((bool)bigExplosionVfx)
		{
			Vector2 vector = base.specRigidbody.HitboxPixelCollider.UnitCenter;
			if ((bool)bigExplosionTransform)
			{
				vector = bigExplosionTransform.position;
			}
			GameObject gameObject = SpawnManager.SpawnVFX(bigExplosionVfx, vector, Quaternion.identity);
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			component.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(component);
			base.sprite.UpdateZDepth();
		}
		Vector2 unitBottomLeft = collider.UnitBottomLeft;
		Vector2 unitCenter = collider.UnitCenter;
		Vector2 unitTopRight = collider.UnitTopRight;
		for (int j = 0; j < debrisCount; j++)
		{
			GameObject prefab = BraveUtility.RandomElement(debrisObjects);
			Vector2 vector2 = BraveUtility.RandomVector2(unitBottomLeft, unitTopRight, new Vector2(-1.5f, -1.5f));
			GameObject gameObject2 = SpawnManager.SpawnVFX(prefab, vector2, Quaternion.identity);
			if ((bool)gameObject2)
			{
				gameObject2.transform.parent = SpawnManager.Instance.VFX;
				DebrisObject orAddComponent = gameObject2.GetOrAddComponent<DebrisObject>();
				if ((bool)base.aiActor)
				{
					base.aiActor.sprite.AttachRenderer(orAddComponent.sprite);
				}
				orAddComponent.angularVelocity = Mathf.Sign(Random.value - 0.5f) * 125f;
				orAddComponent.angularVelocityVariance = 60f;
				orAddComponent.decayOnBounce = 0.5f;
				orAddComponent.bounceCount = 1;
				orAddComponent.canRotate = true;
				float angle = (vector2 - unitCenter).ToAngle() + Random.Range(0f - debrisAngleVariance, debrisAngleVariance);
				Vector2 vector3 = BraveMathCollege.DegreesToVector(angle) * Random.Range(debrisMinForce, debrisMaxForce);
				Vector3 startingForce = new Vector3(vector3.x, (!(vector3.y < 0f)) ? 0f : vector3.y, (!(vector3.y > 0f)) ? 0f : vector3.y);
				if ((bool)orAddComponent.minorBreakable)
				{
					orAddComponent.minorBreakable.enabled = true;
				}
				orAddComponent.Trigger(startingForce, 1f);
			}
		}
		base.sprite.renderer.enabled = false;
		m_statueController.shadowSprite.renderer.enabled = false;
		if (m_statueController.IsKali)
		{
			if (GameStatsManager.Instance.huntProgress != null)
			{
				GameStatsManager.Instance.huntProgress.ProcessStatuesKill();
			}
			Object.Destroy(base.transform.parent.gameObject);
		}
		base.specRigidbody.PixelColliders[0].Enabled = false;
		base.specRigidbody.PixelColliders[1].Enabled = false;
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		m_isReallyDead = true;
		if ((bool)m_statueController)
		{
			Object.Destroy(m_statueController);
		}
		if ((bool)base.aiActor)
		{
			Object.Destroy(base.aiActor);
		}
		if ((bool)base.healthHaver)
		{
			Object.Destroy(base.healthHaver);
		}
		if ((bool)base.behaviorSpeculator)
		{
			Object.Destroy(base.behaviorSpeculator);
		}
		RegenerateCache();
	}

	private IEnumerator DeathFlashCR()
	{
		Color startingColor = base.renderer.material.GetColor("_OverrideColor");
		while (!m_isReallyDead)
		{
			base.renderer.material.SetColor("_OverrideColor", Color.white);
			yield return new WaitForSeconds(deathFlashInterval);
			if (m_isReallyDead)
			{
				break;
			}
			base.renderer.material.SetColor("_OverrideColor", startingColor);
			yield return new WaitForSeconds(deathFlashInterval);
		}
	}
}
