using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Simulation
{
    public class SimulatedModel : IDisposable
    {
        private readonly Random _rng = new Random();
        private bool _isLoaded;
        private string _modelPath;
        private int _inferenceCount;

        public bool IsLoaded => _isLoaded;
        public string ModelPath => _modelPath;
        public int InferenceCount => _inferenceCount;

        public event Action<string> OnStatusChanged;

        public SimulatedModel() { }

        public bool Load(string path)
        {
            _modelPath = path;
            _isLoaded = true;
            _inferenceCount = 0;
            OnStatusChanged?.Invoke($"模拟模型已加载: {Path.GetFileName(path)}");
            return true;
        }

        public void Unload()
        {
            _isLoaded = false;
            OnStatusChanged?.Invoke("模拟模型已卸载");
        }

        public SimulatedInferenceResult Infer(Bitmap image, float threshold = 0.5f)
        {
            _inferenceCount++;
            Thread.Sleep(_rng.Next(30, 80));
            return GeneratePlausibleResult(image, threshold);
        }

        public List<SimulatedInferenceResult> InferBatch(List<Bitmap> images, float threshold = 0.5f)
        {
            var results = new List<SimulatedInferenceResult>();
            foreach (var img in images)
            {
                _inferenceCount++;
                Thread.Sleep(_rng.Next(20, 50));
                results.Add(GeneratePlausibleResult(img, threshold));
            }
            return results;
        }

        private SimulatedInferenceResult GeneratePlausibleResult(Bitmap image, float threshold)
        {
            var result = new SimulatedInferenceResult();
            bool hasDefects = DetectDefectsInImage(image);

            var objects = new List<SimulatedObjectResult>();
            if (hasDefects)
            {
                int defectCount = _rng.Next(1, 4);
                for (int i = 0; i < defectCount; i++)
                {
                    objects.Add(GenerateRandomObject(image.Width, image.Height, true));
                }
            }
            int goodCount = _rng.Next(1, 5);
            for (int i = 0; i < goodCount; i++)
            {
                objects.Add(GenerateRandomObject(image.Width, image.Height, false));
            }

            result.Objects = objects;
            result.InferenceTimeMs = _rng.Next(20, 80);
            result.IsPass = objects.Count == 0;
            return result;
        }

        private bool DetectDefectsInImage(Bitmap image)
        {
            int sampleSize = 100;
            int totalPixels = 0;
            int darkPixels = 0;
            int abnormalPixels = 0;

            using (var bmp = new Bitmap(image))
            {
                for (int i = 0; i < sampleSize; i++)
                {
                    int x = _rng.Next(0, bmp.Width);
                    int y = _rng.Next(0, bmp.Height);
                    var pixel = bmp.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    totalPixels++;
                    if (gray < 60) darkPixels++;
                    if (pixel.R < 80 && pixel.G < 100 && pixel.B < 60) abnormalPixels++;
                }
            }

            float darkRatio = (float)darkPixels / totalPixels;
            float abnormalRatio = (float)abnormalPixels / totalPixels;
            return abnormalRatio > 0.15f || darkRatio > 0.4f;
        }

        private SimulatedObjectResult GenerateRandomObject(int imgW, int imgH, bool isDefect)
        {
            int cx = _rng.Next(imgW / 6, 5 * imgW / 6);
            int cy = _rng.Next(imgH / 6, 5 * imgH / 6);
            int w = _rng.Next(30, 120);
            int h = _rng.Next(30, 120);
            float score = isDefect
                ? (float)(_rng.NextDouble() * 0.3 + 0.65)
                : (float)(_rng.NextDouble() * 0.15 + 0.8);

            string[] defectNames = { "scratch", "dent", "stain", "crack", "solder_bridge",
                "missing_component", "misalignment", "void", "burr", "oxidation" };
            string[] normalNames = { "resistor", "capacitor", "IC", "connector", "pad",
                "via", "trace", "solder_joint", "component", "marking" };

            return new SimulatedObjectResult
            {
                CategoryId = isDefect ? _rng.Next(1, 11) : _rng.Next(11, 21),
                CategoryName = isDefect
                    ? defectNames[_rng.Next(defectNames.Length)]
                    : normalNames[_rng.Next(normalNames.Length)],
                Score = score,
                Bbox = new float[] { cx - w / 2, cy - h / 2, w, h },
                IsDefect = isDefect
            };
        }

        public void Dispose()
        {
            Unload();
        }
    }

    public class SimulatedInferenceResult
    {
        public List<SimulatedObjectResult> Objects { get; set; } = new List<SimulatedObjectResult>();
        public int InferenceTimeMs { get; set; }
        public bool IsPass { get; set; }
    }

    public class SimulatedObjectResult
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public float Score { get; set; }
        public float[] Bbox { get; set; }
        public bool IsDefect { get; set; }

        public override string ToString()
        {
            string tag = IsDefect ? "DEFECT" : "OK";
            return $"[{tag}] {CategoryName} ({Score:F2}) @ [{Bbox[0]:F0},{Bbox[1]:F0},{Bbox[2]:F0},{Bbox[3]:F0}]";
        }
    }
}
