using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityFFB
{
    public class UnityFFB : MonoBehaviour
    {
        private static UnityFFB instance;

        /// <summary>
        /// Whether or not to enable Force Feedback when the behavior starts.
        /// </summary>
        public bool enableOnAwake = true;
        /// <summary>
        /// Whether or not to automatically select the first FFB device on start.
        /// </summary>
        public bool autoSelectFirstDevice = true;
        /// <summary>
        /// Whether or not to automatically add a constant force effect to the device.
        /// </summary>
        public bool addConstantForce = true;

        // Constant force properties
        public int force = 0;
        public int angle = 0;
        [Range(-1,1)] public float Axis_X = 0;
        [Range(-1,1)] public float Axis_Y = 0;
        
        private int[] axisDirections = new int[0];
        public bool ffbEnabled { get; private set; }
        public bool constantForceEnabled { get; private set; }

        private DeviceInfo[] devices = new DeviceInfo[0];

        public DeviceInfo? activeDevice = null;

        private DeviceAxisInfo[] axes = new DeviceAxisInfo[0];
        private DICondition[] springConditions = new DICondition[0];

        protected bool nativeLibLoadFailed = false;

        void Awake()
        {
            instance = this;
            if (enableOnAwake)
            {
                EnableForceFeedback();
            }
        }

        private void FixedUpdate()
        {
            var state = UnityFFBNative.GetJoystickAxisData();
            Axis_X = (float)Math.Round((( 2.0 / 65535.0) * state.xAxis - 1.0), 3);
            Axis_Y = (float)Math.Round((( 2.0 / 65535.0) * state.yAxis - 1.0), 3);
            if (nativeLibLoadFailed) { return; }
            if (constantForceEnabled)
            {
                UnityFFBNative.UpdateConstantForce(force, new int[2]{angle * 100, 0});
            }
        }

        public void EnableForceFeedback()
        {
            if (nativeLibLoadFailed ||  ffbEnabled)
            {
                return;
            }

            try
            {
                if (UnityFFBNative.StartDirectInput() >= 0)
                {
                    ffbEnabled = true;
                }
                else
                {
                    ffbEnabled = false;
                }

                int deviceCount = 0;

                IntPtr ptrDevices = UnityFFBNative.EnumerateFFBDevices(ref deviceCount);

                Debug.Log($"[UnityFFB] Device count: {devices.Length}");
                if (deviceCount > 0)
                {
                    devices = new DeviceInfo[deviceCount];

                    int deviceSize = Marshal.SizeOf(typeof(DeviceInfo));
                    for (int i = 0; i < deviceCount; i++)
                    {
                        IntPtr pCurrent = ptrDevices + i * deviceSize;
                        devices[i] = Marshal.PtrToStructure<DeviceInfo>(pCurrent);
                    }

                    foreach (DeviceInfo device in devices)
                    {
                        string ffbAxis = UnityEngine.JsonUtility.ToJson(device, true);
                        Debug.Log(ffbAxis);
                    }

                    if (autoSelectFirstDevice)
                    {
                        SelectDevice(devices[0].guidInstance);
                    }
                }
            }
            catch (DllNotFoundException e)
            {
                LogMissingRuntimeError();
                throw new DllNotFoundException(e.ToString());
            }
        }

        public void DisableForceFeedback()
        {
            if (nativeLibLoadFailed) { return; }
            try
            {
                UnityFFBNative.StopDirectInput();
            }
            catch (DllNotFoundException e)
            {
                LogMissingRuntimeError();
                throw new DllNotFoundException(e.ToString());
            }
            ffbEnabled = false;
            constantForceEnabled = false;
            devices = new DeviceInfo[0];
            activeDevice = null;
            axes = new DeviceAxisInfo[0];
            springConditions = new DICondition[0];
        }

        public void SelectDevice(string deviceGuid)
        {
            if (nativeLibLoadFailed) { return; }
            try
            {
                // For now just initialize the first FFB Device.
                int hresult = UnityFFBNative.CreateFFBDevice(deviceGuid);
                if (hresult == 0)
                {
                    activeDevice = devices[0];
                    

                    int axisCount = 0;
                    IntPtr ptrAxes = UnityFFBNative.EnumerateFFBAxes(ref axisCount);
                    if (axisCount > 0)
                    {
                        axes = new DeviceAxisInfo[axisCount];
                        axisDirections = new int[axisCount];
                        springConditions = new DICondition[axisCount];

                        int axisSize = Marshal.SizeOf(typeof(DeviceAxisInfo));
                        for (int i = 0; i < axisCount; i++)
                        {
                            IntPtr pCurrent = ptrAxes + i * axisSize;
                            axes[i] = Marshal.PtrToStructure<DeviceAxisInfo>(pCurrent);
                            axisDirections[i] = 0;
                            springConditions[i] = new DICondition();
                        }
                        if (addConstantForce)
                        {
                            hresult = UnityFFBNative.AddFFBEffect(EffectsType.ConstantForce);
                            if (hresult == 0)
                            {
                                axisDirections[0] = force;
                                hresult = UnityFFBNative.UpdateConstantForce(0, axisDirections);
                                if (hresult != 0)
                                {
                                    Debug.LogError($"[UnityFFB] UpdateConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                                }
                                constantForceEnabled = true;
                            }
                            else
                            {
                                Debug.LogError($"[UnityFFB] AddConstantForce Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                            }
                        }
                    }
                    Debug.Log($"[UnityFFB] Axis count: {axes.Length}");
                    foreach (DeviceAxisInfo axis in axes)
                    {
                        string ffbAxis = UnityEngine.JsonUtility.ToJson(axis, true);
                        Debug.Log(ffbAxis);
                    }
                }
                else
                {
                    activeDevice = null;
                    Debug.LogError($"[UnityFFB] 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                }
            }
            catch (DllNotFoundException e)
            {
                LogMissingRuntimeError();
                throw new DllNotFoundException(e.ToString());
            }
        }

        public void SetConstantForceGain(float gainPercent)
        {
            if (nativeLibLoadFailed) { return; }
            if (constantForceEnabled)
            {
                int hresult = UnityFFBNative.UpdateEffectGain(EffectsType.ConstantForce, gainPercent);
                if (hresult != 0)
                {
                    Debug.LogError($"[UnityFFB] UpdateEffectGain Failed: 0x{hresult.ToString("x")} {WinErrors.GetSystemMessage(hresult)}");
                }
            }
        }

        public void StartFFBEffects()
        {
            if (nativeLibLoadFailed) { return; }
            try
            {
                UnityFFBNative.StartAllFFBEffects();
                constantForceEnabled = true;
            }
            catch (DllNotFoundException e)
            {
                LogMissingRuntimeError();
                throw new DllNotFoundException(e.ToString());
            }
        }

        public void StopFFBEffects()
        {
            if (nativeLibLoadFailed) { return; }
            try
            {
                UnityFFBNative.StopAllFFBEffects();
                constantForceEnabled = false;
            }
            catch (DllNotFoundException e)
            {
                LogMissingRuntimeError();
                throw new DllNotFoundException(e.ToString());
            }
        }

        void LogMissingRuntimeError()
        {
            Debug.LogError(
                "Unable to load Force Feedback plugin. Ensure that the following are installed:\n\n" +
                "DirectX End-User Runtime: https://www.microsoft.com/en-us/download/details.aspx?id=35\n" +
                "Visual C++ Redistributable: https://aka.ms/vs/17/release/vc_redist.x64.exe"
            );
            nativeLibLoadFailed = true;
        }

        public void OnApplicationQuit()
        {
            DisableForceFeedback();
        }
    }
}
