public static class GrimmForestWaves
{
    public static int TotalWaves => 10;

    public static WaveDefinition GetWave(int waveNumber)
    {
        return waveNumber switch
        {
            1 => Wave1(),
            2 => Wave2(),
            3 => Wave3(),
            4 => Wave4(),
            5 => Wave5(),
            6 => Wave6(),
            7 => Wave7(),
            8 => Wave8(),
            9 => Wave9(),
            10 => Wave10(),
            _ => Wave1()
        };
    }

    static WaveDefinition Wave1()
    {
        return new WaveDefinition
        {
            note = "Tutorial",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.Imp, count = 8, spawnInterval = 1.2f, delayBeforeGroup = 0f }
            }
        };
    }

    static WaveDefinition Wave2()
    {
        return new WaveDefinition
        {
            note = "Armor appears",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.Imp, count = 5, spawnInterval = 1.1f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.Orc, count = 2, spawnInterval = 2f, delayBeforeGroup = 0.5f }
            }
        };
    }

    static WaveDefinition Wave3()
    {
        return new WaveDefinition
        {
            note = "Air + gold steal",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.GoblinRipper, count = 3, spawnInterval = 1f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.Wraith, count = 2, spawnInterval = 1.5f, delayBeforeGroup = 0.5f }
            }
        };
    }

    static WaveDefinition Wave4()
    {
        return new WaveDefinition
        {
            note = "Healer pressure",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.Imp, count = 10, spawnInterval = 0.9f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.ShadowPriest, count = 1, spawnInterval = 1f, delayBeforeGroup = 1f }
            }
        };
    }

    static WaveDefinition Wave5()
    {
        return new WaveDefinition
        {
            note = "Fire bomber threat",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.Orc, count = 4, spawnInterval = 1.5f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.FireBomber, count = 2, spawnInterval = 2f, delayBeforeGroup = 0.5f }
            }
        };
    }

    static WaveDefinition Wave6()
    {
        return new WaveDefinition
        {
            note = "Summoner pressure",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.WolfRider, count = 3, spawnInterval = 2f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.Imp, count = 6, spawnInterval = 1f, delayBeforeGroup = 0.5f }
            }
        };
    }

    static WaveDefinition Wave7()
    {
        return new WaveDefinition
        {
            note = "First elite",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.RockGolem, count = 1, spawnInterval = 1f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.GoblinRipper, count = 5, spawnInterval = 1f, delayBeforeGroup = 1f }
            }
        };
    }

    static WaveDefinition Wave8()
    {
        return new WaveDefinition
        {
            note = "Tower breaker",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.TowerBreaker, count = 1, spawnInterval = 1f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.ShadowPriest, count = 2, spawnInterval = 2f, delayBeforeGroup = 1.5f }
            }
        };
    }

    static WaveDefinition Wave9()
    {
        return new WaveDefinition
        {
            note = "Mixed pressure",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.Orc, count = 6, spawnInterval = 1.2f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.FireBomber, count = 4, spawnInterval = 1.5f, delayBeforeGroup = 0.5f },
                new WaveSpawnGroup { enemyType = EnemyType.Wraith, count = 3, spawnInterval = 1.2f, delayBeforeGroup = 1f }
            }
        };
    }

    static WaveDefinition Wave10()
    {
        return new WaveDefinition
        {
            note = "Boss wave",
            groups = new[]
            {
                new WaveSpawnGroup { enemyType = EnemyType.AncientDragon, count = 1, spawnInterval = 1f, delayBeforeGroup = 0f },
                new WaveSpawnGroup { enemyType = EnemyType.Imp, count = 8, spawnInterval = 1f, delayBeforeGroup = 2f }
            }
        };
    }
}
