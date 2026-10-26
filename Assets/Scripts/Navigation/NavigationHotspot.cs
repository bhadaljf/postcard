using UnityEngine;

public class NavigationHotspot : MonoBehaviour
{
    [SerializeField] private string targetRoomId;
    [SerializeField] private string displayName;

    public string TargetRoomId => targetRoomId;
    public string DisplayName => displayName;
}
