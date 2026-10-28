using TMPro;
using UnityEngine;

public class InteractionFeedbackUI : MonoBehaviour
{
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private TMP_Text feedbackText;

    private void OnEnable()
    {
        if (interactionManager == null || feedbackText == null)
        {
            Debug.LogError("请在 Inspector 中绑定交互管理器和提示文字。", this);
            return;
        }

        // 登记接收 B 发出的交互反馈。
        interactionManager.OnInteractionFeedback += ShowFeedback;
    }

    private void OnDisable()
    {
        if (interactionManager != null)
        {
            // 界面停用时，取消接收。
            interactionManager.OnInteractionFeedback -= ShowFeedback;
        }
    }

    private void ShowFeedback(string message)
    {
        // 把收到的文字显示到提示框。
        feedbackText.text = message;
    }
}