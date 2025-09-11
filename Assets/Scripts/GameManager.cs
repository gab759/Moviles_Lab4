using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static GameManager Instance;

    [SerializeField] private Transform player;
    [SerializeField] private GameObject buffP;
    [SerializeField] private GameObject enemyPrefab;

    public float spawnCount = 4f;
    public float currentCount = 0;

    public float enemySpawnInterval = 8f;
    public float enemySpawnTimer = 0f;
    public int maxEnemies = 5;
    private int currentEnemies = 0;
    public float enemySpawnRadius = 15f;

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (IsServer && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            currentCount += Time.deltaTime;
            if (currentCount >= spawnCount)
            {
                SpawnBuff();
                currentCount = 0;
            }

            enemySpawnTimer += Time.deltaTime;
            if (enemySpawnTimer >= enemySpawnInterval && currentEnemies < maxEnemies)
            {
                SpawnEnemy();
                enemySpawnTimer = 0f;
            }
        }
    }

    void SpawnBuff()
    {
        Vector3 randomPos = new Vector3(Random.Range(-8, 8), 0.5f, Random.Range(-8, 8));
        GameObject buff = Instantiate(buffP, randomPos, Quaternion.identity);
        buff.GetComponent<NetworkObject>().Spawn(true);
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
        enemyNetObj.Spawn(true);

        NetworkEnemy networkEnemy = enemy.GetComponent<NetworkEnemy>();
        if (networkEnemy != null)
        {
            networkEnemy.SetGameManager(this);
        }

        activeEnemies.Add(enemy);
        currentEnemies++;

        Debug.Log($"Enemigo generado. Total: {currentEnemies}");
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPos;
        bool validPosition = false;
        int attempts = 0;

        do
        {
            spawnPos = new Vector3(
                Random.Range(-enemySpawnRadius, enemySpawnRadius),
                0.5f,
                Random.Range(-enemySpawnRadius, enemySpawnRadius)
            );

            validPosition = IsPositionValid(spawnPos);
            attempts++;

            if (attempts > 10)
            {
                validPosition = true;
                Debug.LogWarning("No se encontró posición ideal para enemigo");
            }

        } while (!validPosition);

        return spawnPos;
    }

    bool IsPositionValid(Vector3 position)
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                float distance = Vector3.Distance(position, client.PlayerObject.transform.position);
                if (distance < 5f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void EnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            currentEnemies--;
            Debug.Log($"Enemigo destruido. Total: {currentEnemies}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.GetComponent<NetworkObject>().Despawn();
                    Destroy(enemy);
                }
            }

            activeEnemies.Clear();
            currentEnemies = 0;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Current players: " + NetworkManager.Singleton.ConnectedClients.Count);
        Debug.Log("Local Client ID: " + NetworkManager.Singleton.LocalClientId);
        InstancePlayerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    public void InstancePlayerRpc(ulong ownerID)
    {
        var playerP = Instantiate(player);
        var netObj = playerP.GetComponent<NetworkObject>();

        netObj.SpawnAsPlayerObject(ownerID, true);

        var ctrl = playerP.GetComponent<SimplePlayerController>();
        if (ctrl != null)
        {
            ctrl.PlayerID.Value = ownerID;
        }
    }

    public static GameManager instance => Instance;
}
