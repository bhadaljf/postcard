using System;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Serializable]
    public class RoomBinding
    {
        public string roomId;
        public string displayName;
        public GameObject roomRoot;
    }

    public static RoomManager Instance { get; private set; }

    public event Action<string> OnRoomChanged;

    [SerializeField] private RoomBinding[] rooms;
    [SerializeField] private string startingRoomId = "S01";

    private string currentRoomId;

    public string CurrentRoomId => currentRoomId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ShowRoom(startingRoomId);
        Debug.Log("房间切换已就绪：点击两侧色块前往相邻房间。");
    }

    public void ShowRoom(string roomId)
    {
        RoomBinding selected = null;

        if (rooms != null)
        {
            foreach (RoomBinding room in rooms)
            {
                if (room == null || room.roomRoot == null)
                {
                    continue;
                }

                bool active = room.roomId == roomId;
                room.roomRoot.SetActive(active);
                if (active)
                {
                    selected = room;
                }
            }
        }

        if (selected == null)
        {
            Debug.LogWarning($"未注册的房间：{roomId}");
            return;
        }

        if (currentRoomId == roomId)
        {
            return;
        }

        currentRoomId = roomId;
        string message = $"当前房间：{selected.displayName}（{roomId}）";
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.Report(message);
        }
        else
        {
            Debug.Log(message);
        }

        OnRoomChanged?.Invoke(roomId);
    }
}
