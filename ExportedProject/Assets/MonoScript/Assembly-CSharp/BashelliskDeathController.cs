using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BashelliskDeathController : BraveBehaviour
{
	public VFXPool HeadVfx;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		StartCoroutine(OnDeathCR());
	}

	private IEnumerator OnDeathCR()
	{
		BashelliskHeadController head = GetComponent<BashelliskHeadController>();
		head.behaviorSpeculator.enabled = false;
		head.enabled = false;
		head.StopAllCoroutines();
		while (head.AvailablePickups.Count > 0)
		{
			BashelliskBodyPickupController value = head.AvailablePickups.First.Value;
			if ((bool)value && (bool)value.healthHaver)
			{
				value.healthHaver.ApplyDamage(100000f, Vector2.zero, "death", CoreDamageTypes.None, DamageCategory.Unstoppable);
			}
			head.AvailablePickups.RemoveFirst();
		}
		head.aiAnimator.PlayUntilCancelled("die", true);
		LinkedListNode<BashelliskSegment> node = head.Body.Last;
		float delay = 0.3f;
		while (node != null)
		{
			if (node.Value is BashelliskBodyController)
			{
				BashelliskBodyController bashelliskBodyController = node.Value as BashelliskBodyController;
				AkSoundEngine.PostEvent("Play_WPN_grenade_blast_01", base.gameObject);
				bashelliskBodyController.enabled = false;
				bashelliskBodyController.majorBreakable.breakVfx.SpawnAtPosition(bashelliskBodyController.specRigidbody.GetUnitCenter(ColliderType.HitBox));
				Object.Destroy(bashelliskBodyController.gameObject);
			}
			else if (node.Value == head)
			{
				head.enabled = false;
				AkSoundEngine.PostEvent("Play_ENM_Kali_explode_01", base.gameObject);
				HeadVfx.SpawnAtPosition(head.specRigidbody.GetUnitCenter(ColliderType.HitBox));
			}
			node = node.Previous;
			if (node != null)
			{
				yield return new WaitForSeconds(delay);
			}
			delay *= 0.9f;
		}
		base.aiActor.StealthDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(base.gameObject);
	}
}
