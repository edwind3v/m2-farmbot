using System.Drawing.Drawing2D;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
public partial class MainForm
{
	private void RenderRadar(List<EntityState> mobList)
	{
		if (TryRenderMinimap(mobList)) return;
		int num = radarPanel.Width;
		int num2 = radarPanel.Height;
		Bitmap bitmap = new Bitmap(num, num2);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.Clear(Color.FromArgb(8, 10, 18));
		float num3 = (float)num / 2f;
		float num4 = (float)num2 / 2f;
		float num5 = (float)Math.Min(num, num2) / 2f - 8f;
		float num6 = 9500f;
		float num7 = num5 / num6;
		float num8 = memory.ReadFloat(AddressCatalog.Property_037B);
		float num9 = memory.ReadFloat(AddressCatalog.Property_037C);
		int num10 = memory.ReadInt32(AddressCatalog.Property_037E);
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(110, 28, 36, 52)))
		{
			graphics.FillEllipse(brush, num3 - num5, num4 - num5, num5 * 2f, num5 * 2f);
		}
		using (Pen pen = new Pen(Color.FromArgb(210, 198, 168), 2f))
		{
			graphics.DrawEllipse(pen, num3 - num5, num4 - num5, num5 * 2f, num5 * 2f);
		}
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse(num3 - num5, num4 - num5, num5 * 2f, num5 * 2f);
		graphics.SetClip(graphicsPath);
		using (Pen pen2 = new Pen(Color.FromArgb(40, 154, 160, 172), 1f))
		{
			int num11 = 10;
			for (float num12 = num3 % (float)num11; num12 < (float)num; num12 += (float)num11)
			{
				graphics.DrawLine(pen2, num12, 0f, num12, num2);
			}
			for (float num13 = num4 % (float)num11; num13 < (float)num2; num13 += (float)num11)
			{
				graphics.DrawLine(pen2, 0f, num13, num, num13);
			}
		}
		if (farmAlongRoute)
		{
			float num14 = routeFarmRange * num7;
			using (Pen pen3 = new Pen(Color.FromArgb(180, 210, 198, 168), 2f))
			{
				graphics.DrawEllipse(pen3, num3 - num14, num4 - num14, num14 * 2f, num14 * 2f);
			}
			using Pen pen4 = new Pen(Color.FromArgb(70, 210, 198, 168), 1f);
			int num15 = 5;
			for (float num16 = num3 - num14; num16 <= num3 + num14; num16 += (float)num15)
			{
				float num17 = num16 - num3;
				float num18 = (float)Math.Sqrt(num14 * num14 - num17 * num17);
				graphics.DrawLine(pen4, num16, num4 - num18, num16, num4 + num18);
			}
			for (float num19 = num4 - num14; num19 <= num4 + num14; num19 += (float)num15)
			{
				float num20 = num19 - num4;
				float num21 = (float)Math.Sqrt(num14 * num14 - num20 * num20);
				graphics.DrawLine(pen4, num3 - num21, num19, num3 + num21, num19);
			}
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(236, 230, 218)))
		{
			graphics.FillEllipse(brush2, num3 - 5f, num4 - 5f, 10f, 10f);
		}
		float num22 = num5 * num5;
		foreach (EntityState mob in mobList)
		{
			if (mob.Property_03C9 != num10)
			{
				float num23 = mob.X - num8;
				float num24 = mob.Y - num9;
				float num25 = num3 + num23 * num7;
				float num26 = num4 - num24 * num7;
				if ((num25 - num3) * (num25 - num3) + (num26 - num4) * (num26 - num4) <= num22)
				{
					Brush brush3;
					if (mob.Property_03CA == AddressCatalog.Field_035C)
					{
						brush3 = new SolidBrush(Color.FromArgb(220, 120, 96));
					}
					else
					{
						brush3 = ((mob.Property_03CA == AddressCatalog.Field_035B) ? new SolidBrush(Color.FromArgb(236, 230, 218)) : new SolidBrush(Color.FromArgb(210, 198, 168)));
					}
					graphics.FillEllipse(brush3, num25 - 4f, num26 - 4f, 6f, 6f);
					brush3.Dispose();
				}
			}
		}
        PaintRecordedRoute(graphics,p=>new PointF(num3+(p.X-num8)*num7,num4-(p.Y-num9)*num7),false);
		radarPanel.BackgroundImage?.Dispose();
		radarPanel.BackgroundImage = bitmap;
		radarPanel.BackgroundImageLayout = ImageLayout.Center;
	}

}

