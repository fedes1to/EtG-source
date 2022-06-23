using System;
using UnityEngine;

public static class BraveRandom
{
	public static bool IgnoreGenerationDifferentiator;

	private static System.Random m_generationRandom;

	public static System.Random GeneratorRandom
	{
		get
		{
			return m_generationRandom;
		}
	}

	public static bool IsInitialized()
	{
		return m_generationRandom != null;
	}

	public static void InitializeRandom()
	{
		m_generationRandom = new System.Random();
	}

	public static void InitializeWithSeed(int seed)
	{
		m_generationRandom = new System.Random(seed);
	}

	public static float GenerationRandomValue()
	{
		return (float)m_generationRandom.NextDouble();
	}

	public static float GenerationRandomRange(float min, float max)
	{
		return (max - min) * GenerationRandomValue() + min;
	}

	public static int GenerationRandomRange(int min, int max)
	{
		return Mathf.FloorToInt((float)(max - min) * GenerationRandomValue()) + min;
	}
}
