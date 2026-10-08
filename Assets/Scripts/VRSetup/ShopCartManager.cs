using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShopCartManager
/// 
/// Single-instance manager for the showroom shopping cart.
/// Tracks added items, maintains the total cart value, and provides
/// events and callbacks for UI components to update live.
/// </summary>
public class ShopCartManager : MonoBehaviour
{
    public static ShopCartManager Instance { get; private set; }

    /// <summary>
    /// Represents an item entry in the shopping cart.
    /// </summary>
    [Serializable]
    public struct CartItem
    {
        public string itemName;
        public float price;

        public CartItem(string name, float itemPrice)
        {
            itemName = name;
            price = itemPrice;
        }
    }

    // Active items currently in the cart: ItemName -> Price
    private readonly Dictionary<string, float> m_ItemsInCart = new Dictionary<string, float>();

    /// <summary>
    /// Current running total price of all items in the cart.
    /// </summary>
    public float TotalPrice { get; private set; } = 0f;

    /// <summary>
    /// Number of distinct items currently in the cart.
    /// </summary>
    public int ItemCount => m_ItemsInCart.Count;

    /// <summary>
    /// Event fired whenever an item is added, removed, or cleared, passing the updated total price.
    /// </summary>
    public event Action<float> OnCartUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Checks if a specific item is already present in the cart.
    /// </summary>
    public bool IsInCart(string itemName)
    {
        return m_ItemsInCart.ContainsKey(itemName);
    }

    /// <summary>
    /// Adds an item to the shopping cart.
    /// </summary>
    public void AddToCart(string itemName, float price)
    {
        if (m_ItemsInCart.ContainsKey(itemName))
        {
            Debug.LogWarning($"[ShopCartManager] '{itemName}' is already in the cart.");
            return;
        }

        m_ItemsInCart.Add(itemName, price);
        TotalPrice += price;

        Debug.Log($"[ShopCartManager] Added '{itemName}' (${price:F2}) to cart. Total: ${TotalPrice:F2} ({m_ItemsInCart.Count} item(s))");
        OnCartUpdated?.Invoke(TotalPrice);
    }

    /// <summary>
    /// Removes an item from the shopping cart.
    /// </summary>
    public void RemoveFromCart(string itemName)
    {
        if (!m_ItemsInCart.TryGetValue(itemName, out float price))
        {
            Debug.LogWarning($"[ShopCartManager] Cannot remove '{itemName}': item not in cart.");
            return;
        }

        m_ItemsInCart.Remove(itemName);
        TotalPrice = Mathf.Max(0f, TotalPrice - price);

        Debug.Log($"[ShopCartManager] Removed '{itemName}' (${price:F2}) from cart. Total: ${TotalPrice:F2} ({m_ItemsInCart.Count} item(s))");
        OnCartUpdated?.Invoke(TotalPrice);
    }

    /// <summary>
    /// Returns a list of all items currently in the cart.
    /// </summary>
    public List<CartItem> GetCartItems()
    {
        var list = new List<CartItem>();
        foreach (var kvp in m_ItemsInCart)
        {
            list.Add(new CartItem(kvp.Key, kvp.Value));
        }
        return list;
    }

    /// <summary>
    /// Clears all items from the shopping cart.
    /// </summary>
    public void ClearCart()
    {
        m_ItemsInCart.Clear();
        TotalPrice = 0f;
        Debug.Log("[ShopCartManager] Cart cleared.");
        OnCartUpdated?.Invoke(TotalPrice);
    }
}

/// <summary>
/// StationCartButton
/// 
/// Attached to each station's small sign on the right side of the cupboard.
/// Holds exposed, inspector-editable item name and price fields,
/// and handles the toggle behavior between "Add to Cart" and "Remove from Cart".
/// </summary>
public class StationCartButton : MonoBehaviour
{
    [Header("Item Properties (Inspect directly in Inspector)")]
    [Tooltip("Exact display name of this furniture piece.")]
    [SerializeField] private string m_ItemName = "Furniture Piece";

    [Tooltip("Exact price in dollars as a numeric value (no string parsing).")]
    [SerializeField] private float m_Price = 500f;

    [Header("UI References")]
    [Tooltip("The Button component on this GameObject.")]
    [SerializeField] private Button m_Button;

    [Tooltip("The TextMeshProUGUI label displaying the button text.")]
    [SerializeField] private TextMeshProUGUI m_LabelText;

    [Header("Visual Feedback")]
    [Tooltip("Button background color when in 'Add to Cart' state.")]
    [SerializeField] private Color m_AddColor = new Color(0.18f, 0.55f, 0.34f); // Forest green

    [Tooltip("Button background color when in 'Remove from Cart' state.")]
    [SerializeField] private Color m_RemoveColor = new Color(0.75f, 0.22f, 0.22f); // Crimson red

    public string ItemName => m_ItemName;
    public float Price => m_Price;

    private void Awake()
    {
        if (m_Button == null)
            m_Button = GetComponent<Button>();

        if (m_LabelText == null)
            m_LabelText = GetComponentInChildren<TextMeshProUGUI>();

        if (m_LabelText != null)
        {
            m_LabelText.enableAutoSizing = true;
            m_LabelText.fontSizeMin = 14f;
            m_LabelText.fontSizeMax = 22f;
            m_LabelText.textWrappingMode = TextWrappingModes.NoWrap;
            m_LabelText.alignment = TextAlignmentOptions.Center;
        }
    }

    private void Start()
    {
        if (m_Button != null)
        {
            m_Button.onClick.AddListener(OnButtonClicked);
        }
        if (ShopCartManager.Instance != null)
        {
            ShopCartManager.Instance.OnCartUpdated += OnCartStateChanged;
        }
        UpdateButtonVisuals();
    }

