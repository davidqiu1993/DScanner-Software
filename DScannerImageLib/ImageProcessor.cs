using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace DScanner.Image
{
    /// <summary>
    /// Two dimensional point.
    /// </summary>
    /// <typeparam name="T">The value type of the point.</typeparam>
    public class Point2D<T>
    {
        #region Local Variables

        /// <summary>
        ///  Value of x-axis component.
        /// </summary>
        private T _X;

        /// <summary>
        /// Value of y-axis component.
        /// </summary>
        private T _Y;

        #endregion


        #region Class Attributes

        /// <summary>
        /// Get or set the x-axis component.
        /// </summary>
        public T X
        {
            get { return _X; }
            set { _X = value; }
        }

        /// <summary>
        /// Get of set the y-axis component.
        /// </summary>
        public T Y
        {
            get { return _Y; }
            set { _Y = value; }
        }

        #endregion


        #region Class Object Constructors

        /// <summary>
        /// Construct Point2D in default.
        /// </summary>
        public Point2D()
        {
            ;
        }

        /// <summary>
        /// Construct Point2D with specific x- and y-axis component values.
        /// </summary>
        /// <param name="x">Specific x-axis component value.</param>
        /// <param name="y">Specific y-axis component value.</param>
        public Point2D(T x, T y)
        {
            _X = x;
            _Y = y;
        }

        #endregion
    }


    /// <summary>
    /// Three dimensional point.
    /// </summary>
    /// <typeparam name="T">The value type of the point.</typeparam>
    public class Point3D<T>
    {
        #region Local Variables

        /// <summary>
        ///  Value of x-axis component.
        /// </summary>
        private T _X;

        /// <summary>
        /// Value of y-axis component.
        /// </summary>
        private T _Y;

        /// <summary>
        /// Value of z-axis component.
        /// </summary>
        private T _Z;

        #endregion


        #region Class Attributes

        /// <summary>
        /// Get or set the x-axis component.
        /// </summary>
        public T X
        {
            get { return _X; }
            set { _X = value; }
        }

        /// <summary>
        /// Get of set the y-axis component.
        /// </summary>
        public T Y
        {
            get { return _Y; }
            set { _Y = value; }
        }

        /// <summary>
        /// Get of set the z-axis component.
        /// </summary>
        public T Z
        {
            get { return _Z; }
            set { _Z = value; }
        }

        #endregion


        #region Class Object Constructors

        /// <summary>
        /// Construct Point3D in default.
        /// </summary>
        public Point3D()
        {
            ;
        }

        /// <summary>
        /// Construct Point2D with specific x-, y- and z-axis component values.
        /// </summary>
        /// <param name="x">Specific x-axis component value.</param>
        /// <param name="y">Specific y-axis component value.</param>
        /// <param name="z">Specific z-axis component value.</param>
        public Point3D(T x, T y, T z)
        {
            _X = x;
            _Y = y;
            _Z = z;
        }

        #endregion
    }
    

    /// <summary>
    /// ImageProcessor class provides functionalities of image processing for DScanner.
    /// </summary>
    public class ImageProcessor
    {
        private const int _CountXThreshold = 5; // Threshold to determine if current line is considered shot by laser bean

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

        /// <summary>
        /// Preprocess an image and extract the positions of laser bean 
        /// points on the screen all in one step. The coordinates of laser 
        /// bean points refer to the central screen as the origin, taking 
        /// left as positive x and up as positive y.
        /// </summary>
        /// <param name="bmp">The image to process and extract points from.</param>
        /// <param name="bias">The threshold bias of brightest areas.</param>
        /// <returns>The set of laser bean points.</returns>
        public static Point2D<int>[] OneStepExtractLaserBeanPoints(ref Bitmap bmp, int bias)
        {
            int max;
            int lowerBoundary;
            int pixelCount;
            int[] rgbsum = new int[bmp.Width * bmp.Height];
            int sumX;
            int countX;
            List<Point2D<int>> points = new List<Point2D<int>>();
            
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
                        // Calculate the average X position
                        int ave_X = (int)(sumX / countX);

                        // Set the pointer
                        pImage -= (bmpdata.Width - ave_X) * 3;

                        // Set brightness
                        pImage[0] = 255;
                        pImage[1] = 255;
                        pImage[2] = 255;

                        // Add to the laser bean point set
                        points.Add(new Point2D<int>(bmpdata.Width / 2 - ave_X, bmpdata.Height / 2 - y));

                        // point to the head of next line
                        pImage += bmpdata.Stride - ave_X * 3;
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

            // Return laser bean point set
            return points.ToArray();
        }

        /// <summary>
        /// Convert a set of laser bean point frames to three dimensional coordinates.
        /// It is assumed that the laser bean point frames cover a full vision of the 
        /// scanned object of 360 degrees.
        /// </summary>
        /// <param name="pointFrames">The set of laser bean point frames.</param>
        /// <param name="angleLaserCamera">Angle between the laser bean and camera.</param>
        /// <returns>The set of three dimensional coordinates.</returns>
        public static Point3D<float>[] ConvertTo3DCoordinates(ref Point2D<int>[][] pointFrames, float angleLaserCamera)
        {
            List<Point3D<float>> pointClouds = new List<Point3D<float>>();

            // Obtain the number of frames
            int frameCount = pointFrames.Length;

            // Calculate the laser camera factor
            float lcFactor = 1.0f / (float)(Math.Sin(angleLaserCamera * Math.PI / 180.0));

            // Process each point frame
            for (int frame = 0; frame < frameCount; ++frame)
            {
                // Calculate the angle theta
                float theta = (float)((((double)frame) * 360.0 / ((double)frameCount)) * Math.PI / 180.0);

                // Process each point in the point frame
                for (int i = 0; i < pointFrames[frame].Length; ++i)
                {
                    // d * sin(angleLC) = Px  ==>  r = Px = d / sin(angleLC)
                    float r = ((float)pointFrames[frame][i].X) * lcFactor;

                    // x = r * cos(theta); y = r * sin(theta);
                    pointClouds.Add(new Point3D<float>(r * ((float)Math.Cos(theta)), r * ((float)Math.Sin(theta)), pointFrames[frame][i].Y));
                }
            }

            // Return the point clouds
            return pointClouds.ToArray();
        }
    }
}
