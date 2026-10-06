using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    /// <summary>Device input only; physics remains independent of the binding scheme.</summary>
    public sealed class VehicleInput : MonoBehaviour
    {
        public float Throttle { get; private set; }
        public float BrakeReverse { get; private set; }
        public float Steering { get; private set; }
        bool resetRequested;
        InputAction throttle, brake, steering, reset, fist;
        public InputAction[] CurrentBindings => new[]{throttle,brake,steering,reset,fist};
        public bool UsingGamepad { get; private set; }
        public string ResetControlLabel
        {
            get
            {
                if(!UsingGamepad)return reset.GetBindingDisplayString(1);
                string label=Gamepad.current?.buttonNorth.displayName;
                return string.IsNullOrEmpty(label)||label=="Button North"?"Y":label;
            }
        }
        void Used(InputAction.CallbackContext context) { UsingGamepad=context.control.device is Gamepad; }

        void Awake()
        {
            throttle = new InputAction("Throttle", InputActionType.Value);
            throttle.AddBinding("<Gamepad>/rightTrigger");
            throttle.AddBinding("<Keyboard>/w");
            throttle.AddBinding("<Keyboard>/upArrow");
            brake = new InputAction("BrakeReverse", InputActionType.Value);
            brake.AddBinding("<Gamepad>/leftTrigger");
            brake.AddBinding("<Keyboard>/s");
            brake.AddBinding("<Keyboard>/downArrow");
            steering = new InputAction("Steering", InputActionType.Value);
            steering.AddBinding("<Gamepad>/leftStick/x").WithProcessor("axisDeadzone(min=0.12,max=0.95)");
            steering.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
            steering.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
            reset = new InputAction("Reset", InputActionType.Button);
            reset.AddBinding("<Gamepad>/buttonNorth");
            reset.AddBinding("<Keyboard>/r");
            // 0.78: shake a fist (cosmetic, RiderGestures). 0.80: on LB, since the right finger is on the throttle trigger.
            // In Trailer Mode LB keeps its 0.25x hold; F still works there.
            fist = new InputAction("Fist wave", InputActionType.Button);
            fist.AddBinding("<Gamepad>/leftShoulder");
            fist.AddBinding("<Keyboard>/f");
            throttle.performed+=Used; brake.performed+=Used; steering.performed+=Used; reset.performed+=Used; fist.performed+=Used;
        }

        void OnEnable() { throttle.Enable(); brake.Enable(); steering.Enable(); reset.Enable(); fist.Enable(); }
        void Update()
        {
            Throttle = throttle.ReadValue<float>();
            BrakeReverse = brake.ReadValue<float>();
            Steering = steering.ReadValue<float>();
            resetRequested |= reset.WasPressedThisFrame();
            if (LoadingScreen.Holding) { Throttle = BrakeReverse = Steering = 0; resetRequested = false; return; } // 0.82: no driving behind the loading screen
            // Not while a menu, the debug overlay or the map has the controls (F is the map's waypoint key).
            if (fist.WasPressedThisFrame() && !(TrailerMode.Active && fist.activeControl?.device is Gamepad) && !MenuInput.Blocked
                && FindAnyObjectByType<ExplorationMap>()?.OwnsInput != true)
                GetComponent<RiderGestures>()?.Wave();
        }
        public bool ConsumeReset() { bool result = resetRequested; resetRequested = false; return result; }
        void OnDisable()
        {
            throttle.Disable(); brake.Disable(); steering.Disable(); reset.Disable(); fist.Disable();
            Throttle = BrakeReverse = Steering = 0; resetRequested = false;
        }
        void OnDestroy() { throttle.Dispose(); brake.Dispose(); steering.Dispose(); reset.Dispose(); fist.Dispose(); }
    }
}
