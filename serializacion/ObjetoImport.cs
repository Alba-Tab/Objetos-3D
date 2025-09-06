// ObjetoImporter.cs
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Proyecto_3D.Core3D; // Objeto, Cara, Triangulo

namespace Proyecto_3D.Serializacion
{
    internal static class ObjetoImporter
    {
        // --- Helpers: DTO -> OpenTK ---
        private static Vector3 V3(Vec3Dto v) => new Vector3(v.x, v.y, v.z);
        private static Vector4 V4(ColorDto c) => new Vector4(c.r, c.g, c.b, c.a);

        private static Triangulo ToTri(TrianguloDto t)
            => new Triangulo(
                new Vector3(t.a.x, t.a.y, t.a.z),
                new Vector3(t.b.x, t.b.y, t.b.z),
                new Vector3(t.c.x, t.c.y, t.c.z)
            );

        private static Cara ToCara(CaraDto cDto)
        {
            var cara = new Cara();
            // Color y bordes
            cara.SetColor(V4(cDto.color));                 // SetColor cambia el color base
            cara.DrawEdges     = cDto.drawEdges;           // propiedad pública
            cara.EdgeLineWidth = cDto.edgeLineWidth;       // propiedad pública
            cara.EdgeColor     = V4(cDto.edgeColor);       // propiedad pública

            // Triángulos
            var tris = new List<Triangulo>(cDto.triangulos.Count);
            foreach (var t in cDto.triangulos)
                tris.Add(ToTri(t));

            // AddRange reconstruye buffers (Mesh) internamente
            cara.AddRange(tris);
            return cara;
        }

        internal static Objeto FromDto(ObjetoDto dto)
        {
            // Construye diccionario de caras preservando el id
            var dict = new Dictionary<int, Cara>(dto.caras.Count);
            foreach (var c in dto.caras)
                dict[c.id] = ToCara(c);

            // Usa el constructor que acepta diccionario + centro
            var obj = new Objeto(dict, V3(dto.centro)); // setea _centro y Posicion = centro
            // Sobrescribe transform/flags desde el JSON
            obj.Posicion    = V3(dto.posicion);
            obj.RotacionDeg = V3(dto.rotacionDeg);
            obj.Dimensiones = V3(dto.dimensiones);
            obj.Visible     = dto.visible;

            return obj;
        }

        internal static Objeto LoadFromJson(string path)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var dto = JsonSerializer.Deserialize<ObjetoDto>(File.ReadAllText(path), options)
                      ?? throw new InvalidDataException("JSON inválido para ObjetoDto.");
            return FromDto(dto);
        }
    }
}
