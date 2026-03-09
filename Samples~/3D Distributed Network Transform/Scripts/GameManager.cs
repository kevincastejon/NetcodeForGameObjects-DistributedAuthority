using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
namespace Caskev.Samples.NetcodeForGameObjects.DistributedAuthority.DistributedNetworkTransform_3D
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private UnityTransport _transport;
        public void Host()
        {
            _networkManager.OnClientConnectedCallback += OnHostDone;
            _networkManager.StartHost();
        }
        private void OnHostDone(ulong obj)
        {
            _networkManager.OnClientConnectedCallback -= OnHostDone;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            Debug.Log("HOSTING");
        }
        private void OnClientConnected(ulong clientId)
        {
            Debug.Log("CLIENT CONNECTED WITH ID " + clientId);
        }
        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log("CLIENT " + clientId + " DISCONNECTED");
        }
        public void Join()
        {
            _networkManager.OnClientConnectedCallback += OnConnected;
            _networkManager.OnClientDisconnectCallback += OnConnectionFailed;
            _networkManager.StartClient();
            Debug.Log("CONNECTING TO HOST...");
        }

        private void OnConnectionFailed(ulong obj)
        {
            _networkManager.OnClientConnectedCallback -= OnConnected;
            _networkManager.OnClientDisconnectCallback -= OnConnectionFailed;
            Debug.Log("CONNECTION FAILED");
        }

        private void OnConnected(ulong clientId)
        {
            _networkManager.OnClientConnectedCallback -= OnConnected;
            _networkManager.OnClientDisconnectCallback -= OnConnectionFailed;
            _networkManager.OnClientDisconnectCallback += OnDisconnected;
            Debug.Log("CONNECTED TO HOST WITH ID " + clientId);
        }

        private void OnDisconnected(ulong obj)
        {
            _networkManager.OnClientDisconnectCallback -= OnDisconnected;
            Debug.Log("DISCONNECTED FROM HOST");
        }

        public void Stop()
        {
            _networkManager.OnClientConnectedCallback -= OnHostDone;
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientConnectedCallback -= OnConnected;
            _networkManager.OnClientDisconnectCallback -= OnConnectionFailed;
            _networkManager.OnClientDisconnectCallback -= OnDisconnected;
            _networkManager.Shutdown();
            Debug.Log("SHUTDOWN");
        }
    }
}
