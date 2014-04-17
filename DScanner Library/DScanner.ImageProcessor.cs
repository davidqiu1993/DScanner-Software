using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DScanner
{
    public class ImageProcessor
    {
        public static void Binaryze(ref Bitmap bmp, int bias, bool full_scan = false)
        {
            int max;
            int tmp;

            if(full_scan)
            {
                max = 0;
                for(int i=0;i<bmp.Height;++i)
                {
                    for (int j = 0; j < bmp.Width; ++j)
                    {
                        tmp = bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B;
                        if (tmp > max) max = tmp;
                    }
                }
                for (int i = 0; i < bmp.Height; ++i)
                {
                    for (int j = 0; j < bmp.Width; ++j)
                    {
                        if (bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B < max - bias) bmp.SetPixel(j, i, Color.FromArgb(0, 0, 0));
                        else bmp.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                    }
                }
            }
            else
            {
                for (int i = 0; i < bmp.Height; ++i)
                {
                    max = 0;
                    for (int j = 0; j < bmp.Width; ++j)
                    {
                        tmp = bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B;
                        if (tmp > max) max = tmp;
                    }
                    for (int j = 0; j < bmp.Width; ++j)
                    {
                        if (bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B < max - bias) bmp.SetPixel(j, i, Color.FromArgb(0, 0, 0));
                        else bmp.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                    }
                }
            }
        }

        public static void TrimBinaryLines(ref Bitmap bmp)
        {
            int sum_x;
            int count_x;
            int ave_x;

            for (int i = 0; i < bmp.Height; ++i)
            {
                sum_x = 0;
                count_x = 0;
                for (int j = 0; j < bmp.Width; ++j)
                {
                    if (bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B == 255 * 3)
                    {
                        sum_x += j;
                        ++count_x;
                        bmp.SetPixel(j, i, Color.FromArgb(0, 0, 0));
                    }
                }
                if (count_x > 0)
                {
                    ave_x = sum_x / count_x;
                    bmp.SetPixel(ave_x, i, Color.FromArgb(255, 255, 255));
                }
            }
        }
    }
}
