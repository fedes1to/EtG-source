using System;
using System.Collections;
using UnityEngine;

public class VoiceOverer : MonoBehaviour
{
	public string[] IntroEvents;

	public string[] IntroKeys;

	public string[] DefeatedPlayerEvents;

	public string[] DefeatedPlayerKeys;

	public string[] KilledByPlayerEvents;

	public string[] KilledByPlayerKeys;

	public string[] DamageBarkEvents;

	public string[] AttackBarkEvents;

	private float m_sinceAnyBark = 100f;

	private float m_sinceBark = 100f;

	private int m_lastBark = -1;

	private bool m_lastFrameHadContinueAttack;

	private float m_sinceAttackBark = 100f;

	private int m_lastAttackBark = -1;

	private BehaviorSpeculator m_bs;

	private void Start()
	{
		m_bs = GetComponent<BehaviorSpeculator>();
		HealthHaver component = GetComponent<HealthHaver>();
		component.OnDamaged += HandleDamaged;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnRealPlayerDeath = (Action<PlayerController>)Delegate.Combine(obj.OnRealPlayerDeath, new Action<PlayerController>(HandlePlayerGameOver));
		}
	}

	private void HandlePlayerGameOver(PlayerController player)
	{
		AIActor component = GetComponent<AIActor>();
		if (component.HasBeenEngaged && player.CurrentRoom == component.ParentRoom)
		{
			StartCoroutine(HandlePlayerLostVO());
		}
		DisconnectCallback();
	}

	private void DisconnectCallback()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnRealPlayerDeath = (Action<PlayerController>)Delegate.Remove(obj.OnRealPlayerDeath, new Action<PlayerController>(HandlePlayerGameOver));
		}
	}

	private void OnDestroy()
	{
		DisconnectCallback();
	}

	private void Update()
	{
		m_sinceBark += BraveTime.DeltaTime;
		m_sinceAttackBark += BraveTime.DeltaTime;
		m_sinceAnyBark += BraveTime.DeltaTime;
		bool flag = m_bs.ActiveContinuousAttackBehavior != null;
		if (flag && !m_lastFrameHadContinueAttack)
		{
			HandleAttackBark();
		}
		m_lastFrameHadContinueAttack = flag;
	}

	private void HandleDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (m_sinceBark > 10f && m_sinceAnyBark > 3f && resultValue > 0f)
		{
			HandleDamageBark();
			m_sinceBark = 0f;
			m_sinceAnyBark = 0f;
		}
	}

	public IEnumerator HandleIntroVO()
	{
		int eventIndex = UnityEngine.Random.Range(0, IntroEvents.Length);
		AkSoundEngine.PostEvent(IntroEvents[eventIndex], base.gameObject);
		AIAnimator aiAnimator = GetComponent<AIAnimator>();
		aiAnimator.PlayUntilCancelled("intro_talk");
		yield return StartCoroutine(HandleTalk(IntroKeys[eventIndex]));
		aiAnimator.EndAnimationIf("intro_talk");
		m_sinceAttackBark = 5f;
	}

	public IEnumerator HandlePlayerLostVO()
	{
		int eventIndex = UnityEngine.Random.Range(0, DefeatedPlayerEvents.Length);
		AkSoundEngine.PostEvent(DefeatedPlayerEvents[eventIndex], base.gameObject);
		yield return StartCoroutine(HandleTalk(DefeatedPlayerKeys[eventIndex]));
	}

	public IEnumerator HandlePlayerWonVO(float maxDuration)
	{
		int eventIndex = UnityEngine.Random.Range(0, KilledByPlayerEvents.Length);
		AkSoundEngine.PostEvent(KilledByPlayerEvents[eventIndex], base.gameObject);
		yield return StartCoroutine(HandleTalk(KilledByPlayerKeys[eventIndex], maxDuration));
	}

	public void HandleDamageBark()
	{
		int num = UnityEngine.Random.Range(0, DamageBarkEvents.Length);
		if (num == m_lastBark && m_lastBark == 2)
		{
			num = 0;
		}
		m_lastBark = num;
		AkSoundEngine.PostEvent(DamageBarkEvents[num], base.gameObject);
	}

	public void HandleAttackBark()
	{
		if (m_sinceAttackBark > 10f && m_sinceAnyBark > 3f)
		{
			m_sinceAttackBark = 0f;
			m_sinceAnyBark = 0f;
			int num;
			for (num = UnityEngine.Random.Range(0, AttackBarkEvents.Length); num == m_lastBark; num = UnityEngine.Random.Range(0, AttackBarkEvents.Length))
			{
			}
			AkSoundEngine.PostEvent(AttackBarkEvents[num], base.gameObject);
		}
	}

	public IEnumerator HandleTalk(string stringKey, float maxDuration = -1f)
	{
		GetComponent<GenericIntroDoer>().SuppressSkipping = true;
		if (string.IsNullOrEmpty(stringKey))
		{
			if (maxDuration > 0f)
			{
				yield return new WaitForSeconds(maxDuration / 2f);
			}
		}
		else
		{
			yield return StartCoroutine(TalkRaw(StringTableManager.GetString(stringKey), maxDuration));
		}
		yield return null;
		GetComponent<GenericIntroDoer>().SuppressSkipping = false;
	}

	private IEnumerator TalkRaw(string plaintext, float maxDuration = -1f)
	{
		TextBoxManager.ShowTextBox(base.transform.position + new Vector3(2.25f, 7.5f, 0f), base.transform, 5f, plaintext, string.Empty, false);
		float elapsed = 0f;
		bool advancedPressed = false;
		while (!advancedPressed)
		{
			advancedPressed = BraveInput.GetInstanceForPlayer(0).WasAdvanceDialoguePressed() || BraveInput.GetInstanceForPlayer(1).WasAdvanceDialoguePressed();
			if (maxDuration > 0f)
			{
				elapsed += GameManager.INVARIANT_DELTA_TIME;
				if (elapsed > maxDuration)
				{
					break;
				}
			}
			yield return null;
		}
		TextBoxManager.ClearTextBox(base.transform);
	}
}
