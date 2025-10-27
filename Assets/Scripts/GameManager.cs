using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prey Spawn Settings")]
    public GameObject preyPrefab;
    public int startPreyCount = 10;
    public float preySpawnInterval = 5.0f;

    [Header("Predator Spawn Settings")]
    public GameObject predatorPrefab;
    public int startPredatorCount = 2;
    public float predatorSpawnInterval = 12.0f;

    [Header("Firefly Spawn Settings")]
    public GameObject fireflyPrefab;
    public int startFireflyCount = 15;
    public float fireflySpawnInterval = 5;

    [Header("Spawn Area (World Space)")]
    public float xMin = -5f;
    public float xMax = 5f;
    public float yMin = -3f;
    public float yMax = 3f;

    private float preyTimer;
    private float predatorTimer;
    private float fireflyTimer;

    void Start()
    {
        for (int i = 0; i < startPreyCount; i++)
        {
            Spawn(preyPrefab);
        }

        for (int i = 0; i < startPredatorCount; i++)
        {
            Spawn(predatorPrefab);
        }

        for (int i = 0; i < startFireflyCount; i++)
        {
            Spawn(fireflyPrefab);
        }

        preyTimer = preySpawnInterval;
        predatorTimer = predatorSpawnInterval;
        fireflyTimer = fireflySpawnInterval;
    }

    void Update()
    {
        preyTimer -= Time.deltaTime;
        if (preyTimer <= 0f)
        {
            Spawn(preyPrefab);
            preyTimer = preySpawnInterval;
        }

        predatorTimer -= Time.deltaTime;
        if (predatorTimer <= 0f)
        {
            Spawn(predatorPrefab);
            predatorTimer = predatorSpawnInterval;
        }

        fireflyTimer -= Time.deltaTime;
        if ( fireflyTimer <= 0f)
        {
            Spawn(fireflyPrefab);
            fireflyTimer = fireflySpawnInterval;
        }
    }

    void Spawn(GameObject prefab)
    {
        if (prefab == null) return;

        float x = Random.Range(xMin, xMax);
        float y = Random.Range(yMin, yMax);
        Vector3 pos = new Vector3(x, y, 0f);

        Instantiate(prefab, pos, Quaternion.identity);
    }
}
