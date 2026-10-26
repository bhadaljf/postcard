using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private readonly Dictionary<string, bool> states =
        new Dictionary<string, bool>();

    public event Action<string, bool> OnStateChanged;
    public event Action<string> OnClipUnlocked;
    public event Action<string> OnItemAdded;
    public event Action OnAllClipsUnlocked;
    public event Action<string> OnVideoPublished;

    public bool AreAllClipsUnlocked
    {
        get
        {
            return GetState("ShotDrink") && GetState("ShotTable") && GetState("ShotWork");
        }
    }

    public bool CanEditPostcard
    {
        get
        {
            return AreAllClipsUnlocked && !GetState("VideoPublished");
        }
    }

    private bool clipsUnlockedNotified;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeStates();
    }

    private void InitializeStates()
    {
        states.Clear();

        states["ShotDrink"] = false;
        states["ShotTable"] = false;

        states["WaterProblemKnown"] = false;
        states["RepairKitOwned"] = false;
        states["WaterRestored"] = false;

        states["WorkAuthorized"] = false;
        states["ShotWork"] = false;

        states["VideoPublished"] = false;
    }

    public bool GetState(string stateId)
    {
        if (states.TryGetValue(stateId, out bool value))
        {
            return value;
        }

        Debug.LogWarning($"未注册的游戏状态：{stateId}");
        return false;
    }

    public void SetState(string stateId, bool value)
    {
        if (!states.ContainsKey(stateId))
        {
            Debug.LogWarning($"尝试修改未注册的游戏状态：{stateId}");
            return;
        }

        if (states[stateId] == value)
        {
            return;
        }

        states[stateId] = value;
        OnStateChanged?.Invoke(stateId, value);

        Debug.Log($"游戏状态更新：{stateId} = {value}");

        if (value && stateId == "ShotDrink")
        {
            OnClipUnlocked?.Invoke("shot_drink");
        }
        else if (value && stateId == "ShotTable")
        {
            OnClipUnlocked?.Invoke("shot_table");
        }
        else if (value && stateId == "ShotWork")
        {
            OnClipUnlocked?.Invoke("shot_work");
        }
        else if (value && stateId == "RepairKitOwned")
        {
            OnItemAdded?.Invoke("repair_kit");
        }

        if (!clipsUnlockedNotified && AreAllClipsUnlocked)
        {
            clipsUnlockedNotified = true;
            Debug.Log("三条拍摄素材都齐了，可以交给明信片编辑。");
            OnAllClipsUnlocked?.Invoke();
        }
    }

    public bool TryPublish(string resultId)
    {
        if (GetState("VideoPublished"))
        {
            Debug.Log("明信片已经发布过了。");
            return false;
        }

        if (!AreAllClipsUnlocked)
        {
            Debug.Log("三条拍摄素材还没齐，还不能发布。");
            return false;
        }

        SetState("VideoPublished", true);
        string publishedId = string.IsNullOrEmpty(resultId) ? "published" : resultId;
        Debug.Log("明信片已发布。");
        OnVideoPublished?.Invoke(publishedId);
        return true;
    }
}
