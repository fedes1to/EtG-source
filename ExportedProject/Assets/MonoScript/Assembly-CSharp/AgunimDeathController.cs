using System.Collections;
using UnityEngine;

public class AgunimDeathController : BraveBehaviour
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
		StartCoroutine(HandlePostDeathExplosionCR());
	}

	private IEnumerator HandlePostDeathExplosionCR()
	{
		yield return null;
		BossKillCam extantCam = Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.ForceCancelSequence();
		}
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(GetComponent<AgunimIntroDoer>());
		base.aiActor.ToggleRenderers(false);
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.enabled = false;
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
		BulletPastRoomController[] bprcs = Object.FindObjectsOfType<BulletPastRoomController>();
		for (int i = 0; i < bprcs.Length; i++)
		{
			if (bprcs[i].RoomIdentifier == BulletPastRoomController.BulletRoomCategory.ROOM_C)
			{
				yield return StartCoroutine(bprcs[i].HandleAgunimDeath(base.transform));
				break;
			}
		}
	}
}
