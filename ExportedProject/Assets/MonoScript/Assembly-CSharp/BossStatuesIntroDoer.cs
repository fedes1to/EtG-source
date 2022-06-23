using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BossStatuesIntroDoer : SpecificIntroDoer
{
	private enum State
	{
		Idle,
		Playing,
		Finished
	}

	public float ghostDelay;

	public float[] ghostMidDelay;

	public List<tk2dSpriteAnimator> ghostAnimators;

	public VFXPool eyeVfx;

	public float dustDelay;

	public List<tk2dSpriteAnimator> dustAnimators;

	public float floatDelay = 0.3f;

	public float floatTime = 2f;

	public float hangTime = 0.5f;

	public float slamTime = 1f;

	public ScreenShakeSettings floatScreenShake;

	public ScreenShakeSettings slamScreenShake;

	private State m_state;

	private BossStatuesController m_statuesController;

	private List<BossStatueController> m_allStatues;

	private List<tk2dSpriteAnimator> m_animators;

	private Vector3[] m_startingPositions;

	private Vector3[] m_startingShadowPositions;

	public Vector2? BossCenter
	{
		get
		{
			Vector2 vector = base.transform.position.XY() + new Vector2((float)base.dungeonPlaceable.placeableWidth / 2f, (float)base.dungeonPlaceable.placeableHeight / 2f);
			return vector + new Vector2(0f, 2f);
		}
	}

	public override bool IsIntroFinished
	{
		get
		{
			return m_state == State.Finished;
		}
	}

	private void Start()
	{
		m_statuesController = GetComponent<BossStatuesController>();
		for (int i = 0; i < m_statuesController.allStatues.Count; i++)
		{
			BossStatueController bossStatueController = m_statuesController.allStatues[i];
			bossStatueController.specRigidbody.CollideWithOthers = false;
			bossStatueController.aiActor.IsGone = true;
		}
	}

	private void Update()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		m_animators = animators;
		for (int i = 0; i < dustAnimators.Count; i++)
		{
			m_animators.Add(dustAnimators[i]);
		}
		for (int j = 0; j < ghostAnimators.Count; j++)
		{
			m_animators.Add(ghostAnimators[j]);
			ghostAnimators[j].renderer.enabled = false;
		}
		m_allStatues = m_statuesController.allStatues;
		m_startingPositions = new Vector3[m_allStatues.Count];
		m_startingShadowPositions = new Vector3[m_allStatues.Count];
		for (int k = 0; k < m_allStatues.Count; k++)
		{
			m_animators.Add(m_allStatues[k].landVfx);
			m_startingPositions[k] = m_allStatues[k].transform.position;
			m_startingShadowPositions[k] = m_allStatues[k].shadowSprite.transform.position;
		}
		StartCoroutine(PlayIntro());
	}

	public override void EndIntro()
	{
		StopAllCoroutines();
		GameUIRoot.Instance.bossController.SetBossName(StringTableManager.GetEnemiesString(GetComponent<GenericIntroDoer>().portraitSlideSettings.bossNameString));
		for (int i = 0; i < m_statuesController.allStatues.Count; i++)
		{
			BossStatueController bossStatueController = m_statuesController.allStatues[i];
			bossStatueController.aiActor.SkipOnEngaged();
			GameUIRoot.Instance.bossController.RegisterBossHealthHaver(bossStatueController.healthHaver);
			bossStatueController.specRigidbody.CollideWithOthers = true;
			bossStatueController.aiActor.IsGone = false;
			bossStatueController.aiActor.State = AIActor.ActorState.Normal;
		}
		for (int j = 0; j < ghostAnimators.Count; j++)
		{
			if (ghostAnimators[j] != null)
			{
				ghostAnimators[j].renderer.enabled = false;
			}
		}
		for (int k = 0; k < dustAnimators.Count; k++)
		{
			dustAnimators[k].renderer.enabled = false;
		}
		if (m_allStatues != null)
		{
			for (int l = 0; l < m_allStatues.Count; l++)
			{
				m_allStatues[l].transform.position = m_startingPositions[l];
				m_allStatues[l].shadowSprite.transform.position = m_startingShadowPositions[l];
			}
		}
		eyeVfx.DestroyAll();
	}

	public override void OnCameraIntro()
	{
		for (int i = 0; i < ghostAnimators.Count; i++)
		{
			ghostAnimators[i].renderer.enabled = false;
		}
	}

	public override void OnBossCard()
	{
		eyeVfx.DestroyAll();
	}

	public override void OnCleanup()
	{
		base.behaviorSpeculator.enabled = true;
	}

	private IEnumerator PlayIntro()
	{
		m_state = State.Playing;
		BraveUtility.RandomizeList(ghostAnimators);
		yield return StartCoroutine(WaitForSecondsInvariant(ghostDelay));
		AkSoundEngine.PostEvent("Play_ENM_statue_intro_01", base.gameObject);
		for (int i = 0; i < ghostAnimators.Count; i++)
		{
			ghostAnimators[i].renderer.enabled = true;
			ghostAnimators[i].Play();
			if (i < ghostMidDelay.Length)
			{
				yield return StartCoroutine(WaitForSecondsInvariant(ghostMidDelay[i]));
			}
		}
		bool done = false;
		while (!done)
		{
			done = true;
			for (int j = 0; j < ghostAnimators.Count; j++)
			{
				if (ghostAnimators[j] == null)
				{
					continue;
				}
				if (ghostAnimators[j].IsPlaying(ghostAnimators[j].DefaultClip))
				{
					done = false;
					continue;
				}
				BossStatueController component = ghostAnimators[j].transform.parent.GetComponent<BossStatueController>();
				eyeVfx.SpawnAtLocalPosition(Vector3.zero, 0f, component.transformPoints[0], null, null, true);
				eyeVfx.SpawnAtLocalPosition(Vector3.zero, 0f, component.transformPoints[1], null, null, true);
				eyeVfx.ForEach(delegate(GameObject go)
				{
					tk2dSpriteAnimator[] componentsInChildren = go.GetComponentsInChildren<tk2dSpriteAnimator>();
					foreach (tk2dSpriteAnimator item in componentsInChildren)
					{
						if (!m_animators.Contains(item))
						{
							m_animators.Add(item);
						}
					}
				});
				ghostAnimators[j].renderer.enabled = false;
				ghostAnimators[j] = null;
			}
			yield return null;
		}
		yield return StartCoroutine(WaitForSecondsInvariant(dustDelay));
		for (int k = 0; k < dustAnimators.Count; k++)
		{
			dustAnimators[k].transform.parent = SpawnManager.Instance.VFX;
			dustAnimators[k].Play();
			dustAnimators[k].GetComponent<SpriteAnimatorKiller>().enabled = true;
		}
		yield return StartCoroutine(WaitForSecondsInvariant(floatDelay));
		GameManager.Instance.MainCameraController.DoScreenShake(floatScreenShake, null);
		float elapsed = 0f;
		for (float duration = floatTime; elapsed < duration; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			float height = elapsed / duration * m_statuesController.attackHopHeight;
			for (int l = 0; l < m_allStatues.Count; l++)
			{
				m_allStatues[l].transform.position = m_startingPositions[l] + new Vector3(0f, height, 0f);
				m_allStatues[l].shadowSprite.transform.position = m_startingShadowPositions[l];
				int frame = Mathf.RoundToInt((float)(m_allStatues[l].shadowSprite.spriteAnimator.DefaultClip.frames.Length - 1) * Mathf.Clamp01(height / m_statuesController.attackHopHeight));
				m_allStatues[l].shadowSprite.spriteAnimator.SetFrame(frame);
			}
			yield return null;
		}
		yield return StartCoroutine(WaitForSecondsInvariant(hangTime));
		float gravity = (0f - 2f * (m_statuesController.attackHopHeight / (0.5f * m_statuesController.attackHopTime))) / (0.5f * m_statuesController.attackHopTime);
		elapsed = 0f;
		for (float duration = m_statuesController.attackHopTime / 2f; elapsed < duration; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			float height2 = m_statuesController.attackHopHeight + 0f * elapsed + 0.5f * gravity * elapsed * elapsed;
			for (int m = 0; m < m_allStatues.Count; m++)
			{
				m_allStatues[m].transform.position = m_startingPositions[m] + new Vector3(0f, height2, 0f);
				m_allStatues[m].shadowSprite.transform.position = m_startingShadowPositions[m];
				int frame2 = Mathf.RoundToInt((float)(m_allStatues[m].shadowSprite.spriteAnimator.DefaultClip.frames.Length - 1) * Mathf.Clamp01(height2 / m_statuesController.attackHopHeight));
				m_allStatues[m].shadowSprite.spriteAnimator.SetFrame(frame2);
			}
			yield return null;
		}
		for (int n = 0; n < m_allStatues.Count; n++)
		{
			m_allStatues[n].transform.position = m_startingPositions[n];
			m_allStatues[n].shadowSprite.transform.position = m_startingShadowPositions[n];
		}
		for (int num = 0; num < m_allStatues.Count; num++)
		{
			m_allStatues[num].landVfx.gameObject.SetActive(true);
			m_allStatues[num].landVfx.GetComponent<SpriteAnimatorKiller>().Restart();
		}
		GameManager.Instance.MainCameraController.DoScreenShake(slamScreenShake, null);
		yield return StartCoroutine(WaitForSecondsInvariant(slamTime));
		m_state = State.Finished;
	}

	private IEnumerator WaitForSecondsInvariant(float time)
	{
		for (float elapsed = 0f; elapsed < time; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
	}
}
