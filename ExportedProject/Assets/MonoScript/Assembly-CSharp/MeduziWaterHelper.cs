using Dungeonator;
using UnityEngine;

public class MeduziWaterHelper : BraveBehaviour
{
	public GameObject ReflectionQuadPrefab;

	public Material floorWaterMaterial;

	private Transform m_quadInstance;

	private RoomHandler m_room;

	private bool m_cachedReflectionsEnabled;

	private void Start()
	{
		AIActor component = base.transform.parent.GetComponent<AIActor>();
		m_room = component.ParentRoom;
		base.transform.parent = m_room.hierarchyParent;
		m_cachedReflectionsEnabled = GameManager.Options.RealtimeReflections;
		ToggleToState(m_cachedReflectionsEnabled);
	}

	private void Update()
	{
		if (m_cachedReflectionsEnabled != GameManager.Options.RealtimeReflections)
		{
			m_cachedReflectionsEnabled = GameManager.Options.RealtimeReflections;
			ToggleToState(m_cachedReflectionsEnabled);
		}
	}

	private void ToggleToState(bool refl)
	{
		if (!m_quadInstance)
		{
			GameObject gameObject = Object.Instantiate(ReflectionQuadPrefab);
			m_quadInstance = gameObject.transform;
			m_quadInstance.position = m_room.GetCenterCell().ToVector3();
			m_quadInstance.position = m_quadInstance.position.WithZ(m_quadInstance.position.y - 16f);
		}
		Material sharedMaterial = m_quadInstance.GetComponent<MeshRenderer>().sharedMaterial;
		if (refl)
		{
			m_quadInstance.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
			sharedMaterial.shader = ShaderCache.Acquire("Brave/ReflectionOnly");
			floorWaterMaterial.SetColor("_LightCausticColor", new Color(0.5f, 0.5f, 0.5f));
		}
		else
		{
			m_quadInstance.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
			sharedMaterial.shader = ShaderCache.Acquire("Particles/Additive");
			floorWaterMaterial.SetColor("_LightCausticColor", new Color(1f, 1f, 1f));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
