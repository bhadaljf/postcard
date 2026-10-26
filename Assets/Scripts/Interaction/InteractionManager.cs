using System;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    public event Action<string> OnInteractionFeedback;

    [SerializeField] private Color hoverColor = new Color(1f, 0.72f, 0.25f, 1f);
    [SerializeField] private float hoverScale = 1.08f;

    private Camera mainCamera;
    private UnityEngine.Object hoveredTarget;
    private SpriteRenderer hoveredVisual;
    private Color hoveredOriginalColor;
    private Vector3 hoveredOriginalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        Debug.Log("交互已就绪：左键调查，右键拍摄，左键点门洞换房间。");
    }

    private void OnDisable()
    {
        ClearHover();
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        if (WaterRepairPuzzle.Instance != null && WaterRepairPuzzle.Instance.IsOpen)
        {
            ClearHover();
            return;
        }

        if (ClipUnlockBoard.Instance != null && ClipUnlockBoard.Instance.IsOpen)
        {
            ClearHover();
            return;
        }

        Vector2 worldPoint = ScreenToWorld(Input.mousePosition);
        Collider2D hit = ChooseHit(Physics2D.OverlapPointAll(worldPoint));
        InteractionObject interaction = hit != null ? hit.GetComponentInParent<InteractionObject>() : null;
        NavigationHotspot hotspot = hit != null ? hit.GetComponentInParent<NavigationHotspot>() : null;
        WaterTank tank = hit != null ? hit.GetComponentInParent<WaterTank>() : null;
        SpriteRenderer doorRenderer = hotspot != null ? hotspot.GetComponent<SpriteRenderer>() : null;
        if (doorRenderer != null && !doorRenderer.enabled)
        {
            doorRenderer = null;
        }

        SpriteRenderer hoverVisual = hotspot != null ? doorRenderer : FindVisual(interaction);
        UnityEngine.Object hoverTarget = hotspot != null ? hotspot : interaction;
        bool scaleHover = interaction == null || interaction.AllowHoverScale;
        UpdateHover(hoverTarget, hoverVisual, scaleHover);

        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && tank != null)
        {
            Report(tank.Use());
            return;
        }

        if (Input.GetMouseButtonDown(0) && hotspot != null)
        {
            if (RoomManager.Instance == null)
            {
                Report("房间管理器不存在，无法切换房间。");
                return;
            }

            RoomManager.Instance.ShowRoom(hotspot.TargetRoomId);
            return;
        }

        if (interaction == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Report(interaction.CanPickUp ? interaction.Pickup() : interaction.Inspect());
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Report(interaction.Capture());
        }
    }

    public void Report(string message)
    {
        Debug.Log(message);
        OnInteractionFeedback?.Invoke(message);
    }

    private void UpdateHover(UnityEngine.Object current, SpriteRenderer visual, bool scaleHover)
    {
        if (hoveredTarget == current)
        {
            return;
        }

        ClearHover();

        if (current == null)
        {
            return;
        }

        hoveredTarget = current;
        if (visual == null)
        {
            return;
        }

        hoveredVisual = visual;
        hoveredOriginalColor = visual.color;
        hoveredOriginalScale = visual.transform.localScale;
        visual.color = hoverColor;
        if (scaleHover)
        {
            float scale = hoverScale > 0f ? hoverScale : 1.08f;
            visual.transform.localScale = hoveredOriginalScale * scale;
        }
    }

    private void ClearHover()
    {
        if (hoveredVisual != null)
        {
            hoveredVisual.color = hoveredOriginalColor;
            hoveredVisual.transform.localScale = hoveredOriginalScale;
        }

        hoveredTarget = null;
        hoveredVisual = null;
    }

    private static SpriteRenderer FindVisual(InteractionObject target)
    {
        if (target == null)
        {
            return null;
        }

        Transform visual = target.transform.Find("Visual");
        if (visual != null)
        {
            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.enabled)
            {
                return renderer;
            }
        }

        SpriteRenderer childRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (childRenderer != null && childRenderer.enabled)
        {
            return childRenderer;
        }

        return null;
    }

    private static Collider2D ChooseHit(Collider2D[] hits)
    {
        Collider2D best = null;
        float bestArea = float.MaxValue;

        if (hits == null)
        {
            return null;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D candidate = hits[i];
            if (candidate == null || !candidate.enabled)
            {
                continue;
            }

            bool interactive = candidate.GetComponentInParent<InteractionObject>() != null
                || candidate.GetComponentInParent<NavigationHotspot>() != null
                || candidate.GetComponentInParent<WaterTank>() != null;
            if (!interactive)
            {
                continue;
            }

            Vector3 size = candidate.bounds.size;
            float area = size.x * size.y;
            if (area < bestArea)
            {
                bestArea = area;
                best = candidate;
            }
        }

        return best;
    }

    private Vector2 ScreenToWorld(Vector3 screenPosition)
    {
        float depth = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
        return world;
    }
}
