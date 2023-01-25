using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTOs
{
    public class XmlDTO
    {
        public string partida { get; set; }
        public string archivo { get; set; }
        public Decimal isr { get; set; }
        public string num { get; set; }
        public string rfc { get; set; }
        public int numQna { get; set; }
        public string folioFiscal { get; set; }
        public string sindicalizado { get; set; }
        public int tipoContrato { get; set; }
        public string numSeguridadSocial { get; set; }
        public string curp { get; set; }
        public char tipoNomina { get; set; }
        public string departamento { get; set; }
        public string puesto { get; set; }
        public int tipoJornada { get; set; }
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
