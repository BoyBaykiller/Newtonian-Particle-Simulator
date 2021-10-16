using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;

namespace Newtonian_Particle_Simulator.Render.Objects
{
    class Framebuffer : IDisposable
    {
        private static int lastBindedID = -1;

        private int idRBO;

        public readonly int ID;
        public Framebuffer()
        {
            GL.CreateFramebuffers(1, out ID);
        }

        public void Clear(ClearBufferMask clearBufferMask)
        {
            Bind();
            GL.Clear(clearBufferMask);
        }

        public void AddRenderTarget(FramebufferAttachment framebufferAttachment, Texture texture)
        {
            GL.NamedFramebufferTexture(ID, framebufferAttachment, texture.ID, 0);
        }

        public void SetRenderbuffer(RenderbufferStorage renderbufferStorage, FramebufferAttachment framebufferAttachment, int width, int height)
        {
            GL.CreateRenderbuffers(1, out idRBO);
            GL.NamedRenderbufferStorage(ID, renderbufferStorage, width, height);
            GL.NamedFramebufferRenderbuffer(ID, framebufferAttachment, RenderbufferTarget.Renderbuffer, idRBO);
        }

        public void SetRenderTarget(params DrawBuffersEnum[] drawBuffersEnums)
        {
            GL.NamedFramebufferDrawBuffers(ID, drawBuffersEnums.Length, drawBuffersEnums);
        }
        public void SetReadTarget(ReadBufferMode readBufferMode)
        {
            GL.NamedFramebufferReadBuffer(ID, readBufferMode);
        }

        public void Bind(FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            if (lastBindedID != ID)
            {
                GL.BindFramebuffer(framebufferTarget, ID);
                lastBindedID = ID;
            }  
        }

        public FramebufferStatus GetFBOStatus()
        {
            return GL.CheckNamedFramebufferStatus(ID, FramebufferTarget.Framebuffer);
        }

        public static void Bind(int id, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            if (lastBindedID != id)
            {
                GL.BindFramebuffer(framebufferTarget, id);
                lastBindedID = id;
            }
        }

        public static void Clear(int id, ClearBufferMask clearBufferMask)
        {
            Framebuffer.Bind(id);
            GL.Clear(clearBufferMask);
        }

        public static Bitmap GetBitmapFramebufferAttachment(int id, FramebufferAttachment framebufferAttachment, int width, int height, int x = 0, int y = 0)
        {
            Bitmap bmp = new Bitmap(width, height);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.NamedFramebufferReadBuffer(id, (ReadBufferMode)framebufferAttachment);

            Bind(id, FramebufferTarget.ReadFramebuffer);
            GL.ReadPixels(x, y, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            GL.Finish();

            bmp.UnlockBits(bmpData);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(ID);
        }
    }
}
