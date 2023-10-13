using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectPseudo : MonoBehaviourPunCallbacks
{
    public TMP_InputField pseudo;

    public void connexion()
    {
        PhotonNetwork.NickName = pseudo.text;
        PhotonNetwork.LoadLevel("Lobby");
    }
}
