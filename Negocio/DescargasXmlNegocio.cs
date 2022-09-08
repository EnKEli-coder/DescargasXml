using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos;
using System.Globalization;
using Entidades.DTOs;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.IO.Compression;
using OfficeOpenXml;
using System.Reflection;
using Entidades.DTOs.DescargasXmlDTOs;
using System.Web;

namespace Negocio
{
    public class DescargasXmlNegocio
    {
        public static List<int> ObtenerAniosXmls()
        {
            return Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerAniosXmls();
        }

        public static Dictionary<int, string> ObtenerMesesXmls(int anio)
        {
            //return Datos.ProductosNominaDB.ObtenerTransacciones.ObtenerMesesXmls(anio);
            List<int> data = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);

            Dictionary<int, string> meses = new Dictionary<int, string>();

            DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;

            data.ForEach(m => meses.Add(m, dateInfo.GetMonthName(m).ToUpper()));

            return meses;
        }

        public static Dictionary<string, List<string>> ObtenerPartidas(int anio)
        {
            List<string> partidas = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerPartidas(anio);

            Dictionary<string, List<string>> partidasPorUnidades = partidas.GroupBy(r => r.Substring(0, 3)).ToDictionary(ramo => ramo.Key, ramo => ramo.ToList()); ;
           
            return partidasPorUnidades;
        }

        
        public static List<int> ObtenerTotal(int anio, int mes, string[] partidas)
        {
            string meses = "";
            string listaPartidas = "";

            int i = 0;

            foreach (var partida in partidas)
            {
                if (i != 0)
                {
                    listaPartidas += ",";
                }
                listaPartidas += "'" + partida + "'";
                i++;
            }

            i = 0;

            if (mes == 0)
            {
                List<int> data = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);

                foreach (var dato in data)
                {
                    if (i != 0)
                    {
                        meses += ",";
                    }
                    meses += "'" + dato + "'";
                    i++;
                }
            }
            else
            {
                meses += mes;
            }


