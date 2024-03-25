using System;
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

        public unsafe void ImmutableAllocate<T>(nint size, in T data, BufferStorageFlags bufferStorageFlags) where T : unmanaged
        {
            fixed (void* ptr = &data)
            {
                ImmutableAllocate(size, (IntPtr)ptr, bufferStorageFlags);
            }
        }

        public void ImmutableAllocate(nint size, IntPtr data, BufferStorageFlags bufferStorageFlags)
        {
            GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
            Size = size;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ID);
        }
    }
}
