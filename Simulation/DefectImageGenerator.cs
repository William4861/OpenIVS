using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Simulation
{
    public enum DefectType
    {
        None,
        Scratch,
        Dent,
        Stain,
        Crack,
        Misalignment
    }

    public class DefectImageGenerator
    {
        private static readonly Random _rng = new Random();
        private const int DefaultWidth = 1280;
        private const int DefaultHeight = 960;

        public static Bitmap GeneratePCB(int width = DefaultWidth, int height = DefaultHeight,
            DefectType defect = DefectType.None, float? defectSeverity = null)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color boardColor = Color.FromArgb(30, 80, 30);
            g.Clear(boardColor);

            DrawCopperTraces(g, width, height);
            DrawPads(g, width, height);
            DrawSilkScreen(g, width, height);
            DrawFiducials(g, width, height);

            if (defect != DefectType.None)
            {
                float severity = defectSeverity ?? (float)(_rng.NextDouble() * 0.5 + 0.3);
                DrawDefect(g, width, height, defect, severity);
            }

            return bmp;
        }

        public static Bitmap GenerateMetalSurface(int width = DefaultWidth, int height = DefaultHeight,
            DefectType defect = DefectType.None, float? defectSeverity = null)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color metalColor = Color.FromArgb(180, 185, 190);
            g.Clear(metalColor);
            AddMetalTexture(g, width, height);

            if (defect != DefectType.None)
            {
                float severity = defectSeverity ?? (float)(_rng.NextDouble() * 0.5 + 0.3);
                DrawDefect(g, width, height, defect, severity);
            }

            return bmp;
        }

        public static Bitmap GenerateRandom(int width = DefaultWidth, int height = DefaultHeight)
        {
            Array defects = Enum.GetValues(typeof(DefectType));
            DefectType defect = (DefectType)defects.GetValue(_rng.Next(defects.Length));
            return _rng.Next(2) == 0
                ? GeneratePCB(width, height, defect)
                : GenerateMetalSurface(width, height, defect);
        }

        public static void SaveSampleImages(string outputDir, int count = 10)
        {
            Directory.CreateDirectory(outputDir);
            Array defects = Enum.GetValues(typeof(DefectType));

            for (int i = 0; i < count; i++)
            {
                DefectType defect = (DefectType)defects.GetValue(_rng.Next(defects.Length));
                bool isPCB = _rng.Next(2) == 0;
                Bitmap img = isPCB
                    ? GeneratePCB(defect: defect)
                    : GenerateMetalSurface(defect: defect);
                string path = Path.Combine(outputDir,
                    $"{DateTime.Now:yyyyMMdd_HHmmss}_{(isPCB ? "PCB" : "Metal")}_{defect}_{i}.jpg");
                img.Save(path, ImageFormat.Jpeg);
                img.Dispose();
            }
        }

        private static void DrawCopperTraces(Graphics g, int w, int h)
        {
            using var tracePen = new Pen(Color.FromArgb(200, 140, 50), 8);
            using var thinPen = new Pen(Color.FromArgb(180, 120, 40), 3);

            int[] xs = { w / 6, w / 3, w / 2, 2 * w / 3, 5 * w / 6 };
            foreach (int x in xs)
            {
                tracePen.Width = _rng.Next(4, 12);
                g.DrawLine(tracePen, x, 0, x + _rng.Next(-20, 20), h);
            }

            for (int i = 0; i < 30; i++)
            {
                thinPen.Width = _rng.Next(1, 4);
                int x1 = _rng.Next(0, w);
                int y1 = _rng.Next(0, h);
                int x2 = x1 + _rng.Next(-80, 80);
                int y2 = y1 + _rng.Next(-80, 80);
                g.DrawLine(thinPen, x1, y1, x2, y2);
            }
        }

        private static void DrawPads(Graphics g, int w, int h)
        {
            using var padBrush = new SolidBrush(Color.FromArgb(220, 160, 60));
            using var holeBrush = new SolidBrush(Color.FromArgb(20, 60, 20));

            for (int i = 0; i < 50; i++)
            {
                int x = _rng.Next(20, w - 20);
                int y = _rng.Next(20, h - 20);
                int size = _rng.Next(12, 30);
                g.FillEllipse(padBrush, x - size / 2, y - size / 2, size, size);
                g.FillEllipse(holeBrush, x - 3, y - 3, 6, 6);
            }
        }

        private static void DrawSilkScreen(Graphics g, int w, int h)
        {
            using var silkPen = new Pen(Color.FromArgb(220, 220, 200), 1);
            using var silkBrush = new SolidBrush(Color.FromArgb(220, 220, 200));

            string[] labels = { "R1", "R2", "C1", "C2", "IC1", "U1", "LED1", "JP1", "D1", "Q1" };
            for (int i = 0; i < 10; i++)
            {
                int x = _rng.Next(50, w - 100);
                int y = _rng.Next(50, h - 30);
                g.DrawString(labels[_rng.Next(labels.Length)],
                    new Font("Arial", _rng.Next(6, 10)), silkBrush, x, y);
                g.DrawRectangle(silkPen, x - 2, y - 2,
                    _rng.Next(20, 60), _rng.Next(10, 20));
            }
        }

        private static void DrawFiducials(Graphics g, int w, int h)
        {
            using var fidBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
            using var fidPen = new Pen(Color.FromArgb(180, 180, 180), 2);

            (int, int)[] corners = { (30, 30), (w - 30, 30), (30, h - 30), (w - 30, h - 30) };
            foreach (var (cx, cy) in corners)
            {
                g.FillEllipse(fidBrush, cx - 8, cy - 8, 16, 16);
                g.DrawEllipse(fidPen, cx - 12, cy - 12, 24, 24);
            }
        }

        private static void AddMetalTexture(Graphics g, int w, int h)
        {
            using var pen = new Pen(Color.FromArgb(8, 0, 0, 0));
            for (int i = 0; i < w * h / 200; i++)
            {
                int x = _rng.Next(0, w);
                int y = _rng.Next(0, h);
                g.DrawLine(pen, x, y, x + _rng.Next(3, 15), y + _rng.Next(1, 3));
            }
        }

        private static void DrawDefect(Graphics g, int w, int h, DefectType type, float severity)
        {
            int cx = _rng.Next(w / 4, 3 * w / 4);
            int cy = _rng.Next(h / 4, 3 * h / 4);
            int size = (int)(_rng.Next(20, 60) * severity);

            switch (type)
            {
                case DefectType.Scratch:
                    using (var scratchPen = new Pen(Color.FromArgb(80, 80, 80), size / 4))
                    {
                        int angle = _rng.Next(0, 360);
                        int len = size * 3;
                        int dx = (int)(len * Math.Cos(angle * Math.PI / 180));
                        int dy = (int)(len * Math.Sin(angle * Math.PI / 180));
                        g.DrawLine(scratchPen, cx - dx, cy - dy, cx + dx, cy + dy);
                        int offset = size / 2;
                        g.DrawLine(scratchPen, cx - dx + offset, cy - dy,
                            cx + dx + offset, cy + dy);
                    }
                    break;

                case DefectType.Dent:
                    using (var dentBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
                    using (var dentHighlight = new SolidBrush(Color.FromArgb(200, 200, 200)))
                    {
                        g.FillEllipse(dentBrush, cx - size, cy - size / 2, size * 2, size);
                        g.FillEllipse(dentHighlight, cx - size / 2, cy - size / 4, size, size / 2);
                    }
                    break;

                case DefectType.Stain:
                    using (var stainBrush = new SolidBrush(Color.FromArgb(
                        _rng.Next(30, 80), _rng.Next(40, 90), _rng.Next(20, 60))))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            int rx = cx + _rng.Next(-size, size);
                            int ry = cy + _rng.Next(-size, size);
                            int rs = _rng.Next(size / 2, size);
                            g.FillEllipse(stainBrush, rx - rs, ry - rs, rs * 2, rs * 2);
                        }
                    }
                    break;

                case DefectType.Crack:
                    using (var crackPen = new Pen(Color.FromArgb(50, 50, 50), 2))
                    {
                        int px = cx, py = cy;
                        for (int i = 0; i < 8; i++)
                        {
                            int nx = px + _rng.Next(-size / 2, size / 2);
                            int ny = py + _rng.Next(size / 4, size / 2);
                            g.DrawLine(crackPen, px, py, nx, ny);
                            px = nx; py = ny;
                        }
                        px = cx; py = cy;
                        for (int i = 0; i < 6; i++)
                        {
                            int nx = px + _rng.Next(-size / 3, size / 3);
                            int ny = py - _rng.Next(size / 4, size / 2);
                            g.DrawLine(crackPen, px, py, nx, ny);
                            px = nx; py = ny;
                        }
                    }
                    break;

                case DefectType.Misalignment:
                    int ox = _rng.Next(5, 15);
                    int oy = _rng.Next(5, 15);
                    using (var origPen = new Pen(Color.FromArgb(180, 120, 40), 6))
                    using (var offsetPen = new Pen(Color.FromArgb(220, 160, 60), 4))
                    {
                        g.DrawLine(origPen, cx - 30, cy, cx + 30, cy);
                        g.DrawLine(offsetPen, cx - 30 + ox, cy + oy, cx + 30 + ox, cy + oy);
                    }
                    break;
            }
        }
    }
}
