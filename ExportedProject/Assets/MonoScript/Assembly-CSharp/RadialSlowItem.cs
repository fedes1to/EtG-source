using System.Collections;
using UnityEngine;

public class RadialSlowItem : AffectEnemiesInRadiusItem
{
	public float InTime;

	public float HoldTime = 5f;

	public float OutTime = 3f;

	public float MaxTimeModifier = 0.25f;

	public bool AllowStealing;

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_time_bell_01", base.gameObject);
		base.DoEffect(user);
		if (AllowStealing)
		{
			user.StartCoroutine(HandleStealEffect(user));
		}
	}

	private IEnumerator HandleStealEffect(PlayerController user)
	{
		user.SetCapableOfStealing(true, "AffectEnemiesInRadiusItem");
		m_activeDuration = InTime + HoldTime + OutTime;
		while (m_activeElapsed < m_activeDuration)
		{
			m_activeElapsed += BraveTime.DeltaTime;
			yield return null;
		}
		user.SetCapableOfStealing(false, "AffectEnemiesInRadiusItem");
	}

	protected override void AffectEnemy(AIActor target)
	{
		if (!base.IsCurrentlyActive)
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleActive());
		}
		target.StartCoroutine(ProcessSlow(target));
	}

	protected override void AffectForgeHammer(ForgeHammerController target)
	{
		if (!base.IsCurrentlyActive)
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleActive());
		}
		target.StartCoroutine(ProcessHammerSlow(target));
	}

	protected override void AffectProjectileTrap(ProjectileTrapController target)
	{
		if (!base.IsCurrentlyActive)
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleActive());
		}
		target.StartCoroutine(ProcessTrapSlow(target));
	}

	protected override void AffectShop(BaseShopController target)
	{
		if (AllowStealing && (bool)target && (bool)target.shopkeepFSM)
		{
			AIAnimator component = target.shopkeepFSM.GetComponent<AIAnimator>();
			if (!base.IsCurrentlyActive)
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleActive());
			}
			target.StartCoroutine(ProcessShopSlow(target, component));
		}
	}

	protected override void AffectMajorBreakable(MajorBreakable target)
	{
		if ((bool)target.behaviorSpeculator)
		{
			target.StartCoroutine(ProcessBehaviorSpeculatorSlow(target.behaviorSpeculator));
		}
	}

	private IEnumerator HandleActive()
	{
		base.IsCurrentlyActive = true;
		m_activeDuration = InTime + HoldTime + OutTime;
		while (m_activeElapsed < m_activeDuration)
		{
			m_activeElapsed += BraveTime.DeltaTime;
			yield return null;
		}
		base.IsCurrentlyActive = false;
	}

	private IEnumerator ProcessSlow(AIActor target)
	{
		float elapsed3 = 0f;
		if (InTime > 0f)
		{
			while (elapsed3 < InTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(t: elapsed3 / InTime, a: 1f, b: MaxTimeModifier);
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (HoldTime > 0f)
		{
			while (elapsed3 < HoldTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = MaxTimeModifier;
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (OutTime > 0f)
		{
			while (elapsed3 < OutTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(t: elapsed3 / OutTime, a: MaxTimeModifier, b: 1f);
				yield return null;
			}
		}
		if ((bool)target)
		{
			target.LocalTimeScale = 1f;
		}
	}

	private IEnumerator ProcessHammerSlow(ForgeHammerController target)
	{
		float elapsed3 = 0f;
		if (InTime > 0f)
		{
			while (elapsed3 < InTime)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(1f, MaxTimeModifier, elapsed3 / InTime);
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (HoldTime > 0f)
		{
			while (elapsed3 < HoldTime)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = MaxTimeModifier;
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (OutTime > 0f)
		{
			while (elapsed3 < OutTime)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(MaxTimeModifier, 1f, elapsed3 / OutTime);
				yield return null;
			}
		}
		if ((bool)target)
		{
			target.LocalTimeScale = 1f;
		}
	}

	private IEnumerator ProcessTrapSlow(ProjectileTrapController target)
	{
		float elapsed3 = 0f;
		if (InTime > 0f)
		{
			while (elapsed3 < InTime)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(1f, MaxTimeModifier, elapsed3 / InTime);
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (HoldTime > 0f)
		{
			while (elapsed3 < HoldTime)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = MaxTimeModifier;
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (OutTime > 0f)
		{
			while (elapsed3 < OutTime)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(MaxTimeModifier, 1f, elapsed3 / OutTime);
				yield return null;
			}
		}
		if ((bool)target)
		{
			target.LocalTimeScale = 1f;
		}
	}

	private IEnumerator ProcessShopSlow(BaseShopController target, AIAnimator shopkeep)
	{
		target.SetCapableOfBeingStolenFrom(true, "RadialSlowItem");
		float elapsed = 0f;
		if (HoldTime + InTime > 0f)
		{
			while (elapsed < HoldTime + InTime && !target.WasCaughtStealing)
			{
				elapsed += BraveTime.DeltaTime;
				shopkeep.aiAnimator.FpsScale = MaxTimeModifier;
				yield return null;
			}
		}
		elapsed = 0f;
		if (OutTime > 0f)
		{
			while (elapsed < OutTime && !target.WasCaughtStealing)
			{
				elapsed += BraveTime.DeltaTime;
				shopkeep.aiAnimator.FpsScale = Mathf.Lerp(MaxTimeModifier, 1f, elapsed / OutTime);
				yield return null;
			}
		}
		shopkeep.aiAnimator.FpsScale = 1f;
		target.SetCapableOfBeingStolenFrom(false, "RadialSlowItem");
	}

	private IEnumerator ProcessBehaviorSpeculatorSlow(BehaviorSpeculator target)
	{
		float elapsed2 = 0f;
		AIAnimator aiAnimator = ((!target) ? null : target.aiAnimator);
		if (InTime > 0f)
		{
			while (elapsed2 < InTime && (bool)target)
			{
				elapsed2 += BraveTime.DeltaTime;
				float t2 = elapsed2 / InTime;
				target.LocalTimeScale = Mathf.Lerp(1f, MaxTimeModifier, t2);
				if ((bool)aiAnimator)
				{
					aiAnimator.FpsScale = Mathf.Lerp(1f, MaxTimeModifier, t2);
				}
				yield return null;
			}
		}
		elapsed2 = 0f;
		if (HoldTime > 0f)
		{
			while (elapsed2 < HoldTime && (bool)target)
			{
				elapsed2 += BraveTime.DeltaTime;
				target.LocalTimeScale = MaxTimeModifier;
				if ((bool)aiAnimator)
				{
					aiAnimator.FpsScale = MaxTimeModifier;
				}
				yield return null;
			}
		}
		elapsed2 = 0f;
		if (OutTime > 0f)
		{
			while (elapsed2 < OutTime && (bool)target)
			{
				elapsed2 += BraveTime.DeltaTime;
				float t = elapsed2 / OutTime;
				target.LocalTimeScale = Mathf.Lerp(MaxTimeModifier, 1f, t);
				if ((bool)aiAnimator)
				{
					aiAnimator.FpsScale = Mathf.Lerp(MaxTimeModifier, 1f, t);
				}
				yield return null;
			}
		}
		if ((bool)aiAnimator)
		{
			aiAnimator.FpsScale = 1f;
		}
		if ((bool)target)
		{
			target.LocalTimeScale = 1f;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
