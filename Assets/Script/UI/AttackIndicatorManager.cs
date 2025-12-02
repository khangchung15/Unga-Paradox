using System.Collections.Generic;
using UnityEngine;

public class AttackIndicatorManager : MonoBehaviour
{
    [Header("Indicator Settings")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform indicatorParent;
    [SerializeField] private float verticalSpacing = 50f;
    [SerializeField] private Vector2 basePosition = new Vector2(0, 272);
    [SerializeField] private int maxIndicators = 2;
    
    [Header("Existing Indicators")]
    [Tooltip("Add any existing attack indicators from the scene here")]
    [SerializeField] private List<AttackIndicator> existingIndicators = new List<AttackIndicator>();
    
    private List<AttackIndicator> indicatorPool = new List<AttackIndicator>();
    private Dictionary<object, AttackIndicator> activeIndicators = new Dictionary<object, AttackIndicator>();
    
    private void Awake()
    {
        if (indicatorParent == null)
        {
            indicatorParent = transform.parent;
        }

        
        foreach (AttackIndicator existing in existingIndicators)
        {
            if (existing != null)
            {
                indicatorPool.Add(existing);
                existing.Hide();
            }
        }
    }
    
    public AttackIndicator RequestIndicator(object requester)
    {
        if (activeIndicators.ContainsKey(requester))
        {
            return activeIndicators[requester];
        }
        
        if (activeIndicators.Count >= maxIndicators)
        {
            return null;
        }
        
        AttackIndicator indicator = GetAvailableIndicator();
        if (indicator == null)
        {
            return null;
        }
        
        activeIndicators[requester] = indicator;
        UpdateIndicatorPositions();
        indicator.Show();
        indicator.ResetIndicator();
        
        return indicator;
    }
    
    public void ReleaseIndicator(object requester)
    {
        if (activeIndicators.TryGetValue(requester, out AttackIndicator indicator))
        {
            indicator.Hide();
            activeIndicators.Remove(requester);
            UpdateIndicatorPositions();
        }
    }
    
    private AttackIndicator GetAvailableIndicator()
    {
        foreach (AttackIndicator indicator in indicatorPool)
        {
            if (!activeIndicators.ContainsValue(indicator))
            {
                return indicator;
            }
        }
        
        if (indicatorPool.Count < maxIndicators && indicatorPrefab != null)
        {
            GameObject newIndicatorObj = Instantiate(indicatorPrefab, indicatorParent);
            AttackIndicator newIndicator = newIndicatorObj.GetComponent<AttackIndicator>();
            
            if (newIndicator != null)
            {
                indicatorPool.Add(newIndicator);
                newIndicator.Hide();
                return newIndicator;
            }
        }
        
        return null;
    }
    
    private void UpdateIndicatorPositions()
    {
        int index = 0;
        foreach (var kvp in activeIndicators)
        {
            AttackIndicator indicator = kvp.Value;
            RectTransform rectTransform = indicator.GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                Vector2 newPosition = basePosition + new Vector2(0, -verticalSpacing * index);
                rectTransform.anchoredPosition = newPosition;
                index++;
            }
        }
    }
    
    public int GetActiveIndicatorCount()
    {
        return activeIndicators.Count;
    }
}
