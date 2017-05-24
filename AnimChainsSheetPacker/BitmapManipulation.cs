using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AnimChainsSheetPacker
{
    /// <summary>OMG IT'S UNSAFE !!</summary>
    internal sealed class BitmapManipulation
    {
        internal static void SetTransparentColor(Bitmap bitmap, Color color)
        {
            if (bitmap == null)
                throw new AnimChainsSheetPackerException("BitmapManipulation.SetTransparentColor(): Bitmap is null.", AnimChainsSheetPackerErrorCode.OutputImage_Error);

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb && bitmap.PixelFormat != PixelFormat.Format32bppPArgb)
                throw new AnimChainsSheetPackerException("BitmapManipulation.SetTransparentColor(): Image pixel format not supported. Only 32 bit RGB-A files are supported.", AnimChainsSheetPackerErrorCode.OutputImage_Error);

            try
            {
                BitmapData lockedBitmap = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                        ImageLockMode.ReadWrite, 
                        bitmap.PixelFormat
                    );

                #region img conversion & write
                unsafe
                {
                    ///////////////////////////////
                    int pixelByleSize = 4; // 32 bits
                    byte* scan0 = (byte*)lockedBitmap.Scan0;

                    // traverse y
                    for (int y = 0; y < lockedBitmap.Height; y++)
                    {
                        // get mem adress of start of row
                        byte* scan = scan0 + (y * lockedBitmap.Stride);

                        // traverse x
                        for (int x = 0; x < lockedBitmap.Width; x++)
                        {
                            int pixelIndex = x * pixelByleSize;

                            /*
                            scan[pixelIndex]        // R
                            scan[pixelIndex + 1]    // G
                            scan[pixelIndex + 2]    // B
                            scan[pixelIndex + 3]    // A
                            */

                            // if alpha is full transarent
                            if (scan[pixelIndex + 3] == 0)
                            {
                                // write requested color
                                scan[pixelIndex] = color.R;
                                scan[pixelIndex + 1] = color.G;
                                scan[pixelIndex + 2] = color.B;
                                //scan[pixelIndex + 3] = 0;
                            }
                        }
                    }
                    ////////////////////////////
                }
                #endregion

                bitmap.UnlockBits(lockedBitmap);
            }
            catch (Exception ex)
            {
                throw new Exception("BitmapManipulation.SetTransparentColor() cause exception when writing to bitmap.", ex);
            }
        }
    }
}
