using System;
using System.Drawing;
using System.Threading.Tasks;
using Simulation;

namespace OpenIVSWPF.Managers
{
    public static class SimulationModeManager
    {
        private static SimulatedCamera _simCamera;
        private static SimulatedModbusPLC _simPLC;
        private static SimulatedModel _simModel;
        private static bool _isSimulationMode;

        public static bool IsSimulationMode => _isSimulationMode;
        public static SimulatedCamera SimCamera => _simCamera;
        public static SimulatedModbusPLC SimPLC => _simPLC;
        public static SimulatedModel SimModel => _simModel;
        public static bool IsConnected { get; private set; } = true;
        public static bool IsGrabbing { get; private set; }

        public static event Action<string> OnStatus;

        public static void Enable()
        {
            if (_isSimulationMode) return;
            _isSimulationMode = true;
            _simCamera = new SimulatedCamera();
            _simPLC = new SimulatedModbusPLC();
            _simModel = new SimulatedModel();
            _simCamera.Open();
            _simPLC.Connect();
            _simModel.Load("sim://mock-model");
            IsConnected = true;
            Log("仿真模式已启用");
        }

        public static void Disable()
        {
            if (!_isSimulationMode) return;
            _simCamera?.Close();
            _simPLC?.Disconnect();
            _simModel?.Unload();
            _simCamera = null;
            _simPLC = null;
            _simModel = null;
            _isSimulationMode = false;
            IsConnected = false;
            Log("仿真模式已禁用");
        }

        public static Bitmap CaptureImage()
        {
            if (!_isSimulationMode || _simCamera == null) return null;
            return _simCamera.CaptureFrame();
        }

        public static async Task<bool> MoveToPositionAsync(float targetPosition)
        {
            if (!_isSimulationMode || _simPLC == null) return false;
            await _simPLC.MoveToPositionAsync(targetPosition);
            return true;
        }

        public static string PerformInference(Bitmap image)
        {
            if (!_isSimulationMode || _simModel == null) return "仿真模型未加载";
            var result = _simModel.Infer(image);
            string output = result.IsPass ? "PASS" : $"FAIL ({result.Objects.Count} defects)";
            foreach (var obj in result.Objects)
            {
                if (obj.IsDefect)
                    output += $"\n  [{obj.CategoryName}] score={obj.Score:F2}";
            }
            return output;
        }

        public static void ResetImageIndex()
        {
        }

        private static void Log(string msg)
        {
            OnStatus?.Invoke(msg);
        }
    }
}
