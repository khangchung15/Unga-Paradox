using System.Collections;
using UnityEngine;

public class RockRainRandom : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform player;            // player transform
    [SerializeField] GameObject rockPrefab;       // FallingRock prefab
    [SerializeField] GameObject dustPrefab;       // optional warning fx

    [Header("Spawn Region (local offsets from this object)")]
    [SerializeField] float xRangeLeft  = -4f;     // left offset
    [SerializeField] float xRangeRight =  4f;     // right offset
    [SerializeField] float yOffset     =  5f;     // height ABOVE this object

    [Header("Timing")]
    [SerializeField] Vector2 intervalRange = new Vector2(0.9f, 1.6f);  // random delay between bursts
    [SerializeField] float   warningTime   = 0.45f;                    // dust before drop
    [SerializeField] Vector2Int rocksPerWave = new Vector2Int(1, 2);   // rocks per burst

    [Header("Targeting")]
    [SerializeField, Range(0f,1f)] float aimAtPlayerChance = 0.6f;     // bias drops over player
    [SerializeField] float aimJitter = 1.2f;                           // ±X jitter when aiming

    Coroutine loop;
    bool enabledRain;

    void Start()
    {
        EnableRain(true); 
    }


    public void EnableRain(bool on)
    {
        if (on && !enabledRain)
        {
            enabledRain = true;
            loop = StartCoroutine(RainLoop());
        }
        else if (!on && enabledRain)
        {
            enabledRain = false;
            if (loop != null) StopCoroutine(loop);
        }
    }

    void OnDisable() { EnableRain(false); } // safety

    IEnumerator RainLoop()
    {
        // Safety clamps
        if (xRangeLeft > xRangeRight) { var t = xRangeLeft; xRangeLeft = xRangeRight; xRangeRight = t; }
        if (intervalRange.x < 0.05f) intervalRange.x = 0.05f;
        if (intervalRange.y < intervalRange.x) intervalRange.y = intervalRange.x;

        while (enabledRain)
        {
            int count = Random.Range(rocksPerWave.x, rocksPerWave.y + 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 basePos = transform.position;

                // Decide X
                float x;
                if (player && Random.value < aimAtPlayerChance)
                {
                    float aimedX = player.position.x + Random.Range(-aimJitter, aimJitter);
                    x = Mathf.Clamp(aimedX, basePos.x + xRangeLeft, basePos.x + xRangeRight);
                }
                else
                {
                    x = basePos.x + Random.Range(xRangeLeft, xRangeRight);
                }

                // Final spawn position
                Vector3 pos = new Vector3(x, basePos.y + yOffset, 0f);

                // Dust warning
                if (dustPrefab)
                {
                    var dust = Instantiate(dustPrefab, pos, Quaternion.identity);
                    Destroy(dust, warningTime + 0.6f);
                }

                // Slightly jitter the warning delay so multiple rocks don't drop at the exact same frame
                StartCoroutine(DropAfter(pos, warningTime + Random.Range(-0.05f, 0.05f)));
            }

            yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y));
        }
    }

    IEnumerator DropAfter(Vector3 pos, float delay)
    {
        yield return new WaitForSeconds(delay);
        var go = Instantiate(rockPrefab, pos, Quaternion.identity);
        var rock = go.GetComponent<RockDamage>();
        if (rock) { rock.ResetRock(); rock.Drop(); }
    }

    // Visualize the moving spawn strip in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.35f);
        Vector3 basePos = transform.position;
        Vector3 left  = new Vector3(basePos.x + xRangeLeft,  basePos.y + yOffset, 0f);
        Vector3 right = new Vector3(basePos.x + xRangeRight, basePos.y + yOffset, 0f);
        Gizmos.DrawLine(left, right);
        Gizmos.DrawCube((left + right) * 0.5f, new Vector3(Mathf.Abs(xRangeRight - xRangeLeft), 0.2f, 0.2f));
    }
}
