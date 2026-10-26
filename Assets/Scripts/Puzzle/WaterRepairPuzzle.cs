using System.Collections;
using UnityEngine;

public class WaterRepairPuzzle : MonoBehaviour
{
    public static WaterRepairPuzzle Instance { get; private set; }

    [SerializeField] private Sprite[] stateSprites;
    [SerializeField] private Sprite sealSprite;
    [SerializeField] private Sprite hoseSprite;
    [SerializeField] private Sprite clampSprite;
    [SerializeField] private Sprite targetSprite;
    [SerializeField] private Sprite successSprite;

    private const float AcceptDistance = 0.72f;

    private readonly Vector3 socket = new Vector3(2.14f, -0.47f, 0f);

    private int step;
    private bool open;
    private bool finishing;
    private int ignoreMouseUntilFrame = -1;
    private GameObject viewRoot;
    private SpriteRenderer boardRenderer;
    private SpriteRenderer successRenderer;
    private GameObject targetMark;
    private RepairPart[] parts;
    private RepairPart dragging;
    private Vector3 dragOffset;

    public bool IsOpen => open;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildView();
    }

    private void Start()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomChanged += HandleRoomChanged;
        }
    }

    private void OnDisable()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomChanged -= HandleRoomChanged;
        }
    }

    public string TryOpen()
    {
        if (GameStateManager.Instance == null)
        {
            return "游戏状态管理器不存在。";
        }

        if (GameStateManager.Instance.GetState("WaterRestored") || step >= 3)
        {
            return "水管已经接好了。";
        }

        if (!GameStateManager.Instance.GetState("WaterProblemKnown"))
        {
            return "这只备用水桶的管子松着。";
        }

        if (!GameStateManager.Instance.GetState("RepairKitOwned"))
        {
            return "管子松了。先去架子上找修复工具。";
        }

        Open();
        return "把发亮的零件拖到水桶侧面的圆圈上。软管在水桶下面。右键可以退出。";
    }

    private void Open()
    {
        if (viewRoot == null)
        {
            BuildView();
        }

        open = true;
        ignoreMouseUntilFrame = Time.frameCount;
        viewRoot.SetActive(true);
        Refresh();
    }

    private void Close()
    {
        open = false;
        finishing = false;
        dragging = null;
        if (viewRoot != null)
        {
            viewRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!open || finishing)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Close();
            return;
        }

        Vector2 mouse = ScreenToWorld(Input.mousePosition);
        if (Time.frameCount <= ignoreMouseUntilFrame)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RepairPart part = FindPart(mouse);
            if (part != null && part.Order == step)
            {
                dragging = part;
                dragOffset = part.Root.position - (Vector3)mouse;
                part.Renderer.sortingOrder = 120;
            }

            return;
        }

        if (dragging == null)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            dragging.Root.position = (Vector3)mouse + dragOffset;
            return;
        }

        Release(dragging, mouse);
        dragging = null;
    }

    private void Release(RepairPart part, Vector2 mouse)
    {
        bool onSocket = Vector2.Distance(mouse, socket) <= AcceptDistance
            || Vector2.Distance(part.Root.position, socket) <= AcceptDistance;

        if (onSocket && part.Order == step)
        {
            part.Placed = true;
            part.Root.gameObject.SetActive(false);
            step++;
            Refresh();
            if (step >= 3)
            {
                Complete();
            }

            return;
        }

        part.Root.localPosition = part.Home;
        part.Renderer.sortingOrder = 110;
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.Report("对准水桶侧面的圆圈再松开。");
        }
    }

    private void Complete()
    {
        finishing = true;
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState("WaterRestored", true);
        }

        if (successRenderer != null)
        {
            successRenderer.gameObject.SetActive(true);
        }

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.Report("水管接好了。");
        }

        StartCoroutine(CloseAfterSuccess());
    }

    private IEnumerator CloseAfterSuccess()
    {
        yield return new WaitForSeconds(1.1f);
        Close();
    }

    private void Refresh()
    {
        if (boardRenderer != null && stateSprites != null && stateSprites.Length > 0)
        {
            int index = Mathf.Clamp(step, 0, stateSprites.Length - 1);
            boardRenderer.sprite = stateSprites[index];
        }

        if (targetMark != null)
        {
            targetMark.SetActive(step < 3);
        }

        if (parts == null)
        {
            return;
        }

        for (int i = 0; i < parts.Length; i++)
        {
            RepairPart part = parts[i];
            bool show = !part.Placed && step < 3;
            part.Root.gameObject.SetActive(show);
            if (!show)
            {
                continue;
            }

            bool current = part.Order == step;
            part.Root.localPosition = part.Home;
            part.Root.localScale = Vector3.one * part.BaseScale * (current ? 1f : 0.92f);
            part.Collider.enabled = current;
            part.Renderer.color = current ? Color.white : new Color(1f, 1f, 1f, 0.28f);
            part.Renderer.sortingOrder = current ? 112 : 110;
        }
    }

    private void HandleRoomChanged(string roomId)
    {
        if (open)
        {
            Close();
        }
    }

    private RepairPart FindPart(Vector2 mouse)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(mouse);
        if (hits == null)
        {
            return null;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null || parts == null)
            {
                continue;
            }

            for (int p = 0; p < parts.Length; p++)
            {
                if (!parts[p].Placed && parts[p].Order == step && hits[i].transform == parts[p].Root)
                {
                    return parts[p];
                }
            }
        }

        return null;
    }

    private Vector2 ScreenToWorld(Vector3 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return screenPosition;
        }

        float depth = Mathf.Abs(camera.transform.position.z);
        Vector3 world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
        return world;
    }

    private void BuildView()
    {
        if (viewRoot != null)
        {
            return;
        }

        viewRoot = new GameObject("RepairView");
        viewRoot.transform.SetParent(transform, false);

        CreateBackdrop();

        GameObject board = new GameObject("Board");
        board.transform.SetParent(viewRoot.transform, false);
        boardRenderer = board.AddComponent<SpriteRenderer>();
        boardRenderer.sortingOrder = 100;

        targetMark = new GameObject("Socket");
        targetMark.transform.SetParent(viewRoot.transform, false);
        targetMark.transform.localPosition = socket;
        targetMark.transform.localScale = Vector3.one * 0.2f;
        SpriteRenderer targetRenderer = targetMark.AddComponent<SpriteRenderer>();
        targetRenderer.sprite = targetSprite;
        targetRenderer.sortingOrder = 102;

        if (stateSprites != null && stateSprites.Length > 1)
        {
            stateSprites[0] = HideLooseHose(stateSprites[0]);
            stateSprites[1] = HideLooseHose(stateSprites[1]);
        }

        parts = new[]
        {
            CreatePart(Crop(sealSprite, 63, 96, 250, 170), new Vector3(-2.7f, 0.85f, 0f), 0, 0.58f),
            CreatePart(Crop(hoseSprite, 40, 150, 278, 100), new Vector3(2.42f, -1.54f, 0f), 1, 0.4f),
            CreatePart(Crop(clampSprite, 20, 90, 244, 183), new Vector3(-2.7f, -0.85f, 0f), 2, 0.58f)
        };

        GameObject success = new GameObject("Success");
        success.transform.SetParent(viewRoot.transform, false);
        success.transform.localScale = Vector3.one * 1.4f;
        successRenderer = success.AddComponent<SpriteRenderer>();
        successRenderer.sprite = successSprite;
        successRenderer.sortingOrder = 130;
        success.SetActive(false);

        viewRoot.SetActive(false);
    }

    private void CreateBackdrop()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);

        GameObject backdrop = new GameObject("Backdrop");
        backdrop.transform.SetParent(viewRoot.transform, false);
        backdrop.transform.localScale = new Vector3(40f, 24f, 1f);
        SpriteRenderer renderer = backdrop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0f, 0f, 0f, 0.62f);
        renderer.sortingOrder = 90;
    }

    private static Sprite Crop(Sprite source, int x, int yFromTop, int width, int height)
    {
        if (source == null || source.texture == null || !source.texture.isReadable)
        {
            return source;
        }

        float bottom = source.texture.height - (yFromTop + height);
        return Sprite.Create(source.texture, new Rect(x, bottom, width, height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
    }

    private static Sprite HideLooseHose(Sprite source)
    {
        if (source == null || source.texture == null || !source.texture.isReadable)
        {
            return source;
        }

        int width = source.texture.width;
        int height = source.texture.height;
        Color[] pixels = source.texture.GetPixels();
        for (int imageY = 575; imageY <= 630; imageY++)
        {
            int texY = height - 1 - imageY;
            if (texY < 0 || texY >= height)
            {
                continue;
            }

            for (int x = 620; x <= 760; x++)
            {
                if (x < 0 || x >= width)
                {
                    continue;
                }

                int index = texY * width + x;
                Color color = pixels[index];
                if (color.a > 0.15f && color.b > color.r + 0.03f && color.b > 0.25f)
                {
                    pixels[index] = Color.clear;
                }
            }
        }

        Texture2D copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
        copy.SetPixels(pixels);
        copy.Apply();
        return Sprite.Create(copy, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
    }

    private RepairPart CreatePart(Sprite sprite, Vector3 home, int order, float scale)
    {
        GameObject root = new GameObject("Part" + order);
        root.transform.SetParent(viewRoot.transform, false);
        root.transform.localPosition = home;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 110;

        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.size = sprite != null ? (Vector2)sprite.bounds.size : new Vector2(1.2f, 1.2f);

        return new RepairPart
        {
            Root = root.transform,
            Renderer = renderer,
            Collider = collider,
            Home = home,
            Order = order,
            BaseScale = scale
        };
    }

    private sealed class RepairPart
    {
        public Transform Root;
        public SpriteRenderer Renderer;
        public BoxCollider2D Collider;
        public Vector3 Home;
        public int Order;
        public float BaseScale;
        public bool Placed;
    }
}
