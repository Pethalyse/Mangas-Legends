using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void chooseTeam(int team)
    {
        PhotonNetwork.LocalPlayer.JoinTeam((byte)team);

        if (PhotonNetwork.CountOfRooms == 0) 
        {
            PhotonNetwork.CreateRoom("Room");
        }
        else
        {
            PhotonNetwork.JoinRoom("Room");
        };
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }

}
