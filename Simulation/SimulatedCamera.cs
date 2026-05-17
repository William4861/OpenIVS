using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation
{
    public class SimulatedCamera : IDisposable
    {
        private readonly Random _rng = new Random();
        private CancellationTokenSource _streamCts;
        private Task _streamTask;
        private int _frameIntervalMs;
        private bool _injectDefects;

        public bool IsStreaming { get; private set; }
        public string CameraName { get; }
        public string SerialNumber { get; }
        public (int width, int height) Resolution { get; set; } = (1280, 960);

        public event Action<Bitmap> OnFrameCaptured;
        public event Action<string> OnStatusChanged;

        public SimulatedCamera(string name = "SimulatedCamera", string serial = "SIM-2024-0001")
        {
            CameraName = name;
            SerialNumber = serial;
        }

        public bool Open()
        {
            OnStatusChanged?.Invoke($"模拟相机 {CameraName} ({SerialNumber}) 已打开");
            return true;
        }

        public void Close()
        {
            StopStream();
            OnStatusChanged?.Invoke("模拟相机已关闭");
        }

        public void StartStream(int fps = 10, bool injectDefects = true)
        {
            if (IsStreaming) return;
            _frameIntervalMs = 1000 / fps;
            _injectDefects = injectDefects;
            _streamCts = new CancellationTokenSource();
            _streamTask = Task.Run(() => StreamLoop(_streamCts.Token));
            IsStreaming = true;
            OnStatusChanged?.Invoke($"模拟相机开始采集 ({fps}FPS)");
        }

        public void StopStream()
        {
            _streamCts?.Cancel();
            _streamTask?.Wait(1000);
            _streamCts?.Dispose();
            _streamCts = null;
            _streamTask = null;
            IsStreaming = false;
        }

        public Bitmap CaptureFrame()
        {
            Array defectTypes = Enum.GetValues(typeof(DefectType));
            DefectType defect = _injectDefects && _rng.NextDouble() < 0.3
                ? (DefectType)defectTypes.GetValue(_rng.Next(1, defectTypes.Length))
                : DefectType.None;
            return DefectImageGenerator.GeneratePCB(
                Resolution.width, Resolution.height, defect, (float)(_rng.NextDouble() * 0.5 + 0.3));
        }

        public List<string> GetAvailableResolutions()
        {
            return new List<string>
            {
                "640x480", "800x600", "1024x768",
                "1280x960", "1600x1200", "1920x1080", "2592x1944"
            };
        }

        private void StreamLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var frame = CaptureFrame();
                OnFrameCaptured?.Invoke(frame);
                try { Task.Delay(_frameIntervalMs, token).Wait(token); }
                catch { break; }
            }
            IsStreaming = false;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
