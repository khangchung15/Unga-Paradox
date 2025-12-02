using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarLatest : MonoBehaviour
{
    [SerializeField]
    private BossHealthLatest _health;

    [SerializeField]
    private RectTransform _barRect;

    [SerializeField]
    private RectMask2D _mask;
    
    public Slider mSlider;

    private void Start()
    {
        if (_health == null)
        {
            Debug.LogError("Health Bar from Boss needs to be assigned. Check the canvas in the inspector.");
        }
    }

    public void SetValue(int newValue)
    {
        if (newValue == 0)
        {
            mSlider.value = 0;
        }
        mSlider.value = newValue;
    }
}