using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostcardEditorUI : MonoBehaviour
{
    [SerializeField] private Image sceneryButtonImage;
    [SerializeField] private Image contextButtonImage;

    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    [SerializeField] private TMP_Text statusText;

    private bool contextSelected;
    private bool hasPublished;

    private void Start()
    {
        RefreshSelection();
    }

    public void SelectScenery()
    {
        if (hasPublished) return;

        contextSelected = false;
        RefreshSelection();
    }

    public void SelectContext()
    {
        if (hasPublished) return;

        contextSelected = true;
        RefreshSelection();
    }

    public void Publish()
    {
        GameStateManager gameState = GameStateManager.Instance;

        if (gameState == null)
        {
            ShowMessage("游戏状态管理器不存在，请检查场景配置。");
            return;
        }

        if (gameState.GetState("VideoPublished"))
        {
            ShowMessage("这张明信片已经发布过了。");
            return;
        }

        if (!gameState.AreAllClipsUnlocked)
        {
            ShowMessage("三段素材还没收集齐，请先完成拍摄任务。");
            return;
        }

        string resultId = contextSelected
            ? "l01_context"
            : "l01_scenery";

        if (gameState.TryPublish(resultId))
        {
            hasPublished = true;

            string copy = contextSelected
                ? "一顿饭从一桶水开始"
                : "沙漠午后的治愈一餐";

            ShowMessage("发布成功：" + copy);
            Debug.Log("发布结果 ID：" + resultId);
        }
    }

    private void RefreshSelection()
    {
        if (sceneryButtonImage == null ||
            contextButtonImage == null ||
            normalSprite == null ||
            selectedSprite == null)
        {
            Debug.LogError("请绑定两个文案按钮的图片和选中素材。", this);
            return;
        }

        sceneryButtonImage.sprite =
            contextSelected ? normalSprite : selectedSprite;

        contextButtonImage.sprite =
            contextSelected ? selectedSprite : normalSprite;
    }

    private void ShowMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
    }
}