using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTOs.SubirXmlsDTOs
{
    public class ArchivoDTO
    {
        public string nombre { get; set; }
        public string folio { get; set; }
        public byte[] contenido { get; set; }
        public bool guardar { get; set; }
    }
}
