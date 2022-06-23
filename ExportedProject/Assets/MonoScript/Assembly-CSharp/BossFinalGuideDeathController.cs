using System.Collections;
using UnityEngine;

public class BossFinalGuideDeathController : BraveBehaviour
{
	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 5f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		base.aiAnimator.ChildAnimator.gameObject.SetActive(false);
		base.aiAnimator.PlayUntilCancelled("death", true);
		StartCoroutine(HandlePostDeathExplosionCR());
		base.healthHaver.OnPreDeath -= OnBossDeath;
		GameObject gameObject = GameObject.Find("BossFinalGuide_DrWolf(Clone)");
		if ((bool)gameObject)
		{
			HealthHaver component = gameObject.GetComponent<HealthHaver>();
			component.healthIsNumberOfHits = false;
			component.ApplyDamage(10000f, Vector2.zero, "Boss Death", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
		}
	}

	private IEnumerator HandlePostDeathExplosionCR()
	{
		while (base.aiAnimator.IsPlaying("death"))
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		Pixelator.Instance.FadeToColor(2f, Color.white);
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
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
		base.specRigidbody.PixelColliders[1].ManualHeight = 32;
		base.specRigidbody.RegenerateColliders = true;
		base.specRigidbody.CollideWithOthers = true;
		GuidePastController gpc = Object.FindObjectOfType<GuidePastController>();
		gpc.OnBossKilled();
	}
}
