using UnityEngine;

public class StateSpriteSwap : MonoBehaviour
{
    [SerializeField] private string requiredStateId = "WaterRestored";
    [SerializeField] private Sprite restoredSprite;

    private SpriteRenderer visual;
    private Sprite originalSprite;
    private bool subscribed;

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        if (visual != null)
        {
            originalSprite = visual.sprite;
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
        if (visual == null)
        {
            return;
        }

        bool restored = GameStateManager.Instance != null
            && !string.IsNullOrEmpty(requiredStateId)
            && GameStateManager.Instance.GetState(requiredStateId);
        visual.sprite = restored && restoredSprite != null ? restoredSprite : originalSprite;
    }
}
