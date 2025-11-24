using UnityEngine;
using UnityEngine.UI;

public class AttackIndicator : MonoBehaviour
{
    [Header("Fill References")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectMask2D mask;
    [SerializeField] private Image fillImage;
    
    [Header("Settings")]
    [SerializeField] private Color chargeColor = Color.yellow;
    [SerializeField] private Color readyColor = Color.red;
    
    private float maxRightMask;
    private float initialRightMask;
    private bool isInitialized = false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        if (barRect == null)
        {
            barRect = transform.Find("HpBarMask/HpBarFill")?.GetComponent<RectTransform>();
        }

        if (mask == null)
        {
            mask = transform.Find("HpBarMask")?.GetComponent<RectMask2D>();
        }

        if (fillImage == null && barRect != null)
        {
            fillImage = barRect.GetComponent<Image>();
        }

        if (barRect != null && mask != null)
        {
            maxRightMask = barRect.rect.width - mask.padding.x - mask.padding.z;
            initialRightMask = mask.padding.z;
        }

        isInitialized = true;
        
        SetFillAmount(0f);
    }

    public void SetFillAmount(float normalizedValue)
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (barRect == null || mask == null) return;

        normalizedValue = Mathf.Clamp01(normalizedValue);
        
        float targetWidth = normalizedValue * maxRightMask;
        float newRightMask = maxRightMask - targetWidth + initialRightMask;
        
        Vector4 padding = mask.padding;
        padding.z = newRightMask;
        mask.padding = padding;

        if (fillImage != null)
        {
            fillImage.color = normalizedValue >= 1f ? readyColor : chargeColor;
        }
    }

    public void ResetIndicator()
    {
        SetFillAmount(0f);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
