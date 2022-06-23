using System.Collections;
using Dungeonator;
using UnityEngine;

public class HelicopterDeathController : BraveBehaviour
{
	public ScreenShakeSettings screenShake;

	public GameObject explosionVfx;

	private float explosionMidDelay = 0.1f;

	private int explosionCount = 35;

	public GameObject bigExplosionVfx;

	private float bigExplosionMidDelay = 0.2f;

	private int bigExplosionCount = 10;

	private bool m_isDestroyed;

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
	}

	private void OnBossDeath(Vector2 dir)
	{
		StartCoroutine(HandleBossDeath());
		AkSoundEngine.PostEvent("Play_State_Volume_Lower_01", base.gameObject);
	}

	private IEnumerator HandleBossDeath()
	{
		base.aiAnimator.PlayUntilCancelled("death", true);
		base.healthHaver.OverrideKillCamTime = 6f;
		yield return StartCoroutine(GetComponent<VoiceOverer>().HandlePlayerWonVO(4f));
		GameManager.Instance.StartCoroutine(HandleLittleExplosionsCR());
		GameManager.Instance.StartCoroutine(HandleBigExplosionsCR());
		GameManager.Instance.StartCoroutine(SinkCR());
		AkSoundEngine.PostEvent("Play_boss_helicopter_death_01", base.gameObject);
	}

	private IEnumerator HandleLittleExplosionsCR()
	{
		for (int i = 0; i < explosionCount; i++)
		{
			if (m_isDestroyed)
			{
				break;
			}
			GameObject vfxObj = SpawnManager.SpawnVFX(explosionVfx, RandomExplosionPos(), Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			yield return new WaitForSeconds(explosionMidDelay);
		}
	}

	private IEnumerator HandleBigExplosionsCR()
	{
		CameraController camera = GameManager.Instance.MainCameraController;
		camera.DoContinuousScreenShake(screenShake, this);
		AkSoundEngine.PostEvent("Stop_State_Volume_Lower_01", base.gameObject);
		yield return new WaitForSeconds((float)explosionCount * explosionMidDelay - (float)bigExplosionCount * bigExplosionMidDelay);
		for (int i = 0; i < bigExplosionCount; i++)
		{
			if (m_isDestroyed)
			{
				break;
			}
			GameObject vfxObj = SpawnManager.SpawnVFX(bigExplosionVfx, RandomExplosionPos(), Quaternion.identity);
			tk2dBaseSprite vfxSprite = vfxObj.GetComponent<tk2dBaseSprite>();
			vfxSprite.HeightOffGround = 0.8f + Random.value * 0.5f;
			base.sprite.AttachRenderer(vfxSprite);
			base.sprite.UpdateZDepth();
			yield return new WaitForSeconds(bigExplosionMidDelay);
		}
		camera.StopContinuousScreenShake(this);
		GetComponent<HelicopterIntroDoer>().IsCameraModified = false;
		camera.OverrideZoomScale = 1f;
		ExplosionDebrisLauncher[] debris = GetComponentsInChildren<ExplosionDebrisLauncher>();
		debris[0].Launch(BraveMathCollege.DegreesToVector(150f));
		debris[1].Launch(BraveMathCollege.DegreesToVector(30f));
		debris[2].Launch(BraveMathCollege.DegreesToVector(90f + BraveUtility.RandomSign() * 45f));
		debris[3].Launch(BraveMathCollege.DegreesToVector(90f + BraveUtility.RandomSign() * 45f));
		debris[4].Launch(BraveMathCollege.DegreesToVector(75f));
		debris[5].Launch(BraveMathCollege.DegreesToVector(105f));
		debris[6].Launch(BraveMathCollege.DegreesToVector(90f + BraveUtility.RandomSign() * 45f));
		debris[7].Launch(BraveMathCollege.DegreesToVector(135f));
		debris[8].Launch(BraveMathCollege.DegreesToVector(45f));
		base.aiActor.StealthDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		GameManager.Instance.Dungeon.StartCoroutine(HandleFlightPitfall());
		m_isDestroyed = true;
		Object.Destroy(base.gameObject);
	}

	private IEnumerator HandleFlightPitfall()
	{
		yield return null;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController)
			{
				playerController.LevelToLoadOnPitfall = "tt_forge";
			}
		}
		yield return new WaitForSeconds(1f);
		while (!Dungeon.IsGenerating && !GameManager.Instance.IsLoadingLevel)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController2 = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController2 && playerController2.IsFlying && !playerController2.IsGhost && playerController2.CurrentRoom != null && playerController2.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
				{
					CellData cell = playerController2.CenterPosition.GetCell();
					if (cell != null && cell.type == CellType.PIT)
					{
						playerController2.ForceFall();
					}
				}
			}
			yield return null;
		}
	}

	private IEnumerator SinkCR()
	{
		GameObject shadowObj = base.aiActor.ShadowObject;
		Vector2 velocity = new Vector2(0f, -0.4f);
		while (!m_isDestroyed)
		{
			base.specRigidbody.Velocity = velocity;
			shadowObj.transform.localPosition -= (Vector3)velocity * BraveTime.DeltaTime;
			yield return null;
		}
	}

	private Vector2 RandomExplosionPos()
	{
		Vector2 vector = base.transform.position;
		switch (Random.Range(0, 8))
		{
		case 0:
			return vector + BraveUtility.RandomVector2(new Vector2(0.75f, 4.625f), new Vector2(3.875f, 5.25f));
		case 1:
			return vector + BraveUtility.RandomVector2(new Vector2(5.625f, 4.625f), new Vector2(8.75f, 5.25f));
		default:
			return vector + BraveUtility.RandomVector2(new Vector2(3.875f, 2f), new Vector2(5.625f, 8.375f));
		}
	}
}
