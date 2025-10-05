using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

public class General_Enemy_PathFinding : MonoBehaviour
{
    private AIPath aiPath;
    private General_Enemy_Wandering enemyWandering;
    private void Awake()
    {
        aiPath = GetComponent<AIPath>();
        enemyWandering = GetComponentInChildren<General_Enemy_Wandering>();
    }

    private void Update()
    {
        aiPath.destination = enemyWandering.target;
        Debug.Log(HelperFuncs.GetOwnerName(transform) + " Pathfinding to: " + aiPath.destination);
    }
}
