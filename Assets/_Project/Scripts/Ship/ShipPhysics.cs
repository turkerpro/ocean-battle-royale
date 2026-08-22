using UnityEngine;

namespace OceanBattleRoyale.Ship
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShipPhysics : MonoBehaviour
    {
        [Header("Ship Stats (set by tier)")]
        public float Mass = 5000f;
        public float MaxSpeed = 12f;
        public float Acceleration = 2f;
        public float TurnRate = 25f;
        public float LinearDrag = 0.5f;
        public float AngularDrag = 2f;

        [Header("Runtime")]
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;
        [SerializeField] private Vector3 _velocity;
        [SerializeField] private Vector3 _angularVelocity;
        [SerializeField] private Vector3 _renderPosition;
        [SerializeField] private Quaternion _renderRotation;

        private Rigidbody _rb;
        private Vector3 _inputMove;
        private Vector3 _targetVelocity;
        private float _targetTurn;

        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;
        public Vector3 Velocity => _velocity;
        public Vector3 RenderPosition => _renderPosition;
        public Quaternion RenderRotation => _renderRotation;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = Mass;
            _rb.drag = LinearDrag;
            _rb.angularDrag = AngularDrag;
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            _position = transform.position;
            _rotation = transform.rotation;
        }

        public void Simulate(in OceanBattleRoyale.Network.ShipInput input, float deltaTime)
        {
            _inputMove = new Vector3(input.Move.x, 0, input.Move.y);

            float forwardInput = _inputMove.z;
            float turnInput = _inputMove.x;

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            float currentSpeed = Vector3.Dot(_velocity, forward);
            float targetSpeed = forwardInput * MaxSpeed;

            float accel = (targetSpeed - currentSpeed) * Acceleration * deltaTime;
            _targetVelocity = forward * Mathf.Clamp(currentSpeed + accel, -MaxSpeed * 0.5f, MaxSpeed);

            _targetTurn = turnInput * TurnRate;

            ApplyPhysics(deltaTime);
        }

        private void ApplyPhysics(float deltaTime)
        {
            Vector3 forward = transform.forward;

            _velocity = Vector3.Lerp(_velocity, _targetVelocity, LinearDrag * deltaTime);
            _angularVelocity = Vector3.Lerp(_angularVelocity, Vector3.up * _targetTurn, AngularDrag * deltaTime);

            _position += _velocity * deltaTime;
            _rotation *= Quaternion.Euler(_angularVelocity * deltaTime);

            transform.position = _position;
            transform.rotation = _rotation;
        }

        public void SetTarget(Vector3 position, Quaternion rotation)
        {
            _renderPosition = position;
            _renderRotation = rotation;
        }

        public void Interpolate(float factor)
        {
            _renderPosition = Vector3.Lerp(_renderPosition, _position, factor);
            _renderRotation = Quaternion.Slerp(_renderRotation, _rotation, factor);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _position = position;
            _rotation = rotation;
            _renderPosition = position;
            _renderRotation = rotation;
            _velocity = Vector3.zero;
            _angularVelocity = Vector3.zero;
            transform.position = position;
            transform.rotation = rotation;
            if (_rb != null)
            {
                _rb.position = position;
                _rb.rotation = rotation;
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        public void ApplyTierStats(int tier)
        {
            switch (tier)
            {
                case 1: // Dinghy
                    Mass = 5000f;
                    MaxSpeed = 12f;
                    Acceleration = 3f;
                    TurnRate = 30f;
                    LinearDrag = 0.3f;
                    AngularDrag = 1.5f;
                    break;
                case 2: // Corvette
                    Mass = 15000f;
                    MaxSpeed = 10f;
                    Acceleration = 2f;
                    TurnRate = 20f;
                    LinearDrag = 0.5f;
                    AngularDrag = 2f;
                    break;
                case 3: // Frigate
                    Mass = 30000f;
                    MaxSpeed = 9f;
                    Acceleration = 1.5f;
                    TurnRate = 15f;
                    LinearDrag = 0.7f;
                    AngularDrag = 3f;
                    break;
                case 4: // Cruiser
                    Mass = 50000f;
                    MaxSpeed = 8f;
                    Acceleration = 1f;
                    TurnRate = 10f;
                    LinearDrag = 1f;
                    AngularDrag = 4f;
                    break;
                case 5: // Battleship
                    Mass = 80000f;
                    MaxSpeed = 6f;
                    Acceleration = 0.5f;
                    TurnRate = 6f;
                    LinearDrag = 1.5f;
                    AngularDrag = 5f;
                    break;
            }

            if (_rb != null)
            {
                _rb.mass = Mass;
                _rb.drag = LinearDrag;
                _rb.angularDrag = AngularDrag;
            }
        }
    }
}
