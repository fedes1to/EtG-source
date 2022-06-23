using System;
using System.Collections.Generic;

[Serializable]
public class EnemyFactoryWaveDefinition
{
	public bool exactDefinition;

	public List<AIActor> enemyList;

	public int inexactMinCount = 2;

	public int inexactMaxCount = 4;
}
