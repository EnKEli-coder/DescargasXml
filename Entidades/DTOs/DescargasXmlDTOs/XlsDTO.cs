using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTOs.DescargasXmlDTOs
{
    public class XlsDTO
    {
        public Decimal isr { get; set; }
        public string num { get; set; }
        public string rfc { get; set; }
        public int numQna { get; set; }
        public string folioFiscal { get; set; }
        public int tipoContrato { get; set; }
        public string curp { get; set; }
        public string tipoNomina { get; set; }
        public string departamento { get; set; }
        public int tipoRegimen { get; set; }
        public string fechaPago { get; set; }
        public string fechaInicialPago { get; set; }
        public string fechaFinalPago { get; set; }
        public float totalPercepciones { get; set; }
        public float totalOtrosPagos { get; set; }
        public float totalDeducciones { get; set; }
        public string contenidoXml { get; set; }
    }
}
