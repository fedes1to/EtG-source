using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatlingGullOutroDoer : BraveBehaviour
{
	public GameObject CrowVFX;

	protected List<GatlingGullCrowController> m_extantCrows;

	private void Start()
	{
		BossKillCam.hackGatlingGullOutroDoer = this;
	}

	public void TriggerSequence()
	{
		AkSoundEngine.PostEvent("Play_ANM_Gull_Outro_01", base.gameObject);
		base.sprite.usesOverrideMaterial = true;
		base.sprite.IsPerpendicular = false;
		m_extantCrows = new List<GatlingGullCrowController>();
		int num = 100;
		for (int i = 0; i < num; i++)
		{
			float num2 = Random.Range(30, 40);
			if (Random.value < 0.04f)
			{
				num2 = Random.Range(20, 30);
			}
			if (i == 0)
			{
				num2 = 10f;
			}
			Vector2 vector = Random.insideUnitCircle.normalized * num2;
			Vector2 vector2 = base.transform.position.XY() + vector;
			Vector2 currentTargetPosition = base.transform.position.XY() + Vector2.Scale(Random.insideUnitCircle, new Vector2(2.25f, 1.75f)) + new Vector2(3.1f, 2.1f);
			GameObject gameObject = SpawnManager.SpawnVFX(CrowVFX, vector2.ToVector3ZUp(vector2.y), Quaternion.identity, true);
			GatlingGullCrowController component = gameObject.GetComponent<GatlingGullCrowController>();
			component.CurrentTargetPosition = currentTargetPosition;
			component.sprite.SortingOrder = 3;
			component.sprite.HeightOffGround = 20f;
			component.CurrentTargetHeight = 2f;
			component.useFacePoint = true;
			component.facePoint = base.transform.position.XY() + new Vector2(3.1f, 2.1f);
			m_extantCrows.Add(component);
		}
		StartCoroutine(HandleSequence());
	}

	private IEnumerator HandleSequence()
	{
		float elapsed = 0f;
		tk2dSprite shadowSprite = null;
		for (int j = 0; j < base.transform.childCount; j++)
		{
			GameObject gameObject = base.transform.GetChild(j).gameObject;
			if (gameObject.name.Contains("shadow", true))
			{
				shadowSprite = gameObject.GetComponent<tk2dSprite>();
				if ((bool)shadowSprite)
				{
					break;
				}
			}
		}
		yield return new WaitForSeconds(8f);
		while (elapsed < 3f)
		{
			elapsed += BraveTime.DeltaTime;
			base.sprite.scale = Vector3.one * (0.5f - elapsed / 6f + 0.5f);
			if ((bool)shadowSprite)
			{
				shadowSprite.scale = Vector3.one * (0.5f - elapsed / 6f + 0.5f);
			}
			yield return null;
		}
		base.sprite.SetSprite("gatling_gill_die_var_011");
		base.sprite.renderer.enabled = false;
		base.sprite.scale = Vector3.one;
		if ((bool)shadowSprite)
		{
			Object.Destroy(shadowSprite.gameObject);
		}
		base.sprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.NONE;
		base.sprite.ForceUpdateMaterial();
		for (int i = 0; i < m_extantCrows.Count; i++)
		{
			if (Random.value < 0.2f)
			{
				yield return new WaitForSeconds(Random.Range(0f, 0.025f));
			}
			m_extantCrows[i].useFacePoint = false;
			Vector2 escapeDir = Random.insideUnitCircle.normalized * Random.Range(50, 60);
			m_extantCrows[i].CurrentTargetHeight = 50f;
			m_extantCrows[i].CurrentTargetPosition = m_extantCrows[i].transform.position.XY() + escapeDir;
			m_extantCrows[i].destroyOnArrival = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
