using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Datos.ProductosNominaDB;
using Entidades.DTOs.SubirXmlsDTOs;

namespace Negocio
{
    public class SubirXmlsNegocio
    {
        public static List<string> obtenerPartidas()
        {
            return SubirXmlsDatos.obtenerPartidas();
        }

        public static List<ArchivoDTO> SubirDatos(List<ArchivoDTO> xmls, string unidad)
        {
            List<Externo> rows = new List<Externo>();
            LoteRegistroDTO lote = new LoteRegistroDTO();
            var fecha = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            foreach (var xml in xmls)
            {
                Externo row = new Externo();
                var documento = new XmlDocument();

                XmlNamespaceManager nm = new XmlNamespaceManager(documento.NameTable);
                nm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                nm.AddNamespace("nomina12", "http://www.sat.gob.mx/nomina12");
                nm.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                nm.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");


                string xmlValue = Encoding.UTF8.GetString(xml.contenido);
                documento.LoadXml(xmlValue);
                //documento.Load(xml.Value);

                XmlElement root = documento.DocumentElement;

                row.Archivo = xml.nombre.Substring(0, xml.nombre.Length-4);
                row.contenidoXML = documento.OuterXml;

                XmlNode timbreFiscalDigital = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/tfd:TimbreFiscalDigital", nm);
                XmlNode receptor = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Receptor", nm);
                XmlNode nomina = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina", nm);
                XmlNode deducciones = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Deducciones", nm);
                XmlNode otrosPagos = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos", nm);
                XmlNode comprobante = root.SelectSingleNode("/cfdi:Comprobante", nm);

                if(timbreFiscalDigital != null)
                {
                    row.foliofiscal = timbreFiscalDigital.Attributes["UUID"] != null ? timbreFiscalDigital.Attributes["UUID"].Value : null;
                    row.SelloCFD = timbreFiscalDigital.Attributes["SelloCFD"] != null ? timbreFiscalDigital.Attributes["SelloCFD"].Value.Substring(0, 50) : null;
                    row.CertificadoSat = timbreFiscalDigital.Attributes["NoCertificadoSAT"] != null ? timbreFiscalDigital.Attributes["NoCertificadoSAT"].Value : null;
                    row.SelloSat = timbreFiscalDigital.Attributes["SelloSAT"] != null ? timbreFiscalDigital.Attributes["SelloSAT"].Value.Substring(0, 50) : null;
                }

                if(receptor != null)
                {
                    row.Rfc = receptor.Attributes["Rfc"] != null ? receptor.Attributes["Rfc"].Value : null;
                }

                if(nomina != null)
                {
                    row.nominaversion = nomina.Attributes["Version"] != null ? nomina.Attributes["Version"].Value : null;
                    row.TipoNomina = nomina.Attributes["TipoNomina"] != null ? nomina.Attributes["TipoNomina"].Value : null;
                    row.FechaPago = DateTime.Parse(nomina.Attributes["FechaPago"] != null ? nomina.Attributes["FechaPago"].Value : null);
                    row.FechaInicialPago = DateTime.Parse(nomina.Attributes["FechaInicialPago"] != null ? nomina.Attributes["FechaInicialPago"].Value : null);
                    row.FechaFinalPago = DateTime.Parse(nomina.Attributes["FechaFinalPago"] != null ? nomina.Attributes["FechaFinalPago"].Value : null);
                    row.TotalPercepciones = Convert.ToDecimal(nomina.Attributes["TotalPercepciones"] != null ? nomina.Attributes["TotalPercepciones"].Value : null);
                    row.TotalDeducciones = Convert.ToDecimal(nomina.Attributes["TotalDeducciones"] != null ? nomina.Attributes["TotalDeducciones"].Value : null);
                    row.TotalOtrosPagos = Convert.ToDecimal(nomina.Attributes["TotalOtrosPagos"] != null ? nomina.Attributes["TotalOtrosPagos"].Value : null);

                    string fechaQna = nomina.Attributes["FechaInicialPago"] != null ? nomina.Attributes["FechaInicialPago"].Value : null;
                    string año = fechaQna.Substring(2, 2);
                    int dia = Convert.ToInt32(fechaQna.Substring(8, 2));
                    int quincena = Convert.ToInt32(fechaQna.Substring(5, 2))*2;

                    if(dia == 1)
                    {
                        quincena--;

                        if(quincena < 10)
                        {
                            row.numqna = año + "0" +quincena;
                        }
                        else
                        {
                            row.numqna = año + quincena;
                        }
                    }
                    else
                    {
                        row.numqna = año + quincena;
                    }

                    row.folio = fechaQna.Substring(0, 4)+quincena+row.foliofiscal.Substring(row.foliofiscal.Length -3);
                    

                    XmlNode nominaReceptor = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Receptor", nm);
                    XmlNode entidadSNCF = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Emisor/nomina12:EntidadSNCF", nm);

                    if(nominaReceptor != null)
                    {
                        row.Num = nominaReceptor.Attributes["NumEmpleado"] != null ? nominaReceptor.Attributes["NumEmpleado"].Value : null;
                        row.TipoContrato = nominaReceptor.Attributes["TipoContrato"] != null ? nominaReceptor.Attributes["TipoContrato"].Value : null;
                        row.NumSeguridadSocial = nominaReceptor.Attributes["NumSeguridadSocial"] != null ? nominaReceptor.Attributes["NumSeguridadSocial"].Value : null;
                        row.Sindicalizado = nominaReceptor.Attributes["Sindicalizado"] != null ? nominaReceptor.Attributes["Sindicalizado"].Value : null;
                        row.Curp = nominaReceptor.Attributes["Curp"] != null ? nominaReceptor.Attributes["Curp"].Value : null;
                        row.Departamento = unidad;
                        row.Puesto = nominaReceptor.Attributes["Puesto"] != null ? nominaReceptor.Attributes["Puesto"].Value : null;
                        row.TipoJornada = nominaReceptor.Attributes["TipoJornada"] != null ? nominaReceptor.Attributes["TipoJornada"].Value : null;
                        row.TipoRegimen = nominaReceptor.Attributes["TipoRegimen"] != null ? nominaReceptor.Attributes["TipoRegimen"].Value : null;
                    }

                    row.folio = row.folio + row.Num;

                    if(entidadSNCF != null)
                    {
                        row.OrigenRecurso = entidadSNCF.Attributes["OrigenRecurso"] != null ? entidadSNCF.Attributes["OrigenRecurso"].Value : null;
                    }

                } 

                if(deducciones != null)
                {
                    XmlNode ISR = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Deducciones/nomina12:Deduccion[contains(@Concepto,'ISR')]", nm);

                    if(ISR == null)
                    {
                        ISR = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Deducciones/nomina12:Deduccion[contains(@Concepto,'RETENCIÓN DEL IMPUESTO SOBRE LA RENTA')]", nm);
                    }

                    if (ISR != null)
                    {
                        row.ISR = Convert.ToDecimal(ISR.Attributes["Importe"] != null ? ISR.Attributes["Importe"].Value : null);
                    }

                    XmlNode ajusteISR = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Deducciones/nomina12:Deduccion[@Concepto = 'AJUSTE ISR']", nm);

                    if(ajusteISR != null)
                    {
                        row.ajusteISR = Convert.ToDecimal(ajusteISR.Attributes["Importe"] != null ? ajusteISR.Attributes["Importe"].Value : null);
                    }
                }

                if(otrosPagos != null)
                {
                    XmlNode subsidio = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos/nomina12:OtroPago[@Concepto = 'SUBSIDIO AL EMPLEO']", nm);
                    XmlNode devISR = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos/nomina12:OtroPago[@Concepto = 'DEV-AJUSTE ISR']", nm);
                    XmlNode subsidioE = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos/nomina12:OtroPago[@Concepto = 'Subsidio para el empleo (efectivamente entregado al trabajador)']", nm);

                    if(subsidio != null)
                    {
                        row.subsidio = Convert.ToDecimal(subsidio.Attributes["Importe"] != null ? subsidio.Attributes["Importe"].Value : null);
                    }

                    if (devISR != null)
                    {
                        row.DevISR = Convert.ToDecimal(devISR.Attributes["Importe"] != null ? devISR.Attributes["Importe"].Value : null);
                    }

                    if (subsidioE != null)
                    {
                        XmlNode subsidioCausado = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos/nomina12:OtroPago[@Concepto = 'Subsidio para el empleo (efectivamente entregado al trabajador)']/nomina12:SubsidioAlEmpleo", nm);
                        if(subsidioCausado != null)
                        {
                            row.subsidiocausado = Convert.ToDecimal(subsidioCausado.Attributes["SubsidioCausado"] != null ? subsidioCausado.Attributes["SubsidioCausado"].Value : null);
                        }

                        row.subsidioE = Convert.ToDecimal(subsidioE.Attributes["Importe"] != null ? subsidioE.Attributes["Importe"].Value : null);
                    }
                }

                if(comprobante != null)
                {
                    row.FechaTimbrado = DateTime.Parse(comprobante.Attributes["Fecha"].Value);
                    row.subTotal = Convert.ToDecimal(comprobante.Attributes["SubTotal"] != null ? comprobante.Attributes["SubTotal"].Value : null);
                    row.Total = Convert.ToDecimal(comprobante.Attributes["Total"] != null ? comprobante.Attributes["Total"].Value : null);
                    row.descuento = Convert.ToDecimal(comprobante.Attributes["Descuento"] != null ? comprobante.Attributes["Descuento"].Value : null);
                }
                
                row.fechaRegistro = fecha;
                
                row._Status = "E";
                row.valida2016 = null;
                row.CadenaTemp = null;
                row.num_che = null;
                row.idregistro = null;
                row.fechacancel = null;
                row.referencia = null;
                row.fecha_retimb = null;
                rows.Add(row);

                xml.folio = row.folio;
            }

            List<Externo> nuevosRegistros = ComprobarBase(rows);

            lote.noRegistros = nuevosRegistros.Count();
            lote.Dependencia = unidad;
            lote.FechaRegistro = fecha;

            string fechaRegistro = lote.FechaRegistro.Substring(2, 2) + lote.FechaRegistro.Substring(5, 2) + lote.FechaRegistro.Substring(8, 2);
            string horaRegistro = lote.FechaRegistro.Substring(11, 2) + lote.FechaRegistro.Substring(14, 2);

            lote.registroId = lote.noRegistros + fechaRegistro + horaRegistro;
            

            bool resultado = SubirXmlsDatos.InsertarXmls(nuevosRegistros);

            if(resultado == true)
            {
                foreach(Externo row in nuevosRegistros)
                {
                    List<string> nuevos = nuevosRegistros.Select(registro => registro.folio).ToList();

                    xmls.Where(xml => nuevos.Contains(xml.folio)).ToList().ForEach(archivo => archivo.guardar = true);
                }

                xmls = xmls.GroupBy(p => p.folio).Select(g => g.First()).ToList();

                if (lote.noRegistros > 0) {
                    SubirXmlsDatos.GuardarRegistro(lote);
                }
                

                List<ArchivoDTO> response = new List<ArchivoDTO>();

                foreach(var xml in xmls)
                {
                    response.Add(new ArchivoDTO{
                        nombre = xml.nombre,
                        guardar = xml.guardar
                    });
                }

                return response;
            }
            else
            {
                List<ArchivoDTO> vacio = new List<ArchivoDTO>();

                return vacio;
            }
        }

        public static List<Externo> ComprobarBase(List<Externo> rows)
        {

            string folios = "";
            List<Externo> nuevosRegistros = new List<Externo>();

            int i = 0;

            foreach(Externo row in rows)
            {
                if (i != 0)
                {
                    folios += ",";
                }
                folios += "'" + row.folio + "'";
                i++;
            }

            List<string> repetidos = SubirXmlsDatos.buscarRepetido(folios);

            nuevosRegistros = rows.Where(a => !repetidos.Contains(a.folio)).ToList();

            nuevosRegistros = nuevosRegistros.GroupBy(p => p.folio).Select(g => g.First()).ToList();

            return nuevosRegistros;
            
        }

        public static List<LoteRegistroDTO> ObtenerRegistros()
        {
            return SubirXmlsDatos.ObtenerRegistros();
        }

        public static bool EliminarRegistro(string fecha)
        {

            DateTime date = DateTime.Parse(fecha);
            return SubirXmlsDatos.EliminarRegistro(date); 
        }
    }
}
