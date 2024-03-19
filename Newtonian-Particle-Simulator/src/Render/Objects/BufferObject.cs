using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Newtonian_Particle_Simulator.Render.Objects
{
    public class BufferObject : IDisposable
    {
        public readonly int ID;
        public nint Size { get; private set; }

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

        public void SubData<T>(nint offset, nint size, T data) where T : struct
        {
            GL.NamedBufferSubData(ID, offset, size, ref data);
        }
        public void SubData<T>(nint offset, nint size, T[] data) where T : struct
        {
            GL.NamedBufferSubData(ID, offset, size, data);
        }
        public void SubData(nint offset, nint size, IntPtr data)
        {
            GL.NamedBufferSubData(ID, offset, size, data);
        }

        public void MutableAllocate<T>(nint size, T data, BufferUsageHint bufferUsageHint) where T : struct
        {
            GL.NamedBufferData(ID, size, ref data, bufferUsageHint);
            Size = size;
        }
        public void MutableAllocate<T>(nint size, T[] data, BufferUsageHint bufferUsageHint) where T : struct
        {
            GL.NamedBufferData(ID, size, data, bufferUsageHint);
            Size = size;
        }
        public void MutableAllocate(nint size, IntPtr data, BufferUsageHint bufferUsageHint)
        {
            GL.NamedBufferData(ID, size, data, bufferUsageHint);
            Size = size;
        }

        public void ImmutableAllocate<T>(nint size, T data, BufferStorageFlags bufferStorageFlags) where T : struct
        {
            GL.NamedBufferStorage(ID, size, ref data, bufferStorageFlags);
            Size = size;
        }
        public void ImmutableAllocate<T>(nint size, T[] data, BufferStorageFlags bufferStorageFlags) where T : struct
        {
            GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
            Size = size;
        }
        public void ImmutableAllocate(nint size, IntPtr data, BufferStorageFlags bufferStorageFlags)
        {
            GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
            Size = size;
        }

        public void GetSubData<T>(nint offset, nint size, out T data) where T : struct
        {
            data = new T();
            GL.GetNamedBufferSubData(ID, offset, size, ref data);
        }
        public void GetSubData<T>(nint offset, nint size, T[] data) where T : struct
        {
            GL.GetNamedBufferSubData(ID, offset, size, data);
        }
        public void GetSubData(nint offset, nint size, out IntPtr data)
        {
            data = Marshal.AllocHGlobal(size);
            GL.GetNamedBufferSubData(ID, offset, size, data);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ID);
        }
    }
}
