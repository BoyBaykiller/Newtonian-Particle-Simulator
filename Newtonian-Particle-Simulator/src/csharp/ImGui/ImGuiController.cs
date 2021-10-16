using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Newtonian_Particle_Simulator.Render.Objects;
using ImGuiNET;

namespace Newtonian_Particle_Simulator.GUI
{
    /// <summary>
    /// This ImGui wrapper is from <see href="https://github.com/mellinoe/ImGui.NET">here</see>.
    /// I modified it to make it work tighter with my project
    /// </summary>
    public class ImGuiController : IDisposable
    {
        private bool frameBegun;

        private VAO vao;
        private ShaderProgram shaderProgram;
        private Texture fontTexture;
        private BufferObject vbo;
        private BufferObject ebo;
        
        private int Width;
        private int Height;

        private System.Numerics.Vector2 scaleFactor = System.Numerics.Vector2.One;
        public ImGuiController(int width, int height, string iniFilePath = null)
        {
            Width = width;
            Height = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            ImGui.LoadIniSettingsFromDisk(iniFilePath);

            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.AddFontDefault();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            Width = width;
            Height = height;
        }

        private void CreateDeviceResources()
        {
            vbo = new BufferObject();
            vbo.MutableAllocate(10000, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            ebo = new BufferObject();
            ebo.MutableAllocate(2000, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            CreateFontDeviceTexture();

            string vertexSource = @"#version 330 core

                uniform mat4 projection_matrix;

                layout(location = 0) in vec2 in_position;
                layout(location = 1) in vec2 in_texCoord;
                layout(location = 2) in vec4 in_color;

                out vec4 color;
                out vec2 texCoord;

                void main()
                {
                    gl_Position = projection_matrix * vec4(in_position, 0, 1);
                    color = in_color;
                    texCoord = in_texCoord;
                }";
            string fragmentSource = @"#version 330 core

                uniform sampler2D in_fontTexture;

                in vec4 color;
                in vec2 texCoord;

                out vec4 outputColor;

                void main()
                {
                    outputColor = color * texture(in_fontTexture, texCoord);
                }";

            shaderProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, vertexSource), new Shader(ShaderType.FragmentShader, fragmentSource));

            vao = new VAO(vbo, ebo, Unsafe.SizeOf<ImDrawVert>());
            vao.SetAttribFormat(0, 2, VertexAttribType.Float, 0 * sizeof(float));
            vao.SetAttribFormat(1, 2, VertexAttribType.Float, 2 * sizeof(float));
            vao.SetAttribFormat(2, 4, VertexAttribType.UnsignedByte, 4 * sizeof(float), true);
        }

        private void CreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);

            fontTexture = new Texture(TextureTarget2d.Texture2D);
            fontTexture.ImmutableAllocate(width, height, 1, SizedInternalFormat.Rgba8);
            fontTexture.SubTexture2D(width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);


            io.Fonts.SetTexID((IntPtr)fontTexture.ID);
            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            if (frameBegun)
            {
                frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (frameBegun)
                ImGui.Render();

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(Width / scaleFactor.X, Height / scaleFactor.Y);
            io.DisplayFramebufferScale = scaleFactor;
            io.DeltaTime = deltaSeconds;
        }

        private readonly List<char> pressedChars = new List<char>();
        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MouseDown[0] = MouseManager.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = MouseManager.IsButtonDown(MouseButton.Right);
            io.MouseDown[2] = MouseManager.IsButtonDown(MouseButton.Middle);

            System.Drawing.Point screenPoint = new System.Drawing.Point(MouseManager.WindowPositionX, MouseManager.WindowPositionY);
            System.Drawing.Point point = wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            io.MouseWheel = MouseManager.DeltaScrollY;
            io.MouseWheelH = MouseManager.DeltaScrollX;

            foreach (Key key in Enum.GetValues(typeof(Key)))
                io.KeysDown[(int)key] = KeyboardManager.IsKeyDown(key);

            for (int i = 0; i < pressedChars.Count; i++)
                io.AddInputCharacter(pressedChars[i]);

            pressedChars.Clear();

            io.KeyCtrl = KeyboardManager.IsKeyDown(Key.ControlLeft) || KeyboardManager.IsKeyDown(Key.ControlRight);
            io.KeyAlt = KeyboardManager.IsKeyDown(Key.AltLeft) || KeyboardManager.IsKeyDown(Key.AltRight);
            io.KeyShift = KeyboardManager.IsKeyDown(Key.ShiftLeft) || KeyboardManager.IsKeyDown(Key.ShiftRight);
            io.KeySuper = KeyboardManager.IsKeyDown(Key.WinLeft) || KeyboardManager.IsKeyDown(Key.WinRight);
        }

        public void PressChar(char keyChar)
        {
            pressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0)
                return;

            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[i];
                int vertexSize = cmdList.VtxBuffer.Size * vao.VertexSize;
                if (vertexSize > vbo.Size)
                {
                    int newSize = (int)Math.Max(vbo.Size * 1.5f, vertexSize);
                    vbo.MutableAllocate(newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }

                int indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > ebo.Size)
                {
                    int newSize = (int)Math.Max(ebo.Size * 1.5f, indexSize);
                    ebo.MutableAllocate(newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
            }

            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(0.0f, io.DisplaySize.X, io.DisplaySize.Y, 0.0f, -1.0f, 1.0f);

            shaderProgram.Use();
            shaderProgram.Upload("projection_matrix", mvp);
            shaderProgram.Upload("in_fontTexture", 0);

            vao.Bind();

            drawData.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One);

            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = drawData.CmdListsRange[i];

                vbo.SubData(0, cmd_list.VtxBuffer.Size * vao.VertexSize, cmd_list.VtxBuffer.Data);
                ebo.SubData(0, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);

                int idx_offset = 0;

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.BindTextureUnit(0, (int)pcmd.TextureId);

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, Height - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), 0);
                        else
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
            }
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            fontTexture.Dispose();
            shaderProgram.Dispose();
            vao.Dispose();
        }
    }
}