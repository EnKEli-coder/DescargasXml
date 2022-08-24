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
            return Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerTotal(anio, mes, partidas);
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
    }
}
