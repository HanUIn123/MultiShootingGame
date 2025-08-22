using UnityEngine;

public class MonsterBullet : MonoBehaviour
{
    public float bulletSpeed = 5f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.down * bulletSpeed;

        Destroy(gameObject, 3f); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject); 
            //Debug.Log("플레이어 피격");
        }
    }
}
