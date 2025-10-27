using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private string playerTag = "Player";
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            timeline.Play();
            
            // Optional: Disable the trigger so it only plays once
            GetComponent<Collider2D>().enabled = false;
        }
    }
}