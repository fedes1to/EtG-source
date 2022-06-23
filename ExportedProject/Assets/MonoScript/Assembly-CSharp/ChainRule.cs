using System;

[Serializable]
public class ChainRule
{
	public string form;

	public string target;

	public float weight = 0.1f;

	public bool mandatory;
}
