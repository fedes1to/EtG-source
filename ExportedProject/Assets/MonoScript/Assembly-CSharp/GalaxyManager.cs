using System;
using Galaxy.Api;
using UnityEngine;

[DisallowMultipleComponent]
public class GalaxyManager : MonoBehaviour
{
	public class AuthListener : GlobalAuthListener
	{
		public override void OnAuthSuccess()
		{
			Debug.Log("Auth success!");
			Initialized = true;
		}

		public override void OnAuthFailure(FailureReason failureReason)
		{
			Debug.LogFormat("Auth failed! {0}", failureReason);
		}

		public override void OnAuthLost()
		{
			Debug.LogFormat("Auth lost!");
		}
	}

	private static GalaxyManager s_instance;

	private static bool s_EverInialized;

	private bool m_bInitialized;

	private GlobalAuthListener m_authListener;

	private static GalaxyManager Instance
	{
		get
		{
			return s_instance ?? new GameObject("GalaxyManager").AddComponent<GalaxyManager>();
		}
	}

	public static bool Initialized
	{
		get
		{
			return Instance.m_bInitialized;
		}
		private set
		{
			Instance.m_bInitialized = value;
			if (value)
			{
				s_EverInialized = true;
			}
		}
	}

	private void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInialized)
		{
			throw new Exception("Tried to Initialize the Galaxy API twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		try
		{
			GalaxyInstance.Init("48944359584830756", "3847f0113681121feddcd75acdcfcde13320be288b24f33b003821c9e776737d");
			m_authListener = new AuthListener();
			GalaxyInstance.User().SignIn();
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			try
			{
				Debug.LogError("GalaxyManager failed to start; attempting shut down.");
				GalaxyInstance.Shutdown();
			}
			catch (Exception)
			{
				Debug.LogError(message);
			}
		}
	}

	private void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized)
		{
		}
	}

	private void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (!m_bInitialized)
			{
			}
			GalaxyInstance.Shutdown();
		}
	}

	private void Update()
	{
		GalaxyInstance.ProcessData();
		if (m_bInitialized)
		{
		}
	}
}
