using System.Collections.Generic;
using UnityEngine;

public class BraveBehaviour : MonoBehaviour
{
	private BraveCache m_cachedCache;

	private static List<BraveBehaviour> s_braveBehaviours = new List<BraveBehaviour>();

	public new Transform transform
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasTransform)
			{
				cache.transform = GetComponent<Transform>();
				cache.hasTransform = true;
			}
			return cache.transform;
		}
	}

	public Renderer renderer
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasRenderer)
			{
				cache.renderer = GetComponent<Renderer>();
				cache.hasRenderer = true;
			}
			return cache.renderer;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.renderer = value;
			cache.hasRenderer = true;
		}
	}

	public Animation unityAnimation
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasUnityAnimation)
			{
				cache.unityAnimation = GetComponent<Animation>();
				cache.hasUnityAnimation = true;
			}
			return cache.unityAnimation;
		}
	}

	public ParticleSystem particleSystem
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasParticleSystem)
			{
				cache.particleSystem = GetComponent<ParticleSystem>();
				cache.hasParticleSystem = true;
			}
			return cache.particleSystem;
		}
	}

	public DungeonPlaceableBehaviour dungeonPlaceable
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasDungeonPlaceable)
			{
				cache.dungeonPlaceable = GetComponent<DungeonPlaceableBehaviour>();
				cache.hasDungeonPlaceable = true;
			}
			return cache.dungeonPlaceable;
		}
	}

	public AIActor aiActor
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasAiActor)
			{
				cache.aiActor = GetComponent<AIActor>();
				cache.hasAiActor = true;
			}
			return cache.aiActor;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.aiActor = value;
			cache.hasAiActor = true;
		}
	}

	public AIShooter aiShooter
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasAiShooter)
			{
				cache.aiShooter = GetComponent<AIShooter>();
				cache.hasAiShooter = true;
			}
			return cache.aiShooter;
		}
	}

	public AIBulletBank bulletBank
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasBulletBank)
			{
				cache.bulletBank = GetComponent<AIBulletBank>();
				cache.hasBulletBank = true;
			}
			return cache.bulletBank;
		}
	}

	public HealthHaver healthHaver
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasHealthHaver)
			{
				cache.healthHaver = GetComponent<HealthHaver>();
				cache.hasHealthHaver = true;
			}
			return cache.healthHaver;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.healthHaver = value;
			cache.hasHealthHaver = true;
		}
	}

	public KnockbackDoer knockbackDoer
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasKnockbackDoer)
			{
				cache.knockbackDoer = GetComponent<KnockbackDoer>();
				cache.hasKnockbackDoer = true;
			}
			return cache.knockbackDoer;
		}
	}

	public HitEffectHandler hitEffectHandler
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasHitEffectHandler)
			{
				cache.hitEffectHandler = GetComponent<HitEffectHandler>();
				cache.hasHitEffectHandler = true;
			}
			return cache.hitEffectHandler;
		}
	}

	public AIAnimator aiAnimator
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasAiAnimator)
			{
				cache.aiAnimator = GetComponent<AIAnimator>();
				cache.hasAiAnimator = true;
			}
			return cache.aiAnimator;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.aiAnimator = value;
			cache.hasAiAnimator = true;
		}
	}

	public BehaviorSpeculator behaviorSpeculator
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasBehaviorSpeculator)
			{
				cache.behaviorSpeculator = GetComponent<BehaviorSpeculator>();
				cache.hasBehaviorSpeculator = true;
			}
			return cache.behaviorSpeculator;
		}
	}

	public GameActor gameActor
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasGameActor)
			{
				cache.gameActor = GetComponent<GameActor>();
				cache.hasGameActor = true;
			}
			return cache.gameActor;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.gameActor = value;
			cache.hasGameActor = true;
		}
	}

	public MinorBreakable minorBreakable
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasMinorBreakable)
			{
				cache.minorBreakable = GetComponent<MinorBreakable>();
				cache.hasMinorBreakable = true;
			}
			return cache.minorBreakable;
		}
	}

	public MajorBreakable majorBreakable
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasMajorBreakable)
			{
				cache.majorBreakable = GetComponent<MajorBreakable>();
				cache.hasMajorBreakable = true;
			}
			return cache.majorBreakable;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.majorBreakable = value;
			cache.hasMajorBreakable = true;
		}
	}

	public Projectile projectile
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasProjectile)
			{
				cache.projectile = GetComponent<Projectile>();
				cache.hasProjectile = true;
			}
			return cache.projectile;
		}
	}

	public ObjectVisibilityManager visibilityManager
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasVisibilityManager)
			{
				cache.visibilityManager = GetComponent<ObjectVisibilityManager>();
				cache.hasVisibilityManager = true;
			}
			return cache.visibilityManager;
		}
	}

	public TalkDoerLite talkDoer
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasTalkDoer)
			{
				cache.talkDoer = GetComponent<TalkDoerLite>();
				cache.hasTalkDoer = true;
			}
			return cache.talkDoer;
		}
	}

	public UltraFortunesFavor ultraFortunesFavor
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasUltraFortunesFavor)
			{
				cache.ultraFortunesFavor = GetComponent<UltraFortunesFavor>();
				cache.hasUltraFortunesFavor = true;
			}
			return cache.ultraFortunesFavor;
		}
	}

	public DebrisObject debris
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasDebris)
			{
				cache.debris = GetComponent<DebrisObject>();
				cache.hasDebris = true;
			}
			return cache.debris;
		}
	}

	public EncounterTrackable encounterTrackable
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasEncounterTrackable)
			{
				cache.encounterTrackable = GetComponent<EncounterTrackable>();
				cache.hasEncounterTrackable = true;
			}
			return cache.encounterTrackable;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.encounterTrackable = value;
			cache.hasEncounterTrackable = true;
		}
	}

	public SpeculativeRigidbody specRigidbody
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasSpecRigidbody)
			{
				cache.specRigidbody = GetComponent<SpeculativeRigidbody>();
				cache.hasSpecRigidbody = true;
			}
			return cache.specRigidbody;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.specRigidbody = value;
			cache.hasSpecRigidbody = true;
		}
	}

	public tk2dBaseSprite sprite
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasSprite)
			{
				cache.sprite = GetComponent<tk2dBaseSprite>();
				cache.hasSprite = true;
			}
			return cache.sprite;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.sprite = value;
			cache.hasSprite = true;
		}
	}

	public tk2dSpriteAnimator spriteAnimator
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasSpriteAnimator)
			{
				cache.spriteAnimator = GetComponent<tk2dSpriteAnimator>();
				cache.hasSpriteAnimator = true;
			}
			return cache.spriteAnimator;
		}
		set
		{
			BraveCache cache = GetCache();
			cache.spriteAnimator = value;
			cache.hasSpriteAnimator = true;
		}
	}

	public PlayMakerFSM playmakerFsm
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasPlaymakerFsm)
			{
				cache.playmakerFsm = GetComponent<PlayMakerFSM>();
				cache.hasPlaymakerFsm = true;
			}
			return cache.playmakerFsm;
		}
	}

	public PlayMakerFSM[] playmakerFsms
	{
		get
		{
			BraveCache cache = GetCache();
			if (!cache.hasPlaymakerFsms)
			{
				cache.playmakerFsms = GetComponents<PlayMakerFSM>();
				cache.hasPlaymakerFsms = true;
			}
			return cache.playmakerFsms;
		}
	}

	public void RegenerateCache()
	{
		GetComponents(s_braveBehaviours);
		m_cachedCache = new BraveCache();
		m_cachedCache.name = base.gameObject.name;
		for (int i = 0; i < s_braveBehaviours.Count; i++)
		{
			s_braveBehaviours[i].m_cachedCache = m_cachedCache;
		}
		s_braveBehaviours.Clear();
	}

	protected virtual void OnDestroy()
	{
		m_cachedCache = null;
	}

	public void SendPlaymakerEvent(string eventName)
	{
		PlayMakerFSM[] array = playmakerFsms;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].enabled)
			{
				array[i].SendEvent(eventName);
			}
		}
	}

	public PlayMakerFSM GetDungeonFSM()
	{
		PlayMakerFSM[] array = playmakerFsms;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].FsmName.Contains("Dungeon"))
			{
				return array[i];
			}
		}
		return playmakerFsm;
	}

	public PlayMakerFSM GetFoyerFSM()
	{
		PlayMakerFSM[] array = playmakerFsms;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].FsmName.Contains("Foyer"))
			{
				return array[i];
			}
		}
		return playmakerFsm;
	}

	private BraveCache GetCache()
	{
		if (m_cachedCache == null)
		{
			if (s_braveBehaviours == null)
			{
				s_braveBehaviours = new List<BraveBehaviour>();
			}
			s_braveBehaviours.Clear();
			GetComponents(s_braveBehaviours);
			for (int i = 0; i < s_braveBehaviours.Count; i++)
			{
				if (s_braveBehaviours[i].m_cachedCache != null)
				{
					m_cachedCache = s_braveBehaviours[i].m_cachedCache;
				}
			}
			if (m_cachedCache == null)
			{
				m_cachedCache = new BraveCache();
				m_cachedCache.name = base.gameObject.name;
				for (int j = 0; j < s_braveBehaviours.Count; j++)
				{
					s_braveBehaviours[j].m_cachedCache = m_cachedCache;
				}
			}
			s_braveBehaviours.Clear();
		}
		return m_cachedCache;
	}
}
