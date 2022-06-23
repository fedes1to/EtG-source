using System.Collections;
using UnityEngine;

public class ResourcefulRatDeathController : BraveBehaviour
{
	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 5f;
		base.healthHaver.TrackDuringDeath = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		StartCoroutine(BossDeathCR());
		base.healthHaver.OnPreDeath -= OnBossDeath;
	}

	private IEnumerator BossDeathCR()
	{
		yield return new WaitForSeconds(0.66f);
		ResourcefulRatBossRoomController roomController = Object.FindObjectOfType<ResourcefulRatBossRoomController>();
		roomController.OpenGrate();
		yield return new WaitForSeconds(0.33f);
		Vector2 target = base.aiActor.ParentRoom.area.UnitBottomLeft + new Vector2(17f, 12f);
		Vector2 toTarget = target - base.specRigidbody.UnitCenter;
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = toTarget.ToAngle();
		if (!base.specRigidbody.UnitCenter.IsWithin(target + new Vector2(-2f, -2f), target + new Vector2(2f, 2f)))
		{
			base.aiAnimator.PlayUntilCancelled("move");
			float moveSpeed = 7f;
			bool hasDove = false;
			Vector2 velocity = toTarget.normalized * moveSpeed;
			float timer = toTarget.magnitude / moveSpeed;
			while (timer > 0f)
			{
				base.specRigidbody.Velocity = velocity;
				timer -= BraveTime.DeltaTime;
				if (!hasDove)
				{
					float magnitude = (target - base.specRigidbody.UnitCenter).magnitude;
					if (magnitude < 2.5f)
					{
						base.aiAnimator.PlayUntilCancelled("dodge");
						hasDove = true;
					}
				}
				yield return null;
			}
		}
		base.specRigidbody.Velocity = Vector2.zero;
		base.aiAnimator.PlayUntilCancelled("pitfall");
		yield return new WaitForSeconds(base.aiAnimator.CurrentClipLength);
		roomController.EnablePitfalls(true);
		BossKillCam extantCam = Object.FindObjectOfType<BossKillCam>();
		if ((bool)extantCam)
		{
			extantCam.SetPhaseCountdown(0.5f);
		}
		base.aiActor.StealthDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		Object.Destroy(base.gameObject);
	}
}
