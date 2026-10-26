using UnityEngine;

public class RevealedSprite : MonoBehaviour
{
    [SerializeField] private string requiredStateId = "WaterRestored";
    [SerializeField] private Sprite sourceSprite;
    [SerializeField] private int cropX;
    [SerializeField] private int cropYFromTop;
    [SerializeField] private int cropWidth;
    [SerializeField] private int cropHeight;

    private SpriteRenderer visual;
    private bool subscribed;

    private void Awake()
    {
        visual = GetComponentInChildren<SpriteRenderer>(true);
        ApplyCrop();
        if (visual != null)
        {
            visual.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged += HandleState;
            subscribed = true;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (subscribed && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= HandleState;
        }

        subscribed = false;
    }

    private void HandleState(string stateId, bool value)
    {
        if (stateId == requiredStateId)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (visual == null || visual.sprite == null)
        {
            return;
        }

        bool show = GameStateManager.Instance != null
            && !string.IsNullOrEmpty(requiredStateId)
            && GameStateManager.Instance.GetState(requiredStateId);
        visual.enabled = show;
    }

    private void ApplyCrop()
    {
        if (visual == null || sourceSprite == null)
        {
            return;
        }

        if (cropWidth <= 0 || cropHeight <= 0 || sourceSprite.texture == null || !sourceSprite.texture.isReadable)
        {
            visual.sprite = sourceSprite;
            return;
        }

        float bottom = sourceSprite.texture.height - (cropYFromTop + cropHeight);
        visual.sprite = Sprite.Create(
            sourceSprite.texture,
            new Rect(cropX, bottom, cropWidth, cropHeight),
            new Vector2(0.5f, 0.5f),
            sourceSprite.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect);
    }
}
