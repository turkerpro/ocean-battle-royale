using UnityEngine;
using UnityEngine.InputSystem;

namespace OceanBattleRoyale.Network
{
    public class LocalPlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private MobileControls _mobileControls;

        private InputAction _moveAction;
        private InputAction _aimAction;
        private InputAction _fireAction;
        private InputAction _mineAction;
        private InputAction _weaponSwitchAction;

        private NetworkedShip _networkedShip;

        private void Awake()
        {
            _networkedShip = GetComponent<NetworkedShip>();
            SetupInputActions();
            SetupMobileControls();
        }

        private void Start()
        {
            if (_networkedShip != null && _networkedShip.IsLocalPlayer)
            {
                SetupCamera();
            }
        }

        private void SetupInputActions()
        {
            if (_inputActions == null) return;

            _moveAction = _inputActions.FindAction("Move", "Player");
            _aimAction = _inputActions.FindAction("Aim", "Player");
            _fireAction = _inputActions.FindAction("Fire", "Player");
            _mineAction = _inputActions.FindAction("DeployMine", "Player");
            _weaponSwitchAction = _inputActions.FindAction("WeaponSwitch", "Player");

            _inputActions.Enable();
        }

        private void SetupMobileControls()
        {
            if (_mobileControls == null)
            {
                _mobileControls = FindObjectOfType<MobileControls>();
            }
        }

        private void SetupCamera()
        {
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 20, -30);
            Camera.main.transform.localRotation = Quaternion.Euler(30, 0, 0);
        }

        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.Disable();
            }
        }

        public ShipInput GetNetworkInput()
        {
            return new ShipInput
            {
                Move = GetMoveInput(),
                Aim = GetAimInput(),
                Fire = GetFireInput(),
                DeployMine = GetMineInput(),
                WeaponSwitch = GetWeaponSwitchInput()
            };
        }

        private Vector2 GetMoveInput()
        {
            if (_mobileControls != null && _mobileControls.gameObject.activeInHierarchy)
                return _mobileControls.MoveInput;

            if (_moveAction != null)
                return _moveAction.ReadValue<Vector2>();

            return Vector2.zero;
        }

        private Vector2 GetAimInput()
        {
            if (_mobileControls != null && _mobileControls.gameObject.activeInHierarchy)
                return _mobileControls.AimInput;

            if (_aimAction != null)
                return _aimAction.ReadValue<Vector2>();

            return Vector2.zero;
        }

        private bool GetFireInput()
        {
            if (_mobileControls != null && _mobileControls.gameObject.activeInHierarchy)
                return _mobileControls.FirePressed;

            if (_fireAction != null)
                return _fireAction.IsPressed();

            return false;
        }

        private bool GetMineInput()
        {
            if (_mobileControls != null && _mobileControls.gameObject.activeInHierarchy)
                return _mobileControls.MinePressed;

            if (_mineAction != null)
                return _mineAction.WasPressedThisFrame();

            return false;
        }

        private byte GetWeaponSwitchInput()
        {
            if (_mobileControls != null && _mobileControls.gameObject.activeInHierarchy)
                return _mobileControls.WeaponSwitchPressed ? (byte)1 : (byte)0;

            if (_weaponSwitchAction != null && _weaponSwitchAction.WasPressedThisFrame())
                return 1;

            return 0;
        }
    }
}
