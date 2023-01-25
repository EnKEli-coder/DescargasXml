using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTOs.DescargasXmlDTOs
{
    public class ReporteAudiDTO
    {
        public string Codigo{ get; set; }
        public string Departamento { get; set; }
        public decimal Percepciones { get; set; }
        public decimal Deducciones { get; set; }
        public decimal Liquido { get; set; }
    }
}
