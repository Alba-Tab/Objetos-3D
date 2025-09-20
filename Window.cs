using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Proyecto_3D.Core3D;
using Proyecto_3D.Serializacion;
using System.Linq;

namespace Proyecto_3D
{
    public class Window : GameWindow
    {
        private Shader _shader = null!;
        private Escenario _escenario = null!;
        private Escenario _ejes = null!;

        private enum ModoTransformacion { Escenario, Objeto }
        private ModoTransformacion _modoActual = ModoTransformacion.Escenario;
        private Objeto? _objetoSeleccionado = null;
        private int _indiceObjetoSeleccionado = 0;

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

            if (_escenario.Hijos.Count > 0)
            {
                _indiceObjetoSeleccionado = 0;
                _objetoSeleccionado = _escenario.Hijos.Values.ElementAt(_indiceObjetoSeleccionado);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var viewProjection = _view * _projection;

            _escenario.Draw(_shader, viewProjection);

            DibujarReflejoEscenario(Transform.MatrizReflexionXY(), viewProjection);

            _ejes.Draw(_shader, viewProjection);
            SwapBuffers();
        }
        

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();

            if (KeyboardState.IsKeyPressed(Keys.J)) SerializarEscenario("escenarioTransformado.json");

            if (KeyboardState.IsKeyPressed(Keys.L)) CargarEscenario("Serializacion/escenario.json");
            if (KeyboardState.IsKeyPressed(Keys.K)) CargarEscenario("Serializacion/escenarioTransformado.json");


            if (KeyboardState.IsKeyPressed(Keys.O))
            {
                _modoActual = _modoActual == ModoTransformacion.Escenario ? ModoTransformacion.Objeto : ModoTransformacion.Escenario;
                Console.WriteLine($"Modo de transformación: {_modoActual}");
            }

            if (KeyboardState.IsKeyPressed(Keys.N))
            {
                if (_escenario.Hijos.Count > 0)
                {
                    _indiceObjetoSeleccionado = (_indiceObjetoSeleccionado + 1) % _escenario.Hijos.Count;
                    _objetoSeleccionado = _escenario.Hijos.Values.ElementAt(_indiceObjetoSeleccionado);
                    Console.WriteLine($"Objeto seleccionado: {_objetoSeleccionado.Name} (Caras: {_objetoSeleccionado.Hijos.Count})");
                }
            }


            float time = (float)e.Time;

            // Traslación (WASD + R/F)
            float t = 1.0f * time;
            var transformDestino = _modoActual == ModoTransformacion.Escenario ?
                    _escenario.Transform : (_objetoSeleccionado?.Transform ?? _escenario.Transform);

            if (KeyboardState.IsKeyDown(Keys.W)) transformDestino.Traslacion += new Vector3(0, 0, -t);
            if (KeyboardState.IsKeyDown(Keys.S)) transformDestino.Traslacion += new Vector3(0, 0, t);
            if (KeyboardState.IsKeyDown(Keys.A)) transformDestino.Traslacion += new Vector3(-t, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.D)) transformDestino.Traslacion += new Vector3(t, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.R)) transformDestino.Traslacion += new Vector3(0, t, 0);
            if (KeyboardState.IsKeyDown(Keys.F)) transformDestino.Traslacion += new Vector3(0, -t, 0);

            // Rotación (Q/E/Z/C)
            float r = 45f * time;
            if (KeyboardState.IsKeyDown(Keys.Q)) transformDestino.Rotacion.Y -= r;
            if (KeyboardState.IsKeyDown(Keys.E)) transformDestino.Rotacion.Y += r;
            if (KeyboardState.IsKeyDown(Keys.Z)) transformDestino.Rotacion.X -= r;
            if (KeyboardState.IsKeyDown(Keys.C)) transformDestino.Rotacion.X += r;

            // Escala (T/G)
            float escDelta = (KeyboardState.IsKeyDown(Keys.T) ? 1f : KeyboardState.IsKeyDown(Keys.G) ? -1f : 0f) * time;
            if (MathF.Abs(escDelta) > float.Epsilon)
            {
                float factor = 1.0f + escDelta;
                transformDestino.Escala *= factor;
                transformDestino.Escala = new Vector3(
                    MathF.Max(0.1f, transformDestino.Escala.X),
                    MathF.Max(0.1f, transformDestino.Escala.Y),
                    MathF.Max(0.1f, transformDestino.Escala.Z)
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

        private void SerializarEscenario(string name)
        {
            EscenarioCompletoSerializer.Save("Serializacion/" + name, _escenario);
            Console.WriteLine("Escenario guardados en Serializacion/" + name);
        }

        private void CargarEscenario(String ruta)
        {
            _escenario.Dispose();
            _escenario = EscenarioCompletoSerializer.Load(ruta);
            Console.WriteLine("Escena cargada desde " + ruta);

            if (_escenario.Hijos.Count > 0)
            {
                _indiceObjetoSeleccionado %= _escenario.Hijos.Count;
                _objetoSeleccionado = _escenario.Hijos.Values.ElementAt(_indiceObjetoSeleccionado);
            }
            else
            {
                _objetoSeleccionado = null;
                _indiceObjetoSeleccionado = 0;
            }
        }
        
        private void DibujarReflejoEscenario(Matrix4 matrizReflexion, Matrix4 viewProjection)
        {
            var originales = new List<(Cara cara, Vector4 color)>();
            foreach (var obj in _escenario.Hijos.Values)
                foreach (var cara in obj.Hijos.Values)
                {
                    originales.Add((cara, cara.GetColor()));
                    var c = cara.GetColor();
                    cara.SetColor(new Vector4(c.X, c.Y, c.Z, 0.3f));
                }
            _escenario.Draw(_shader, matrizReflexion * viewProjection);

            foreach (var (cara, color) in originales)
                cara.SetColor(color);
        }

    }
}
