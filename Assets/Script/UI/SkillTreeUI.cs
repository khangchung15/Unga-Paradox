using UnityEngine;
using UnityEngine.InputSystem;

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject skillTreePanel;

    [Header("Player References")]
    [SerializeField] private PlayerController playerController; // to disable movement

    [Header("Settings")]
    [SerializeField] private bool pauseGameWhenOpen = true;
    [SerializeField] private Key toggleKey = Key.K; 

    private bool isOpen = false;

    private void Awake()
    {
        // detect the skill tree panel if not assigned in the inspector
        if (skillTreePanel == null)
        {
            var taggedPanel = GameObject.FindWithTag("SkillTreePanel");
            if (taggedPanel != null)
            {
                skillTreePanel = taggedPanel;
            }
            else
            {
                // look for a child named "SkillTreePanel" under any Canvas if not found
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    var panelTransform = canvas.transform.Find("SkillTreePanel");
                    if (panelTransform != null)
                    {
                        skillTreePanel = panelTransform.gameObject;
                    }
                }
            }

            if (skillTreePanel == null)
            {
                Debug.LogWarning("SkillTreeUI: Could not auto-detect skillTreePanel. Assign it in the inspector or tag it 'SkillTreePanel'.");
            }
        }

        // detect the player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("SkillTreeUI: Could not find a PlayerController in the scene.");
            }
        }
    }

    private void Start()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleSkillTree();
        }
    }

    public void ToggleSkillTree()
    {
        isOpen = !isOpen;

        if (skillTreePanel != null)
            skillTreePanel.SetActive(isOpen);

        if (pauseGameWhenOpen)
        {
            Time.timeScale = isOpen ? 0f : 1f;
        }

        if (playerController != null)
        {
            playerController.enabled = !isOpen;
        }
        
    }
}