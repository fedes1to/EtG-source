using System;
using UnityEngine;

public class AmmonomiconBookmarkController : MonoBehaviour
{
	public dfAnimationClip AppearClip;

	public string SelectSpriteName;

	public dfAnimationClip SelectClip;

	public string DeselectSelectedSpriteName;

	public string TargetNewPageLeft;

	public AmmonomiconPageRenderer.PageType LeftPageType;

	public string TargetNewPageRight;

	public AmmonomiconPageRenderer.PageType RightPageType;

	private bool m_isCurrentPage;

	private dfButton m_sprite;

	private dfSpriteAnimation m_animator;

	private AmmonomiconInstanceManager m_ammonomiconInstance;

	public bool IsFocused
	{
		get
		{
			return m_sprite.HasFocus;
		}
	}

	public bool IsCurrentPage
	{
		get
		{
			return m_isCurrentPage;
		}
		set
		{
			if (m_isCurrentPage == value)
			{
				return;
			}
			int currentlySelectedTabIndex = m_ammonomiconInstance.CurrentlySelectedTabIndex;
			m_isCurrentPage = value;
			if (m_isCurrentPage)
			{
				m_sprite.BackgroundSprite = DeselectSelectedSpriteName;
				TriggerSelectedAnimation();
				for (int i = 0; i < m_ammonomiconInstance.bookmarks.Length; i++)
				{
					if (m_ammonomiconInstance.bookmarks[i] != this && m_ammonomiconInstance.bookmarks[i].IsCurrentPage)
					{
						m_ammonomiconInstance.bookmarks[i].IsCurrentPage = false;
					}
				}
				m_ammonomiconInstance.CurrentlySelectedTabIndex = Array.IndexOf(m_ammonomiconInstance.bookmarks, this);
				if (m_ammonomiconInstance.CurrentlySelectedTabIndex > currentlySelectedTabIndex)
				{
					AmmonomiconController.Instance.TurnToNextPage(TargetNewPageLeft, LeftPageType, TargetNewPageRight, RightPageType);
				}
				else if (m_ammonomiconInstance.CurrentlySelectedTabIndex < currentlySelectedTabIndex)
				{
					AmmonomiconController.Instance.TurnToPreviousPage(TargetNewPageLeft, LeftPageType, TargetNewPageRight, RightPageType);
				}
				m_sprite.Focus();
			}
			else
			{
				m_animator.Stop();
				m_sprite.BackgroundSprite = AppearClip.Sprites[AppearClip.Sprites.Count - 1];
			}
		}
	}

	private void Start()
	{
		m_sprite = GetComponent<dfButton>();
		m_ammonomiconInstance = m_sprite.GetManager().GetComponent<AmmonomiconInstanceManager>();
		m_sprite.IsVisible = false;
		m_animator = base.gameObject.AddComponent<dfSpriteAnimation>();
		m_animator.LoopType = dfTweenLoopType.Once;
		m_animator.Target = new dfComponentMemberInfo();
		dfComponentMemberInfo target = m_animator.Target;
		target.Component = m_sprite;
		target.MemberName = "BackgroundSprite";
		m_animator.Clip = AppearClip;
		m_animator.Length = 0.35f;
		m_sprite.MouseEnter += OnMouseEnter;
		m_sprite.MouseLeave += OnMouseLeave;
		m_sprite.GotFocus += Focus;
		m_sprite.LostFocus += Defocus;
		m_sprite.Click += SelectThisTab;
		UIKeyControls component = GetComponent<UIKeyControls>();
		if (!(component != null))
		{
			return;
		}
		component.OnRightDown = (Action)Delegate.Combine(component.OnRightDown, (Action)delegate
		{
			if (AmmonomiconController.Instance.ImpendingLeftPageRenderer != null && AmmonomiconController.Instance.ImpendingLeftPageRenderer.LastFocusTarget != null)
			{
				InControlInputAdapter.SkipInputForRestOfFrame = true;
				AmmonomiconController.Instance.ImpendingLeftPageRenderer.LastFocusTarget.Focus();
			}
			else if (AmmonomiconController.Instance.CurrentLeftPageRenderer.LastFocusTarget != null)
			{
				InControlInputAdapter.SkipInputForRestOfFrame = true;
				AmmonomiconController.Instance.CurrentLeftPageRenderer.LastFocusTarget.Focus();
			}
			else if (AmmonomiconController.Instance.CurrentRightPageRenderer.LastFocusTarget != null)
			{
				InControlInputAdapter.SkipInputForRestOfFrame = true;
				AmmonomiconController.Instance.CurrentRightPageRenderer.LastFocusTarget.Focus();
			}
		});
	}

	private void OnMouseLeave(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!m_animator.IsPlaying && !m_sprite.HasFocus)
		{
			Defocus(control, null);
		}
	}

	private void OnMouseEnter(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!m_animator.IsPlaying)
		{
			if (IsCurrentPage)
			{
				m_sprite.BackgroundSprite = SelectClip.Sprites[SelectClip.Sprites.Count - 1];
			}
			else
			{
				m_sprite.BackgroundSprite = SelectSpriteName;
			}
			m_sprite.Focus();
		}
	}

	public void Enable()
	{
		m_sprite.Enable();
		m_sprite.IsVisible = true;
		m_sprite.IsInteractive = true;
		m_sprite.Click += SelectThisTab;
	}

	public void Disable()
	{
		m_animator.Stop();
		m_sprite.Disable();
		m_sprite.IsVisible = false;
		m_sprite.IsInteractive = false;
		m_sprite.Click -= SelectThisTab;
	}

	public void ForceFocus()
	{
		if (m_sprite != null)
		{
			m_sprite.Focus();
		}
	}

	private void SelectThisTab(dfControl control, dfMouseEventArgs mouseEvent)
	{
		IsCurrentPage = true;
	}

	private void Defocus(dfControl control, dfFocusEventArgs args)
	{
		m_animator.Stop();
		if (IsCurrentPage)
		{
			m_sprite.BackgroundSprite = DeselectSelectedSpriteName;
		}
		else
		{
			m_sprite.BackgroundSprite = AppearClip.Sprites[AppearClip.Sprites.Count - 1];
		}
	}

	public void Focus(dfControl control, dfFocusEventArgs args)
	{
		if (IsCurrentPage)
		{
			m_sprite.BackgroundSprite = SelectClip.Sprites[SelectClip.Sprites.Count - 1];
		}
		else
		{
			m_sprite.BackgroundSprite = SelectSpriteName;
		}
	}

	public void TriggerAppearAnimation()
	{
		Enable();
		if (!IsCurrentPage)
		{
			m_animator.Clip = AppearClip;
			m_animator.Length = 0.35f;
			m_animator.Play();
		}
		else
		{
			TriggerSelectedAnimation();
		}
	}

	public void TriggerSelectedAnimation()
	{
		m_sprite.IsVisible = true;
		m_animator.Clip = SelectClip;
		m_animator.Length = 0.275f;
		m_animator.Play();
	}
}
