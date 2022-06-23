using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DragunCracktonMap : MonoBehaviour
{
	[SerializeField]
	public List<Texture> crackSprites;

	private List<string> m_crackSpriteNames;

	private Dictionary<tk2dSpriteCollectionData, Dictionary<int, Texture>> m_cracktonMap = new Dictionary<tk2dSpriteCollectionData, Dictionary<int, Texture>>();

	public void Start()
	{
		m_crackSpriteNames = new List<string>(crackSprites.Count);
		for (int i = 0; i < crackSprites.Count; i++)
		{
			if ((bool)crackSprites[i])
			{
				m_crackSpriteNames.Add(crackSprites[i].name);
			}
		}
		tk2dSprite[] componentsInChildren = GetComponentsInChildren<tk2dSprite>(true);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].GenerateUV2 = true;
			componentsInChildren[j].usesOverrideMaterial = true;
			componentsInChildren[j].SpriteChanged += HandleCracktonChanged;
			componentsInChildren[j].ForceBuild();
			HandleCracktonChanged(componentsInChildren[j]);
		}
	}

	public void ConvertToCrackton()
	{
		StartCoroutine(HandleAmbient());
		StartCoroutine(HandleConversion());
	}

	private IEnumerator HandleAmbient()
	{
		float elapsed = 0f;
		float ambientChangeTime = 2f;
		Color startColor = RenderSettings.ambientLight;
		RoomHandler parentRoom = base.transform.position.GetAbsoluteRoom();
		while (elapsed < ambientChangeTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / ambientChangeTime;
			parentRoom.area.runtimePrototypeData.usesCustomAmbient = true;
			parentRoom.area.runtimePrototypeData.customAmbient = new Color(0.92f, 0.92f, 0.92f);
			parentRoom.area.runtimePrototypeData.customAmbientLowQuality = new Color(0.95f, 0.95f, 0.95f);
			RenderSettings.ambientLight = Color.Lerp(startColor, new Color(0.92f, 0.92f, 0.92f), t);
			yield return null;
		}
	}

	private IEnumerator HandleConversion()
	{
		float elapsed = 0f;
		float charTime = 1.7f;
		float crackTime = 0.7f;
		tk2dSprite[] childSprites = GetComponentsInChildren<tk2dSprite>(true);
		while (elapsed < charTime + crackTime)
		{
			elapsed += BraveTime.DeltaTime;
			float charT = Mathf.Clamp01(elapsed / charTime);
			float crackT = Mathf.Clamp01((elapsed - charTime) / crackTime);
			foreach (tk2dSprite tk2dSprite2 in childSprites)
			{
				if ((bool)tk2dSprite2)
				{
					tk2dSprite2.renderer.material.SetFloat("_CharAmount", charT);
					tk2dSprite2.renderer.material.SetFloat("_CrackAmount", crackT);
				}
			}
			yield return null;
			if (!this)
			{
				break;
			}
		}
	}

	public void PreGold()
	{
		tk2dSprite[] componentsInChildren = GetComponentsInChildren<tk2dSprite>(true);
		foreach (tk2dSprite tk2dSprite2 in componentsInChildren)
		{
			if ((bool)tk2dSprite2)
			{
				tk2dSprite2.renderer.material.SetFloat("_CharAmount", 1f);
				tk2dSprite2.renderer.material.SetFloat("_CrackAmount", 1f);
			}
		}
	}

	public void ConvertToGold()
	{
		StartCoroutine(HandleGoldAmbient());
		StartCoroutine(HandleGoldConversion());
	}

	private IEnumerator HandleGoldAmbient()
	{
		float elapsed = 0f;
		float ambientChangeTime = 2f;
		Color startColor = RenderSettings.ambientLight;
		RoomHandler parentRoom = base.transform.position.GetAbsoluteRoom();
		while (elapsed < ambientChangeTime)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / ambientChangeTime;
			parentRoom.area.runtimePrototypeData.usesCustomAmbient = true;
			parentRoom.area.runtimePrototypeData.customAmbient = new Color(1f, 0.78f, 0.6f);
			parentRoom.area.runtimePrototypeData.customAmbientLowQuality = new Color(1f, 0.82f, 0.64f);
			RenderSettings.ambientLight = Color.Lerp(startColor, new Color(1f, 0.78f, 0.6f), t);
			yield return null;
		}
	}

	private IEnumerator HandleGoldConversion()
	{
		float elapsed = 0f;
		float crackTime = 3.83f;
		tk2dSprite[] childSprites = GetComponentsInChildren<tk2dSprite>(true);
		while (elapsed < crackTime)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float crackT = 1f - Mathf.Clamp01(elapsed / crackTime);
			foreach (tk2dSprite tk2dSprite2 in childSprites)
			{
				if ((bool)tk2dSprite2)
				{
					tk2dSprite2.renderer.material.SetFloat("_CrackAmount", crackT);
				}
			}
			yield return null;
			if (!this)
			{
				yield break;
			}
		}
		foreach (tk2dSprite tk2dSprite3 in childSprites)
		{
			if ((bool)tk2dSprite3)
			{
				tk2dSprite3.renderer.material.SetFloat("_CharAmount", 0f);
				tk2dSprite3.renderer.material.SetFloat("_CrackAmount", 0f);
			}
		}
	}

	private void HandleCracktonChanged(tk2dBaseSprite obj)
	{
		tk2dSpriteCollectionData collection = obj.Collection;
		int spriteId = obj.spriteId;
		Dictionary<int, Texture> value;
		if (!m_cracktonMap.TryGetValue(collection, out value))
		{
			value = new Dictionary<int, Texture>();
			m_cracktonMap.Add(collection, value);
		}
		Texture value2;
		if (value.TryGetValue(spriteId, out value2))
		{
			if (value2 != null)
			{
				obj.renderer.material.SetTexture("_CracksTex", value2);
			}
			return;
		}
		string text = obj.GetCurrentSpriteDef().name;
		string item = text.Insert(text.Length - 4, "_crackton");
		int num = m_crackSpriteNames.IndexOf(item);
		if (num >= 0)
		{
			value.Add(spriteId, crackSprites[num]);
			obj.renderer.material.SetTexture("_CracksTex", crackSprites[num]);
			return;
		}
		item = text.Substring(0, text.Length - 4) + "_crackton_001";
		num = m_crackSpriteNames.IndexOf(item);
		if (num >= 0)
		{
			value.Add(spriteId, crackSprites[num]);
			obj.renderer.material.SetTexture("_CracksTex", crackSprites[num]);
			return;
		}
		if (text.Length > 12)
		{
			text = obj.GetCurrentSpriteDef().name;
			item = text.Insert(11, "_crack");
			num = m_crackSpriteNames.IndexOf(item);
			if (num >= 0)
			{
				value.Add(spriteId, crackSprites[num]);
				obj.renderer.material.SetTexture("_CracksTex", crackSprites[num]);
				return;
			}
		}
		value.Add(spriteId, null);
	}
}
