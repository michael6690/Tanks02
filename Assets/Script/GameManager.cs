using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tanks;
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    string gameVersion = "1";
    public static GameObject localPlayer;

    private GameObject defaultSpawnPoints;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogErrorFormat(gameObject,
                    "Multiple instances of {0} is not allow", GetType().Name);
            DestroyImmediate(gameObject);
            return;
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        DontDestroyOnLoad(gameObject);
        instance = this;

        defaultSpawnPoints = new GameObject("Default SpawnPoints");
        defaultSpawnPoints.transform.position = new Vector3(0, 0, 0);
        defaultSpawnPoints.transform.SetParent(transform, false);
    }
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        SceneManager.sceneLoaded += OnSceneLoaded;

    }
    public override void OnConnected()
    {
        Debug.Log("PUN Connected");
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Connected to Master");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Disconnected was called by PUN with reason {0}", cause);
    }
    public void JoinGameRoom()
    {
        var options = new RoomOptions
        {
            MaxPlayers = 6
        };
        PhotonNetwork.JoinOrCreateRoom("Kingdom", options, null);
    }
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Created room!!");
            PhotonNetwork.LoadLevel("GameScene");
        }
        else
        {
            Debug.Log("Joined room!!");
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Joined room failed!!");
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        var spawnPoint = GetRandomSpawnPoints();

        localPlayer = PhotonNetwork.Instantiate("TankPlayer",spawnPoint.position,spawnPoint.rotation,0);
        Debug.Log("Player Instance ID: " + localPlayer.GetInstanceID());
    }
    #region Utility
    private Transform GetRandomSpawnPoints()
    {
        var spawnPoints = GetAllObjectsOfTypeInScene<SpawnPoint>();
        return spawnPoints.Count == 0
            ? defaultSpawnPoints.transform
            : spawnPoints[Random.Range(0,spawnPoints.Count)].transform;
    }
    public static List<GameObject> GetAllObjectsOfTypeInScene<T>()
    {
        var objectsInScene = new List<GameObject>();
        foreach (var go in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (go.hideFlags == HideFlags.NotEditable ||
                go.hideFlags == HideFlags.HideAndDontSave)
                continue;
            if (go.GetComponent<T>() != null)
                objectsInScene.Add(go);
        }
        return objectsInScene;
    }
    #endregion
}