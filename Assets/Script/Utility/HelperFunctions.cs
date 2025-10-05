using UnityEngine;

public static class HelperFuncs
{
    public static string GetOwnerName(Transform transform)
    {
        if (transform.parent != null)
        {
            return transform.parent.gameObject.name;
        }
        else
        {
            return transform.gameObject.name;
        }
    }
}
