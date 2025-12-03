using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CreditsScroller : MonoBehaviour
{
    [Header("Credits Content")]
    [TextArea(20, 50)]
    [SerializeField] private string creditsText = "Game Title\n\nDirector\nYour Name\n\nProgramming\nYour Name\n\nArt\nYour Name\n\nMusic\nYour Name\n\nSpecial Thanks\nUnity Technologies";
    
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 20f;
    [SerializeField] private float lineSpacing = 2f;
    [SerializeField] private float startDelay = 1f;
    
    [Header("Fade Zone Settings")]
    [SerializeField] private float fadeZoneTop = 5f;
    [SerializeField] private float fadeZoneBottom = -5f;
    [SerializeField] private float fadeDistance = 2f;
    
    [Header("Text Settings")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private float fontSize = 3f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private TextAlignmentOptions alignment = TextAlignmentOptions.Center;
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private string sortingLayerName = "Default";
    
    [Header("Optional")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool loopCredits = false;
    [SerializeField] private float restartDelay = 3f;
    
    private List<TextMeshPro> creditLines = new List<TextMeshPro>();
    private bool isScrolling = false;
    private float scrollTimer = 0f;
    private GameObject creditsContainer;

    void Start()
    {
        if (autoStart)
        {
            Invoke(nameof(StartCredits), startDelay);
        }
    }

    public void StartCredits()
    {
        CreateCreditLines();
        isScrolling = true;
        scrollTimer = 0f;
    }

    public void StopCredits()
    {
        isScrolling = false;
        ClearCredits();
    }

    void Update()
    {
        if (!isScrolling) return;

        scrollTimer += Time.deltaTime;

        bool allLinesOutOfView = true;

        foreach (TextMeshPro textLine in creditLines)
        {
            if (textLine == null) continue;

            textLine.transform.position += Vector3.up * scrollSpeed * Time.deltaTime;

            float yPos = textLine.transform.position.y;
            
            UpdateTextAlpha(textLine, yPos);

            if (yPos < fadeZoneTop + fadeDistance)
            {
                allLinesOutOfView = false;
            }
        }

        if (allLinesOutOfView && loopCredits)
        {
            ClearCredits();
            Invoke(nameof(StartCredits), restartDelay);
        }
        else if (allLinesOutOfView)
        {
            isScrolling = false;
        }
    }

    private void CreateCreditLines()
    {
        creditsContainer = new GameObject("Credits Container");
        creditsContainer.transform.SetParent(transform);
        creditsContainer.transform.localPosition = Vector3.zero;

        string[] lines = creditsText.Split('\n');
        float currentY = fadeZoneBottom - fadeDistance - 2f;

        for (int i = 0; i < lines.Length; i++)
        {
            GameObject lineObj = new GameObject($"Credit Line {i}");
            lineObj.transform.SetParent(creditsContainer.transform);
            lineObj.transform.position = new Vector3(transform.position.x, currentY, transform.position.z);

            TextMeshPro tmp = lineObj.AddComponent<TextMeshPro>();
            tmp.text = lines[i];
            tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.color = textColor;
            tmp.alignment = alignment;
            
            tmp.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            tmp.GetComponent<MeshRenderer>().sortingLayerName = sortingLayerName;

            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                currentY -= lineSpacing * 0.5f;
            }
            else
            {
                currentY -= lineSpacing;
            }

            Color initialColor = textColor;
            initialColor.a = 0f;
            tmp.color = initialColor;

            creditLines.Add(tmp);
        }
    }

    private void UpdateTextAlpha(TextMeshPro textLine, float yPos)
    {
        float alpha = 1f;

        float bottomFadeStart = fadeZoneBottom;
        float bottomFadeEnd = fadeZoneBottom + fadeDistance;
        
        if (yPos < bottomFadeEnd)
        {
            if (yPos <= bottomFadeStart)
            {
                alpha = 0f;
            }
            else
            {
                alpha = Mathf.InverseLerp(bottomFadeStart, bottomFadeEnd, yPos);
            }
        }

        float topFadeStart = fadeZoneTop;
        float topFadeEnd = fadeZoneTop - fadeDistance;
        
        if (yPos > topFadeEnd)
        {
            if (yPos >= topFadeStart)
            {
                alpha = 0f;
            }
            else
            {
                alpha = Mathf.InverseLerp(topFadeStart, topFadeEnd, yPos);
            }
        }

        Color color = textLine.color;
        color.a = alpha;
        textLine.color = color;
    }

    private void ClearCredits()
    {
        foreach (TextMeshPro tmp in creditLines)
        {
            if (tmp != null)
            {
                Destroy(tmp.gameObject);
            }
        }
        
        creditLines.Clear();

        if (creditsContainer != null)
        {
            Destroy(creditsContainer);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position;
        
        float boxHeight = fadeZoneTop - fadeZoneBottom;
        Vector3 boxCenter = new Vector3(center.x, (fadeZoneTop + fadeZoneBottom) / 2f, center.z);
        Gizmos.DrawWireCube(boxCenter, new Vector3(10f, boxHeight, 0.1f));
        
        Gizmos.color = Color.yellow;
        Vector3 topFadeStart = new Vector3(center.x, fadeZoneTop, center.z);
        Vector3 topFadeEnd = new Vector3(center.x, fadeZoneTop - fadeDistance, center.z);
        Gizmos.DrawLine(topFadeStart + Vector3.left * 5f, topFadeStart + Vector3.right * 5f);
        Gizmos.DrawLine(topFadeEnd + Vector3.left * 5f, topFadeEnd + Vector3.right * 5f);
        
        Vector3 bottomFadeStart = new Vector3(center.x, fadeZoneBottom, center.z);
        Vector3 bottomFadeEnd = new Vector3(center.x, fadeZoneBottom + fadeDistance, center.z);
        Gizmos.DrawLine(bottomFadeStart + Vector3.left * 5f, bottomFadeStart + Vector3.right * 5f);
        Gizmos.DrawLine(bottomFadeEnd + Vector3.left * 5f, bottomFadeEnd + Vector3.right * 5f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, new Vector3(10f, boxHeight - (fadeDistance * 2), 0.1f));
    }

    void OnDestroy()
    {
        ClearCredits();
    }
}
