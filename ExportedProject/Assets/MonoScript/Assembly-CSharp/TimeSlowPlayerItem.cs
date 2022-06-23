using System.Collections;

public class TimeSlowPlayerItem : PlayerItem
{
	public float timeScale = 0.5f;

	public float duration = 5f;

	public bool HasSynergy;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public float overrideTimeScale;

	public RadialSlowInterface test;

	protected override void DoEffect(PlayerController user)
	{
		user.StartCoroutine(HandleDuration(user));
	}

	private IEnumerator HandleDuration(PlayerController user)
	{
		AkSoundEngine.PostEvent("State_Bullet_Time_on", base.gameObject);
		base.IsCurrentlyActive = true;
		m_activeElapsed = 0f;
		m_activeDuration = duration;
		float ts = ((!HasSynergy || !user || !user.HasActiveBonusSynergy(RequiredSynergy)) ? timeScale : overrideTimeScale);
		test.DoRadialSlow(user.CenterPosition, user.CurrentRoom);
		float ela = 0f;
		while (ela < m_activeDuration)
		{
			ela = (m_activeElapsed = ela + GameManager.INVARIANT_DELTA_TIME);
			yield return null;
		}
		if ((bool)this)
		{
			AkSoundEngine.PostEvent("State_Bullet_Time_off", base.gameObject);
		}
		base.IsCurrentlyActive = false;
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			StopAllCoroutines();
			AkSoundEngine.PostEvent("State_Bullet_Time_off", base.gameObject);
			BraveTime.ClearMultiplier(base.gameObject);
			base.IsCurrentlyActive = false;
		}
	}

	protected override void OnDestroy()
	{
		if (base.IsCurrentlyActive)
		{
			StopAllCoroutines();
			AkSoundEngine.PostEvent("State_Bullet_Time_off", base.gameObject);
			BraveTime.ClearMultiplier(base.gameObject);
			base.IsCurrentlyActive = false;
		}
		base.OnDestroy();
	}
}
