using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarFuture : MonoBehaviour
{
    [SerializeField]
    private HealthFuture _health;

    [SerializeField]
    private RectTransform _barRect;

    [SerializeField]
    private RectMask2D _mask;
    
    [SerializeField]
    private TMP_Text _hpIndicator;
    
    private float _maxRightMask;
    private float _initialRightMask;

    private void Start()
    {
        if (_health == null)
        {
            Debug.LogError("Health Bar from player needs to be assigned. Check PlayerUI canvas in Inspector.");
        }
        _maxRightMask = _barRect.rect.width - _mask.padding.x - _mask.padding.z;
        _hpIndicator.SetText($"{_health.currentHealth}/{_health.maxHealth}");
        _initialRightMask = _mask.padding.z;
    }

    public void SetValue(int newValue)
    {
        newValue = Mathf.Max(0, newValue);
        
        if (newValue <= 0)
        {
            var padding = _mask.padding;
            padding.z = _barRect.rect.width;
            _mask.padding = padding;
            _hpIndicator.SetText($"0/{_health.maxHealth}");
        }
        else
        {
            var targetWidth = newValue * _maxRightMask / _health.maxHealth;
            var newRightMask = _maxRightMask - targetWidth + _initialRightMask;
            var padding = _mask.padding;
            padding.z = newRightMask;
            _mask.padding = padding;
            _hpIndicator.SetText($"{newValue}/{_health.maxHealth}");
        }
    }
}
