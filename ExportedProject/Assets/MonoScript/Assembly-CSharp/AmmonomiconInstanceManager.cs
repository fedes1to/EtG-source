using System.Collections;
using UnityEngine;

public class AmmonomiconInstanceManager : MonoBehaviour
{
	public AmmonomiconBookmarkController[] bookmarks;

	private int m_currentlySelectedBookmark;

	private dfGUIManager m_manager;

	public int CurrentlySelectedTabIndex
	{
		get
		{
			return m_currentlySelectedBookmark;
		}
		set
		{
			m_currentlySelectedBookmark = value;
		}
	}

	public dfGUIManager GuiManager
	{
		get
		{
			if (m_manager == null)
			{
				m_manager = GetComponent<dfGUIManager>();
			}
			return m_manager;
		}
	}

	public bool BookmarkHasFocus
	{
		get
		{
			for (int i = 0; i < bookmarks.Length; i++)
			{
				if (bookmarks[i].IsFocused)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void Open()
	{
		m_currentlySelectedBookmark = 0;
		StartCoroutine(HandleOpenAmmonomicon());
	}

	public void Close()
	{
		for (int i = 0; i < bookmarks.Length; i++)
		{
			bookmarks[i].Disable();
		}
	}

	public void LateUpdate()
	{
		if (dfGUIManager.ActiveControl == null && bookmarks != null && bookmarks[m_currentlySelectedBookmark] != null)
		{
			bookmarks[m_currentlySelectedBookmark].ForceFocus();
		}
	}

	public void OpenDeath()
	{
		m_currentlySelectedBookmark = bookmarks.Length - 1;
		StartCoroutine(HandleOpenAmmonomiconDeath());
	}

	public IEnumerator InvariantWait(float t)
	{
		float elapsed = 0f;
		while (elapsed < t)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
	}

	public IEnumerator HandleOpenAmmonomiconDeath()
	{
		int currentBookmark = 0;
		while (currentBookmark < bookmarks.Length)
		{
			bookmarks[currentBookmark].TriggerAppearAnimation();
			if (currentBookmark != bookmarks.Length - 1)
			{
				bookmarks[currentBookmark].Disable();
			}
			currentBookmark++;
			yield return StartCoroutine(InvariantWait(0.1f));
		}
		m_currentlySelectedBookmark = bookmarks.Length - 1;
		bookmarks[m_currentlySelectedBookmark].IsCurrentPage = true;
	}

	public IEnumerator HandleOpenAmmonomicon()
	{
		dfGUIManager.SetFocus(null);
		int currentBookmark = 0;
		bookmarks[m_currentlySelectedBookmark].IsCurrentPage = true;
		while (currentBookmark < bookmarks.Length - 1)
		{
			if (!AmmonomiconController.Instance.IsOpen)
			{
				yield break;
			}
			bookmarks[currentBookmark].TriggerAppearAnimation();
			currentBookmark++;
			yield return StartCoroutine(InvariantWait(0.05f));
		}
		bookmarks[m_currentlySelectedBookmark].IsCurrentPage = true;
	}
}
