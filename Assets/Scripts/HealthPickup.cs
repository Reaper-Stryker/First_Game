using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthRestore = 20;
    public Vector3 spinRotationSpeed = new Vector3(0, 180, 0);

    [Header("Collision Settings")]
    public LayerMask targetLayer = Physics2D.AllLayers;

    private void Update()
    {
        transform.Rotate(spinRotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check layer match using bitwise operation
        if ((targetLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        Damageable damageable = collision.GetComponent<Damageable>();
        if (!damageable) return;

        Debug.Log($"Health pickup triggered by {collision.name}");

        bool wasHealed = damageable.Heal(healthRestore);
        Debug.Log($"Heal attempt result: {wasHealed}");

        if (wasHealed)
        {
            Debug.Log("Destroying health pickup");
            Destroy(gameObject);
        }
    }
}
