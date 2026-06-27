using UnityEngine;

/// <summary>
/// 相机固定 —— 确保相机始终锁定在原点，不随玩家移动
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedCamera : MonoBehaviour
{
    private Vector3 fixedPosition;

    void Start()
    {
        fixedPosition = transform.position;
    }

    void LateUpdate()
    {
        // 每帧强制将相机拉回固定位置，防止任何脚本意外移动相机
        if (transform.position != fixedPosition)
            transform.position = fixedPosition;
    }
}
