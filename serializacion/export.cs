// ExportDtos.cs
using System.Collections.Generic;
namespace Proyecto_3D.Serializacion
{
public class ObjetoDto
{
    public Vec3Dto posicion { get; set; }
    public Vec3Dto rotacionDeg { get; set; }
    public Vec3Dto dimensiones { get; set; }
    public bool    visible { get; set; }
    public Vec3Dto centro { get; set; }
    public List<CaraDto> caras { get; set; } = new();
}

public class CaraDto
{
    public int      id { get; set; }
    public ColorDto color { get; set; }
    public bool     drawEdges { get; set; }
    public float    edgeLineWidth { get; set; }
    public ColorDto edgeColor { get; set; }
    public List<TrianguloDto> triangulos { get; set; } = new();
}

public class TrianguloDto
{
    public Vec3Dto a { get; set; }
    public Vec3Dto b { get; set; }
    public Vec3Dto c { get; set; }
}

public class Vec3Dto
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
}

    public class ColorDto
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }
    }
}