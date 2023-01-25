using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTOs.SubirXmlsDTOs
{
    public class LoteRegistroDTO
    {
        public string registroId { get; set; }
        public int noRegistros { get; set; }
        public string Dependencia { get; set; }
        public string FechaRegistro { get; set; }
    }
}
