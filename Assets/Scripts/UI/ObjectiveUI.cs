using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private GameStateManager gameState;
    [SerializeField] private TMP_Text objectiveText;

    private void Start()
    {
        if (gameState == null || objectiveText == null)
        {
            Debug.LogError("请绑定状态管理器和目标文字。", this);
            return;
        }

        gameState.OnStateChanged += HandleStateChanged;
        RefreshObjective();
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(string stateId, bool value)
    {
        RefreshObjective();
    }

    private void RefreshObjective()
    {
        bool drinkDone = gameState.GetState("ShotDrink");
        bool tableDone = gameState.GetState("ShotTable");

        string message;

        if (!drinkDone && !tableDone)
        {
            message = "右键拍摄柠檬饮料和餐桌";
        }
        else if (!drinkDone)
        {
            message = "右键拍摄柠檬饮料";
        }
        else if (!tableDone)
        {
            message = "右键拍摄餐桌";
        }
        else if (gameState.GetState("ShotWork"))
        {
            message = "三段素材已收集完成";
        }
        else if (!gameState.GetState("WaterProblemKnown"))
        {
            message = "前往厨房，与女主人交谈";
        }
        else if (!gameState.GetState("RepairKitOwned"))
        {
            message = "前往储物间，寻找维修工具";
        }
        else if (!gameState.GetState("WaterRestored"))
        {
            message = "修复储物间的供水设施";
        }
        else if (!gameState.GetState("WorkAuthorized"))
        {
            message = "返回厨房，与女主人交谈取得拍摄许可";
        }
        else
        {
            message = "右键拍摄厨房的配饮台";
        }

        objectiveText.text = "当前目标：" + message;
    }
}