using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Handles NPC teleportation.")]
	[ActionCategory(".NPCs")]
	public class Teleport : FsmStateAction
	{
		public enum Mode
		{
			Out,
			In,
			Both
		}

		public new enum State
		{
			TeleportOut,
			MidStep,
			TeleportIn
		}

		public enum Timing
		{
			Simultaneous,
			VfxThenAnimation,
			AnimationThenVfx,
			Delays
		}

		[Tooltip("Teleportation type; In and Out handle visibility and effects, Both also handles translation.")]
		public Mode mode;

		[Tooltip("How long the NPC is completely gone (i.e. the delay between the In finishing and the Out starting).")]
		public FsmFloat goneTime;

		[Tooltip("How far the NPC should move during the teleport, in Unity units (i.e. tiles).")]
		public FsmVector2 positionDelta;

		[Tooltip("When true, will ignore positionDelta and teleport to the end of the attached path.")]
		public bool useEndOfPath;

		[Tooltip("If true, lerps any Brent lights on this object while the the teleport animation is playing.")]
		public FsmBool lerpLight = false;

		[Tooltip("The new light intensity to set to.")]
		public FsmFloat newLightIntensity;

		private TalkDoerLite m_talkDoer;

		private State m_state;

		private Mode m_submode;

		private float m_stateTimer;

		private IEnumerator m_coroutine;

		private SetBraveLightIntensity m_lightIntensityAction;

		private float m_cachedLightIntensity;

		public override void Reset()
		{
			mode = Mode.In;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (component.spriteAnimator == null && component.aiAnimator == null)
			{
				return text + "Requires a 2D Toolkit animator or an AI Animator.\n";
			}
			if (component.aiAnimator != null)
			{
				if ((mode == Mode.In || mode == Mode.Both) && !component.aiAnimator.HasDirectionalAnimation(component.teleportInSettings.anim))
				{
					text = text + "Unknown animation " + component.teleportInSettings.anim + ".\n";
				}
				if ((mode == Mode.Out || mode == Mode.Both) && !component.aiAnimator.HasDirectionalAnimation(component.teleportOutSettings.anim))
				{
					text = text + "Unknown animation " + component.teleportOutSettings.anim + ".\n";
				}
			}
			else if (component.spriteAnimator != null)
			{
				if ((mode == Mode.In || mode == Mode.Both) && component.spriteAnimator.GetClipByName(component.teleportInSettings.anim) == null)
				{
					text = text + "Unknown animation " + component.teleportInSettings.anim + ".\n";
				}
				if ((mode == Mode.Out || mode == Mode.Both) && component.spriteAnimator.GetClipByName(component.teleportOutSettings.anim) == null)
				{
					text = text + "Unknown animation " + component.teleportOutSettings.anim + ".\n";
				}
			}
			return text;
		}

		public override void OnEnter()
		{
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
			m_coroutine = null;
			m_lightIntensityAction = null;
			if (mode == Mode.In)
			{
				m_state = State.TeleportIn;
				m_coroutine = HandleAnimAndVfx(m_talkDoer.teleportInSettings);
			}
			else if (mode == Mode.Out || mode == Mode.Both)
			{
				m_state = State.TeleportOut;
				m_coroutine = HandleAnimAndVfx(m_talkDoer.teleportOutSettings);
			}
		}

		public override void OnUpdate()
		{
			if (m_state == State.TeleportOut)
			{
				if (!m_coroutine.MoveNext())
				{
					if (mode == Mode.Both)
					{
						m_state = State.MidStep;
						m_stateTimer = goneTime.Value;
					}
					else
					{
						Finish();
					}
				}
			}
			else if (m_state == State.MidStep)
			{
				m_stateTimer -= BraveTime.DeltaTime;
				if (m_stateTimer <= 0f)
				{
					PathMover component = m_talkDoer.GetComponent<PathMover>();
					m_talkDoer.transform.position = ((!useEndOfPath) ? (m_talkDoer.transform.position + (Vector3)positionDelta.Value) : component.GetPositionOfNode(component.Path.nodes.Count - 1).ToVector3ZUp());
					m_talkDoer.specRigidbody.Reinitialize();
					PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_talkDoer.specRigidbody, CollisionMask.LayerToMask(CollisionLayer.PlayerCollider));
					m_state = State.TeleportIn;
					m_coroutine = HandleAnimAndVfx(m_talkDoer.teleportInSettings);
				}
			}
			else if (m_state == State.TeleportIn && !m_coroutine.MoveNext())
			{
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_talkDoer.specRigidbody, CollisionMask.LayerToMask(CollisionLayer.PlayerCollider));
				Finish();
			}
		}

		private IEnumerator HandleAnimAndVfx(TalkDoerLite.TeleportSettings teleportSettings)
		{
			if (teleportSettings.timing == Timing.Simultaneous)
			{
				GameObject vfx4 = SpawnVfx(teleportSettings.vfx, teleportSettings.vfxAnchor);
				PlayAnim(teleportSettings.anim);
				yield return null;
				while ((bool)vfx4 || IsPlaying(teleportSettings.anim))
				{
					yield return null;
				}
				FinishAnim();
			}
			else if (teleportSettings.timing == Timing.VfxThenAnimation)
			{
				GameObject vfx3 = SpawnVfx(teleportSettings.vfx, teleportSettings.vfxAnchor);
				while ((bool)vfx3)
				{
					yield return null;
				}
				PlayAnim(teleportSettings.anim);
				yield return null;
				while (IsPlaying(teleportSettings.anim))
				{
					yield return null;
				}
				FinishAnim();
			}
			else if (teleportSettings.timing == Timing.AnimationThenVfx)
			{
				PlayAnim(teleportSettings.anim);
				yield return null;
				while (IsPlaying(teleportSettings.anim))
				{
					yield return null;
				}
				FinishAnim();
				GameObject vfx2 = SpawnVfx(teleportSettings.vfx, teleportSettings.vfxAnchor);
				while ((bool)vfx2)
				{
					yield return null;
				}
			}
			else
			{
				if (teleportSettings.timing != Timing.Delays)
				{
					yield break;
				}
				float animTimer = teleportSettings.animDelay;
				float vfxTimer = teleportSettings.vfxDelay;
				bool playedAnim = false;
				bool waitForAnimComplete = false;
				bool playedVfx = !teleportSettings.vfx;
				GameObject vfx = null;
				yield return null;
				while (!playedAnim || !playedVfx || (bool)vfx || IsPlaying(teleportSettings.anim))
				{
					if (m_lightIntensityAction != null && !m_lightIntensityAction.Finished)
					{
						m_lightIntensityAction.OnUpdate();
					}
					if (waitForAnimComplete && !IsPlaying(teleportSettings.anim))
					{
						FinishAnim();
						waitForAnimComplete = false;
					}
					if (!playedVfx && vfxTimer >= 0f)
					{
						vfxTimer -= BraveTime.DeltaTime;
						if (vfxTimer <= 0f)
						{
							playedVfx = true;
							vfx = SpawnVfx(teleportSettings.vfx, teleportSettings.vfxAnchor);
						}
					}
					if (!playedAnim && animTimer >= 0f)
					{
						animTimer -= BraveTime.DeltaTime;
						if (animTimer <= 0f)
						{
							playedAnim = true;
							PlayAnim(teleportSettings.anim);
							waitForAnimComplete = true;
						}
					}
					yield return null;
				}
				if (m_lightIntensityAction != null && !m_lightIntensityAction.Finished)
				{
					m_lightIntensityAction.OnUpdate();
					m_lightIntensityAction.OnExit();
					m_lightIntensityAction = null;
				}
				if (waitForAnimComplete && !IsPlaying(teleportSettings.anim))
				{
					FinishAnim();
				}
			}
		}

		private void PlayAnim(string anim)
		{
			if (m_state == State.TeleportIn)
			{
				SetNpcVisibility.SetVisible(m_talkDoer, true);
				m_talkDoer.ShowOutlines = true;
			}
			if ((bool)m_talkDoer.aiAnimator)
			{
				m_talkDoer.aiAnimator.PlayUntilCancelled(anim);
			}
			else if ((bool)m_talkDoer.spriteAnimator)
			{
				m_talkDoer.spriteAnimator.Play(anim);
			}
			if (lerpLight.Value)
			{
				float num = ((mode != Mode.Both) ? newLightIntensity.Value : ((m_state != 0) ? m_cachedLightIntensity : 0f));
				m_lightIntensityAction = new SetBraveLightIntensity();
				m_lightIntensityAction.specifyLights = new ShadowSystem[0];
				m_lightIntensityAction.intensity = num;
				m_lightIntensityAction.transitionTime = m_talkDoer.spriteAnimator.CurrentClip.BaseClipLength;
				m_lightIntensityAction.Owner = base.Owner;
				m_lightIntensityAction.IsKeptAction = true;
				m_lightIntensityAction.OnEnter();
				m_cachedLightIntensity = ((m_lightIntensityAction.specifyLights.Length <= 0) ? 0f : m_lightIntensityAction.specifyLights[0].uLightIntensity);
				m_lightIntensityAction.OnUpdate();
			}
		}

		private bool IsPlaying(string anim)
		{
			if ((bool)m_talkDoer.aiAnimator)
			{
				return m_talkDoer.aiAnimator.IsPlaying(anim);
			}
			if ((bool)m_talkDoer.spriteAnimator)
			{
				return m_talkDoer.spriteAnimator.IsPlaying(anim);
			}
			return false;
		}

		private void FinishAnim()
		{
			if (m_state == State.TeleportOut)
			{
				SetNpcVisibility.SetVisible(m_talkDoer, false);
				m_talkDoer.ShowOutlines = false;
			}
		}

		private GameObject SpawnVfx(GameObject vfxPrefab, GameObject anchor)
		{
			if (!vfxPrefab)
			{
				return null;
			}
			GameObject gameObject = Object.Instantiate(vfxPrefab, m_talkDoer.specRigidbody.GetUnitCenter(ColliderType.HitBox), Quaternion.identity);
			if ((bool)anchor)
			{
				gameObject.transform.parent = anchor.transform;
				gameObject.transform.localPosition = Vector3.zero;
			}
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			if ((bool)component && component.IsPerpendicular == m_talkDoer.sprite.IsPerpendicular)
			{
				m_talkDoer.sprite.AttachRenderer(component);
				component.HeightOffGround = 0.05f;
				component.UpdateZDepth();
			}
			return gameObject;
		}
	}
}
