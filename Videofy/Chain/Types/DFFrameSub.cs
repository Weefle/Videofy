﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Videofy.Main;

namespace Videofy.Chain.Types
{
    class DFFrameSubYUV
    {
        private byte[] _frame; // array pinned in memory, permanent OpenCV Mat data storage.
        private GCHandle _handle;// handle of pinned _frame
        private IntPtr ptr; //address of _frame in memory
        private Mat _mat;
        private int _blockPos;
        private int _blockWidth;
        private int _size, _height, _width;
        public int Size { get { return _size; } }


        public DFFrameSubYUV(int width, int height, int blockWidth)
        {
           
            _frame = new byte[height * width];
            _handle = GCHandle.Alloc(_frame, GCHandleType.Pinned);
            ptr = _handle.AddrOfPinnedObject();
            _mat = new Mat(height, width, MatType.CV_8U, ptr);
            _blockPos = 0;
            _blockWidth = blockWidth;
            _size = height * width;
            _height = height;
            _width = width;
        }

        public void Free()
        {
            _mat.Dispose();
            _handle.Free();
        }

        public Boolean IsFull
        {
            get
            {
                return _blockPos >= _size;
            }
        }

        private void BlockPointerInc(int blockWidth)
        {
            _blockPos += blockWidth;
            if ((_blockPos % _width) == 0) 
            {
                // now Pos is at start of second line
                // need to skip N-1 lines to go at start of next blocks row
                _blockPos += _width * (blockWidth /* width = height */ - 1);
            }
        }

        public void SetBlock8(DFFrameBlock block)
        {
            if (IsFull) throw new ArgumentOutOfRangeException();
            int x = _blockPos % _width;
            int y = _blockPos / _width;
            Mat ROI = _mat.SubMat(new Rect(x, y, 8, 8));
            block.Body.CopyTo(ROI);
            BlockPointerInc(8);
        }

        public void SetBlock4(DFFrameBlock block)
        {
            if (IsFull) throw new ArgumentOutOfRangeException();
            int x = _blockPos % _width;
            int y = _blockPos / _width;
            Mat ROI = _mat.SubMat(new Rect(x, y, 4, 4));
            block.Body.CopyTo(ROI);
            BlockPointerInc(4);
        }


        public DFFrameBlock GetBlock8()
        {
            if (IsFull) throw new ArgumentOutOfRangeException();
            int x = _blockPos % _width;
            int y = _blockPos / _width;
            Mat ROI = _mat.SubMat(new Rect(x, y, 8, 8));
            DFFrameBlock block = new DFFrameBlock();
            ROI.CopyTo(block.Body);
            BlockPointerInc(8);
            return block;
        }

        public DFFrameBlock GetBlock4()
        {
            if (IsFull) throw new ArgumentOutOfRangeException();
            int x = _blockPos % _width;
            int y = _blockPos / _width;
            Mat ROI = _mat.SubMat(new Rect(x, y, 4, 4));
            DFFrameBlock block = new DFFrameBlock();
            ROI.CopyTo(block.Body);
            BlockPointerInc(4);
            return block;
        }

        public void FromArray(byte[] array)
        {
            if (array.Length != _frame.Length) throw new ArgumentOutOfRangeException();
            Array.Copy(array, _frame, array.Length);
            _mat = new Mat(_height, _width, MatType.CV_8U, ptr);
        }

        public byte[] ToArray()
        {
            byte[] array = new byte[_frame.Length];
            Array.Copy(_frame, array, _frame.Length);
            return array;
        }

    }
}