            return Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerTotal(anio, meses, listaPartidas);
        }


        public static byte[] ObtenerXmls(int anio, int mes, string[] partidas, int carpetas)
        {
            try
            {
                List<XmlDTO> xmls = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXmls(anio, mes, partidas);

                //crear carpeta(s)

                DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                string nombreMes = dateInfo.GetMonthName(mes);

                string raiz = @"C:\Reporte\";
                string carpetaDescargas;

                if (carpetas == 0)
                {
                    carpetaDescargas = Path.Combine(raiz, "XMLs_" + anio + "_" + nombreMes);
                    Directory.CreateDirectory(carpetaDescargas);

                    foreach (XmlDTO xml in xmls)
                    {
                        byte[] bytes = Encoding.Default.GetBytes(xml.contenidoXml);
                        string xmlValue = Encoding.UTF8.GetString(bytes);

                        string archivoRuta = Path.Combine(carpetaDescargas, xml.archivo + ".xml");
                        TextWriter Escribir = new StreamWriter(archivoRuta);
                        Escribir.WriteLine(xmlValue);
                        Escribir.Close();
                    }
                }
                else
                {
                    carpetaDescargas = Path.Combine(raiz, "Descargas_" + anio + "_" + nombreMes);
                    Directory.CreateDirectory(carpetaDescargas);
                    List<string> ramos = new List<string>();

                    foreach (string partida in partidas)
                    {
                        string ramo = partida.Substring(0, 3);
                        ramos.Add(ramo);
                    }

                    ramos = ramos.Distinct().ToList();

                    foreach (string ramo in ramos)
                    {
                        string carpetaRamo = Path.Combine(carpetaDescargas, "Ramo_" + ramo);
                        Directory.CreateDirectory(carpetaRamo);

                        var partidasGroup = partidas.GroupBy(partida => partida.Substring(0, 3));
                        var xmlsGroup = xmls.GroupBy(xml => xml.partida);

                        foreach (IGrouping<string, string> grupo in partidasGroup)
                        {
                            if (grupo.Key == ramo)
                            {
                                foreach (string part in grupo)
                                {
                                    string carpetaPartida = Path.Combine(carpetaRamo, "Partida_" + part);
                                    Directory.CreateDirectory(carpetaPartida);

                                    foreach (IGrouping<string, XmlDTO> archivos in xmlsGroup)
                                    {
                                        if (archivos.Key == part)
                                        {
                                            foreach (XmlDTO partArchivos in archivos)
                                            {
                                                byte[] bytes = Encoding.Default.GetBytes(partArchivos.contenidoXml);
                                                string xmlValue = Encoding.UTF8.GetString(bytes);

                                                string archivoRuta = Path.Combine(carpetaPartida, partArchivos.archivo + ".xml");
                                                TextWriter Escribir = new StreamWriter(archivoRuta);
                                                Escribir.WriteLine(xmlValue);
                                                Escribir.Close();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //origin del ramo se verifica y si existe se comprime
                string zip = carpetaDescargas + ".zip";

                if (Directory.Exists(carpetaDescargas))
                {
                    if (!Directory.Exists(zip))
                    {
                        ZipFile.CreateFromDirectory(carpetaDescargas, zip);
                        Directory.Delete(carpetaDescargas, true);

                    }
                }

                //obtener array de bits y regresar
                var arrayDeBytesZip = File.ReadAllBytes(zip);



                if (File.Exists(zip))
                {
                    File.Delete(zip);
                }



                return arrayDeBytesZip;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static byte[] ObtenerXmlsAnio(int anio, string[] partidas, int carpetas)
        {

            try
            {
                List<int> meses = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);
                string raiz = @"C:\Reporte\";
                string carpetaDescargas;

                if (carpetas == 0)
                {

                    carpetaDescargas = Path.Combine(raiz, "XMLs_" + anio);
                    Directory.CreateDirectory(carpetaDescargas);

                    foreach (int mes in meses)
                    {
                        List<XmlDTO> xmls = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXmls(anio, mes, partidas);

                        foreach (XmlDTO xml in xmls)
                        {
                            byte[] bytes = Encoding.Default.GetBytes(xml.contenidoXml);
                            string xmlValue = Encoding.UTF8.GetString(bytes);

                            string archivoRuta = Path.Combine(carpetaDescargas, xml.archivo + ".xml");
                            TextWriter Escribir = new StreamWriter(archivoRuta);
                            Escribir.WriteLine(xmlValue);
                            Escribir.Close();
                        }
                    }
                }
                else
                {
                    carpetaDescargas = Path.Combine(raiz, "XMLs_" + anio);
                    Directory.CreateDirectory(carpetaDescargas);

                    foreach (int mes in meses)
                    {
                        List<XmlDTO> xmls = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXmls(anio, mes, partidas);

                        //crear carpeta(s)

                        DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                        string nombreMes = dateInfo.GetMonthName(mes);

                        string carpetaMes = Path.Combine(carpetaDescargas, "XMLs_" + anio + "_" + nombreMes);
                        Directory.CreateDirectory(carpetaMes);

                        List<string> ramos = new List<string>();

                        foreach (string partida in partidas)
                        {
                            string ramo = partida.Substring(0, 3);
                            ramos.Add(ramo);
                        }

                        ramos = ramos.Distinct().ToList();

                        foreach (string ramo in ramos)
                        {
                            string carpetaRamo = Path.Combine(carpetaMes, "Ramo_" + ramo);
                            Directory.CreateDirectory(carpetaRamo);

                            var partidasGroup = partidas.GroupBy(partida => partida.Substring(0, 3));
                            var xmlsGroup = xmls.GroupBy(xml => xml.partida);

                            foreach (IGrouping<string, string> grupo in partidasGroup)
                            {
                                if (grupo.Key == ramo)
                                {
                                    foreach (string part in grupo)
                                    {
                                        string carpetaPartida = Path.Combine(carpetaRamo, "Partida_" + part);
                                        Directory.CreateDirectory(carpetaPartida);

                                        foreach (IGrouping<string, XmlDTO> archivos in xmlsGroup)
                                        {
                                            if (archivos.Key == part)
                                            {
                                                foreach (XmlDTO partArchivos in archivos)
                                                {
                                                    byte[] bytes = Encoding.Default.GetBytes(partArchivos.contenidoXml);
                                                    string xmlValue = Encoding.UTF8.GetString(bytes);

                                                    string archivoRuta = Path.Combine(carpetaPartida, partArchivos.archivo + ".xml");
                                                    TextWriter Escribir = new StreamWriter(archivoRuta);
                                                    Escribir.WriteLine(xmlValue);
                                                    Escribir.Close();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                //origin del ramo se verifica y si existe se comprime
                string zip = carpetaDescargas + ".zip";

                if (Directory.Exists(carpetaDescargas))
                {
                    if (!Directory.Exists(zip))
                    {
                        ZipFile.CreateFromDirectory(carpetaDescargas, zip);
                        Directory.Delete(carpetaDescargas, true);

                    }
                }

                //obtener array de bits y regresar
                var arrayDeBytesZip = File.ReadAllBytes(zip);



                if (File.Exists(zip))
                {
                    File.Delete(zip);
                }



                return arrayDeBytesZip;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] ObtenerXls(int anio, int mesEscogido,string[] partidas)
        {
            Dictionary<string, List<XlsDTO>> infoPorMes= new Dictionary<string, List<XlsDTO>>();
            DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;

            //Si mes = 0, obtener informacion de todo el año
            if (mesEscogido == 0)
            {
                List<int> meses = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);

                foreach(int mes in meses)
                {
                    List<XlsDTO> info = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXls(anio, mes, partidas);
                    info = rellenarDatosXml(info);
                    
                    string nombreMes = dateInfo.GetMonthName(mes);
                    infoPorMes.Add(nombreMes, info);
                }
            }
            else
            {
                List<XlsDTO> info = Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXls(anio, mesEscogido, partidas);
                info = rellenarDatosXml(info);

                string nombreMes = dateInfo.GetMonthName(mesEscogido);
                infoPorMes.Add(nombreMes, info);
            }
            return crearXls(infoPorMes);
        }

        public static List<XlsDTO> rellenarDatosXml(List<XlsDTO> xlss)
        {
            foreach(XlsDTO xls in xlss)
            {
                XmlDocument documento = new XmlDocument();
                XmlNamespaceManager nm = new XmlNamespaceManager(documento.NameTable);
                nm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                nm.AddNamespace("nomina12", "http://www.sat.gob.mx/nomina12");

                byte[] bytes = Encoding.Default.GetBytes(xls.contenidoXml);
                string xmlValue = Encoding.UTF8.GetString(bytes);
                documento.LoadXml(xmlValue);

                XmlElement root = documento.DocumentElement;
                XmlNode node = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina", nm);
                xls.totalPercepciones = node.Attributes["TotalPercepciones"] != null ? float.Parse(node.Attributes["TotalPercepciones"].Value) : 0;
                xls.totalDeducciones = node.Attributes["TotalDeduccciones"] != null? float.Parse(node.Attributes["TotalDeducciones"].Value): 0;
                xls.totalOtrosPagos = node.Attributes["TotalOtrosPagos"] != null ? float.Parse(node.Attributes["TotalOtrosPagos"].Value): 0;
                xls.fechaPago = node.Attributes["FechaPago"].Value;
                xls.fechaInicialPago = node.Attributes["FechaInicialPago"].Value;
                xls.fechaFinalPago = node.Attributes["FechaFinalPago"].Value;
            }

            return xlss;
        }

        public static byte[] crearXls(Dictionary<string, List<XlsDTO>> registros)
        {
            var ruta = @"C:\Reporte\test.xlsx";
            var archivoBase = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Recursos/AcumuladoVales.xlsx");

            var path = new FileInfo(ruta);
            path.Refresh();
            var rutaBase = new FileInfo(archivoBase);
            rutaBase.Refresh();

            ExcelPackage origen = new ExcelPackage(rutaBase);
            ExcelPackage excel = new ExcelPackage(path);

            var plantilla = origen.Workbook.Worksheets["Vales_Acumulado"];

            foreach (string mes in registros.Keys)
            {
                var sheet = excel.Workbook.Worksheets.Add(mes, plantilla);

                PropertyInfo[] propiedades = typeof(XlsDTO).GetProperties();

                var columnCount = 1;
                var rowCount = 7;

                foreach (PropertyInfo propiedad in propiedades)
                {
                    sheet.Cells[rowCount, columnCount].Value = propiedad.Name;
                    columnCount++;
                }

                foreach (XlsDTO registro in registros[mes])
                {
                    columnCount = 1;
                    rowCount++;

                    foreach (PropertyInfo propiedad in propiedades)
                    {
                        sheet.Cells[rowCount, columnCount].Value = propiedad.GetValue(registro);
                        columnCount++;
                    }
                }
            }

            excel.Save();



            var arrayDeBytesZip = File.ReadAllBytes(ruta);

            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }

            return arrayDeBytesZip;
        }

    }
}
