using UnityEngine;

public class SteeringBehaviorsTransform : MonoBehaviour
{
    public enum SteeringType
    {
        Seek,
        // 1
        Flee,         // 2
        Evade,        // 3
        Arrive,       // 4
        Pursuit,      // 5
        Wander        // 6
    }

    [Header("Settings")]
    public SteeringType currentSteering;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float rotationSpeed = 8f;
    public float arrivalRadius = 0.5f;

    [Header("References")]
    public Transform target;

    private Vector3 _velocity;
    private Vector3 _targetPreviousPosition;

    [Header("Evade Settings")]
    [SerializeField] float evadePredictionMultiplier = 1.2f;

    [Header("Arrive Settings")]
    [SerializeField] float slowingRadius = 5f;  // Radio de desaceleración
    [SerializeField] float stopThreshold = 0.1f;
    void Update()
    {
        HandleInput();
        ApplySteeringBehavior();
        ApplyMovement();
        ApplyRotation();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentSteering = SteeringType.Seek;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentSteering = SteeringType.Flee;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentSteering = SteeringType.Evade; // Tecla 3
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentSteering = SteeringType.Arrive;

        if (Input.GetKeyDown(KeyCode.Alpha5)) currentSteering = SteeringType.Pursuit;
    }

    void ApplySteeringBehavior()
    {
        switch (currentSteering)
        {
            case SteeringType.Seek:
                _velocity += CalculateSeek();
                break;

            case SteeringType.Flee:
                _velocity += CalculateFlee();
                break;

            case SteeringType.Pursuit:
                _velocity += CalculatePursuit();
                break;
            case SteeringType.Evade:
                _velocity += CalculateEvade();
                break;
            case SteeringType.Arrive:
                _velocity += CalculateArrive();
                break;
        }

        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
    }

    Vector3 CalculateArrive()
    {
        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        // Si está lo suficientemente cerca, detenerse
        if (distance < stopThreshold)
        {
            _velocity = Vector3.zero;
            return Vector3.zero;
        }

        // Calcular velocidad deseada según la distancia
        float desiredSpeed = maxSpeed;
        if (distance <= slowingRadius)
        {
            // Reducir velocidad dentro del radio de desaceleración
            desiredSpeed = maxSpeed * (distance / slowingRadius);
        }

        Vector3 desiredVelocity = toTarget.normalized * desiredSpeed;
        Vector3 steering = desiredVelocity - _velocity;

        return Vector3.ClampMagnitude(steering, maxForce * Time.deltaTime);
    }
    Vector3 CalculateEvade()
    {
        // 1. Calcular velocidad del target
        Vector3 targetVelocity = (target.position - _targetPreviousPosition) / Time.deltaTime;
        _targetPreviousPosition = target.position;

        // 2. Calcular tiempo de predicción dinámico
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float predictionTime = Mathf.Clamp(
            distanceToTarget / (maxSpeed * evadePredictionMultiplier),
            0.3f,
            1.5f
        );

        // 3. Calcular posición futura del target
        Vector3 futurePosition = target.position + targetVelocity * predictionTime;

        // 4. Calcular fuerza para huir de la posición futura
        Vector3 desired = (transform.position - futurePosition).normalized * maxSpeed;
        Vector3 steering = desired - _velocity;

        return Vector3.ClampMagnitude(steering, maxForce * Time.deltaTime);
    }
    // Seek original independiente
    Vector3 CalculateSeek()
    {
        Vector3 toTarget = target.position - transform.position;

        if (toTarget.magnitude < arrivalRadius)
        {
            _velocity = Vector3.zero;
            return Vector3.zero;
        }

        Vector3 desired = toTarget.normalized * maxSpeed;
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, maxForce * Time.deltaTime);
    }

    // Flee independiente
    Vector3 CalculateFlee()
    {
        Vector3 desired = (transform.position - target.position).normalized * maxSpeed;
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, maxForce * Time.deltaTime);
    }

    // Pursuit completamente independiente
    Vector3 CalculatePursuit()
    {
        // 1. Calcular velocidad del target
        Vector3 targetVelocity = (target.position - _targetPreviousPosition) / Time.deltaTime;
        _targetPreviousPosition = target.position;

        // 2. Calcular predicción
        float predictionTime = Mathf.Clamp(
            Vector3.Distance(transform.position, target.position) / maxSpeed,
            0.5f,
            2f
        );

        // 3. Posición futura
        Vector3 predictedPosition = target.position + targetVelocity * predictionTime;

        // 4. Seek hacia posición futura (sin usar el Seek original)
        Vector3 toPredicted = predictedPosition - transform.position;
        Vector3 desired = toPredicted.normalized * maxSpeed;
        Vector3 steering = desired - _velocity;

        return Vector3.ClampMagnitude(steering, maxForce * Time.deltaTime);
    }

    void ApplyMovement()
    {
        transform.position += _velocity * Time.deltaTime;
    }

    void ApplyRotation()
    {
        if (_velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_velocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}