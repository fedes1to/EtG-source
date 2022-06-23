using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Handles light intensity for Brave lights.")]
	[ActionCategory(".Brave")]
	public class SetBraveLightIntensity : FsmStateAction
	{
		[Tooltip("Specify lights to control; if empty, this action will affect all lights on its owner.")]
		public ShadowSystem[] specifyLights;

		[Tooltip("New light intensity after the transition.")]
		public FsmFloat intensity;

		[Tooltip("Duraiton of the transition.")]
		public FsmFloat transitionTime;

		private float[] m_startIntensity;

		private Color[] m_startColors;

		private Color[] m_endColors;

		private float m_timer;

		private SceneLightManager[] m_lightManagers;

		private Material[] m_materials;

		public bool IsKeptAction { get; set; }

		public override void Reset()
		{
			specifyLights = new ShadowSystem[0];
			intensity = 1f;
			transitionTime = 0f;
		}

		public override void OnEnter()
		{
			if (specifyLights.Length == 0)
			{
				specifyLights = base.Owner.gameObject.GetComponentsInChildren<ShadowSystem>();
				if (specifyLights.Length == 0)
				{
					Finish();
					return;
				}
			}
			m_lightManagers = new SceneLightManager[specifyLights.Length];
			for (int i = 0; i < specifyLights.Length; i++)
			{
				m_lightManagers[i] = specifyLights[i].GetComponent<SceneLightManager>();
			}
			m_materials = new Material[specifyLights.Length];
			for (int j = 0; j < specifyLights.Length; j++)
			{
				m_materials[j] = specifyLights[j].GetComponent<Renderer>().material;
			}
			if (transitionTime.Value <= 0f)
			{
				for (int k = 0; k < specifyLights.Length; k++)
				{
					specifyLights[k].uLightIntensity = intensity.Value;
					if ((bool)m_lightManagers[k])
					{
						Color value = m_lightManagers[k].validColors[Random.Range(0, m_lightManagers[k].validColors.Length)];
						m_materials[k].SetColor("_TintColor", value);
					}
					else
					{
						m_materials[k].SetColor("_TintColor", Color.white);
					}
				}
				Finish();
			}
			else
			{
				m_timer = 0f;
				m_startIntensity = null;
				m_startColors = null;
				m_endColors = null;
			}
		}

		public override void OnUpdate()
		{
			if (m_startIntensity == null)
			{
				m_startIntensity = new float[specifyLights.Length];
				m_startColors = new Color[specifyLights.Length];
				m_endColors = new Color[specifyLights.Length];
				for (int i = 0; i < specifyLights.Length; i++)
				{
					m_startIntensity[i] = specifyLights[i].uLightIntensity;
					m_startColors[i] = m_materials[i].GetColor("_TintColor");
					if (intensity.Value <= 0f)
					{
						m_endColors[i] = new Color(0.5f, 0.5f, 0.5f, 1f);
					}
					else
					{
						m_endColors[i] = ((!m_lightManagers[i]) ? Color.white : m_lightManagers[i].validColors[Random.Range(0, m_lightManagers[i].validColors.Length)]);
					}
				}
				m_timer = 0f;
			}
			else
			{
				m_timer += BraveTime.DeltaTime;
				for (int j = 0; j < specifyLights.Length; j++)
				{
					specifyLights[j].uLightIntensity = Mathf.Lerp(m_startIntensity[j], intensity.Value, m_timer / transitionTime.Value);
					m_materials[j].SetColor("_TintColor", Color.Lerp(m_startColors[j], m_endColors[j], m_timer / transitionTime.Value));
				}
				if (m_timer >= transitionTime.Value)
				{
					Finish();
				}
			}
		}

		public override void OnExit()
		{
			for (int i = 0; i < specifyLights.Length; i++)
			{
				specifyLights[i].uLightIntensity = intensity.Value;
				m_materials[i].SetColor("_TintColor", m_endColors[i]);
			}
		}

		public new void Finish()
		{
			if (!IsKeptAction)
			{
				base.Finish();
			}
			else
			{
				base.Finished = true;
			}
		}
	}
}
