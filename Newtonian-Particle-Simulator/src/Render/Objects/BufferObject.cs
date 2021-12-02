﻿using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Newtonian_Particle_Simulator.Render.Objects
{
    public class BufferObject : IDisposable
    {
        public readonly int ID;
        public int Size { get; private set; }

        public BufferObject(BufferRangeTarget bufferRangeTarget, int bindingIndex)
        {
            GL.CreateBuffers(1, out ID);
            BindBase(bufferRangeTarget, bindingIndex);
        }

        public BufferObject()
        {
            GL.CreateBuffers(1, out ID);
        }

        public void BindBase(BufferRangeTarget bufferRangeTarget, int index)
        {
            GL.BindBufferBase(bufferRangeTarget, index, ID);
        }

        public void Bind(BufferTarget bufferTarget)
        {
            GL.BindBuffer(bufferTarget, ID);
        }

        /// <summary>
        /// Overrides all of this buffers memory with 0
        /// </summary>
        public void Reset()
        {
            IntPtr intPtr = Marshal.AllocHGlobal(Size);
            GL.NamedBufferSubData(ID, IntPtr.Zero, Size, intPtr);
            Marshal.FreeHGlobal(intPtr);
        }

        public void SubData<T>(int offset, int size, T data) where T : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, ref data);
        }
        public void SubData<T>(int offset, int size, T[] data) where T : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, data);
        }
        public void SubData(int offset, int size, IntPtr data)
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, data);
        }

        public void MutableAllocate<T>(int size, T data, BufferUsageHint bufferUsageHint) where T : struct
        {
            GL.NamedBufferData(ID, size, ref data, bufferUsageHint);
            Size = size;
        }
        public void MutableAllocate<T>(int size, T[] data, BufferUsageHint bufferUsageHint) where T : struct
        {
            GL.NamedBufferData(ID, size, data, bufferUsageHint);
            Size = size;
        }
        public void MutableAllocate(int size, IntPtr data, BufferUsageHint bufferUsageHint)
        {
            GL.NamedBufferData(ID, size, data, bufferUsageHint);
            Size = size;
        }

        public void ImmutableAllocate<T>(int size, T data, BufferStorageFlags bufferStorageFlags) where T : struct
        {
            GL.NamedBufferStorage(ID, size, ref data, bufferStorageFlags);
            Size = size;
        }
        public void ImmutableAllocate<T>(int size, T[] data, BufferStorageFlags bufferStorageFlags) where T : struct
        {
            GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
            Size = size;
        }
        public void ImmutableAllocate(int size, IntPtr data, BufferStorageFlags bufferStorageFlags)
        {
            GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
            Size = size;
        }

        public void GetSubData<T>(int offset, int size, out T data) where T : struct
        {
            data = new T();
            GL.GetNamedBufferSubData(ID, (IntPtr)offset, size, ref data);
        }
        public void GetSubData<T>(int offset, int size, T[] data) where T : struct
        {
            GL.GetNamedBufferSubData(ID, (IntPtr)offset, size, data);
        }
        public void GetSubData(int offset, int size, out IntPtr data)
        {
            data = Marshal.AllocHGlobal(size);
            GL.GetNamedBufferSubData(ID, (IntPtr)offset, size, data);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ID);
        }
    }
}
