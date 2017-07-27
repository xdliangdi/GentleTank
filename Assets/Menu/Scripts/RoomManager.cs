﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : Photon.MonoBehaviour
{
    public GameObject playerPanelPerfab;                    // 玩家面板预设
    public Transform playerGroup;                           // 玩家面板组
    public Text roomTitle;                                  // 房间标题
    public Text roommatesCount;                             // 房间成员数
    public Button startButton;                              // 开始按钮
    public WindowPanel windowPanel;                         // 窗口按钮
    public Toast toast;                                     // 提示
    public float refreshRate = 1f;                          // 刷新信息时间

    private float elapsed;                                  // 计时器
    private int maxPlayers;                                 // 房间玩家总容量
    private List<PlayerPanelManager> playerPanelList;       // 玩家面板列表
    private bool isReady;                                   // 是否可以开始游戏
    private Dictionary<PhotonPlayer, PlayerPanelManager> playerDic;     // 玩家信息对应面板
    private List<PhotonPlayer> temPlayerList;                           // 临时玩家列表

    /// <summary>
    /// 进入房间，如果失去连接回到大厅，没有就初始化
    /// </summary>
    private void Awake()
    {
        if (!PhotonNetwork.connected)                       // 若进入时没连接，直接回去大厅
        {
            AllSceneManager.LoadScene(GameScene.LobbyScene);
            return;
        }
        playerDic = new Dictionary<PhotonPlayer, PlayerPanelManager>();
    }

    /// <summary>
    /// 初始化房间，房主
    /// </summary>
    private void Start()
    {
        InitRoom();
        RefreshMaster();
    }

    /// <summary>
    /// 周期刷新信息
    /// </summary>
    private void Update()
    {
        elapsed -= Time.deltaTime;
        if (elapsed < 0f)
        {
            elapsed = refreshRate;
            Refresh(PhotonNetwork.playerList);
        }
    }

    /// <summary>
    /// 初始化房间信息
    /// </summary>
    public void InitRoom()
    {
        roomTitle.text = PhotonNetwork.room.Name;
        maxPlayers = PhotonNetwork.room.MaxPlayers;
        roommatesCount.text = "1/" + maxPlayers;
        playerPanelList = new List<PlayerPanelManager>();
        for (int i = 0; i < maxPlayers; i++)
        {
            playerPanelList.Add(Instantiate(playerPanelPerfab, playerGroup).GetComponent<PlayerPanelManager>());
            playerPanelList[i].Init(i + 1);
        }
        RefeshPlayerDic(PhotonNetwork.playerList);
    }

    /// <summary>
    /// 清除无效玩家
    /// </summary>
    /// <param name="photonPlayers">服务器发来的玩家列表</param>
    public void CleanInvalidPlayerPanels(PhotonPlayer[] photonPlayers)
    {
        temPlayerList = new List<PhotonPlayer>(photonPlayers);
        for (int i = 0; i < playerPanelList.Count; i++)
        {
            // 如果面板没人再用，或者有人但这人在列表里面，跳过。
            if (!playerPanelList[i].IsUsed || temPlayerList.Contains(playerPanelList[i].Player))
                continue;

            Debug.Log("Cleaned " + playerPanelList[i].Player.NickName);
            playerDic.Remove(playerPanelList[i].Player);
            playerPanelList[i].Clean();
            StartBtnTrigger(false);
        }
    }

    /// <summary>
    /// 刷新信息
    /// </summary>
    public void Refresh(PhotonPlayer[] photonPlayers)
    {
        roommatesCount.text = PhotonNetwork.room.PlayerCount + "/" + maxPlayers;

        if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount == maxPlayers && !isReady)
            StartBtnTrigger(true);
    }

    /// <summary>
    /// 刷新玩家字典，玩家进入房间时
    /// </summary>
    public void RefeshPlayerDic(PhotonPlayer[] photonPlayers)
    {
        for (int i = 0; i < photonPlayers.Length; i++)
            if (!playerDic.ContainsKey(photonPlayers[i]))
            {
                playerDic[photonPlayers[i]] = GetEmptyPlayerPanel();
                playerDic[photonPlayers[i]].Fill(photonPlayers[i]);
            }
    }

    /// <summary>
    /// 获取空的玩家面板
    /// </summary>
    /// <returns>空玩家面板</returns>
    public PlayerPanelManager GetEmptyPlayerPanel()
    {
        for (int i = 0; i < playerPanelList.Count; i++)
            if (!playerPanelList[i].IsUsed)
                return playerPanelList[i];
        return null;
    }

    /// <summary>
    /// 开始按钮触发
    /// </summary>
    /// <param name="ready">是否准备好</param>
    public void StartBtnTrigger(bool ready)
    {
        roommatesCount.gameObject.SetActive(!ready);
        startButton.gameObject.SetActive(ready);
        isReady = ready;
    }

    /// <summary>
    /// 刷新房主标记
    /// </summary>
    public void RefreshMaster()
    {
        for (int i = 0; i < playerPanelList.Count; i++)
            if (playerPanelList[i].IsUsed)
                playerPanelList[i].SetMaster(playerPanelList[i].Player.IsMasterClient);
            else
                playerPanelList[i].SetMaster(false);
    }

    /// <summary>
    /// 更换房间主人时调用
    /// </summary>
    /// <param name="player"></param>
    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.Log("OnMasterClientSwitched: " + player);
    }

    /// <summary>
    /// 退出房间
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 自己离开房间时调用
    /// </summary>
    public void OnLeftRoom()
    {
        Debug.Log("自己离开了房间");
        AllSceneManager.LoadScene(GameScene.LobbyScene);
    }

    /// <summary>
    /// 失去连接时调用
    /// </summary>
    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        windowPanel.OpenWindow("连接中断", "连接中断", "返回大厅", false, () => { AllSceneManager.LoadScene(GameScene.LobbyScene); });
        AllSceneManager.LoadScene(GameScene.LobbyScene);
    }

    /// <summary>
    /// 实力创建时调用
    /// </summary>
    /// <param name="info"></param>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
    }

    /// <summary>
    /// 新玩家进入是更新玩家字典
    /// </summary>
    /// <param name="player">新玩家</param>
    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        RefeshPlayerDic(PhotonNetwork.playerList);
        RefreshMaster();
    }

    /// <summary>
    /// 玩家离开时清除掉
    /// </summary>
    /// <param name="player"></param>
    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        CleanInvalidPlayerPanels(PhotonNetwork.playerList);
        RefreshMaster();
    }

    /// <summary>
    /// 连接失败
    /// </summary>
    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");
        AllSceneManager.LoadScene(GameScene.LobbyScene);
    }

}
