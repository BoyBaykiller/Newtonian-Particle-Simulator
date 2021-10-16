using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Newtonian_Particle_Simulator.Render.Objects;

namespace Newtonian_Particle_Simulator
{
    static class Helper
    {
        public const string SHADER_DIRECTORY_PATH = "res/Shaders/";
        public static readonly int APIMajor = (int)char.GetNumericValue(GL.GetString(StringName.Version)[0]);
        public static readonly int APIMinor = (int)char.GetNumericValue(GL.GetString(StringName.Version)[2]);

        public static string GetPathContent(this string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"{path} does not exist");

            return File.ReadAllText(path);
        }

        public static void ParallelLoadCubemapImages(Texture texture, string[] paths, SizedInternalFormat sizedInternalFormat)
        {
            if (texture.Target != TextureTarget.TextureCubeMap)
                throw new ArgumentException($"texture must be {TextureTarget.TextureCubeMap}");

            if (paths.Length != 6)
                throw new ArgumentException($"Number of images must be equal to six");

            if (!paths.All(p => File.Exists(p)))
                throw new FileNotFoundException($"At least on of the specified paths is invalid");

            Bitmap[] bitmaps = new Bitmap[6];
            Task.Run(() =>
            {
                Parallel.For(0, 6, i =>
                {
                    bitmaps[i] = new Bitmap(paths[i]);
                });
            }).Wait();
            if (!bitmaps.All(i => i.Width == i.Height && i.Width == bitmaps[0].Width))
                throw new ArgumentException($"Individual cubemap textures must be squares and every texture must be of the same size");
            
            int size = bitmaps[0].Width;
            texture.ImmutableAllocate(size, size, 1, sizedInternalFormat);
            for (int i = 0; i < 6; i++)
            {
                System.Drawing.Imaging.BitmapData bitmapData = bitmaps[i].LockBits(new Rectangle(0, 0, size, size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                texture.SubTexture3D(size, size, 1, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0, 0, 0, 0, i);
                bitmaps[i].UnlockBits(bitmapData);
                bitmaps[i].Dispose();
            }
        }

        public static Vector3 RandomUnitVector(Random rng)
        {
            float z = (float)rng.NextDouble() * 2.0f - 1.0f;
            float a = (float)rng.NextDouble() * 2.0f * MathF.PI;
            float r = MathF.Sqrt(1.0f - z * z);
            float x = r * MathF.Cos(a);
            float y = r * MathF.Sin(a);

            return new Vector3(x, y, z);
        }

        private static IEnumerable<string> GetExtensions()
        {
            for (int i = 0; i < GL.GetInteger(GetPName.NumExtensions); i++)
                yield return GL.GetString(StringNameIndexed.Extensions, i);
        }

        private static readonly HashSet<string> glExtensions = new HashSet<string>(GetExtensions());


        /// <summary>
        /// Extensions are not part of any GL specification. Some of them are widely implemented on any hardware, others are supported only on specific vendors like NVIDIA and newer hardware.
        /// </summary>
        /// <param name="extension">The extension to check against. Examples: GL_ARB_bindless_texture or WGL_EXT_swap_control</param>
        /// <returns>True if the extension is available</returns>
        public static bool IsExtensionsAvailable(string extension)
        {
            return glExtensions.Contains(extension);
        }

        /// <summary>
        /// Core Extensions are those which are core in a specific version and are very likely to be supported in following releases as well. There functionality may also be available in lower GL versions.
        /// See all core extensions <see href="https://www.khronos.org/opengl/wiki/History_of_OpenGL#Summary_of_version_changes">here</see>
        /// </summary>
        /// <param name="extension">The extension to check against. Examples: GL_ARB_direct_state_access or GL_ARB_compute_shader</param>
        /// <param name="firstMajor">The major API version the extension became part of the core profile</param>
        /// <param name="firstMinor">The minor API version the extension became part of the core profile</param>
        /// <param name="lastMajor">The last major API version the extension was part of the core profile</param>
        /// <param name="lastMinor">The last minor API version the extension was part of the core profile</param>
        /// <returns>True if this OpenGL version is in the specified range or the extension is otherwise available</returns>
        public static bool IsCoreExtensionAvailable(string extension, int firstMajor, int firstMinor, int lastMajor = 4, int lastMinor = 6)
        {
            int firstVersion = Convert.ToInt32($"{firstMajor}{firstMinor}");
            int lastVersion = Convert.ToInt32($"{lastMajor}{lastMinor}");
            int thisVersion = Convert.ToInt32($"{APIMajor}{APIMinor}");

            return (thisVersion >= firstVersion && thisVersion <= lastVersion) || IsExtensionsAvailable(extension);
        }
    }
}