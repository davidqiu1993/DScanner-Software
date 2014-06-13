using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace DScanner.Image
{
    /// <summary>
    /// ImageProcessor class provides functionalities of image processing for DScanner.
    /// </summary>
    public class ImageProcessor
    {
        private const int _CountXThreshold = 5;

        /// <summary>
        /// Binaryze a bitmap to select the brightest areas with threshold bias.
        /// </summary>
        /// <param name="bmp">The bitmap to process.</param>
        /// <param name="bias">The threshold bias of brightest areas.</param>
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

        /// <summary>
        /// Slim the binaryzed image into a single line.
        /// </summary>
        /// <param name="bmp">The bitmap to process.</param>
        public static void Slim(ref Bitmap bmp)
        {
            int sumX;
            int countX;

            // Lock the bits of the bitmap
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Process the bits
            unsafe
            {
                // Obtain the image data pointer
                byte* pImage = (byte*)(bmpdata.Scan0.ToPointer());

                for(int y=0;y<bmpdata.Height;++y)
                {
                    // Reset the sum of X positions and counter
                    sumX = 0;
                    countX = 0;

                    // Sum up the bright area
                    for(int x=0;x<bmpdata.Width;++x)
                    {
                        // Check if the current pixel is bright
                        if (pImage[0] == 255)
                        {
                            // Add the X position of the bright pixel and add up the counter
                            sumX += x;
                            ++countX;

                            // Darken the current pixel
                            pImage[0] = 0;
                            pImage[1] = 0;
                            pImage[2] = 0;
                        }

                        // Point to the next pixel
                        pImage += 3;
                    }

                    // Check the counter
                    if(countX > _CountXThreshold)
                    {
                        // Calculate the average X position and set the pointer
                        pImage -= (bmpdata.Width - (int)(sumX / countX)) * 3;

                        // Set brightness
                        pImage[0] = 255;
                        pImage[1] = 255;
                        pImage[2] = 255;

                        // point to the head of next line
                        pImage += bmpdata.Stride - (int)(sumX / countX) * 3;
                    }
                    else
                    {
                        // point to the head of next line
                        pImage += bmpdata.Stride - bmpdata.Width * 3;
                    }
                }
            }

            // Unlock the bits of the bitmap
            bmp.UnlockBits(bmpdata);
        }

        /// <summary>
        /// Process the bitmap all in one step.
        /// </summary>
        /// <param name="bmp">The bitmap to process.</param>
        /// <param name="bias">The threshold bias of brightest areas.</param>
        public static void OneStepProcess(ref Bitmap bmp, int bias)
        {
            int max;
            int lowerBoundary;
            int pixelCount;
            int[] rgbsum = new int[bmp.Width * bmp.Height];
            int sumX;
            int countX;

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

                // Filter the background color and slim the bright area
                pixelCount = 0;
                lowerBoundary = max - bias * 3;
                for (int y = 0; y < bmpdata.Height; ++y)
                {
                    // Reset the sum of X positions and the counter
                    sumX = 0;
                    countX = 0;

                    // Process a bitmap line
                    for (int x = 0; x < bmpdata.Width; ++x)
                    {
                        // Compare the pixel and lower boundary
                        if (rgbsum[pixelCount] > lowerBoundary)
                        {
                            // Sum up the X positions and add up the counter
                            sumX += x;
                            ++countX;
                        }

                        // Turn the pixel to background color
                        pImage[0] = (byte)0;
                        pImage[1] = (byte)0;
                        pImage[2] = (byte)0;

                        // Add the pixel count
                        ++pixelCount;

                        // Point to the next pixel
                        pImage += 3;
                    }

                    // Check if there is bright area in the current line
                    if (countX > _CountXThreshold)
                    {
                        // Calculate the average X position and set the pointer
                        pImage -= (bmpdata.Width - (int)(sumX / countX)) * 3;

                        // Set brightness
                        pImage[0] = 255;
                        pImage[1] = 255;
                        pImage[2] = 255;

                        // point to the head of next line
                        pImage += bmpdata.Stride - (int)(sumX / countX) * 3;
                    }
                    else
                    {
                        // Add the strke tail
                        pImage += bmpdata.Stride - bmpdata.Width * 3;
                    }
                }
            }

            // Unlock the bits of the bitmap
            bmp.UnlockBits(bmpdata);
        }
    }
}
