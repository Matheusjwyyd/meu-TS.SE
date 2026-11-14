using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TS_SE_Tool
{
    internal static class ModernBrandArtwork
    {
        internal static Bitmap CreateBrandMark()
        {
            Image source = Properties.Resources.ResourceManager.GetObject("MeuSaveEditorBrand") as Image;
            if (source != null)
                return new Bitmap(source);

            return Properties.Resources.MainIco.ToBitmap();
        }

        internal static Image[] CreateMainTabIcons(int size)
        {
            Image[] icons = new Image[7];
            for (int i = 0; i < icons.Length; i++)
                icons[i] = CreateTabIcon(i, size);

            return icons;
        }

        internal static Bitmap CreateBanner(int width, int height)
        {
            Bitmap image = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(image))
            using (LinearGradientBrush background = new LinearGradientBrush(
                new Rectangle(0, 0, width, height),
                Color.FromArgb(46, 51, 57),
                Color.FromArgb(77, 84, 92),
                0f))
            using (Pen roadShadow = new Pen(Color.FromArgb(100, 4, 17, 30), Math.Max(24, height / 3f)))
            using (Pen roadLine = new Pen(Color.FromArgb(255, 255, 187, 87), Math.Max(2, height / 55f)))
            using (Pen border = new Pen(Color.FromArgb(90, 215, 221, 227), 1f))
            using (Font titleFont = new Font("Segoe UI", Math.Max(18f, height / 7f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font subtitleFont = new Font("Segoe UI", Math.Max(10f, height / 19f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush titleBrush = new SolidBrush(Color.White))
            using (SolidBrush subtitleBrush = new SolidBrush(Color.FromArgb(204, 211, 218)))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.FillRectangle(background, 0, 0, width, height);

                using (GraphicsPath route = new GraphicsPath())
                {
                    float x = width * 0.80f;
                    route.AddBezier(x, height * 1.05f, width * 0.70f, height * 0.70f,
                                    width * 0.98f, height * 0.55f, width * 0.79f, height * -0.08f);
                    roadShadow.StartCap = LineCap.Round;
                    roadShadow.EndCap = LineCap.Round;
                    roadLine.StartCap = LineCap.Round;
                    roadLine.EndCap = LineCap.Round;
                    roadLine.DashStyle = DashStyle.Dash;
                    roadLine.DashPattern = new float[] { 3f, 4f };
                    graphics.DrawPath(roadShadow, route);
                    graphics.DrawPath(roadLine, route);
                }

                using (Bitmap mark = CreateBrandMark())
                {
                    int markSize = Math.Min(height - 24, 128);
                    graphics.DrawImage(mark, new Rectangle(16, (height - markSize) / 2, markSize, markSize));
                }

                float textX = Math.Min(Math.Max(width * 0.10f, 112f), 176f);
                float titleY = height * 0.31f;
                graphics.DrawString("MEU SAVE EDITOR", titleFont, titleBrush, textX, titleY);
                graphics.DrawString("ETS2  /  ATS  |  SAVE STUDIO", subtitleFont, subtitleBrush, textX + 2f, titleY + titleFont.Size + 7f);

                graphics.DrawLine(border, 10, 1, width - 10, 1);
                graphics.DrawLine(border, 10, height - 2, width - 10, height - 2);
            }

            return image;
        }

        private static Bitmap CreateTabIcon(int kind, int size)
        {
            Bitmap image = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(image))
            using (Pen primary = new Pen(Color.FromArgb(103, 112, 122), 1.8f))
            using (Pen detail = new Pen(Color.FromArgb(58, 65, 72), 1.45f))
            using (SolidBrush accent = new SolidBrush(Color.FromArgb(160, 168, 176)))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.ScaleTransform(size / 24f, size / 24f);
                primary.StartCap = LineCap.Round;
                primary.EndCap = LineCap.Round;
                primary.LineJoin = LineJoin.Round;
                detail.StartCap = LineCap.Round;
                detail.EndCap = LineCap.Round;
                detail.LineJoin = LineJoin.Round;

                switch (kind)
                {
                    case 0: // Profile
                        graphics.DrawEllipse(primary, 8f, 3f, 8f, 8f);
                        graphics.DrawArc(primary, 4f, 11f, 16f, 11f, 190f, 160f);
                        graphics.FillEllipse(accent, 10.5f, 5.5f, 3f, 3f);
                        break;
                    case 1: // Company
                        graphics.DrawRectangle(primary, 4f, 3f, 16f, 18f);
                        graphics.DrawLine(detail, 8f, 7f, 8f, 9f);
                        graphics.DrawLine(detail, 12f, 7f, 12f, 9f);
                        graphics.DrawLine(detail, 16f, 7f, 16f, 9f);
                        graphics.DrawLine(detail, 8f, 12f, 8f, 14f);
                        graphics.DrawLine(detail, 12f, 12f, 12f, 14f);
                        graphics.DrawLine(detail, 16f, 12f, 16f, 14f);
                        graphics.DrawRectangle(detail, 10f, 17f, 4f, 4f);
                        break;
                    case 2: // Truck
                        graphics.DrawRectangle(primary, 2f, 7f, 12f, 9f);
                        graphics.DrawLine(primary, 14f, 9f, 19f, 9f);
                        graphics.DrawLine(primary, 19f, 9f, 22f, 13f);
                        graphics.DrawLine(primary, 22f, 13f, 22f, 16f);
                        graphics.DrawLine(primary, 14f, 16f, 22f, 16f);
                        graphics.DrawEllipse(detail, 5f, 14f, 4f, 4f);
                        graphics.DrawEllipse(detail, 16f, 14f, 4f, 4f);
                        graphics.DrawLine(detail, 16f, 10f, 18f, 10f);
                        break;
                    case 3: // Trailer
                        graphics.DrawRectangle(primary, 2f, 5f, 17f, 10f);
                        graphics.DrawLine(detail, 19f, 12f, 22f, 12f);
                        graphics.DrawLine(detail, 22f, 12f, 22f, 15f);
                        graphics.DrawEllipse(detail, 5f, 14f, 4f, 4f);
                        graphics.DrawEllipse(detail, 15f, 14f, 4f, 4f);
                        graphics.DrawLine(primary, 5f, 8f, 16f, 8f);
                        break;
                    case 4: // Freight route
                        graphics.DrawRectangle(primary, 4f, 5f, 13f, 13f);
                        graphics.DrawLine(detail, 4f, 8f, 10.5f, 12f);
                        graphics.DrawLine(detail, 17f, 8f, 10.5f, 12f);
                        graphics.DrawLine(detail, 10.5f, 12f, 10.5f, 18f);
                        graphics.DrawLine(primary, 16f, 20f, 21f, 20f);
                        graphics.DrawLine(primary, 21f, 20f, 21f, 16f);
                        break;
                    case 5: // Cargo
                        graphics.DrawPolygon(primary, new PointF[] {
                            new PointF(12f, 3f), new PointF(21f, 8f), new PointF(21f, 17f),
                            new PointF(12f, 22f), new PointF(3f, 17f), new PointF(3f, 8f)
                        });
                        graphics.DrawLine(detail, 3f, 8f, 12f, 13f);
                        graphics.DrawLine(detail, 21f, 8f, 12f, 13f);
                        graphics.DrawLine(detail, 12f, 13f, 12f, 22f);
                        graphics.FillEllipse(accent, 10f, 10.5f, 4f, 4f);
                        break;
                    default: // Convoy / GPS
                        using (GraphicsPath route = new GraphicsPath())
                        {
                            route.AddBezier(4f, 19f, 7f, 17f, 5f, 12f, 9f, 11f);
                            route.AddBezier(9f, 11f, 13f, 10f, 12f, 6f, 18f, 5f);
                            graphics.DrawPath(primary, route);
                        }
                        graphics.FillEllipse(accent, 2f, 17f, 4f, 4f);
                        graphics.FillEllipse(accent, 16f, 3f, 4f, 4f);
                        graphics.DrawEllipse(detail, 10f, 14f, 8f, 6f);
                        break;
                }
            }

            return image;
        }

        internal static Bitmap CreateActionIcon(int kind, int size)
        {
            Bitmap image = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(image))
            using (Pen line = new Pen(Color.FromArgb(77, 86, 96), Math.Max(1.5f, size / 10f)))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(77, 86, 96)))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                line.StartCap = LineCap.Round;
                line.EndCap = LineCap.Round;
                line.LineJoin = LineJoin.Round;

                if (kind == 0)
                {
                    graphics.DrawArc(line, 3f, 3f, size - 7f, size - 7f, 45f, 145f);
                    graphics.DrawArc(line, 3f, 3f, size - 7f, size - 7f, 225f, 145f);
                    graphics.FillPolygon(fill, new PointF[] { new PointF(size - 3f, size * .27f), new PointF(size - 3f, size * .52f), new PointF(size * .68f, size * .40f) });
                    graphics.FillPolygon(fill, new PointF[] { new PointF(3f, size * .73f), new PointF(3f, size * .48f), new PointF(size * .32f, size * .60f) });
                }
                else if (kind == 1)
                {
                    graphics.DrawLine(line, size * .27f, size * .73f, size * .72f, size * .28f);
                    graphics.DrawLine(line, size * .20f, size * .80f, size * .38f, size * .76f);
                    graphics.DrawLine(line, size * .68f, size * .23f, size * .79f, size * .34f);
                    graphics.DrawLine(line, size * .18f, size * .86f, size * .82f, size * .86f);
                }
                else
                {
                    graphics.DrawLine(line, size * .22f, size * .58f, size * .22f, size * .82f);
                    graphics.DrawLine(line, size * .22f, size * .82f, size * .78f, size * .82f);
                    graphics.DrawLine(line, size * .78f, size * .82f, size * .78f, size * .58f);
                    graphics.DrawLine(line, size * .50f, size * .68f, size * .50f, size * .18f);
                    graphics.DrawLine(line, size * .50f, size * .18f, size * .31f, size * .38f);
                    graphics.DrawLine(line, size * .50f, size * .18f, size * .69f, size * .38f);
                }
            }
            return image;
        }

        internal static Bitmap CreateMenuIcon(int kind, int size)
        {
            Bitmap image = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(image))
            using (Pen line = new Pen(Color.FromArgb(199, 205, 211), 1.7f))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(199, 205, 211)))
            using (Font glyphFont = new Font("Segoe UI", size * .78f, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                line.StartCap = LineCap.Round;
                line.EndCap = LineCap.Round;
                line.LineJoin = LineJoin.Round;

                if (kind == 0)
                {
                    float center = size / 2f;
                    float outer = size * .40f;
                    for (int i = 0; i < 8; i++)
                    {
                        double angle = Math.PI * i / 4d;
                        float x1 = center + (float)Math.Cos(angle) * outer;
                        float y1 = center + (float)Math.Sin(angle) * outer;
                        float x2 = center + (float)Math.Cos(angle) * (outer + size * .12f);
                        float y2 = center + (float)Math.Sin(angle) * (outer + size * .12f);
                        graphics.DrawLine(line, x1, y1, x2, y2);
                    }
                    graphics.DrawEllipse(line, size * .18f, size * .18f, size * .64f, size * .64f);
                    graphics.DrawEllipse(line, size * .39f, size * .39f, size * .22f, size * .22f);
                }
                else if (kind == 1)
                {
                    graphics.DrawEllipse(line, size * .12f, size * .12f, size * .76f, size * .76f);
                    graphics.DrawEllipse(line, size * .34f, size * .12f, size * .32f, size * .76f);
                    graphics.DrawLine(line, size * .15f, size * .39f, size * .85f, size * .39f);
                    graphics.DrawLine(line, size * .15f, size * .61f, size * .85f, size * .61f);
                }
                else
                {
                    graphics.DrawEllipse(line, size * .12f, size * .12f, size * .76f, size * .76f);
                    graphics.DrawString("?", glyphFont, fill, new PointF(size * .27f, size * .08f));
                }
            }
            return image;
        }
    }
}
