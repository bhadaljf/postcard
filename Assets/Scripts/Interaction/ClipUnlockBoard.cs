using UnityEngine;
using UnityEngine.UI;

public class ClipUnlockBoard : MonoBehaviour
{
    public static ClipUnlockBoard Instance { get; private set; }

    [SerializeField] private Sprite drinkSprite;
    [SerializeField] private Sprite tableSprite;
    [SerializeField] private Sprite workSprite;

    private GameObject root;
    private int ignoreUntilFrame;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnAllClipsUnlocked += Show;
        }
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnAllClipsUnlocked -= Show;
            GameStateManager.Instance.OnAllClipsUnlocked += Show;
        }
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnAllClipsUnlocked -= Show;
        }
    }

    private void Update()
    {
        if (!IsOpen || Time.frameCount <= ignoreUntilFrame)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Close();
        }
    }

    private void Show()
    {
        if (root == null)
        {
            Build();
        }

        root.SetActive(true);
        IsOpen = true;
        ignoreUntilFrame = Time.frameCount;
        Debug.Log("三条素材都齐了：柠檬饮料、餐桌、配饮台。可以做成明信片了。右键或左键关闭。");
    }

    private void Close()
    {
        if (root != null)
        {
            root.SetActive(false);
        }

        IsOpen = false;
    }

    private void Build()
    {
        root = new GameObject("ClipUnlockView");
        root.transform.SetParent(transform, false);
        CreateBackdrop(root.transform);
        CreatePhoto(drinkSprite, new Vector3(-4.6f, 0.7f, 0f), root.transform);
        CreatePhoto(tableSprite, new Vector3(0f, 0.7f, 0f), root.transform);
        CreatePhoto(workSprite, new Vector3(4.6f, 0.7f, 0f), root.transform);
        CreateCaption(root.transform);
        root.SetActive(false);
    }

    private static void CreateBackdrop(Transform parent)
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
        backdrop.transform.SetParent(parent, false);
        backdrop.transform.localScale = new Vector3(40f, 24f, 1f);
        SpriteRenderer renderer = backdrop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0f, 0f, 0f, 0.72f);
        renderer.sortingOrder = 200;
    }

    private static void CreatePhoto(Sprite sprite, Vector3 position, Transform parent)
    {
        if (sprite == null)
        {
            return;
        }

        GameObject photo = new GameObject("Photo");
        photo.transform.SetParent(parent, false);
        photo.transform.localPosition = position;
        photo.transform.localScale = Vector3.one * 0.32f;
        SpriteRenderer renderer = photo.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 210;
    }

    private static void CreateCaption(Transform parent)
    {
        Font font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "DengXian" }, 42);
        if (font == null)
        {
            return;
        }

        string message = "三条素材都齐了：柠檬饮料、餐桌、配饮台\n可以做成明信片了。点击关闭。";
        font.RequestCharactersInTexture(message, 42, FontStyle.Normal);

        GameObject canvasObject = new GameObject("Caption");
        canvasObject.transform.SetParent(parent, false);
        canvasObject.transform.localPosition = new Vector3(0f, -1.85f, 0f);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 220;
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1400f, 180f);
        canvasRect.localScale = Vector3.one * 0.009f;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(canvasObject.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 42;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = new Color(0.98f, 0.96f, 0.9f, 1f);
        text.text = message;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
