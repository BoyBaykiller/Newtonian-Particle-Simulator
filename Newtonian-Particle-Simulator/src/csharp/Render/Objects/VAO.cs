using System;
using OpenTK.Graphics.OpenGL4;


namespace Newtonian_Particle_Simulator.Render.Objects
{
    class VAO : IDisposable
    {
        private static int lastBindedID = -1;

        public readonly int VertexSize;
        public readonly int ID;
        public VAO(BufferObject vbo, int vertexSize)
        {
            VertexSize = vertexSize;
            GL.CreateVertexArrays(1, out ID);
            GL.VertexArrayVertexBuffer(ID, 0, vbo.ID, IntPtr.Zero, vertexSize);
        }

        public VAO(BufferObject vbo, BufferObject ebo, int stride)
        {
            VertexSize = stride;
            GL.CreateVertexArrays(1, out ID);
            GL.VertexArrayVertexBuffer(ID, 0, vbo.ID, IntPtr.Zero, stride);
            GL.VertexArrayElementBuffer(ID, ebo.ID);
        }

        public void SetAttribFormat(int index, int attribTypeElements, VertexAttribType vertexAttribType, int offset, bool normalize = false)
        {
            GL.EnableVertexArrayAttrib(ID, index);
            GL.VertexArrayAttribFormat(ID, index, attribTypeElements, vertexAttribType, normalize, offset);
            GL.VertexArrayAttribBinding(ID, index, 0);
        }

        public void Bind()
        {
            if (lastBindedID != ID)
            {
                GL.BindVertexArray(ID);
                lastBindedID = ID;
            }
        }

        public static void Bind(int id)
        {
            if (lastBindedID != id)
            {
                GL.BindVertexArray(id);
                lastBindedID = id;
            }
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(ID);
        }
    }
}
