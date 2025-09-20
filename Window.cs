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
            float escDelta = 0f;
            if (KeyboardState.IsKeyDown(Keys.T)) escDelta = 1f * time;
            else if (KeyboardState.IsKeyDown(Keys.G)) escDelta = -1f * time;

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

            // Cambiar a color rosa la primera cara de la CPU
            var cpu = _escenario.Hijos.Values.FirstOrDefault(obj => obj.Name == "CPU");
            if (cpu != null && cpu.Hijos.Count > 0)
            {
                var cara = cpu.Hijos.Values.First();
                cara.SetColor(new Vector4(1f, 0.4f, 0.7f, 1f)); // Rosa
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
// =======================
        // Construcción de escena
        // =======================
        private Escenario BuildEjes()
        {
            var ejes = new Escenario("Ejes");

            var redTransparent   = new Vector4(1f, 0f, 0f, 0.1f);
            var greenTransparent = new Vector4(0f, 1f, 0f, 0.1f);
            var blueTransparent  = new Vector4(0f, 0f, 1f, 0.1f);

            // Cubos ultrafinos que hacen de planos
            AgregarCubo(ejes, "Eje YZ", Vector3.Zero, new Vector3(0.0001f, 10f, 10f), redTransparent);
            AgregarCubo(ejes, "Eje XZ", Vector3.Zero, new Vector3(10f, 0.0001f, 10f), greenTransparent);
            AgregarCubo(ejes, "Eje XY", Vector3.Zero, new Vector3(10f, 10f, 0.0001f), blueTransparent);


            return ejes;
        }

        private void AgregarCubo(Escenario esc, string nombre, Vector3 posicion, Vector3 dimensiones, Vector4 color)
        {
            var cara = CrearCubo(dimensiones, color);   // esta función ya crea triángulos y edges
            var obj = new Objeto(nombre, esc);
            obj.Transform.Traslacion = posicion;
            obj.Add(cara);
            esc.Add(nombre, obj);
        }
        // Escena de PC de escritorio
        private Escenario BuildPC()
        {
            var escenario = new Escenario("PC");

            // CPU
            var cpu = CrearObjetoCubo(
                "CPU", escenario,
                posicion: new Vector3(-0.7f, 0.4f, 0f),
                dimensiones: new Vector3(0.35f, 0.80f, 0.40f),
                color: new Vector4(0.25f, 0.26f, 0.28f, 1f)
            );
            escenario.Add("CPU", cpu);

            // Monitor compuesto
            var monitor = CrearMonitor(
                parent: escenario,
                basePos: new Vector3(0f, 0.015f, 0f),
                baseDim: new Vector3(0.45f, 0.03f, 0.25f),
                baseColor: new Vector4(0.25f, 0.26f, 0.28f, 1f),
                soportePos: new Vector3(0f, 0.15f, 0f),
                soporteDim: new Vector3(0.06f, 0.25f, 0.06f),
                soporteColor: new Vector4(0.25f, 0.26f, 0.28f, 1f),
                pantallaPos: new Vector3(0f, 0.5f, 0f),
                pantallaDim: new Vector3(0.90f, 0.55f, 0.035f),
                pantallaColor: new Vector4(0.08f, 0.08f, 0.10f, 1f),
                contenidoPos: new Vector3(0f, 0.5f, 0.018f),
                contenidoDim: new Vector3(0.86f, 0.50f, 0.004f),
                contenidoColor: new Vector4(0.23f, 0.45f, 0.85f, 1f)
            );
            escenario.Add("Monitor", monitor);

            // Teclado
            var teclado = CrearObjetoCubo(
                "Teclado", escenario,
                posicion: new Vector3(0f, 0.01f, 0.30f),
                dimensiones: new Vector3(0.40f, 0.02f, 0.20f),
                color: new Vector4(0.75f, 0.75f, 0.78f, 1f)
            );
            escenario.Add("Teclado", teclado);

            // Mouse
            var mouse = CrearObjetoCubo(
                "Mouse", escenario,
                posicion: new Vector3(0.32f, 0.015f, 0.30f),
                dimensiones: new Vector3(0.10f, 0.03f, 0.06f),
                color: new Vector4(0.75f, 0.75f, 0.78f, 1f)
            );
            escenario.Add("Mouse", mouse);

            return escenario;
        }

        // Helpers de construcción

        private Objeto CrearObjetoCubo(string nombre, Escenario parent, Vector3 posicion, Vector3 dimensiones, Vector4 color)
        {
            var cara = CrearCubo(dimensiones, color);
            var obj = new Objeto(nombre, parent);
            obj.Transform.Traslacion = posicion;
            obj.Add(cara);
            return obj;
        }

        private Objeto CrearMonitor(
            Escenario parent,
            Vector3 basePos, Vector3 baseDim, Vector4 baseColor,
            Vector3 soportePos, Vector3 soporteDim, Vector4 soporteColor,
            Vector3 pantallaPos, Vector3 pantallaDim, Vector4 pantallaColor,
            Vector3 contenidoPos, Vector3 contenidoDim, Vector4 contenidoColor)
        {
            var monitor = new Objeto("Monitor", parent);

            var caraBase = CrearCubo(baseDim, baseColor);
            caraBase.Transform.Traslacion = basePos;
            monitor.Add(caraBase);

            var caraSoporte = CrearCubo(soporteDim, soporteColor);
            caraSoporte.Transform.Traslacion = soportePos;
            monitor.Add(caraSoporte);

            var caraPantalla = CrearCubo(pantallaDim, pantallaColor);
            caraPantalla.Transform.Traslacion = pantallaPos;
            monitor.Add(caraPantalla);

            var caraContenido = CrearCubo(contenidoDim, contenidoColor);
            caraContenido.Transform.Traslacion = contenidoPos;
            monitor.Add(caraContenido);

            return monitor;
        }

        // dentro de Window.CrearCubo(...)
        private Cara CrearCubo(Vector3 dimensiones, Vector4 color)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            var vertices = new float[]
            {
                -hx, -hy,  hz,  hx, -hy,  hz,  hx,  hy,  hz,  -hx,  hy,  hz,
                -hx, -hy, -hz,  hx, -hy, -hz,  hx,  hy, -hz,  -hx,  hy, -hz,
            };

            var tri = new uint[]
            {
                0,1,2, 0,2,3,   5,4,7, 5,7,6,
                4,0,3, 4,3,7,   1,5,6, 1,6,2,
                3,2,6, 3,6,7,   4,5,1, 4,1,0
            };

            // 12 aristas (pares)
            var edges = new uint[]
            {
                0,1, 1,2, 2,3, 3,0,
                4,5, 5,6, 6,7, 7,4,
                0,4, 1,5, 2,6, 3,7
            };

            // PASA edges aquí
            var cara = new Cara("Cubo", vertices, tri, edges);
            cara.SetColor(color);
            cara.EdgeWidth = 2.0f; // opcional

            return cara;
        }


        private void AddFace(Cara cara, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            cara.Add(new Triangulo(a, b, c));
            cara.Add(new Triangulo(a, c, d));
        }
    }
}
