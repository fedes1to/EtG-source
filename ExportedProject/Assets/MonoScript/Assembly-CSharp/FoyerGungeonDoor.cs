using System;
using System.Collections;
using UnityEngine;

public class FoyerGungeonDoor : BraveBehaviour
{
	public bool LoadsCustomLevel;

	[ShowInInspectorIf("LoadsCustomLevel", false)]
	public string LevelNameToLoad = string.Empty;

	public bool LoadsCharacterSelect;

	public bool ReturnToFoyerFromTutorial;

	public bool southernDoor;

	private bool m_triggered;

	private bool m_coopTriggered;

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggered));
	}

	private void OnTriggered(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (LoadsCustomLevel && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			return;
		}
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if (ReturnToFoyerFromTutorial)
		{
			if (!m_triggered && component != null && component == GameManager.Instance.PrimaryPlayer)
			{
				m_triggered = true;
				StartCoroutine(HandleLoading(component));
			}
		}
		else if (!m_triggered && component != null && component == GameManager.Instance.PrimaryPlayer)
		{
			m_triggered = true;
			StartCoroutine(HandleLoading(component));
		}
		else if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && component != null && component == GameManager.Instance.SecondaryPlayer && m_triggered && !m_coopTriggered)
		{
			m_coopTriggered = true;
			StartCoroutine(HandleCoopAnimation(component));
		}
	}

	private IEnumerator HandleCoopAnimation(PlayerController p)
	{
		p.specRigidbody.Velocity = Vector2.zero;
		p.CurrentInputState = PlayerInputState.NoInput;
		p.ToggleShadowVisiblity(false);
		if (!southernDoor)
		{
			p.QueueSpecificAnimation("doorway");
			float elapsed = 0f;
			float duration = 0.5f;
			while (elapsed < duration)
			{
				elapsed += BraveTime.DeltaTime;
				p.specRigidbody.Velocity = Vector2.up * 2f;
				yield return null;
			}
		}
		else
		{
			p.spriteAnimator.Stop();
		}
	}

	private IEnumerator HandleLoading(PlayerController p)
	{
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().ForceHideMetaCurrencyPanel();
		p.specRigidbody.Velocity = Vector2.zero;
		p.CurrentInputState = PlayerInputState.NoInput;
		p.ToggleShadowVisiblity(false);
		if (ReturnToFoyerFromTutorial)
		{
			p.specRigidbody.Velocity = Vector2.down * p.stats.MovementSpeed;
		}
		else if (!southernDoor)
		{
			p.QueueSpecificAnimation("doorway");
			float elapsed = 0f;
			float duration = 0.5f;
			while (elapsed < duration)
			{
				elapsed += BraveTime.DeltaTime;
				p.specRigidbody.Velocity = Vector2.up * 2f;
				yield return null;
			}
		}
		else
		{
			p.spriteAnimator.Stop();
		}
		Pixelator.Instance.FadeToBlack(0.5f);
		if (ReturnToFoyerFromTutorial)
		{
			Foyer.DoIntroSequence = false;
			Foyer.DoMainMenu = false;
			GameManager.Instance.DelayedReturnToFoyer(0.75f);
		}
		else if (LoadsCharacterSelect)
		{
			GameManager.Instance.DelayedLoadMainMenu(0.75f);
		}
		else if (LoadsCustomLevel)
		{
			GameManager.Instance.DelayedLoadCustomLevel(0.75f, LevelNameToLoad);
		}
		else
		{
			GameManager.Instance.DelayedLoadNextLevel(0.75f);
		}
		yield return new WaitForSeconds(0.5f);
		if (!ReturnToFoyerFromTutorial)
		{
			Foyer.Instance.OnDepartedFoyer();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
