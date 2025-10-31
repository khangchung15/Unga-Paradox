using UnityEngine;

public class WindEffect : MonoBehaviour
{
    [Header("Wind Settings")]
    public float windForce = 10f;
    public float windFrequency = 1f;
    public float windRandomness = 0.5f;
    
    private Rigidbody2D rb;
    private HingeJoint2D hingeJoint;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hingeJoint = GetComponent<HingeJoint2D>();
    }
    
    void FixedUpdate()
    {
        // Create oscillating wind effect
        float windOscillation = Mathf.Sin(Time.time * windFrequency);
        float randomFactor = Random.Range(1f - windRandomness, 1f + windRandomness);
        
        float finalWindForce = windForce * windOscillation * randomFactor;
        
        // Apply torque to simulate wind
        rb.AddTorque(finalWindForce);
    }
}