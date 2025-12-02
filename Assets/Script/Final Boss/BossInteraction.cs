using UnityEngine;

public class BossInteraction : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private NPC npcComponent;
    [SerializeField] private GameObject dialogueTriggerObject;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defeatedSprite;
    
    private Collider2D interactionCollider;
    private BossDialogueTrigger dialogueTrigger;
    private bool isInteractable = false;
    
    private void Awake()
    {
        if (npcComponent == null)
        {
            npcComponent = GetComponent<NPC>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (dialogueTriggerObject != null)
        {
            interactionCollider = dialogueTriggerObject.GetComponent<Collider2D>();
            dialogueTrigger = dialogueTriggerObject.GetComponent<BossDialogueTrigger>();
        }
        
        if (interactionCollider != null)
        {
            interactionCollider.enabled = false;
        }
        
        if (dialogueTriggerObject != null)
        {
            dialogueTriggerObject.SetActive(false);
        }
    }
    
    public void SetupDefeatedBoss()
    {
        isInteractable = true;
        
        if (dialogueTriggerObject != null)
        {
            dialogueTriggerObject.SetActive(true);
        }
        
        if (dialogueTrigger != null)
        {
            dialogueTrigger.SetBossInteraction(this);
        }
        
        if (interactionCollider != null)
        {
            interactionCollider.enabled = true;
        }
        
        if (spriteRenderer != null && defeatedSprite != null)
        {
            spriteRenderer.sprite = defeatedSprite;
        }
        
    }
    
    public bool CanInteract()
    {
        bool canInteract = isInteractable && npcComponent != null && npcComponent.CanInteract();
        return canInteract;
    }
    
    public void Interact()
    {
        if (npcComponent != null && isInteractable)
        {
            npcComponent.Interact();
        }
    }
}
