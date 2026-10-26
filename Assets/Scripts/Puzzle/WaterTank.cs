using UnityEngine;

public class WaterTank : MonoBehaviour
{
    public string Use()
    {
        if (WaterRepairPuzzle.Instance == null)
        {
            return "修复面板不存在。";
        }

        return WaterRepairPuzzle.Instance.TryOpen();
    }
}
