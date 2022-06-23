using System.Collections;
using UnityEngine;

public class IntroMovieClipPlayer : MonoBehaviour
{
	public MovieTexture movieTexture;

	public AudioClip movieAudio;

	public dfTextureSprite guiTexture;

	private AudioSource m_source;

	private void Start()
	{
		GameManager.AttemptSoundEngineInitialization();
		m_source = Camera.main.gameObject.AddComponent<AudioSource>();
		m_source.clip = movieAudio;
		m_source.pitch = 1.02f;
		movieTexture.loop = false;
		guiTexture.Texture = movieTexture;
	}

	public void TriggerMovie()
	{
		StartCoroutine(Do());
	}

	private IEnumerator Do()
	{
		AkSoundEngine.PostEvent("Play_UI_titleintro", base.gameObject);
		yield return new WaitForSeconds(0.1f);
		movieTexture.Play();
		yield return null;
	}
}
