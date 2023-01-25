using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTOs.SubirXmlsDTOs
{
    public class Externo
    {
        public string Archivo { get; set; }
        public string contenidoXML { get; set; }
        public string foliofiscal { get; set; }
        public DateTime? FechaTimbrado { get; set; }
        public string SelloCFD { get; set; }
        public string CertificadoSat { get; set; }
        public string SelloSat { get; set; }
        public decimal? ISR { get; set; }
        public decimal? subsidio { get; set; }
        public decimal? ajusteISR { get; set; }
        public decimal? DevISR { get; set; }
        public string nominaversion { get; set; }
        public string Num { get; set; }
        public string Rfc { get; set; }
        public string TipoContrato { get; set; }
        public string NumSeguridadSocial { get; set; }
        public string Sindicalizado { get; set; }
        public string Curp { get; set; }
        public string TipoNomina { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaInicialPago { get; set; }
        public DateTime FechaFinalPago { get; set; }
        public string Departamento { get; set; }
        public string Puesto { get; set; }
        public string TipoJornada { get; set; }
        public string TipoRegimen { get; set; }
        public decimal? TotalPercepciones { get; set; }
        public decimal? TotalDeducciones { get; set; }
        public decimal? TotalOtrosPagos { get; set; }
        public decimal? subTotal { get; set; }
        public decimal? Total { get; set; }
        public decimal? descuento { get; set; }
        public string OrigenRecurso { get; set; }
        public string numqna { get; set; }
        public string _Status { get; set; }
        public bool? valida2016 { get; set; }
        public string CadenaTemp { get; set; }
        public string folio { get; set; }
        public string num_che { get; set; }
        public long? idregistro { get; set; }
        public DateTime? fechacancel { get; set; }
        public string referencia { get; set; }
        public decimal? subsidioE { get; set; }
        public decimal? subsidiocausado { get; set; }
        public DateTime? fecha_retimb { get; set; }
        public string fechaRegistro { get; set; }
    }
}
