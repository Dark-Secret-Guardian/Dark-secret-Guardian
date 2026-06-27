using UnityEngine;

/// <summary>
/// 玩家移动 —— WASD/方向键移动，根据方向自动切换角色 Sprite
/// 需要在 Inspector 中绑定 6 张方向图
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;  // 移动速度

    [Header("方向 Sprite")]
    public Sprite spriteFront;   // 正面（朝下）
    public Sprite spriteBack;    // 背面（朝上）
    public Sprite spriteLeft;    // 左立
    public Sprite spriteRight;   // 右立
    public Sprite spriteLeftWalk; // 左走
    public Sprite spriteRightWalk;// 右走

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;

    // 走路动画参数
    private float walkTimer = 0f;
    private float walkInterval = 0.15f;  // 切换间隔（秒）
    private bool isWalking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 读取输入
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // 更新方向图
        UpdateSprite();
    }

    void FixedUpdate()
    {
        rb.velocity = moveInput * moveSpeed;
    }

    /// <summary>
    /// 根据移动方向切换 Sprite
    /// 左右方向：站立/走路交替（模拟步行动画）
    /// 上下方向：切换正/背面图
    /// </summary>
    void UpdateSprite()
    {
        if (sr == null) return;

        bool moving = moveInput.magnitude > 0.1f;

        // 左右移动优先
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            if (moveInput.x < 0)
            {
                // 向左
                if (moving)
                {
                    // 走路动画：左立 ↔ 左走
                    walkTimer += Time.deltaTime;
                    if (walkTimer >= walkInterval)
                    {
                        sr.sprite = sr.sprite == spriteLeft ? spriteLeftWalk : spriteLeft;
                        walkTimer = 0f;
                    }
                }
                else
                {
                    sr.sprite = spriteLeft;
                }
            }
            else
            {
                // 向右
                if (moving)
                {
                    walkTimer += Time.deltaTime;
                    if (walkTimer >= walkInterval)
                    {
                        sr.sprite = sr.sprite == spriteRight ? spriteRightWalk : spriteRight;
                        walkTimer = 0f;
                    }
                }
                else
                {
                    sr.sprite = spriteRight;
                }
            }
        }
        else if (moveInput.y > 0.1f)
        {
            // 向上：背面
            sr.sprite = spriteBack;
        }
        else if (moveInput.y < -0.1f)
        {
            // 向下：正面
            sr.sprite = spriteFront;
        }
        // 停止时保持当前图
    }
}
