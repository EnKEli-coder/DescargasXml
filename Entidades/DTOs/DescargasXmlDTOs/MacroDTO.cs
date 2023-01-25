using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTOs.DescargasXmlDTOs
{
    public class MacroDTO
    {
        public string ramo { get; set; }
        public string nombreRamo { get; set; }
        public string Dependencia { get; set; }
        public string tipoNomina { get; set; }
        public int numQna { get; set; }
        public string folioFiscal { get; set; }
        public string rfc { get; set; }
        public int tipoRegimen { get; set; }
        public string fechaPago { get; set; }
        public decimal isr { get; set; }
        public decimal subEmp { get; set; }
        public decimal ajusteIsr { get; set; }
        public decimal ajusteSub { get; set; }
        public decimal isrComp { get; set; }
        public string contenidoXml { get; set; }
    }
}
