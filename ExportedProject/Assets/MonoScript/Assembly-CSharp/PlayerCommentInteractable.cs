using System;
using System.Collections;
using UnityEngine;

public class PlayerCommentInteractable : BraveBehaviour, IPlayerInteractable
{
	public CommentModule[] comments;

	public bool onlyTriggerOnce;

	public PlayerCommentInteractable[] linkedInteractables;

	public bool keyIsSequential;

	[Header("Interactable Region")]
	public bool usesOverrideInteractionRegion;

	[ShowInInspectorIf("usesOverrideInteractionRegion", false)]
	public Vector2 overrideRegionOffset = Vector2.zero;

	[ShowInInspectorIf("usesOverrideInteractionRegion", false)]
	public Vector2 overrideRegionDimensions = Vector2.zero;

	private bool m_isDoing;

	private bool m_hasBeenTriggered;

	public Action OnInteractionBegan;

	public Action OnInteractionFinished;

	private int m_seqIndex;

	private void Start()
	{
	}

	private IEnumerator Do()
	{
		m_isDoing = true;
		if (OnInteractionBegan != null)
		{
			OnInteractionBegan();
		}
		Transform primaryTransform = GameManager.Instance.PrimaryPlayer.transform;
		Transform secondaryTransform = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? null : GameManager.Instance.SecondaryPlayer.transform);
		Transform dogTransform = ((GameManager.Instance.PrimaryPlayer.companions.Count <= 0) ? null : GameManager.Instance.PrimaryPlayer.companions[0].transform);
		for (int i = 0; i < comments.Length; i++)
		{
			CommentModule currentModule = comments[i];
			Transform targetTransform = null;
			Vector3 targetOffset = Vector3.zero;
			string audioTag = string.Empty;
			switch (currentModule.target)
			{
			case CommentModule.CommentTarget.PRIMARY:
				targetTransform = primaryTransform;
				targetOffset = new Vector3(0.5f, 1.5f, 0f);
				audioTag = GameManager.Instance.PrimaryPlayer.characterAudioSpeechTag;
				break;
			case CommentModule.CommentTarget.SECONDARY:
				targetTransform = secondaryTransform;
				targetOffset = new Vector3(0.5f, 1.5f, 0f);
				audioTag = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? GameManager.Instance.PrimaryPlayer.characterAudioSpeechTag : GameManager.Instance.SecondaryPlayer.characterAudioSpeechTag);
				break;
			case CommentModule.CommentTarget.DOG:
				targetTransform = dogTransform;
				targetOffset = new Vector3(0.25f, 1f, 0f);
				break;
			}
			if (targetTransform != null)
			{
				DoAmbientTalk(targetTransform, targetOffset, currentModule.stringKey, currentModule.duration, audioTag);
				yield return new WaitForSeconds(currentModule.delay);
			}
		}
		if (OnInteractionFinished != null)
		{
			OnInteractionFinished();
		}
		m_isDoing = false;
	}

	public void DoAmbientTalk(Transform baseTransform, Vector3 offset, string stringKey, float duration, string overrideAudioTag = "")
	{
		string text;
		if (keyIsSequential)
		{
			text = StringTableManager.GetStringSequential(stringKey, ref m_seqIndex);
			for (int i = 0; i < linkedInteractables.Length; i++)
			{
				linkedInteractables[i].m_seqIndex++;
			}
		}
		else
		{
			text = StringTableManager.GetString(stringKey);
		}
		TextBoxManager.ShowThoughtBubble(baseTransform.position + offset, baseTransform, duration, text, false, false, overrideAudioTag);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (m_hasBeenTriggered && onlyTriggerOnce)
		{
			return 1000f;
		}
		if (m_isDoing)
		{
			return 1000f;
		}
		if (usesOverrideInteractionRegion)
		{
			return BraveMathCollege.DistToRectangle(point, base.transform.position.XY() + overrideRegionOffset * 0.0625f, overrideRegionDimensions * 0.0625f);
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)base.sprite)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
		}
	}

	public void ForceDisable()
	{
		m_hasBeenTriggered = true;
		onlyTriggerOnce = true;
	}

	public void Interact(PlayerController interactor)
	{
		if ((!m_hasBeenTriggered || !onlyTriggerOnce) && !m_isDoing)
		{
			for (int i = 0; i < linkedInteractables.Length; i++)
			{
				linkedInteractables[i].m_hasBeenTriggered = true;
			}
			m_hasBeenTriggered = true;
			StartCoroutine(Do());
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}
}
