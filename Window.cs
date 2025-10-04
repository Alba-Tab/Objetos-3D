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

        // Sistema de animación
        private AnimationRecorder _recorder = null!;
        private AnimationPlayer _player = null!;
        private AnimationClip? _ultimoClip = null;
        private bool _modoAnimacion = false;

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
            var eye = new Vector3(2f, 1f, 2f);
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

            // Inicializar sistema de animación
            _recorder = new AnimationRecorder(keyframeInterval: 0.1f);
            _player = new AnimationPlayer();

            Console.WriteLine("\n=== SISTEMA DE ANIMACIÓN ===");
            Console.WriteLine("Space: Iniciar grabación");
            Console.WriteLine("Space: Detener grabación y guardar");
            Console.WriteLine("P: Reproducir última animación");
            Console.WriteLine("I: Detener animación");
            Console.WriteLine("============================\n");
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
            
            float deltaTime = (float)e.Time;

            var transformDestino = _modoActual == ModoTransformacion.Escenario ?
                    _escenario.Transform : (_objetoSeleccionado?.Transform ?? _escenario.Transform);

            if (_modoAnimacion && _player.IsPlaying)
                _player.ApplyToTransform(transformDestino, deltaTime);
            else if (!_modoAnimacion)
                TransformacionManual(transformDestino, deltaTime);
            
        }       

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Keys.Escape) Close();

            if (e.Key == Keys.J) SerializarEscenario("escenarioTransformado.json");
            if (e.Key == Keys.L) CargarEscenario("Serializacion/escenario.json");
            if (e.Key == Keys.K) CargarEscenario("Serializacion/escenarioTransformado.json");


            if (e.Key == Keys.O)
            {
                _modoActual = _modoActual == ModoTransformacion.Escenario ? ModoTransformacion.Objeto : ModoTransformacion.Escenario;
                Console.WriteLine($"Modo de transformación: {_modoActual}");
            }

            if (e.Key == Keys.N)
            {
                if (_escenario.Hijos.Count > 0)
                {
                    _indiceObjetoSeleccionado = (_indiceObjetoSeleccionado + 1) % _escenario.Hijos.Count;
                    _objetoSeleccionado = _escenario.Hijos.Values.ElementAt(_indiceObjetoSeleccionado);
                    Console.WriteLine($"Objeto seleccionado: {_objetoSeleccionado.Name} (Caras: {_objetoSeleccionado.Hijos.Count})");
                }
            }

            // === CONTROLES DE ANIMACIÓN ===
            // Space: Iniciar/Detener grabación
            if (e.Key == Keys.Space)
            {
                if (!_recorder.IsRecording)
                {
                    string targetName = _modoActual == ModoTransformacion.Escenario ? "Escenario" : _objetoSeleccionado?.Name ?? "Objeto";
                    _recorder.StartRecording($"Anim_{targetName}");
                    _modoAnimacion = false; // Modo manual para grabar
                }else 
                {
                    _ultimoClip = _recorder.StopRecording();
                }
            }

            // Reproducir última animación
            if (e.Key == Keys.P)
            {
                if (_ultimoClip != null && _ultimoClip.Keyframes.Count > 0)
                {
                    _player.Play(_ultimoClip);
                    _modoAnimacion = true;
                }
                else
                {
                    Console.WriteLine("⚠️ No hay animación para reproducir. Graba una primero (Space para iniciar).");
                }
            }

            // I: Detener animación
            if (e.Key == Keys.I)
            {
                _player.Stop();
                _modoAnimacion = false;
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
        private void TransformacionManual(Transform transformDestino, float deltaTime)
        {            
                // Traslación (WASD + R/F)
                float t = 1.0f * deltaTime;

                if (KeyboardState.IsKeyDown(Keys.W)) transformDestino.Traslacion += new Vector3(0, 0, -t);
                if (KeyboardState.IsKeyDown(Keys.S)) transformDestino.Traslacion += new Vector3(0, 0, t);
                if (KeyboardState.IsKeyDown(Keys.A)) transformDestino.Traslacion += new Vector3(-t, 0, 0);
                if (KeyboardState.IsKeyDown(Keys.D)) transformDestino.Traslacion += new Vector3(t, 0, 0);
                if (KeyboardState.IsKeyDown(Keys.R)) transformDestino.Traslacion += new Vector3(0, t, 0);
                if (KeyboardState.IsKeyDown(Keys.F)) transformDestino.Traslacion += new Vector3(0, -t, 0);

                // Rotación (Q/E/Z/C)
                float r = 45f * deltaTime;
                if (KeyboardState.IsKeyDown(Keys.Q)) transformDestino.Rotacion.Y -= r;
                if (KeyboardState.IsKeyDown(Keys.E)) transformDestino.Rotacion.Y += r;
                if (KeyboardState.IsKeyDown(Keys.Z)) transformDestino.Rotacion.X -= r;            
                if (KeyboardState.IsKeyDown(Keys.C)) transformDestino.Rotacion.X += r;

                // Escala (T/G)
                float escDelta = 0f;
                if (KeyboardState.IsKeyDown(Keys.T)) escDelta = 1f * deltaTime;
                else if (KeyboardState.IsKeyDown(Keys.G)) escDelta = -1f * deltaTime;

                if (escDelta != 0f)
                {
                    float factor = 1.0f + escDelta;
                    transformDestino.Escala *= factor;
                    transformDestino.Escala = new Vector3(
                        MathF.Max(0.1f, transformDestino.Escala.X),
                        MathF.Max(0.1f, transformDestino.Escala.Y),
                        MathF.Max(0.1f, transformDestino.Escala.Z)
                    );
                }

                // Si estamos grabando, capturar el estado actual
                if (_recorder.IsRecording)
                {
                    _recorder.UpdateRecording(deltaTime, transformDestino);
                }
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
