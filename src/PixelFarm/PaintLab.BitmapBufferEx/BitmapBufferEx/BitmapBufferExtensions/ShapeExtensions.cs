﻿//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-07-20 11:44:36 +0200 (Mo, 20 Jul 2015) $
//   Changed in:        $Revision: 114480 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapShapeExtensions.cs $
//   Id:                $Id: WriteableBitmapShapeExtensions.cs 114480 2015-07-20 09:44:36Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//

namespace BitmapBufferEx
{
    /// <summary>
    /// Collection of extension methods for the WriteableBitmap class.
    /// </summary>
    public static partial class BitmapBufferExtensions
    {
        /// <summary>
        /// Draws a polyline. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawPolyline(this BitmapBuffer bmp, int[] points, ColorInt color)
        {

            bmp.DrawPolyline(points, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a polyline anti-aliased. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawPolyline(this BitmapBuffer bmp, int[] points, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;
                int x1 = points[0];
                int y1 = points[1];

                int len = points.Length;
                for (int i = 2; i < len; i += 2)
                {

                    DrawLine(context, w, h,
                        x1, y1,
                        x1 += points[i], y1 += points[i + 1], //also update x1,y1 
                        color);

                }
            }
        }


        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="color">The color.</param>
        public static void DrawTriangle(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int x3, int y3, ColorInt color)
        {

            bmp.DrawTriangle(x1, y1, x2, y2, x3, y3, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="color">The color.</param>
        public static void DrawTriangle(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int x3, int y3, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;

                DrawLine(context, w, h, x1, y1, x2, y2, color);
                DrawLine(context, w, h, x2, y2, x3, y3, color);
                DrawLine(context, w, h, x3, y3, x1, y1, color);
            }
        }

        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="x4">The x-coordinate of the 4th point.</param>
        /// <param name="y4">The y-coordinate of the 4th point.</param>
        /// <param name="color">The color.</param>
        public static void DrawQuad(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, ColorInt color)
        {

            bmp.DrawQuad(x1, y1, x2, y2, x3, y3, x4, y4, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="x4">The x-coordinate of the 4th point.</param>
        /// <param name="y4">The y-coordinate of the 4th point.</param>
        /// <param name="color">The color.</param>
        public static void DrawQuad(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;

                DrawLine(context, w, h, x1, y1, x2, y2, color);
                DrawLine(context, w, h, x2, y2, x3, y3, color);
                DrawLine(context, w, h, x3, y3, x4, y4, color);
                DrawLine(context, w, h, x4, y4, x1, y1, color);
            }
        }



        /// <summary>
        /// Draws a rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static void DrawRectangle(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, ColorInt color)
        {

            bmp.DrawRectangle(x1, y1, x2, y2, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static unsafe void DrawRectangle(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;
                int* pixels = context.Pixels._inf32Buffer;

                // Check boundaries
                if ((x1 < 0 && x2 < 0) || (y1 < 0 && y2 < 0)
                 || (x1 >= w && x2 >= w) || (y1 >= h && y2 >= h))
                {
                    return;
                }

                // Clamp boundaries
                if (x1 < 0) { x1 = 0; }
                if (y1 < 0) { y1 = 0; }
                if (x2 < 0) { x2 = 0; }
                if (y2 < 0) { y2 = 0; }
                if (x1 >= w) { x1 = w - 1; }
                if (y1 >= h) { y1 = h - 1; }
                if (x2 >= w) { x2 = w - 1; }
                if (y2 >= h) { y2 = h - 1; }

                int startY = y1 * w;
                int endY = y2 * w;

                int offset2 = endY + x1;
                int endOffset = startY + x2;
                int startYPlusX1 = startY + x1;

                // top and bottom horizontal scanlines
                for (int x = startYPlusX1; x <= endOffset; x++)
                {
                    pixels[x] = color; // top horizontal line
                    pixels[offset2] = color; // bottom horizontal line
                    offset2++;
                }

                // offset2 == endY + x2

                // vertical scanlines
                endOffset = startYPlusX1 + w;
                offset2 -= w;

                for (int y = startY + x2 + w; y <= offset2; y += w)
                {
                    pixels[y] = color; // right vertical line
                    pixels[endOffset] = color; // left vertical line
                    endOffset += w;
                }
            }
        }



        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawEllipse(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, ColorInt color)
        {

            bmp.DrawEllipse(x1, y1, x2, y2, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawEllipse(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Calc center and radius
            int xr = (x2 - x1) >> 1;
            int yr = (y2 - y1) >> 1;
            int xc = x1 + xr;
            int yc = y1 + yr;
            bmp.DrawEllipseCentered(xc, yc, xr, yr, color);
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawEllipseCentered(this BitmapBuffer bmp, int xc, int yc, int xr, int yr, ColorInt color)
        {
            bmp.DrawEllipseCentered(xc, yc, xr, yr, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        public static unsafe void DrawEllipseCentered(this BitmapBuffer bmp, int xc, int yc, int xr, int yr, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            using (BitmapContext context = bmp.GetBitmapContext())
            {

                int* pixels = context.Pixels._inf32Buffer;

                int w = context.Width;
                int h = context.Height;

                // Avoid endless loop
                if (xr < 1 || yr < 1)
                {
                    return;
                }

                // Init vars
                int uh, lh, uy, ly, lx, rx;
                int x = xr;
                int y = 0;
                int xrSqTwo = (xr * xr) << 1;
                int yrSqTwo = (yr * yr) << 1;
                int xChg = yr * yr * (1 - (xr << 1));
                int yChg = xr * xr;
                int err = 0;
                int xStopping = yrSqTwo * xr;
                int yStopping = 0;

                // Draw first set of points counter clockwise where tangent line slope > -1.
                while (xStopping >= yStopping)
                {
                    // Draw 4 quadrant points at once
                    uy = yc + y;                  // Upper half
                    ly = yc - y;                  // Lower half
                    if (uy < 0) uy = 0;          // Clip
                    if (uy >= h) uy = h - 1;      // ...
                    if (ly < 0) ly = 0;
                    if (ly >= h) ly = h - 1;
                    uh = uy * w;                  // Upper half
                    lh = ly * w;                  // Lower half

                    rx = xc + x;
                    lx = xc - x;
                    if (rx < 0) rx = 0;          // Clip
                    if (rx >= w) rx = w - 1;      // ...
                    if (lx < 0) lx = 0;
                    if (lx >= w) lx = w - 1;
                    pixels[rx + uh] = color;      // Quadrant I (Actually an octant)
                    pixels[lx + uh] = color;      // Quadrant II
                    pixels[lx + lh] = color;      // Quadrant III
                    pixels[rx + lh] = color;      // Quadrant IV

                    y++;
                    yStopping += xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                    if ((xChg + (err << 1)) > 0)
                    {
                        x--;
                        xStopping -= yrSqTwo;
                        err += xChg;
                        xChg += yrSqTwo;
                    }
                }

                // ReInit vars
                x = 0;
                y = yr;
                uy = yc + y;                  // Upper half
                ly = yc - y;                  // Lower half
                if (uy < 0) uy = 0;          // Clip
                if (uy >= h) uy = h - 1;      // ...
                if (ly < 0) ly = 0;
                if (ly >= h) ly = h - 1;
                uh = uy * w;                  // Upper half
                lh = ly * w;                  // Lower half
                xChg = yr * yr;
                yChg = xr * xr * (1 - (yr << 1));
                err = 0;
                xStopping = 0;
                yStopping = xrSqTwo * yr;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xStopping <= yStopping)
                {
                    // Draw 4 quadrant points at once
                    rx = xc + x;
                    lx = xc - x;
                    if (rx < 0) rx = 0;          // Clip
                    if (rx >= w) rx = w - 1;      // ...
                    if (lx < 0) lx = 0;
                    if (lx >= w) lx = w - 1;
                    pixels[rx + uh] = color;      // Quadrant I (Actually an octant)
                    pixels[lx + uh] = color;      // Quadrant II
                    pixels[lx + lh] = color;      // Quadrant III
                    pixels[rx + lh] = color;      // Quadrant IV

                    x++;
                    xStopping += yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                    if ((yChg + (err << 1)) > 0)
                    {
                        y--;
                        uy = yc + y;                  // Upper half
                        ly = yc - y;                  // Lower half
                        if (uy < 0) uy = 0;          // Clip
                        if (uy >= h) uy = h - 1;      // ...
                        if (ly < 0) ly = 0;
                        if (ly >= h) ly = h - 1;
                        uh = uy * w;                  // Upper half
                        lh = ly * w;                  // Lower half
                        yStopping -= xrSqTwo;
                        err += yChg;
                        yChg += xrSqTwo;
                    }
                }
            }
        }
    }
}
