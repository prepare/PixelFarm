﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;

using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.LibJpeg
{
    /// <summary>
    /// Main class for work with JPEG images.
    /// </summary>
#if EXPOSE_LIBJPEG
    public
#endif
    sealed class JpegImage : IDisposable
    {
        bool m_alreadyDisposed;

        /// <summary>
        /// Description of image pixels (samples)
        /// </summary>
        List<SampleRow> _rows = new List<SampleRow>();

        int _width;
        int _height;
        byte _bitsPerComponent;
        byte _componentsPerSample;
        Colorspace _colorspace;

        // Fields below (m_compressedData, m_decompressedData, m_bitmap) are not initialized in constructors necessarily.
        // Instead direct access to these field you should use corresponding properties (compressedData, decompressedData, bitmap)
        // Such agreement allows to load required data (e.g. compress image) only by request.

        /// <summary>
        /// Bytes of jpeg image. Refreshed when m_compressionParameters changed.
        /// </summary>
        MemoryStream m_compressedData;

        /// <summary>
        /// Current compression parameters corresponding with compressed data.
        /// </summary>
        CompressionParameters m_compressionParameters;

        /// <summary>
        /// Bytes of decompressed image (bitmap)
        /// </summary>
        MemoryStream m_decompressedData;

        /// <summary>
        /// Creates <see cref="JpegImage"/> from stream with an arbitrary image data
        /// </summary>
        /// <param name="imageData">Stream containing bytes of image in 
        /// arbitrary format (BMP, Jpeg, GIF, PNG, TIFF, e.t.c)</param>
        public JpegImage(Stream imageData, IDecompressDestination dst)
        {
            //new DecompressorToJpegImage(this)
            createFromStream(imageData, dst);
        }
        public JpegImage(Stream imageData)
        {
            createFromStream(imageData, new DecompressorToJpegImage(this));
        }
        ///// <summary>
        ///// Creates <see cref="JpegImage"/> from pixels
        ///// </summary>
        ///// <param name="sampleData">Description of pixels.</param>
        ///// <param name="colorspace">Colorspace of image.</param>
        ///// <seealso cref="SampleRow"/>
        //public JpegImage(SampleRow[] sampleData, Colorspace colorspace)
        //{
        //    if (sampleData == null)
        //        throw new ArgumentNullException("sampleData");

        //    if (sampleData.Length == 0)
        //        throw new ArgumentException("sampleData must be no empty");

        //    if (colorspace == Colorspace.Unknown)
        //        throw new ArgumentException("Unknown colorspace");

        //    m_rows = new List<SampleRow>(sampleData);

        //    SampleRow firstRow = m_rows[0];
        //    m_width = firstRow.Length;
        //    m_height = m_rows.Count;

        //    Sample firstSample = firstRow[0];
        //    m_bitsPerComponent = firstSample.BitsPerComponent;
        //    m_componentsPerSample = firstSample.ComponentCount;
        //    m_colorspace = colorspace;
        //}

        //#if !NETSTANDARD
        //        /// <summary>
        //        /// Creates <see cref="JpegImage"/> from <see cref="System.Drawing.Bitmap">.NET bitmap</see>
        //        /// </summary>
        //        /// <param name="bitmap">Source .NET bitmap.</param>
        //        /// <returns>Created instance of <see cref="JpegImage"/> class.</returns>
        //        /// <remarks>Same as corresponding <see cref="M:BitMiracle.LibJpeg.JpegImage.#ctor(System.Drawing.Bitmap)">constructor</see>.</remarks>
        //        public static JpegImage FromBitmap(Bitmap bitmap)
        //        {
        //            return new JpegImage(bitmap);
        //        }
        //#endif

        /// <summary>
        /// Frees and releases all resources allocated by this <see cref="JpegImage"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_alreadyDisposed)
            {
                if (disposing)
                {
                    // dispose managed resources
                    if (m_compressedData != null)
                        m_compressedData.Dispose();

                    if (m_decompressedData != null)
                        m_decompressedData.Dispose();


                }

                // free native resources
                m_compressionParameters = null;
                m_compressedData = null;
                m_decompressedData = null;
                _rows = null;
                m_alreadyDisposed = true;
            }
        }

        /// <summary>
        /// Gets the width of image in <see cref="Sample">samples</see>.
        /// </summary>
        /// <value>The width of image.</value>
        public int Width
        {
            get
            {
                return _width;
            }
            internal set
            {
                _width = value;
            }
        }

        /// <summary>
        /// Gets the height of image in <see cref="Sample">samples</see>.
        /// </summary>
        /// <value>The height of image.</value>
        public int Height
        {
            get
            {
                return _height;
            }
            internal set
            {
                _height = value;
            }
        }

        /// <summary>
        /// Gets the number of color components per <see cref="Sample">sample</see>.
        /// </summary>
        /// <value>The number of color components per sample.</value>
        public byte ComponentsPerSample
        {
            get
            {
                return _componentsPerSample;
            }
            internal set
            {
                _componentsPerSample = value;
            }
        }

        /// <summary>
        /// Gets the number of bits per color component of <see cref="Sample">sample</see>.
        /// </summary>
        /// <value>The number of bits per color component.</value>
        public byte BitsPerComponent
        {
            get
            {
                return _bitsPerComponent;
            }
            internal set
            {
                _bitsPerComponent = value;
            }
        }

        /// <summary>
        /// Gets the colorspace of image.
        /// </summary>
        /// <value>The colorspace of image.</value>
        public Colorspace Colorspace
        {
            get
            {
                return _colorspace;
            }
            internal set
            {
                _colorspace = value;
            }
        }


        /// <summary>
        /// Retrieves the required row of image.
        /// </summary>
        /// <param name="rowNumber">The number of row.</param>
        /// <returns>Image row of samples.</returns>
        public SampleRow GetRow(int rowNumber)
        {
            return _rows[rowNumber];
        }

        /// <summary>
        /// Writes compressed JPEG image to stream.
        /// </summary>
        /// <param name="output">Output stream.</param>
        public void WriteJpeg(Stream output)
        {
            WriteJpeg(output, new CompressionParameters());
        }

        /// <summary>
        /// Compresses image to JPEG with given parameters and writes it to stream.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="parameters">The parameters of compression.</param>
        public void WriteJpeg(Stream output, CompressionParameters parameters)
        {
            compress(parameters);
            compressedData.WriteTo(output);
        }

        /// <summary>
        /// Writes decompressed image data as bitmap to stream.
        /// </summary>
        /// <param name="output">Output stream.</param>
        public void WriteBitmap(Stream output)
        {
            decompressedData.WriteTo(output);
        }


        private MemoryStream compressedData
        {
            get
            {
                if (m_compressedData == null)
                    compress(new CompressionParameters());

                Debug.Assert(m_compressedData != null);
                Debug.Assert(m_compressedData.Length != 0);

                return m_compressedData;
            }
        }

        private MemoryStream decompressedData
        {
            get
            {
                if (m_decompressedData == null)
                    fillDecompressedData();

                Debug.Assert(m_decompressedData != null);

                return m_decompressedData;
            }
        }

        /// <summary>
        /// Needs for DecompressorToJpegImage class
        /// </summary>
        internal void addSampleRow(SampleRow row)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            _rows.Add(row);
        }

        /// <summary>
        /// Checks if imageData contains jpeg image
        /// </summary>
        private static bool isCompressed(Stream imageData)
        {
            if (imageData == null)
                return false;

            if (imageData.Length <= 2)
                return false;

            imageData.Seek(0, SeekOrigin.Begin);
            int first = imageData.ReadByte();
            int second = imageData.ReadByte();
            return (first == 0xFF && second == (int)JPEG_MARKER.SOI);
        }

        private void createFromStream(Stream imageData, IDecompressDestination dst)
        {
            if (imageData == null)
                throw new ArgumentNullException("imageData");

            if (isCompressed(imageData))
            {
                m_compressedData = Utils.CopyStream(imageData);

                decompress(dst);
            }
            else
            {

                throw new NotImplementedException("JpegImage.createFromStream(Stream)");

            }
        }

        private void compress(CompressionParameters parameters)
        {
            Debug.Assert(_rows != null);
            Debug.Assert(_rows.Count != 0);

            RawImage source = new RawImage(_rows, _colorspace);
            compress(source, parameters);
        }

        private void compress(IRawImage source, CompressionParameters parameters)
        {
            Debug.Assert(source != null);

            if (!needCompressWith(parameters))
                return;

            m_compressedData = new MemoryStream();
            m_compressionParameters = new CompressionParameters(parameters);

            Jpeg jpeg = new Jpeg();
            jpeg.CompressionParameters = m_compressionParameters;
            jpeg.Compress(source, m_compressedData);
        }

        private bool needCompressWith(CompressionParameters parameters)
        {
            return m_compressedData == null ||
                   m_compressionParameters == null ||
                   !m_compressionParameters.Equals(parameters);
        }

        private void decompress(IDecompressDestination dst)
        {
            Jpeg jpeg = new Jpeg();
            jpeg.Decompress(compressedData, dst);
        }

        private void fillDecompressedData()
        {
            Debug.Assert(m_decompressedData == null);

            m_decompressedData = new MemoryStream();
            BitmapDestination dest = new BitmapDestination(m_decompressedData);

            Jpeg jpeg = new Jpeg();
            jpeg.Decompress(compressedData, dest);
        }

    }
}