    private void OnDestroy()
    {
        if (m_Button != null)
        {
            m_Button.onClick.RemoveListener(OnButtonClicked);
        }
        if (ShopCartManager.Instance != null)
        {
            ShopCartManager.Instance.OnCartUpdated -= OnCartStateChanged;
        }
    }

    private void OnCartStateChanged(float total)
    {
        UpdateButtonVisuals();
    }

    /// <summary>
    /// Configures the item details from code or editor scripts.
    /// </summary>
    public void SetItemDetails(string itemName, float price)
    {
        m_ItemName = itemName;
        m_Price = price;
    }

    /// <summary>
    /// Invoked whenever the player clicks the button (via ray pointer or mouse).
    /// </summary>
    public void OnButtonClicked()
    {
        if (ShopCartManager.Instance == null)
        {
            Debug.LogError("[StationCartButton] ShopCartManager instance not found in scene!");
            return;
        }

        if (!ShopCartManager.Instance.IsInCart(m_ItemName))
        {
            // Add to cart
            ShopCartManager.Instance.AddToCart(m_ItemName, m_Price);
        }
        else
        {
            // Remove from cart
            ShopCartManager.Instance.RemoveFromCart(m_ItemName);
        }

        UpdateButtonVisuals();
    }

    /// <summary>
    /// Synchronizes button label and color with current cart status.
    /// </summary>
    public void UpdateButtonVisuals()
    {
        bool inCart = ShopCartManager.Instance != null && ShopCartManager.Instance.IsInCart(m_ItemName);

        if (m_LabelText != null)
        {
            m_LabelText.text = inCart ? "Remove from Cart" : "Add to Cart";
        }

        if (m_Button != null && m_Button.targetGraphic is Image img)
        {
            img.color = inCart ? m_RemoveColor : m_AddColor;
        }
    }
}

/// <summary>
/// CartSummaryUI
/// 
/// Attached to the Cart Summary world-space canvas beside the entrance.
/// Displays the live running total, list of cart items, and handles the Buy confirmation.
/// </summary>
public class CartSummaryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text displaying the list of items in the cart.")]
    [SerializeField] private TextMeshProUGUI m_ItemsListText;

    [Tooltip("Text displaying the running total price.")]
    [SerializeField] private TextMeshProUGUI m_TotalText;

    [Tooltip("Text displaying on-screen order confirmation messages.")]
    [SerializeField] private TextMeshProUGUI m_ConfirmationText;

    [Tooltip("The Buy button component.")]
    [SerializeField] private Button m_BuyButton;

    private void Start()
    {
        if (m_BuyButton != null)
        {
            m_BuyButton.onClick.AddListener(OnBuyClicked);
        }

        if (ShopCartManager.Instance != null)
        {
            ShopCartManager.Instance.OnCartUpdated += UpdateSummaryDisplay;
        }

        if (m_ConfirmationText != null)
        {
            m_ConfirmationText.gameObject.SetActive(false);
        }

        UpdateSummaryDisplay(ShopCartManager.Instance != null ? ShopCartManager.Instance.TotalPrice : 0f);
    }

    private void OnDestroy()
    {
        if (m_BuyButton != null)
        {
            m_BuyButton.onClick.RemoveListener(OnBuyClicked);
        }

        if (ShopCartManager.Instance != null)
        {
            ShopCartManager.Instance.OnCartUpdated -= UpdateSummaryDisplay;
        }
    }

    /// <summary>
    /// Updates the item list and total price display whenever the cart changes.
    /// </summary>
    public void UpdateSummaryDisplay(float total)
    {
        if (ShopCartManager.Instance == null) return;

        // Hide old confirmation message when cart changes
        if (m_ConfirmationText != null)
        {
            m_ConfirmationText.gameObject.SetActive(false);
        }

        // Update items list
        if (m_ItemsListText != null)
        {
            var items = ShopCartManager.Instance.GetCartItems();
            if (items.Count == 0)
            {
                m_ItemsListText.text = "<color=#9E9E9E>Cart is currently empty.\nVisit stations to add furniture.</color>";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in items)
                {
                    sb.AppendLine($"• {item.itemName}  <color=#81C784>${item.price:F2}</color>");
                }
                m_ItemsListText.text = sb.ToString();
            }
        }

        // Update Total
        if (m_TotalText != null)
        {
            m_TotalText.text = $"Total: ${total:F2}";
        }
    }

    /// <summary>
    /// Invoked when the Buy button is clicked.
    /// Displays on-screen order confirmation and clears the cart.
    /// </summary>
    public void OnBuyClicked()
    {
        if (ShopCartManager.Instance == null) return;

        float total = ShopCartManager.Instance.TotalPrice;
        if (total <= 0f)
        {
            if (m_ConfirmationText != null)
            {
                m_ConfirmationText.gameObject.SetActive(true);
                m_ConfirmationText.text = "<color=#FFA726>Cart is empty! Please add items first.</color>";
            }
            return;
        }

        // Display on-screen confirmation
        if (m_ConfirmationText != null)
        {
            m_ConfirmationText.gameObject.SetActive(true);
            m_ConfirmationText.text = $"<color=#81C784>✔ Order placed successfully!\nTotal Paid: ${total:F2}</color>";
        }

        Debug.Log($"[CartSummaryUI] Order confirmed! Total: ${total:F2}");

        // Clear cart
        ShopCartManager.Instance.ClearCart();

        // Refresh all station buttons so they revert back to 'Add to Cart'
        var stationButtons = FindObjectsByType<StationCartButton>(FindObjectsSortMode.None);
        foreach (var btn in stationButtons)
        {
            btn.UpdateButtonVisuals();
        }
    }
}
