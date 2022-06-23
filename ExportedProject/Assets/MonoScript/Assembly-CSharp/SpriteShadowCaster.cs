using System.Collections.Generic;
using UnityEngine;

public class SpriteShadowCaster : MonoBehaviour
{
	public float radius = 10f;

	public float shadowDepth = -0.05f;

	public Material material;

	private List<SpriteShadow> m_shadows;

	private Camera m_camera;

	private void Start()
	{
		m_camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		m_shadows = new List<SpriteShadow>();
	}

	public Material GetMaterialInstance()
	{
		return Object.Instantiate(material);
	}

	private void Update()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_camera);
		foreach (Collider collider in array)
		{
			tk2dSprite component = collider.GetComponent<tk2dSprite>();
			if ((collider.name != "PlayerSprite" && collider.GetComponent<AIActor>() == null) || (collider.GetComponent<MeshRenderer>() != null && !collider.GetComponent<MeshRenderer>().enabled) || !GeometryUtility.TestPlanesAABB(planes, collider.bounds) || !(component != null))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < m_shadows.Count; j++)
			{
				if (m_shadows[j].shadowedSprite == component)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				SpriteShadow item = new SpriteShadow(component, this);
				m_shadows.Add(item);
			}
		}
		for (int k = 0; k < m_shadows.Count; k++)
		{
			SpriteShadow spriteShadow = m_shadows[k];
			if (spriteShadow.shadowedSprite == null || !spriteShadow.shadowedSprite.enabled || !GeometryUtility.TestPlanesAABB(planes, spriteShadow.shadowedSprite.GetComponent<Collider>().bounds) || (spriteShadow.shadowedSprite.transform.position - base.transform.position).magnitude > radius)
			{
				m_shadows.RemoveAt(k);
				k--;
				spriteShadow.Destroy();
			}
			else
			{
				spriteShadow.UpdateShadow();
			}
		}
	}
}
