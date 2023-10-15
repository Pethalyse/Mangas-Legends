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

    private GameObject tp;


    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 0)
            {
                tp = PhotonNetwork.Instantiate(Path.Combine("Champion/AinzOoalGown", playerPrefab.name), blueSpawns.ToArray()[b].transform.position, Quaternion.identity);
                pw.RPC("initTheTeam", RpcTarget.All, Teams.Bleu);
                pw.RPC("supprSpawn", RpcTarget.All, 0);
            }

            else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 1)
            {
                tp = PhotonNetwork.Instantiate(Path.Combine("Champion/AinzOoalGown", playerPrefab.name), redSpawns.ToArray()[r].transform.position, Quaternion.identity);
                pw.RPC("initTheTeam", RpcTarget.All, Teams.Rouge);
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

    [PunRPC]
    internal void initTheTeam(Teams t)
    {
        tp.GetComponent<TeamManager>().Team = t;
    }
}
