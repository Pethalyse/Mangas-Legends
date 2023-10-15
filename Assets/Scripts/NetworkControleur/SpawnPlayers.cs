using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using System.Collections.Generic;
using Photon.Pun.UtilityScripts;

public class SpawnPlayers : MonoBehaviourPun
{
    public GameObject playerPrefab;

    public List<GameObject> blueSpawns;
    int b = 0;
    public List<GameObject> redSpawns;
    int r = 0;

    public PhotonView pw;


    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 0)
            {
                GameObject tp = PhotonNetwork.Instantiate(Path.Combine("Champion/AinzOoalGown", playerPrefab.name), blueSpawns.ToArray()[b].transform.position, Quaternion.identity);
                tp.GetComponent<TeamManager>().Team = Teams.Bleu;
                pw.RPC("supprSpawn", RpcTarget.All, 0);
            }

            else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 1)
            {
                GameObject tp = PhotonNetwork.Instantiate(Path.Combine("Champion/AinzOoalGown", playerPrefab.name), redSpawns.ToArray()[r].transform.position, Quaternion.identity);
                tp.GetComponent<TeamManager>().Team = Teams.Rouge;
                pw.RPC("supprSpawn", RpcTarget.All, 1);
            }
        }

    }

    [PunRPC]
    public void supprSpawn(int spawns)
    {
        if(spawns == 0)
        {
            blueSpawns.Remove(blueSpawns.ToArray()[b]);
            b++;
        }
        else if (spawns == 1)
        {
            redSpawns.Remove(redSpawns.ToArray()[r]);
            r++;
        }

        

    }
}
