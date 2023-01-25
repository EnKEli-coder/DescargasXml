using Entidades.DTOs;
using Entidades.DTOs.DescargasXmlDTOs;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Serilog;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Negocio
{
    public class DescargasExternosNegocio
    {

        public static List<int> ObtenerAniosXmls()
        {
            return Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerAniosXmls();
        }

        public static Dictionary<int, string> ObtenerMesesXmls(int anio)
        {
            Dictionary<int, string> meses = new Dictionary<int, string>();
            List<int> data = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerMesesXmls(anio);
            DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;
            data.ForEach(m => meses.Add(m, dateInfo.GetMonthName(m).ToUpper()));
            return meses;
        }

        public static Dictionary<string, List<string>> ObtenerPartidas(int anio)
        {
            Dictionary<string, List<string>> partidasPorUnidades = new Dictionary<string, List<string>>();
            List<PartidaDTO> partidas = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerPartidas(anio);
            partidasPorUnidades = partidas.GroupBy(r => r.ramo + " " + r.nombreRamo).ToDictionary(ramo => ramo.Key, ramo => ramo.Select(part => part.partida + " " + part.nombrePartida).ToList());
            return partidasPorUnidades;
        }

        public static List<Decimal> ObtenerTotal(int anio, int mes, string[] partidas)
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
                List<int> data = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerMesesXmls(anio);

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


            return Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerTotal(anio, meses, listaPartidas);
        }

        public static byte[] ObtenerXmls(int anio, int mes, string[] partidas, int carpetas)
        {
            try
            {
                List<XmlDTO> xmls = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerXmls(anio, mes, partidas);

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

                    var ramos = xmls.GroupBy(xml => xml.partida.Substring(0, 3));

                    foreach (var ramo in ramos)
                    {
                        string carpetaRamo = Path.Combine(carpetaDescargas, "Ramo_" + ramo.Key);
                        Directory.CreateDirectory(carpetaRamo);

                        var parts = ramo.GroupBy(part => part.partida);

                        foreach (var part in parts)
                        {
                            string carpetaPartida = Path.Combine(carpetaRamo, "Partida_" + part.Key);
                            Directory.CreateDirectory(carpetaPartida);

                            foreach (var xml in part)
                            {
                                byte[] bytes = Encoding.Default.GetBytes(xml.contenidoXml);
                                string xmlValue = Encoding.UTF8.GetString(bytes);

                                string archivoRuta = Path.Combine(carpetaPartida, xml.archivo + ".xml");
                                TextWriter Escribir = new StreamWriter(archivoRuta);
                                Escribir.WriteLine(xmlValue);
                                Escribir.Close();
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
                List<int> meses = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerMesesXmls(anio);
                string raiz = @"C:\Reporte\";
                string carpetaDescargas;

                if (carpetas == 0)
                {

                    carpetaDescargas = Path.Combine(raiz, "XMLs_" + anio);
                    Directory.CreateDirectory(carpetaDescargas);

                    foreach (int mes in meses)
                    {
                        List<XmlDTO> xmls = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerXmls(anio, mes, partidas);

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
                        List<XmlDTO> xmls = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerXmls(anio, mes, partidas);
                        DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                        string nombreMes = dateInfo.GetMonthName(mes);

                        string carpetaMes = Path.Combine(carpetaDescargas, "XMLs_" + anio + "_" + nombreMes);
                        Directory.CreateDirectory(carpetaMes);

                        var ramos = xmls.GroupBy(xml => xml.partida.Substring(0, 2));

                        foreach (var ramo in ramos)
                        {
                            string carpetaRamo = Path.Combine(carpetaMes, "Ramo_" + ramo.Key);
                            Directory.CreateDirectory(carpetaRamo);

                            var parts = ramo.GroupBy(part => part.partida);

                            foreach (var part in parts)
                            {
                                string carpetaPartida = Path.Combine(carpetaRamo, "Partida_" + part.Key);
                                Directory.CreateDirectory(carpetaPartida);

                                foreach (var xml in part)
                                {
                                    byte[] bytes = Encoding.Default.GetBytes(xml.contenidoXml);
                                    string xmlValue = Encoding.UTF8.GetString(bytes);

                                    string archivoRuta = Path.Combine(carpetaPartida, xml.archivo + ".xml");
                                    TextWriter Escribir = new StreamWriter(archivoRuta);
                                    Escribir.WriteLine(xmlValue);
                                    Escribir.Close();
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
            List<MacroDTO> depIsrList = new List<MacroDTO>();

            //Si mes = 0, obtener informacion de todo el año
            if (mesEscogido == 0)
            {
                List<int> meses = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerMesesXmls(anio);

                foreach (int mes in meses)
                {
                    List<MacroDTO> info = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerXls(anio, mes, partidas);

                    info = rellenarDatosXml(info);
                    depIsrList.AddRange(info);
                }
            }
            else
            {
                List<MacroDTO> info = Datos.ProductosNominaDB.DescargasExternosDatos.ObtenerXls(anio, mesEscogido, partidas);
                info = rellenarDatosXml(info);
                depIsrList.AddRange(info);
            }
            return crearReporteMacro(depIsrList);
        }

        public static List<MacroDTO> rellenarDatosXml(List<MacroDTO> xlss)
        {
            foreach (MacroDTO row in xlss)
            {
                XmlDocument documento = new XmlDocument();
                XmlNamespaceManager nm = new XmlNamespaceManager(documento.NameTable);
                nm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                nm.AddNamespace("nomina12", "http://www.sat.gob.mx/nomina12");

                documento.LoadXml(row.contenidoXml);

                XmlElement root = documento.DocumentElement;
                XmlNode node = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina", nm);
                XmlNode deducciones = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Deducciones", nm);
                XmlNode otrosPagos = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos", nm);
                row.tipoNomina = node.Attributes["TipoNomina"] != null ? node.Attributes["TipoNomina"].Value : "";
                if (row.tipoNomina == "O")
                {
                    row.tipoNomina = "NORMAL";
                }
                else if (row.tipoNomina == "E")
                {
                    row.tipoNomina = "EXTRAORDINARIO";
                }
                row.fechaPago = node.Attributes["FechaPago"].Value;
                if (deducciones != null)
                {
                    XmlNode ajusteISR = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Deducciones/nomina12:Deduccion[@Concepto = 'AJUSTE ISR']", nm);

                    if (ajusteISR != null)
                    {
                        row.ajusteIsr = Convert.ToDecimal(ajusteISR.Attributes["Importe"] != null ? ajusteISR.Attributes["Importe"].Value : null);
                    }
                }

                if (otrosPagos != null)
                {
                    XmlNode devISR = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos/nomina12:OtroPago[@Concepto = 'DEVOLUCION POR AJUSTE']", nm);
                    XmlNode subsidioE = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos/nomina12:OtroPago[@Concepto = 'Subsidio para el empleo (efectivamente entregado al trabajador)']", nm);
                    if (subsidioE != null)
                    {
                        row.subEmp = Convert.ToDecimal(subsidioE.Attributes["Importe"] != null ? subsidioE.Attributes["Importe"].Value : null);
                    }

                    if (devISR != null)
                    {
                        row.isrComp = Convert.ToDecimal(devISR.Attributes["Importe"] != null ? devISR.Attributes["Importe"].Value : null);
                    }

                }
            }
            return xlss;
        }

        public static byte[] crearXls(Dictionary<string, List<XlsDTO>> registros)
        {
            try
            {
                //var ruta = @"C:\Reporte\test.xlsx";
                var ruta = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Recursos/test.xlsx");
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
            catch(Exception)
            {
                throw;
            }
        }

        public static byte[] crearReporteMacro(List<MacroDTO> depIsrList)
        {
            DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;

            var nuevo = @"C:\Reporte\Macro.xlsx";
            var archivoBase = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Recursos/PlantillaXlsMacro.xlsx");
            var hojasMes = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Recursos/PlantillaMesMacro.xlsx");

            var rutaNuevo = new FileInfo(nuevo);
            rutaNuevo.Refresh();
            var rutaBase = new FileInfo(archivoBase);
            rutaBase.Refresh();
            var rutaMes = new FileInfo(hojasMes);
            rutaMes.Refresh();

            ExcelPackage origen = new ExcelPackage(rutaBase);
            ExcelPackage hoja = new ExcelPackage(rutaMes);
            ExcelPackage excel = new ExcelPackage(rutaNuevo);

            var hojaOrigen = origen.Workbook.Worksheets["ORIGEN"];
            var hojaDepISR = origen.Workbook.Worksheets["ISR DEP"];
            var hojaReISR = origen.Workbook.Worksheets["RE ISR"];
            var hojaMes = hoja.Workbook.Worksheets["Plantilla"];

            //Hoja Origen
            var sheetOrigen = excel.Workbook.Worksheets.Add("ORIGEN", hojaOrigen);
            var listaOrigen = depIsrList.GroupBy(x => new {x.nombreRamo, x.tipoRegimen, x.tipoNomina, x.numQna })
                .OrderBy(g => g.Key.tipoRegimen)
                .ThenBy(g => g.Key.tipoNomina)
                .ThenBy(g => g.Key.numQna)
                .Select(g => new {
                    Dependencia = g.Key.nombreRamo,
                    tipoNomina = g.Key.tipoNomina,
                    numQna = g.Key.numQna,
                    tipoRegimen = g.Key.tipoRegimen,
                    cantidad = g.Count(),
                    isr = g.Sum(h => Convert.ToDecimal(h.isr))
                });

            var columnCount = 1;
            var rowCount = 3;

            foreach (var group in listaOrigen)
            {
                columnCount = 1;

                foreach (var propiedad in group.GetType().GetProperties())
                {
                    sheetOrigen.Cells[rowCount, columnCount].Value = propiedad.GetValue(group);
                    columnCount++;
                }
                rowCount++;
            }

            rowCount++;

            sheetOrigen.Cells[rowCount, 5, rowCount, 6].Style.Font.Bold = true;
            sheetOrigen.Cells[rowCount, 5].Value = depIsrList.Count();
            sheetOrigen.Cells[rowCount, 6].Value = depIsrList.Sum(h => Convert.ToDecimal(h.isr));

            //Hoja Dep ISR
            var sheetDepIsr = excel.Workbook.Worksheets.Add("ISR DEP", hojaDepISR);

            rowCount = 3;
            columnCount = 5;

            var ramos = depIsrList.GroupBy(x => x.ramo);

            foreach (var departamento in ramos)
            {
                sheetDepIsr.Cells[rowCount, 1].Value = departamento.Key;
                sheetDepIsr.Cells[rowCount, 2, rowCount, 4].Merge = true;
                sheetDepIsr.Cells[rowCount, 2].Value = departamento.ToList().Select(x => x.nombreRamo);
                Decimal total = 0;
                foreach (var meses in departamento.ToList().Where(x => x.tipoRegimen == 2).GroupBy(x => x.fechaPago.Substring(5, 2)))
                {
                    sheetDepIsr.Cells[rowCount, Convert.ToInt32(meses.Key)+5].Value = meses.Sum(x => x.isr);
                    total += meses.Sum(x => x.isr);
                    columnCount++;
                }
                sheetDepIsr.Cells[rowCount, 17].Value = total;
                rowCount++;
                columnCount = 5;
            }

            var porMeses = depIsrList.GroupBy(x => x.fechaPago.Substring(5, 2));

            foreach (var mes in porMeses)
            {
                sheetDepIsr.Cells[rowCount, Convert.ToInt32(mes.Key) + 5].Value = mes.ToList().Where(x => x.tipoRegimen == 2).Sum(x => x.isr);
                columnCount++;
            }

            sheetDepIsr.Cells[rowCount, 17].Value = depIsrList.Where(x => x.tipoRegimen == 2).Sum(x => x.isr);
            rowCount++;

            rowCount++;
            sheetDepIsr.Cells[rowCount, 5, rowCount, 16].Merge = true;
            sheetDepIsr.Cells[rowCount, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheetDepIsr.Cells[rowCount, 5].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            sheetDepIsr.Cells[rowCount, 5].Style.Font.Color.SetColor(Color.White);
            sheetDepIsr.Cells[rowCount, 5].Value = "ISR RETENCIONES POR ASIMILADOS A SALARIOS";
            rowCount++;

            foreach (var departamento in ramos)
            {
                sheetDepIsr.Cells[rowCount, 1].Value = departamento.Key;
                sheetDepIsr.Cells[rowCount, 2, rowCount, 4].Merge = true;
                sheetDepIsr.Cells[rowCount, 2].Value = departamento.ToList().Select(x => x.nombreRamo);
                Decimal total = 0;
                foreach (var meses in departamento.ToList().Where(x => x.tipoRegimen == 9).GroupBy(x => x.fechaPago.Substring(5, 2)))
                {
                    sheetDepIsr.Cells[rowCount, Convert.ToInt32(meses.Key) + 5].Value = meses.Sum(x => x.isr);
                    total += meses.Sum(x => x.isr);
                    columnCount++;
                }

                sheetDepIsr.Cells[rowCount, 17].Value = total;
                rowCount++;
                columnCount = 5;
            }

            foreach (var mes in porMeses)
            {
                sheetDepIsr.Cells[rowCount, Convert.ToInt32(mes.Key) + 5].Value = mes.ToList().Where(x => x.tipoRegimen == 9).Sum(x => x.isr);
                columnCount++;
            }

            sheetDepIsr.Cells[rowCount, 17].Value = depIsrList.Where(x => x.tipoRegimen == 9).Sum(x => x.isr);
            rowCount++;
            columnCount = 5;

            //Hojas Meses
            PropertyInfo[] propiedades = typeof(MacroDTO).GetProperties();

            foreach (var mes in porMeses)
            {
                string nombreMes = dateInfo.GetMonthName(Convert.ToInt32(mes.Key)).ToUpper();
                var sheet = excel.Workbook.Worksheets.Add(nombreMes, hojaMes);

                columnCount = 1;
                rowCount = 1;

                foreach (MacroDTO registro in mes.ToList())
                {
                    columnCount = 1;
                    rowCount++;

                    foreach (PropertyInfo propiedad in propiedades)
                    {
                        if (propiedad.Name != "contenidoXml")
                        {
                            sheet.Cells[rowCount, columnCount].Value = propiedad.GetValue(registro);
                            columnCount++;
                        }
                    }
                }
            }

            excel.Save();

            var arrayDeBytesZip = File.ReadAllBytes(nuevo);

            if (File.Exists(nuevo))
            {
                File.Delete(nuevo);
            }

            return arrayDeBytesZip;
        }

        public static string SinTildes(string texto)
        {
            string sinTilde = new String(
                texto.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray()
            )
            .Normalize(NormalizationForm.FormC);

            return sinTilde;
        }
    }
}
