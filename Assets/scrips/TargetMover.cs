using UnityEngine;

public class TargetMover : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] bool moveInCircles = true;
    [SerializeField] float circleRadius = 2f;

    private Vector3 _initialPosition;

    void Start()
    {
        _initialPosition = transform.position;
    }

    void Update()
    {
        if (moveInCircles)
        {
            // Movimiento circular automático
            float angle = Time.time * moveSpeed;
            transform.position = _initialPosition + new Vector3(
                Mathf.Sin(angle) * circleRadius,
                0f,
                Mathf.Cos(angle) * circleRadius
            );
        }
    }

    // Dibuja un gizmo para ver el radio del círculo
    void OnDrawGizmosSelected()
    {
        if (moveInCircles)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Application.isPlaying ? _initialPosition : transform.position, circleRadius);
        }
    }
}
