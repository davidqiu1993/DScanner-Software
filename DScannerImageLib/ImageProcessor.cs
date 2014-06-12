using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace DScanner.Image
{
    public class ImageProcessor
    {
        public static void Binaryze(ref Bitmap bmp, int bias)
        {
            int max;
            int lowerBoundary;
            int pixelCount;
            int[] rgbsum = new int[bmp.Width * bmp.Height];

            // Lock the bits of the bitmap
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Process the bits
            unsafe
            {
                // Obtain the image data pointer
                byte* pImage = (byte*)(bmpdata.Scan0.ToPointer());
                
                // Find the maximum pixel sum
                max = 0;
                pixelCount = 0;
                for (int y = 0; y < bmpdata.Height; ++y)
                {
                    for (int x = 0; x < bmpdata.Width; ++x)
                    {
                        // Sum up the RGB elements of the pixel
                        rgbsum[pixelCount] = (int)pImage[0] + (int)pImage[1] + (int)pImage[2];

                        // Compare the current maximum pixel RGB sum
                        if (rgbsum[pixelCount] > max) max = rgbsum[pixelCount];

                        // Add up the pixel count
                        ++pixelCount;

                        // Point to the next pixel
                        pImage += 3;
                    }

                    // Add the strike tail
                    pImage += bmpdata.Stride - bmpdata.Width * 3;
                }

                // Reset the image data pointer
                pImage = (byte*)(bmpdata.Scan0.ToPointer());
                
                // Filter the background color
                pixelCount = 0;
                lowerBoundary = max - bias * 3;
                for (int y = 0; y < bmpdata.Height; ++y)
                {
                    for (int x = 0; x < bmpdata.Width; ++x)
                    {
                        // Compare the pixel and lower boundary
                        if (rgbsum[pixelCount] > lowerBoundary)
                        {
                            pImage[0] = (byte)255;
                            pImage[1] = (byte)255;
                            pImage[2] = (byte)255;
                        }
                        else
                        {
                            pImage[0] = (byte)0;
                            pImage[1] = (byte)0;
                            pImage[2] = (byte)0;
                        }
                        
                        // Add the pixel count
                        ++pixelCount;

                        // Point to the next pixel
                        pImage += 3;
                    }
                    
                    // Add the strke tail
                    pImage += bmpdata.Stride - bmpdata.Width * 3;
                }
            }

            // Unlock the bits of the bitmap
            bmp.UnlockBits(bmpdata);
        }
    }
}
