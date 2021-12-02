﻿using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Newtonian_Particle_Simulator.Render.Objects
{
    struct Shader : IDisposable
    {
        public readonly int ID;
        public readonly ShaderType ShaderType;

        public Shader(ShaderType shaderType, string sourceCode)
        {
            ShaderType = shaderType;
            
            ID = GL.CreateShader(shaderType);

            GL.ShaderSource(ID, sourceCode);
            GL.CompileShader(ID);

            string compileInfo = GL.GetShaderInfoLog(ID);
            if (compileInfo != string.Empty)
                Console.WriteLine(compileInfo);
        }

        public void Dispose()
        {
            GL.DeleteShader(ID);
        }
    }

    class ShaderProgram : IDisposable
    {
        private static int lastBindedID = -1;

        public readonly int ID;
        public ShaderProgram(params Shader[] shaders)
        {
            if (shaders is null || shaders.Length == 0 || !shaders.All(s => s.ID != 0))
                throw new IndexOutOfRangeException($"Shader array is empty or null. Or at least one shader has ID 0");

            if(!shaders.All(s => shaders.All(s1 => s.ID == s1.ID || s1.ShaderType != s.ShaderType)))
                throw new Exception($"A ShaderProgram can only hold one instance of every ShaderType. Validate the shader array.");

            ID = GL.CreateProgram();

            for (int i = 0; i < shaders.Length; i++)
                GL.AttachShader(ID, shaders[i].ID);

            GL.LinkProgram(ID);
            for (int i = 0; i < shaders.Length; i++)
            {
                GL.DetachShader(ID, shaders[i].ID);
                shaders[i].Dispose();
            }
        }

        public void Use()
        {
            if (lastBindedID != ID)
            {
                GL.UseProgram(ID);
                lastBindedID = ID;
            }
        }

        public static void Use(int id)
        {
            if (lastBindedID != id)
            {
                GL.UseProgram(id);
                lastBindedID = id;
            }
        }

        public static void UploadToProgram(int id, int location, Matrix4 matrix4, bool transpose = false)
        {
            GL.ProgramUniformMatrix4(id, location, transpose, ref matrix4);
        }
        public void Upload(int location, Matrix4 matrix4, bool transpose = false)
        {
            GL.ProgramUniformMatrix4(ID, location, transpose, ref matrix4);
        }
        public void Upload(string name, Matrix4 matrix4, bool transpose = false)
        {
            GL.ProgramUniformMatrix4(ID, GetUniformLocation(name), transpose, ref matrix4);
        }

        public static void UploadToProgram(int id, int location, Vector4 vector4)
        {
            GL.ProgramUniform4(id, location, vector4);
        }
        public void Upload(int location, Vector4 vector4)
        {
            GL.ProgramUniform4(ID, location, vector4);
        }
        public void Upload(string name, Vector4 vector4)
        {
            GL.ProgramUniform4(ID, GetUniformLocation(name), vector4);
        }

        public static void UploadToProgram(int id, int location, Vector3 vector3)
        {
            GL.ProgramUniform3(id, location, vector3);
        }
        public void Upload(int location, Vector3 vector3)
        {
            GL.ProgramUniform3(ID, location, vector3);
        }
        public void Upload(string name, Vector3 vector3)
        {
            GL.ProgramUniform3(ID, GetUniformLocation(name), vector3);
        }

        public static void UploadToProgram(int id, int location, Vector2 vector2)
        {
            GL.ProgramUniform2(id, location, vector2);
        }
        public void Upload(int location, Vector2 vector2)
        {
            GL.ProgramUniform2(ID, location, vector2);
        }
        public void Upload(string name, Vector2 vector2)
        {
            GL.ProgramUniform2(ID, GetUniformLocation(name), vector2);
        }

        public static void UploadToProgram(int id, int location, float x)
        {
            GL.ProgramUniform1(id, location, x);
        }
        public void Upload(int location, float x)
        {
            GL.ProgramUniform1(ID, location, x);
        }
        public void Upload(string name, float x)
        {
            GL.ProgramUniform1(ID, GetUniformLocation(name), x);
        }

        public static void UploadToProgram(int id, int location, int x)
        {
            GL.ProgramUniform1(id, location, x);
        }
        public void Upload(int location, int x)
        {
            GL.ProgramUniform1(ID, location, x);
        }
        public void Upload(string name, int x)
        {
            GL.ProgramUniform1(ID, GetUniformLocation(name), x);
        }

        public static void UploadToProgram(int id, int location, uint x)
        {
            GL.ProgramUniform1(id, location, x);
        }
        public void Upload(int location, uint x)
        {
            GL.ProgramUniform1(ID, location, x);
        }
        public void Upload(string name, uint x)
        {
            GL.ProgramUniform1(ID, GetUniformLocation(name), x);
        }

        public static void UploadToProgram(int id, int location, bool x)
        {
            GL.ProgramUniform1(id, location, x ? 1 : 0);
        }
        public void Upload(int location, bool x)
        {
            GL.ProgramUniform1(ID, location, x ? 1 : 0);
        }
        public void Upload(string name, bool x)
        {
            GL.ProgramUniform1(ID, GetUniformLocation(name), x ? 1 : 0);
        }

        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(ID, name);
        }


        public void Dispose()
        {
            GL.DeleteProgram(ID);
        }
    }
}