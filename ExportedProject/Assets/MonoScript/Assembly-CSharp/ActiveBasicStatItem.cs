using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveBasicStatItem : PlayerItem
{
	public float duration = 5f;

	[BetterList]
	public List<StatModifier> modifiers;

	private PlayerController m_cachedUser;

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);
		m_cachedUser = user;
		StartCoroutine(HandleDuration(user));
	}

	private IEnumerator HandleDuration(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			Debug.LogError("Using a ActiveBasicStatItem while it is already active!");
			yield break;
		}
		base.IsCurrentlyActive = true;
		user.stats.RecalculateStats(user);
		m_activeElapsed = 0f;
		m_activeDuration = duration;
		while (m_activeElapsed < m_activeDuration && base.IsCurrentlyActive)
		{
			yield return null;
		}
		base.IsCurrentlyActive = false;
		user.stats.RecalculateStats(user);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (base.IsCurrentlyActive && (bool)m_cachedUser)
		{
			m_cachedUser.stats.RecalculateStats(m_cachedUser);
		}
		m_cachedUser = null;
		base.OnPreDrop(user);
	}

	public override void OnItemSwitched(PlayerController user)
	{
		base.OnItemSwitched(user);
		base.IsCurrentlyActive = false;
		if ((bool)m_cachedUser)
		{
			m_cachedUser.stats.RecalculateStats(m_cachedUser);
		}
	}

	protected override void OnDestroy()
	{
		base.IsCurrentlyActive = false;
		if ((bool)m_cachedUser)
		{
			m_cachedUser.stats.RecalculateStats(m_cachedUser);
		}
		m_cachedUser = null;
		base.OnDestroy();
	}
}
