﻿using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public ulong OwnerClientId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsServer)
        {
            Invoke("SimpleDespawn", 5);
        }
    }

    public void SimpleDespawn()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Wall"))
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
        else if (other.CompareTag("Player"))
        {
            SimplePlayerController player = other.GetComponent<SimplePlayerController>();
            if (player != null && player.OwnerClientId != OwnerClientId)
            {
                // obtener daño del dueño
                SimplePlayerController ownerPlayer = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<SimplePlayerController>();
                int damage = ownerPlayer.Damage.Value;

                player.ApplyDamage(damage);
            }

            GetComponent<NetworkObject>().Despawn(true);
        }
    }

}
