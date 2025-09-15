using Unity.Netcode;
using UnityEngine;

public class RandomBuff : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the Mono Behaviour is created
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SimplePlayerController player = other.GetComponent<SimplePlayerController>();
            if (player != null)
            {
                int extraDamage = Random.Range(1, 4);
                player.Damage.Value += extraDamage;

                Debug.Log($"Jugador {player.OwnerClientId} obtuvo buff de +{extraDamage} daño. Daño total: {player.Damage.Value}");
            }

            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    [Rpc(SendTo.Server)]
    private void AddBuffPlayerRpc(ulong playerID)
    {
        print("aplicar buf " + playerID);
        GetComponent<NetworkObject>().Despawn(true);
    }
}