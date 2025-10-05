using UnityEngine;

public class General_Enemy_Collision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private General_Enemy_Wandering enemyWandering;

    void Awake()
    {   // this transform.parent is needed to call the sibling script in the same parent gameObject
        enemyWandering = transform.parent.GetComponentInChildren<General_Enemy_Wandering>(); // go to the parent, then actually get the component in the parent's children lmao
        if (enemyWandering == null)
            Debug.LogError("General_Enemy_Wandering not found on " + gameObject.name);
    }

    public void OnParentCollision(Collision2D collision) // needs to update collision detection
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log(HelperFuncs.GetOwnerName(transform) + " Collided With Character! Stopping and wandering away.");
            StopAllCoroutines(); // Stop any current wandering coroutine  
            StartCoroutine(enemyWandering.IdleAndSetNewWanderPoint());
        }
       
       
    }
}
