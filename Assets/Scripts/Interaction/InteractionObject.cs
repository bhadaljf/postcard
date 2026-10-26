using UnityEngine;

public class InteractionObject : MonoBehaviour
{
    [Header("对象信息")]
    [SerializeField] private string objectId;
    [SerializeField] private string displayName;

    [TextArea(2, 5)]
    [SerializeField] private string inspectText;

    [Header("交互配置")]
    [SerializeField] private bool isInspectable = true;
    [SerializeField] private bool isCapturable;

    [Header("条件")]
    [SerializeField] private string requiredStateId;
    [TextArea(1, 3)]
    [SerializeField] private string lockedText;

    [Header("拍摄")]
    [SerializeField] private string captureRequiredStateId;
    [SerializeField] private string captureStateId;
    [SerializeField] private string captureClipId;

    [TextArea(1, 3)]
    [SerializeField] private string uncapturableText;

    [Header("拾取")]
    [SerializeField] private string pickupStateId;
    [SerializeField] private string itemId;

    [TextArea(1, 3)]
    [SerializeField] private string pickupText;

    [Header("得知")]
    [SerializeField] private string learnStateId;

    [TextArea(1, 3)]
    [SerializeField] private string learnedText;

    [Header("按状态调查")]
    [SerializeField] private string stateInspectStateId;

    [TextArea(1, 3)]
    [SerializeField] private string stateInspectText;

    [Header("后续得知")]
    [SerializeField] private string nextRequiredStateId;
    [SerializeField] private string nextLearnStateId;

    [TextArea(1, 3)]
    [SerializeField] private string nextLearnText;

    [TextArea(1, 3)]
    [SerializeField] private string nextLearnedText;

    [SerializeField] private bool allowHoverScale = true;

    public string ObjectId => objectId;
    public string DisplayName => displayName;
    public string ItemId => itemId;
    public bool IsInspectable => isInspectable;
    public bool IsCapturable => isCapturable;
    public bool CanPickUp => !string.IsNullOrEmpty(pickupStateId);
    public bool AllowHoverScale => allowHoverScale;

    public string Inspect()
    {
        if (!isInspectable)
        {
            return $"{displayName}目前不能调查。";
        }

        if (IsLocked(out string lockedMessage))
        {
            return lockedMessage;
        }

        if (!string.IsNullOrEmpty(stateInspectStateId) &&
            !string.IsNullOrEmpty(stateInspectText) &&
            GameStateManager.Instance != null &&
            GameStateManager.Instance.GetState(stateInspectStateId))
        {
            return $"{displayName}：{stateInspectText}";
        }

        if (TryNextLine(out string nextLine))
        {
            return nextLine;
        }

        if (!string.IsNullOrEmpty(learnStateId) && GameStateManager.Instance != null)
        {
            if (GameStateManager.Instance.GetState(learnStateId))
            {
                if (!string.IsNullOrEmpty(learnedText))
                {
                    return $"{displayName}：{learnedText}";
                }
            }
            else
            {
                GameStateManager.Instance.SetState(learnStateId, true);
            }
        }

        return $"{displayName}：{inspectText}";
    }

    public string Capture()
    {
        if (!isCapturable)
        {
            if (GameStateManager.Instance != null &&
                !string.IsNullOrEmpty(nextLearnStateId) &&
                GameStateManager.Instance.GetState(nextLearnStateId) &&
                !string.IsNullOrEmpty(nextLearnedText))
            {
                return $"{displayName}：{nextLearnedText}";
            }

            if (!string.IsNullOrEmpty(uncapturableText))
            {
                return uncapturableText;
            }

            return $"{displayName}目前不能拍摄。";
        }

        if (IsLocked(out string lockedMessage))
        {
            return lockedMessage;
        }

        if (!string.IsNullOrEmpty(captureRequiredStateId) &&
            (GameStateManager.Instance == null || !GameStateManager.Instance.GetState(captureRequiredStateId)))
        {
            return string.IsNullOrEmpty(lockedText)
                ? $"{displayName}暂时不能拍摄。"
                : lockedText;
        }

        if (string.IsNullOrEmpty(captureStateId))
        {
            return $"{displayName}没有配置拍摄结果。";
        }

        if (GameStateManager.Instance == null)
        {
            return "游戏状态管理器不存在，无法拍摄。";
        }

        if (GameStateManager.Instance.GetState(captureStateId))
        {
            return $"{displayName}已经拍摄过了。";
        }

        GameStateManager.Instance.SetState(captureStateId, true);

        if (string.IsNullOrEmpty(captureClipId))
        {
            return $"已拍摄{displayName}。";
        }

        return $"已拍摄{displayName}，解锁片段 {captureClipId}。";
    }

    public string Pickup()
    {
        if (!CanPickUp)
        {
            return Inspect();
        }

        if (IsLocked(out string lockedMessage))
        {
            return lockedMessage;
        }

        if (GameStateManager.Instance == null)
        {
            return "游戏状态管理器不存在，无法拾取。";
        }

        if (GameStateManager.Instance.GetState(pickupStateId))
        {
            return $"{displayName}已经在道具栏里。";
        }

        GameStateManager.Instance.SetState(pickupStateId, true);

        if (string.IsNullOrEmpty(pickupText))
        {
            return $"获得{displayName}。";
        }

        return pickupText;
    }

    private bool IsLocked(out string message)
    {
        message = null;

        if (string.IsNullOrEmpty(requiredStateId))
        {
            return false;
        }

        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.GetState(requiredStateId))
        {
            return false;
        }

        message = string.IsNullOrEmpty(lockedText)
            ? $"{displayName}暂时不能这样操作。"
            : lockedText;
        return true;
    }

    private bool TryNextLine(out string line)
    {
        line = null;

        if (string.IsNullOrEmpty(nextLearnStateId) || GameStateManager.Instance == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(nextRequiredStateId) &&
            !GameStateManager.Instance.GetState(nextRequiredStateId))
        {
            return false;
        }

        if (GameStateManager.Instance.GetState(nextLearnStateId))
        {
            string later = string.IsNullOrEmpty(nextLearnedText) ? nextLearnText : nextLearnedText;
            line = $"{displayName}：{later}";
            return true;
        }

        GameStateManager.Instance.SetState(nextLearnStateId, true);
        line = $"{displayName}：{nextLearnText}";
        return true;
    }
}
