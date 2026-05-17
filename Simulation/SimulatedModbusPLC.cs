using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation
{
    public class SimulatedModbusPLC : IDisposable
    {
        private readonly Dictionary<ushort, bool> _coils = new Dictionary<ushort, bool>();
        private readonly Dictionary<ushort, ushort> _holdingRegisters = new Dictionary<ushort, ushort>();
        private readonly Random _rng = new Random();
        private bool _isConnected;
        private float _currentPosition;
        private float _targetPosition;
        private float _speed = 100f;
        private CancellationTokenSource _motionCts;

        public bool IsConnected => _isConnected;
        public float CurrentPosition => _currentPosition;
        public string PortName { get; private set; } = "SIM-PLC-01";

        public event Action<float> OnPositionChanged;
        public event Action<string> OnStatusChanged;

        public SimulatedModbusPLC()
        {
            _coils[0x0000] = false;
            _coils[0x0001] = false;
            _coils[0x0002] = false;
            _holdingRegisters[0x0000] = 0;
            _holdingRegisters[0x0001] = 0;
        }

        public bool Connect(string portName = null)
        {
            if (!string.IsNullOrEmpty(portName))
                PortName = portName;
            _isConnected = true;
            _currentPosition = 0;
            OnStatusChanged?.Invoke($"模拟PLC已连接 ({PortName})");
            OnPositionChanged?.Invoke(0);
            return true;
        }

        public void Disconnect()
        {
            StopMotion();
            _isConnected = false;
            OnStatusChanged?.Invoke("模拟PLC已断开");
        }

        public bool WriteSingleCoil(ushort address, bool value)
        {
            if (!_isConnected) return false;
            _coils[address] = value;
            OnStatusChanged?.Invoke($"模拟PLC: 线圈 0x{address:X4} = {(value ? "ON" : "OFF")}");
            return true;
        }

        public bool ReadCoil(ushort address)
        {
            return _isConnected && _coils.TryGetValue(address, out var val) && val;
        }

        public bool[] ReadCoils(ushort startAddress, ushort count)
        {
            var result = new bool[count];
            for (int i = 0; i < count; i++)
                result[i] = _coils.TryGetValue((ushort)(startAddress + i), out var val) && val;
            return result;
        }

        public ushort ReadHoldingRegister(ushort address)
        {
            return _holdingRegisters.TryGetValue(address, out var val) ? val : (ushort)0;
        }

        public bool WriteHoldingRegister(ushort address, ushort value)
        {
            if (!_isConnected) return false;
            _holdingRegisters[address] = value;
            return true;
        }

        public async Task<bool> MoveToPositionAsync(float targetPosition, float speed = 100f, CancellationToken token = default)
        {
            if (!_isConnected) return false;
            _speed = speed;
            _targetPosition = targetPosition;
            float distance = Math.Abs(targetPosition - _currentPosition);
            if (distance < 0.1f) return true;

            int moveTime = (int)(distance / speed * 1000);
            float step = speed * 0.02f;
            float direction = targetPosition > _currentPosition ? 1 : -1;

            OnStatusChanged?.Invoke($"模拟PLC: 从 {_currentPosition:F1} 移动到 {targetPosition:F1}");

            try
            {
                while (Math.Abs(_currentPosition - targetPosition) > 1f && !token.IsCancellationRequested)
                {
                    _currentPosition += direction * step;
                    if ((direction > 0 && _currentPosition > targetPosition) ||
                        (direction < 0 && _currentPosition < targetPosition))
                        _currentPosition = targetPosition;
                    OnPositionChanged?.Invoke(_currentPosition);
                    await Task.Delay(20, token);
                }
                OnStatusChanged?.Invoke($"模拟PLC: 到达位置 {_currentPosition:F1}");
                return true;
            }
            catch (OperationCanceledException)
            {
                OnStatusChanged?.Invoke($"模拟PLC: 移动被取消 (位置: {_currentPosition:F1})");
                return false;
            }
        }

        public bool SendStopCommand()
        {
            return WriteSingleCoil(0x0001, true);
        }

        private void StopMotion()
        {
            _motionCts?.Cancel();
            _motionCts?.Dispose();
            _motionCts = null;
        }

        public List<string> ScanDevices()
        {
            return new List<string> { PortName };
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
