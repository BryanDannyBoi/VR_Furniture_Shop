using UnityEngine;
using TMPro;

/// <summary>
/// CartHUD
/// 
/// Real-time HUD badge and Wrist tracker displaying live cart total and item count.
/// Subscribes to ShopCartManager.OnCartUpdated so the player can always glance
/// and check the sum wherever they are in the showroom.
/// </summary>
public class CartHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_Text;

    private void Awake()
    {
        if (m_Text == null)
            m_Text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (ShopCartManager.Instance != null)
        {
            ShopCartManager.Instance.OnCartUpdated += OnCartUpdated;
            UpdateDisplay(ShopCartManager.Instance.TotalPrice, ShopCartManager.Instance.ItemCount);
        }
        else
        {
            UpdateDisplay(0f, 0);
        }
    }

    private void OnDestroy()
    {
        if (ShopCartManager.Instance != null)
        {
            ShopCartManager.Instance.OnCartUpdated -= OnCartUpdated;
        }
    }

    private void OnCartUpdated(float total)
    {
        int count = ShopCartManager.Instance != null ? ShopCartManager.Instance.ItemCount : 0;
        UpdateDisplay(total, count);
    }

    public void UpdateDisplay(float total, int count)
    {
        if (m_Text == null) return;

        if (count == 0)
        {
            m_Text.text = "<b>🛒 Cart:</b> <color=#B0BEC5>$0</color> <size=80%>(Empty)</size>";
        }
        else
        {
            string itemStr = count == 1 ? "item" : "items";
            m_Text.text = $"<b>🛒 Cart:</b> <color=#66BB6A><b>${total:F0}</b></color> <size=85%>({count} {itemStr})</size>";
        }
    }
}
