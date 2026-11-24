using Mirror;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MinionSpawner : NetworkBehaviour
{
    public float minionMoveSpeed;
    public float minionCanonMoveSpeed;

    public GameObject minionPrefab;
    public GameObject minionCanonPrefab;
    public Transform[] spawnsPoints;
    public float spawnInterval = 20f;
    public int minionsPerWave = 6;
    public int wavesUntilMinionCanon = 3;
    private int waveCount = 0;

    public float delayBetweenMinions;

    [Server]
    private void Start()
    {
        if(!isServer) { return; }
        StartCoroutine(SpawnMinions());
    }

    [Server]
    private IEnumerator SpawnMinions()
    {
        while (true)
        {
            waveCount++;

            if (waveCount % wavesUntilMinionCanon == 0)
            {
                for (int i = 0; i < minionsPerWave / 2; i++)
                {
                    spawnRegularMinion();
                    yield return new WaitForSeconds(delayBetweenMinions);
                }

                spawnMinionCanon();
                yield return new WaitForSeconds(delayBetweenMinions);

                for (int i = minionsPerWave / 2; i < minionsPerWave - 1; i++)
                {
                    spawnRegularMinion();
                    yield return new WaitForSeconds(delayBetweenMinions);
                }

                spawnRegularMinion();
                yield return new WaitForSeconds(spawnInterval - delayBetweenMinions * (minionsPerWave - 1) - delayBetweenMinions);
            }
            else
            {
                for (int i = 0; i < minionsPerWave; i++)
                {
                    spawnRegularMinion();
                    yield return new WaitForSeconds(delayBetweenMinions);
                }
                yield return new WaitForSeconds(spawnInterval - delayBetweenMinions * minionsPerWave);
            }
        }
    }

    [Server]
    private void spawnRegularMinion()
    {
        if (isServer)
        {
            Transform spawnPoint = spawnsPoints[Random.Range(0, spawnsPoints.Length)];
            GameObject minion = Instantiate(minionPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(minion);
            minion.transform.parent = gameObject.transform;

            NavMeshAgent minionAgent = minion.GetComponent<NavMeshAgent>();
            minionAgent.speed = minionCanonMoveSpeed;
        }
    }

    [Server]
    private void spawnMinionCanon()
    {
        if (isServer)
        {
            Transform spawnPoint = spawnsPoints[Random.Range(0, spawnsPoints.Length)];
            GameObject minion = Instantiate(minionCanonPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(minion);
            minion.transform.parent = gameObject.transform;

            NavMeshAgent minionAgent = minion.GetComponent<NavMeshAgent>();
            minionAgent.speed = minionMoveSpeed;
        }
    }
}
