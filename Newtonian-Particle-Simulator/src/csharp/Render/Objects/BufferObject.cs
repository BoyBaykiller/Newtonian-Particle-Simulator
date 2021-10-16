using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Newtonian_Particle_Simulator.Render.Objects
{
    public class BufferObject : IDisposable
    {
        public static readonly Dictionary<BufferRangeTarget, List<int>> bufferTypeBindingIndexDict = new Dictionary<BufferRangeTarget, List<int>>()
        {
            { BufferRangeTarget.AtomicCounterBuffer, new List<int>() },
            { BufferRangeTarget.ShaderStorageBuffer, new List<int>() },
            { BufferRangeTarget.TransformFeedbackBuffer, new List<int>() },
            { BufferRangeTarget.UniformBuffer, new List<int>() }
        };

        public readonly int ID;
        public int BufferOffset;
        public int Size { get; private set; }

        public BufferObject(BufferRangeTarget bufferRangeTarget, int bindingIndex)
        {
            if (bufferTypeBindingIndexDict[bufferRangeTarget].Contains(bindingIndex))
            {
                Console.WriteLine($"BindingIndex {bindingIndex} is already bound to a {bufferRangeTarget}");
                bufferTypeBindingIndexDict[bufferRangeTarget].Add(bindingIndex);
            }
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
        /// Sets <seealso cref="BufferOffset"/> to 0 and overrides the content with 0
        /// </summary>
        public void Reset()
        {
            BufferOffset = 0;
            GL.NamedBufferSubData(ID, IntPtr.Zero, Size, new byte[Size]);
        }

        public void Append<T>(int size, T data) where T : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)BufferOffset, size, ref data);
            BufferOffset += size;
        }
        public void Append<T>(int size, T[] data) where T : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)BufferOffset, size, data);
            BufferOffset += size;
        }
        public void Append(int size, IntPtr data)
        {
            GL.NamedBufferSubData(ID, (IntPtr)BufferOffset, size, data);
            BufferOffset += size;
        }

        public void SubData<T>(int offset, int size, T data) where T : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, ref data);
            BufferOffset = offset + size;
        }
        public void SubData<T>(int offset, int size, T[] data) where T : struct
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, data);
            BufferOffset = offset + size;
        }
        public void SubData(int offset, int size, IntPtr data)
        {
            GL.NamedBufferSubData(ID, (IntPtr)offset, size, data);
            BufferOffset = offset + size;
        }

        public void MutableAllocate<T>(int size, T data, BufferUsageHint bufferUsageHint) where T : struct
        {
            GL.NamedBufferData(ID, size, ref data, bufferUsageHint);
            BufferOffset = 0;
            Size = size;
        }
        public void MutableAllocate<T>(int size, T[] data, BufferUsageHint bufferUsageHint) where T : struct
        {
            GL.NamedBufferData(ID, size, data, bufferUsageHint);
            BufferOffset = 0;
            Size = size;
        }
        public void MutableAllocate(int size, IntPtr data, BufferUsageHint bufferUsageHint)
        {
            GL.NamedBufferData(ID, size, data, bufferUsageHint);
            BufferOffset = 0;
            Size = size;
        }

        public void ImmutableAllocate<T>(int size, T data, BufferStorageFlags bufferStorageFlags) where T : struct
        {
            GL.NamedBufferStorage(ID, size, ref data, bufferStorageFlags);
            BufferOffset = 0;
            Size = size;
        }
        public void ImmutableAllocate<T>(int size, T[] data, BufferStorageFlags bufferStorageFlags) where T : struct
        {
            GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
            BufferOffset = 0;
            Size = size;
        }
        public void ImmutableAllocate(int size, IntPtr data, BufferStorageFlags bufferStorageFlags)
        {
            GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
            BufferOffset = 0;
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
            data = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            GL.GetNamedBufferSubData(ID, (IntPtr)offset, size, data);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ID);
        }
    }
}
