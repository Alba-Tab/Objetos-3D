using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_3D.Core3D
{
    internal class Face
    {
        public readonly List<uint> Indices = new();


        public Face(IEnumerable<uint> idxs) => Indices.AddRange(idxs);
    }
}
