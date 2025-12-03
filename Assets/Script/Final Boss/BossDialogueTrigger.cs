using UnityEngine;

public class BossDialogueTrigger : MonoBehaviour, IInteractable
{
    private BossInteraction bossInteraction;
    
    public void SetBossInteraction(BossInteraction interaction)
    {
        bossInteraction = interaction;
    }
    
    public bool CanInteract()
    {
        bool canInteract = false;
        if (bossInteraction != null)
        {
            canInteract = bossInteraction.CanInteract();
        }
        
        return canInteract;
    }
    
    public void Interact()
    {
        if (bossInteraction != null)
        {
            bossInteraction.Interact();
        }
    }
    
}
