// File: Proyecto_3D/Serializacion/EscenarioCompletoSerializer.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using OpenTK.Mathematics;
using Proyecto_3D.Core3D;

namespace Proyecto_3D.Serializacion
{
    
    public static class EscenarioCompletoSerializer
    {
        // ---------------- DTOs ----------------
        private sealed class TransformDTO
        {
            public float[] Traslacion { get; set; } = new float[3];
            public float[] Rotacion { get; set; } = new float[3];
            public float[] Escala { get; set; } = new float[3] { 1, 1, 1 };
            public bool Enabled { get; set; } = true;
        }

        private sealed class CaraDTO
        {
            public string Name { get; set; } = "Cara";
            public TransformDTO Transform { get; set; } = new();
            public float[] Color { get; set; } = new float[4] { 1, 1, 1, 1 };
            public float EdgeWidth { get; set; } = 1.5f;

            public float[] Vertices { get; set; } = Array.Empty<float>(); // xyz xyz ...
            public uint[] Triangles { get; set; } = Array.Empty<uint>();  // 3 índices
            public uint[] Edges { get; set; } = Array.Empty<uint>();      // pares de índices
        }

        private sealed class ObjetoDTO
        {
            public string Name { get; set; } = "Objeto";
            public TransformDTO Transform { get; set; } = new();
            public List<CaraDTO> Caras { get; set; } = new();
        }

        private sealed class EscenaDTO
        {
            public string Name { get; set; } = "Escenario";
            public TransformDTO Transform { get; set; } = new();
            public List<ObjetoDTO> Objetos { get; set; } = new();
        }

        // --------------- API pública ---------------
        public static void Save(string path, Escenario escena)
        {
            if (escena is null) throw new ArgumentNullException(nameof(escena));
            var dto = ToDto(escena);

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            var full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, json);
        }

        public static Escenario Load(string path)
        {
            var full = Path.GetFullPath(path);
            if (!File.Exists(full)) throw new FileNotFoundException("No se encontró el archivo.", full);

            var json = File.ReadAllText(full);
            var dto = JsonSerializer.Deserialize<EscenaDTO>(json)
                      ?? throw new InvalidOperationException("JSON inválido para EscenaDTO.");

            return FromDto(dto);
        }

        // --------------- Mapping runtime -> DTO ---------------
        private static EscenaDTO ToDto(Escenario e)
        {
            return new EscenaDTO
            {
                Name = e.Name,
                Transform = ToDto(e.Transform),
                Objetos = e.Hijos.Values.Select(ToDto).ToList()
            };
        }

        private static ObjetoDTO ToDto(Objeto o)
        {
            return new ObjetoDTO
            {
                Name = o.Name,
                Transform = ToDto(o.Transform),
                Caras = o.Hijos.Values.Select(ToDto).ToList()
            };
        }

        private static CaraDTO ToDto(Cara c)
        {
            var (verts, tris, edges) = ExtractGeometry(c); // lanza si no puede extraer

            // Color y EdgeWidth, con defaults si no existen
            Vector4 color = TryGetColor(c) ?? new Vector4(1, 1, 1, 1);
            float edgeWidth = TryGetEdgeWidth(c) ?? 1.5f;

            return new CaraDTO
            {
                Name = c.Name,
                Transform = ToDto(c.Transform),
                Color = new[] { color.X, color.Y, color.Z, color.W },
                EdgeWidth = edgeWidth,
                Vertices = verts,
                Triangles = tris,
                Edges = edges
            };
        }

        private static TransformDTO ToDto(Transform t)
        {
            return new TransformDTO
            {
                Traslacion = new[] { t.Traslacion.X, t.Traslacion.Y, t.Traslacion.Z },
                Rotacion = new[] { t.Rotacion.X, t.Rotacion.Y, t.Rotacion.Z },
                Escala = new[] { t.Escala.X, t.Escala.Y, t.Escala.Z },
                Enabled = t.Enabled
            };
        }

        // --------------- Mapping DTO -> runtime ---------------
        private static Escenario FromDto(EscenaDTO eDto)
        {
            var esc = new Escenario(eDto.Name);
            Apply(esc.Transform, eDto.Transform);

            foreach (var oDto in eDto.Objetos)
            {
                var obj = new Objeto(oDto.Name, esc);
                Apply(obj.Transform, oDto.Transform);

                foreach (var cDto in oDto.Caras)
                {
                    var cara = CreateCara(cDto);
                    obj.Add(cara);
                }

                esc.Add(oDto.Name, obj);
            }

            return esc;
        }

