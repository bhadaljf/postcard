using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocialFeedbackUI : MonoBehaviour
{
    [SerializeField] private GameObject postcardEditorPanel;
    [SerializeField] private GameObject socialFeedbackPanel;

    [SerializeField] private Image publishedImage;
    [SerializeField] private TMP_Text publishedCopyText;

    [SerializeField] private Sprite scenerySprite;
    [SerializeField] private Sprite contextSprite;

    [SerializeField] private TMP_Text commentOneText;
    [SerializeField] private TMP_Text commentTwoText;
    [SerializeField] private TMP_Text brandMessageText;
    [SerializeField] private TMP_Text privateNoteText;

    private GameStateManager gameState;

    private void Start()
    {
        if (socialFeedbackPanel != null)
        {
            socialFeedbackPanel.SetActive(false);
        }

        if (postcardEditorPanel == null ||
            socialFeedbackPanel == null ||
            publishedImage == null ||
            publishedCopyText == null ||
            scenerySprite == null ||
            contextSprite == null ||
            commentOneText == null ||
            commentTwoText == null ||
            brandMessageText == null ||
            privateNoteText == null)
        {
            Debug.LogError("请完整绑定反馈页的对象、文字和图片。", this);
            return;
        }

        gameState = GameStateManager.Instance;

        if (gameState == null)
        {
            Debug.LogError("场景中缺少游戏状态管理器。", this);
            return;
        }

        gameState.OnVideoPublished += ShowFeedback;
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnVideoPublished -= ShowFeedback;
        }
    }

    private void ShowFeedback(string resultId)
    {
        if (resultId == "l01_scenery")
        {
            publishedImage.sprite = scenerySprite;
            publishedCopyText.text = "沙漠午后的治愈一餐";

            commentOneText.text =
                "看起来好治愈，真想去这里吃一顿午餐。";

            commentTwoText.text =
                "远离城市的忙碌，生活一定很悠闲吧。";

            brandMessageText.text =
                "品牌私信：这条内容很符合夏日午餐的主题。";

            privateNoteText.text =
                "林遥：镜头里的悠闲，藏起了她们为这顿饭付出的忙碌。";
        }
        else if (resultId == "l01_context")
        {
            publishedImage.sprite = contextSprite;
            publishedCopyText.text = "一顿饭从一桶水开始";

            commentOneText.text =
                "原来准备这顿饭之前，还要先把供水修好。";

            commentTwoText.text =
                "她同意被拍摄了吗？也想听听她自己的讲述。";

            brandMessageText.text =
                "品牌私信：记录了准备过程，后续也可以多呈现人物。";

            privateNoteText.text =
                "林遥：我补上了劳动的过程，但这仍是由我选择的讲述。";
        }
        else
        {
            Debug.LogError("无法识别的发布结果：" + resultId, this);
            return;
        }

        postcardEditorPanel.SetActive(false);
        socialFeedbackPanel.SetActive(true);
    }
}