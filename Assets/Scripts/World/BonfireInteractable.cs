using UnityEngine;

public class BonfireInteractable : MonoBehaviour
{
    public void RequestDestroy()
    {
        Destroy(gameObject);
    }
}
