using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Network
{
    public class RTSNetworkManager : MonoBehaviour
    {
        public static string requiredlayerIndexText = "0";
        // Start is called before the first frame update
        void Start()
        {

        }

        void Update()
        {

        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                requiredlayerIndexText = GUILayout.TextField(requiredlayerIndexText, 25);
                if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
                if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
                if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
            }
            else
            {
                var mode = NetworkManager.Singleton.IsHost ?
                    "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

                GUILayout.Label("Transport: " +
                    NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
                GUILayout.Label("Mode: " + mode);

                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
                    out var networkedClient))
                {
                    var player = networkedClient.PlayerObject.GetComponent<LocalPlayerScript>();
                    if (player)
                    {
                        GUILayout.Label(player.PlayerIndex.ToString());
                    }
                    else
                    {
                        GUILayout.Label("Get player failed");
                    }
                }
            }

            GUILayout.EndArea();
        }

        static void SubmitNewPosition()
        {
            //if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
            //{
            //    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
            //        out var networkedClient))
            //    {
            //        var player = networkedClient.PlayerObject.GetComponent<HelloWorldPlayer>();
            //        if (player)
            //        {
            //            player.Move();
            //        }
            //    }
            //}
        }
    }
}