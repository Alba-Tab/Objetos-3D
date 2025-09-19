using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Proyecto_3D.Core3D;
using Proyecto_3D.Serializacion;

namespace Proyecto_3D
{
    public class Window : GameWindow
    {
        private Shader _shader = null!;
        private Escenario _escenario = null!;
        private Escenario _ejes = null!;

        // Cámara
        private Matrix4 _view;
        private Matrix4 _projection;

        public Window()
            : base(GameWindowSettings.Default, new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 600),
                Title = "PC-Objetos 3D"
            })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GraphicsConfig.InicializarGLDefault();

            _shader = new Shader("Assets/shader.vert", "Assets/shader.frag");
            _shader.Use();

            // Cámara inicial
            var eye = new Vector3(1.8f, 1.2f, 2.8f);
            var target = new Vector3(0f, 0.25f, 0f);
            _view = Matrix4.LookAt(eye, target, Vector3.UnitY);

            // Proyección inicial
            float aspect = Size.X / (float)Size.Y;
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f), aspect, 0.1f, 100f);


            _escenario = EscenarioCompletoSerializer.Load("Serializacion/escenario.json");
            _ejes = EscenarioCompletoSerializer.Load("Serializacion/ejes.json");
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var viewProjection = _view * _projection;
            
            _escenario.Draw(_shader, viewProjection);
            _ejes.Draw(_shader, viewProjection);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();

            if (KeyboardState.IsKeyPressed(Keys.J))
            {
                    EscenarioCompletoSerializer.Save("Serializacion/escenario.json", _escenario);
                    Console.WriteLine("Ejes guardados en Serializacion/escenario.json");
            }

            if (KeyboardState.IsKeyPressed(Keys.L))
            {
                    _escenario.Dispose();
                    _escenario = EscenarioCompletoSerializer.Load("Serializacion/escenario.json");
                    Console.WriteLine("Escena cargada desde Serializacion/escenario.json");
            }

            // Controles de transformación del escenario
            float dt = (float)e.Time;

            // Traslación (WASD + R/F)
            float t = 1.0f * dt;
            if (KeyboardState.IsKeyDown(Keys.W)) _escenario.Transform.Traslacion += new Vector3(0, 0, -t);
            if (KeyboardState.IsKeyDown(Keys.S)) _escenario.Transform.Traslacion += new Vector3(0, 0, t);
            if (KeyboardState.IsKeyDown(Keys.A)) _escenario.Transform.Traslacion += new Vector3(-t, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.D)) _escenario.Transform.Traslacion += new Vector3(t, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.R)) _escenario.Transform.Traslacion += new Vector3(0, t, 0);
            if (KeyboardState.IsKeyDown(Keys.F)) _escenario.Transform.Traslacion += new Vector3(0, -t, 0);

            // Rotación (Q/E/Z/C)
            float r = 45f * dt;
            if (KeyboardState.IsKeyDown(Keys.Q)) _escenario.Transform.Rotacion.Y -= r;
            if (KeyboardState.IsKeyDown(Keys.E)) _escenario.Transform.Rotacion.Y += r;
            if (KeyboardState.IsKeyDown(Keys.Z)) _escenario.Transform.Rotacion.X -= r;
            if (KeyboardState.IsKeyDown(Keys.C)) _escenario.Transform.Rotacion.X += r;

            // Escala (T/G)
            float escDelta = (KeyboardState.IsKeyDown(Keys.T) ? 1f : KeyboardState.IsKeyDown(Keys.G) ? -1f : 0f) * dt;
            if (MathF.Abs(escDelta) > float.Epsilon)
            {
                float factor = 1.0f + escDelta;
                _escenario.Transform.Escala *= factor;
                _escenario.Transform.Escala = new Vector3(
                    MathF.Max(0.1f, _escenario.Transform.Escala.X),
                    MathF.Max(0.1f, _escenario.Transform.Escala.Y),
                    MathF.Max(0.1f, _escenario.Transform.Escala.Z)
                );
            }
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

            // Recalcular proyección con el nuevo aspect
            float aspect = e.Width / (float)e.Height;
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f), aspect, 0.1f, 100f);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _escenario?.Dispose();
            _ejes?.Dispose();
            _shader?.Dispose();
        }

    }
}