        private static Cara CreateCara(CaraDTO cDto)
        {
            // Requiere un ctor: Cara(string name, float[] vertices, uint[] triangles, uint[] edges)
            var cara = new Cara(cDto.Name, cDto.Vertices, cDto.Triangles, cDto.Edges);

            // TRS
            cara.Transform.Traslacion = ArrV3(cDto.Transform.Traslacion, Vector3.Zero);
            cara.Transform.Rotacion = ArrV3(cDto.Transform.Rotacion, Vector3.Zero);
            cara.Transform.Escala = ArrV3(cDto.Transform.Escala, Vector3.One);
            cara.Transform.Enabled = cDto.Transform.Enabled;

            // Apariencia
            if (cDto.Color is { Length: >= 3 })
                cara.SetColor(new Vector4(
                    cDto.Color[0],
                    cDto.Color[1],
                    cDto.Color[2],
                    cDto.Color.Length > 3 ? cDto.Color[3] : 1f
                ));
            cara.EdgeWidth = cDto.EdgeWidth;

            return cara;
        }

        // --------------- Helpers ---------------
        private static void Apply(Transform t, TransformDTO d)
        {
            t.Traslacion = ArrV3(d.Traslacion, Vector3.Zero);
            t.Rotacion = ArrV3(d.Rotacion, Vector3.Zero);
            t.Escala = ArrV3(d.Escala, Vector3.One);
            t.Enabled = d.Enabled;
        }

        private static Vector3 ArrV3(float[]? a, Vector3 fallback)
        {
            if (a == null) return fallback;
            float x = a.Length > 0 ? a[0] : fallback.X;
            float y = a.Length > 1 ? a[1] : fallback.Y;
            float z = a.Length > 2 ? a[2] : fallback.Z;
            return new Vector3(x, y, z);
        }

        private static (float[] verts, uint[] tris, uint[] edges) ExtractGeometry(Cara c)
        {
            // 1) Métodos públicos esperados
            var mV = c.GetType().GetMethod("GetVertices", BindingFlags.Instance | BindingFlags.Public);
            var mT = c.GetType().GetMethod("GetTriangles", BindingFlags.Instance | BindingFlags.Public);
            var mE = c.GetType().GetMethod("GetEdges", BindingFlags.Instance | BindingFlags.Public);
            if (mV != null && mT != null && mE != null)
            {
                return (
                    (float[])mV.Invoke(c, null)!,
                    (uint[])mT.Invoke(c, null)!,
                    (uint[])mE.Invoke(c, null)!
                );
            }

            // 2) Campos comunes via reflection (por si los guardas en la clase)
            var verts = TryGetField<float[]>(c, new[] { "_vertices", "vertices", "Verts", "Vertices" });
            var tris = TryGetField<uint[]>(c, new[] { "_triangles", "triangles", "Tris", "Triangles" });
            var edges = TryGetField<uint[]>(c, new[] { "_edges", "edges", "Lines", "LineIndices" });
            if (verts != null && tris != null && edges != null)
            {
                return (verts, tris, edges);
            }

            // 3) No se pudo. Sé explícito para evitar “guardé nada”.
            throw new InvalidOperationException(
                "Cara no expone la geometría. Agrega en Cara:\n" +
                "  public float[] GetVertices() => _vertices;\n" +
                "  public uint[]  GetTriangles() => _triangles;\n" +
                "  public uint[]  GetEdges() => _edges;\n" +
                "o guarda esos arrays y nómbralos como _vertices/_triangles/_edges.");
        }

        private static TField? TryGetField<TField>(object obj, string[] names) where TField : class
        {
            var type = obj.GetType();
            foreach (var n in names)
            {
                var f = type.GetField(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (f != null && typeof(TField).IsAssignableFrom(f.FieldType))
                    return (TField?)f.GetValue(obj);
            }
            return null;
        }

        private static Vector4? TryGetColor(Cara c)
        {
            // Espera: GetColor() o campo _color
            var m = c.GetType().GetMethod("GetColor", BindingFlags.Instance | BindingFlags.Public);
            if (m != null) return (Vector4)m.Invoke(c, null)!;

            var f = c.GetType().GetField("_color", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f != null && f.FieldType == typeof(Vector4)) return (Vector4)f.GetValue(c)!;

            return null;
        }

        private static float? TryGetEdgeWidth(Cara c)
        {
            // Espera: propiedad EdgeWidth o campo
            var p = c.GetType().GetProperty("EdgeWidth", BindingFlags.Instance | BindingFlags.Public);
            if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(c)!;

            var f = c.GetType().GetField("EdgeWidth", BindingFlags.Instance | BindingFlags.Public);
            if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(c)!;

            var fPriv = c.GetType().GetField("_edgeWidth", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fPriv != null && fPriv.FieldType == typeof(float)) return (float)fPriv.GetValue(c)!;

            return null;
        }
    }
}
