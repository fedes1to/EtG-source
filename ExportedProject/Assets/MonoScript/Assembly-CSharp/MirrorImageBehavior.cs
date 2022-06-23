using System.Collections.Generic;
using FullInspector;
using UnityEngine;

public class MirrorImageBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		Summoning,
		Splitting
	}

	public int NumImages = 2;

	public int MaxImages = 5;

	public float MirrorHealth = 15f;

	public float SpawnDelay = 0.5f;

	public float SplitDelay = 1f;

	public float SplitDistance = 1f;

	[InspectorCategory("Visuals")]
	public string Anim;

	[InspectorCategory("Visuals")]
	public bool AnimRequiresTransparency;

	[InspectorCategory("Visuals")]
	public string MirrorDeathAnim;

	[InspectorCategory("Visuals")]
	public string[] MirroredAnims;

	private State m_state;

	private Shader m_cachedShader;

	private AIActor m_enemyPrefab;

	private float m_timer;

	private float m_startAngle;

	private List<AIActor> m_actorsToSplit = new List<AIActor>();

	private List<AIActor> m_allImages = new List<AIActor>();

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
		for (int num = m_allImages.Count - 1; num >= 0; num--)
		{
			if (!m_allImages[num] || !m_allImages[num].healthHaver || m_allImages[num].healthHaver.IsDead)
			{
				m_allImages.RemoveAt(num);
			}
		}
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_enemyPrefab = EnemyDatabase.GetOrLoadByGuid(m_aiActor.EnemyGuid);
		m_aiAnimator.PlayUntilFinished(Anim, true);
		if (AnimRequiresTransparency)
		{
			m_cachedShader = m_aiActor.renderer.material.shader;
			m_aiActor.sprite.usesOverrideMaterial = true;
			m_aiActor.SetOutlines(false);
			m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
		}
		m_aiActor.ClearPath();
		m_timer = SpawnDelay;
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(true, "MirrorImageBehavior");
		}
		m_aiActor.IsGone = true;
		m_aiActor.specRigidbody.CollideWithOthers = false;
		m_actorsToSplit.Clear();
		m_actorsToSplit.Add(m_aiActor);
		m_state = State.Summoning;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == State.Summoning)
		{
			if (m_timer <= 0f)
			{
				int num = Mathf.Min(NumImages, MaxImages - m_allImages.Count);
				for (int i = 0; i < num; i++)
				{
					AIActor aIActor = AIActor.Spawn(m_enemyPrefab, m_aiActor.specRigidbody.UnitBottomLeft, m_aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Spawn);
					aIActor.transform.position = m_aiActor.transform.position;
					aIActor.specRigidbody.Reinitialize();
					aIActor.IsGone = true;
					aIActor.specRigidbody.CollideWithOthers = false;
					if (!string.IsNullOrEmpty(MirrorDeathAnim))
					{
						aIActor.aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "death").anim.Prefix = MirrorDeathAnim;
					}
					aIActor.PreventBlackPhantom = true;
					if (aIActor.IsBlackPhantom)
					{
						aIActor.UnbecomeBlackPhantom();
					}
					m_actorsToSplit.Add(aIActor);
					m_allImages.Add(aIActor);
					aIActor.aiAnimator.healthHaver.SetHealthMaximum(MirrorHealth * AIActor.BaseLevelHealthModifier);
					MirrorImageController mirrorImageController = aIActor.gameObject.AddComponent<MirrorImageController>();
					mirrorImageController.SetHost(m_aiActor);
					for (int j = 0; j < MirroredAnims.Length; j++)
					{
						mirrorImageController.MirrorAnimations.Add(MirroredAnims[j]);
					}
					if (AnimRequiresTransparency)
					{
						aIActor.sprite.usesOverrideMaterial = true;
						aIActor.procedurallyOutlined = false;
						aIActor.SetOutlines(false);
						aIActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
					}
				}
				m_startAngle = Random.Range(0f, 360f);
				m_state = State.Splitting;
				m_timer = SplitDelay;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Splitting)
		{
			float num2 = 360f / (float)m_actorsToSplit.Count;
			for (int k = 0; k < m_actorsToSplit.Count; k++)
			{
				m_actorsToSplit[k].BehaviorOverridesVelocity = true;
				m_actorsToSplit[k].BehaviorVelocity = BraveMathCollege.DegreesToVector(m_startAngle + num2 * (float)k, SplitDistance / SplitDelay);
			}
			if (m_timer <= 0f)
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (AnimRequiresTransparency && (bool)m_cachedShader)
		{
			for (int i = 0; i < m_actorsToSplit.Count; i++)
			{
				AIActor aIActor = m_actorsToSplit[i];
				if ((bool)aIActor)
				{
					aIActor.sprite.usesOverrideMaterial = false;
					aIActor.procedurallyOutlined = true;
					aIActor.SetOutlines(true);
					aIActor.renderer.material.shader = m_cachedShader;
				}
			}
			m_cachedShader = null;
		}
		if (!string.IsNullOrEmpty(Anim))
		{
			m_aiAnimator.EndAnimationIf(Anim);
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "MirrorImageBehavior");
		}
		for (int j = 0; j < m_actorsToSplit.Count; j++)
		{
			AIActor aIActor2 = m_actorsToSplit[j];
			if (!aIActor2)
			{
				continue;
			}
			aIActor2.BehaviorOverridesVelocity = false;
			aIActor2.IsGone = false;
			aIActor2.specRigidbody.CollideWithOthers = true;
			if (aIActor2 != m_aiActor)
			{
				aIActor2.PreventBlackPhantom = false;
				if (m_aiActor.IsBlackPhantom)
				{
					aIActor2.BecomeBlackPhantom();
				}
			}
		}
		m_actorsToSplit.Clear();
		m_state = State.Idle;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsReady()
	{
		if (MaxImages > 0 && m_allImages.Count >= MaxImages)
		{
			return false;
		}
		return base.IsReady();
	}
}
