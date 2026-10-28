using UnityEngine;

public class TaskPhoneUI : MonoBehaviour
{
    [SerializeField] private GameStateManager gameState;
    [SerializeField] private GameObject drinkCheck;
    [SerializeField] private GameObject tableCheck;
    [SerializeField] private GameObject workCheck;

    private void OnEnable()
    {
        if (gameState == null || drinkCheck == null ||
            tableCheck == null || workCheck == null)
        {
            Debug.LogError("请绑定状态管理器和三个勾选对象。", this);
            return;
        }

        // 手机打开时，开始接收状态变化，并立即更新显示。
        gameState.OnStateChanged += HandleStateChanged;
        RefreshChecks();
    }

    private void OnDisable()
    {
        if (gameState != null)
        {
            // 手机关闭时，取消接收。
            gameState.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(string stateId, bool value)
    {
        RefreshChecks();
    }

    private void RefreshChecks()
    {
        // 拍过就显示勾，没拍过就隐藏勾。
        drinkCheck.SetActive(gameState.GetState("ShotDrink"));
        tableCheck.SetActive(gameState.GetState("ShotTable"));
        workCheck.SetActive(gameState.GetState("ShotWork"));
    }
}