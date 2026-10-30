using UnityEngine;

public class L01OpeningUI : MonoBehaviour
{
    [SerializeField] private GameObject taskPhonePanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject postcardEditorPanel;
    [SerializeField] private GameObject openPhoneButton;

    private GameStateManager gameState;

    private void Start()
    {
        if (taskPhonePanel == null ||
            feedbackPanel == null ||
            objectivePanel == null ||
            postcardEditorPanel == null ||
            openPhoneButton == null)
        {
            Debug.LogError("请绑定开局界面的五个对象。", this);
            return;
        }

        postcardEditorPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        objectivePanel.SetActive(false);
        openPhoneButton.SetActive(true);
        taskPhonePanel.SetActive(true);

        gameState = GameStateManager.Instance;

        if (gameState == null)
        {
            Debug.LogError("场景中缺少游戏状态管理器。", this);
            return;
        }

        gameState.OnAllClipsUnlocked += ShowEditor;

        if (gameState.AreAllClipsUnlocked)
        {
            ShowEditor();
        }
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnAllClipsUnlocked -= ShowEditor;
        }
    }

    private void ShowEditor()
    {
        taskPhonePanel.SetActive(false);
        feedbackPanel.SetActive(false);
        objectivePanel.SetActive(false);
        openPhoneButton.SetActive(false);

        // 编辑明信片时暂停场景物体的点击和拍摄。
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.enabled = false;
        }

        postcardEditorPanel.SetActive(true);
    }
}