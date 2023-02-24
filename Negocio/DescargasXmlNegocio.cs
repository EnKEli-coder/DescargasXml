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
using System.Drawing;
using OfficeOpenXml.Style;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace Negocio
{
    public class DescargasXmlNegocio
    {
        public static List<int> ObtenerAniosXmls()
        {
            return Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerAniosXmls();
        }

        public static async Task<Dictionary<int, string>> ObtenerMesesXmls(int anio)
        {
            //return Datos.ProductosNominaDB.ObtenerTransacciones.ObtenerMesesXmls(anio);
            List<int> data = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);

            Dictionary<int, string> meses = new Dictionary<int, string>();

            DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;

            data.ForEach(m => meses.Add(m, dateInfo.GetMonthName(m).ToUpper()));

            return meses;
        }

        public static async Task<Dictionary<string, List<string>>> ObtenerPartidas(int anio)
        {
            List<PartidaDTO> partidas = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerPartidas(anio);

            Dictionary<string, List<string>> partidasPorUnidades = new Dictionary<string, List<string>>();

            partidasPorUnidades = partidas.GroupBy(r => r.ramo + " " + r.nombreRamo).ToDictionary(ramo => ramo.Key, ramo => ramo.Select(part => part.partida + " " + part.nombrePartida).ToList());

            return partidasPorUnidades;
        }

        public static async Task<List<Decimal>> ObtenerTotal(int anio, int mes, string[] partidas)
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
                List<int> data =  await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);

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


            return await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerTotal(anio, meses, listaPartidas);
        }

        public static async Task<byte[]> ObtenerXmls(int anio, int mes, string[] partidas, int carpetas)
        {
            List<XmlDTO> xmls = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXmls(anio, mes, partidas);

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

            string zip = carpetaDescargas + ".zip";

            if (Directory.Exists(carpetaDescargas))
            {
                if (!Directory.Exists(zip))
                {
                    ZipFile.CreateFromDirectory(carpetaDescargas, zip);
                    Directory.Delete(carpetaDescargas, true);

                }
            }
            var arrayDeBytesZip = File.ReadAllBytes(zip);

            if (File.Exists(zip))
            {
                File.Delete(zip);
            }

            return arrayDeBytesZip;
        }

        public static async Task<byte[]> ObtenerXmlsAnio(int anio, string[] partidas, int carpetas)
        {
            List<int> meses = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);
            string raiz = @"C:\Reporte\";
            string carpetaDescargas;

            if (carpetas == 0)
            {
                carpetaDescargas = Path.Combine(raiz, "XMLs_" + anio);
                Directory.CreateDirectory(carpetaDescargas);

                foreach (int mes in meses)
                {
                    List<XmlDTO> xmls = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXmls(anio, mes, partidas);

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
                    List<XmlDTO> xmls = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerXmls(anio, mes, partidas);

                    //crear carpeta(s)

                    DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;
                    string nombreMes = dateInfo.GetMonthName(mes);

                    string carpetaMes = Path.Combine(carpetaDescargas, "XMLs_" + anio + "_" + nombreMes);
                    Directory.CreateDirectory(carpetaMes);

                    var ramos = xmls.GroupBy(xml => xml.partida.Substring(0, 3));

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

            string zip = carpetaDescargas + ".zip";

            if (Directory.Exists(carpetaDescargas))
            {
                if (!Directory.Exists(zip))
                {
                    ZipFile.CreateFromDirectory(carpetaDescargas, zip);
                    Directory.Delete(carpetaDescargas, true);
                }
            }

            var arrayDeBytesZip = File.ReadAllBytes(zip);

            if (File.Exists(zip))
            {
                File.Delete(zip);
            }

            return arrayDeBytesZip;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public static async Task<byte[]> ObtenerCfdis(int anio, int mes, string[] lista)
        {
            List<CfdiDTO> cfdis = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerCfdis(anio, mes, lista);
            string raiz = @"C:\Reporte\CFDI";
            string carpetaDescargas = @"\\172.19.3.172\E\Descargas_CFDI\" + anio;
            Directory.CreateDirectory(raiz);
            string username = "finanzas\\diego.ruz";
            string password = "Analista101";


            NetworkShare.ConnectToShare(@"\\172.19.3.172\E", username, password);

            if (Directory.Exists(carpetaDescargas))
            {
                foreach (var cfdi in cfdis)
                {
                        File.Copy(cfdi.Ruta, raiz + "\\" + cfdi.Nombre + ".pdf");
                }
            }

            NetworkShare.DisconnectFromShare(@"\\172.19.3.172\E", false);

            string zip = raiz + ".zip";

            if (Directory.Exists(raiz))
            {
                if (!Directory.Exists(zip))
                {
                    ZipFile.CreateFromDirectory(raiz, zip);
                    Directory.Delete(raiz, true);
                }
            }

            var arrayDeBytesZip = File.ReadAllBytes(zip);

            if (File.Exists(zip))
            {
                File.Delete(zip);
            }

            return arrayDeBytesZip;

            //DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;

            //var count = 0;

            //foreach(var cfdi in cfdis)
            //{
            //    cfdi.Mes = dateInfo.GetMonthName(Convert.ToInt32(cfdi.Mes)).ToUpper();
            //}

            //var meses = cfdis.GroupBy(x => x.Mes);

            //try
            //{
            //    var username = "finanzas\\diego.ruz";
            //    var password = "Analista101";
            //    var raiz = @"C:\Reporte\CFDI";

            //    Directory.CreateDirectory(raiz);

            //    NetworkShare.ConnectToShare(@"\\172.19.3.172\E", username, password);

            //    if (Directory.Exists(@"\\172.19.3.172\E\Descargas_CFDI\"+anio))
            //    {
            //        string carpetaDescargas = @"\\172.19.3.172\E\Descargas_CFDI\"+anio;

            //        foreach(var m in meses)
            //        {
            //            string carpetaMes = carpetaDescargas + @"\" + m.Key;

            //            if (Directory.Exists(carpetaMes))
            //            {
            //                var qnas = m.ToList().GroupBy(x => x.NumQna);

            //                foreach(var qna in qnas)
            //                {
            //                    string carpetaQna= carpetaMes + @"\" + qna.Key;
            //                    string carpetaAdic = carpetaMes + @"\ADIC";

            //                    if (Directory.Exists(carpetaQna))
            //                    {
            //                        string carpetaArchivos;
            //                        foreach (var cfdi in qna)
            //                        {
            //                            if(cfdi.Adicional == "") {
            //                                carpetaArchivos = carpetaQna; 
            //                            } else {
            //                                carpetaArchivos = carpetaAdic;
            //                            }

            //                            var files = Directory.GetFiles(carpetaArchivos, cfdi.Nombre + ".pdf").FirstOrDefault();

            //                            if (files != null){
            //                                File.Copy(files, raiz + "\\" + cfdi.Nombre + ".pdf");
            //                            }
            //                        }
            //                    }
            //                }

            //            }

            //        }

            //    }
            //    NetworkShare.DisconnectFromShare(@"\\172.19.3.172\E", false);

            //    string zip = raiz + ".zip";

            //    if (Directory.Exists(raiz))
            //    {
            //        if (!Directory.Exists(zip))
            //        {
            //            ZipFile.CreateFromDirectory(raiz, zip);
            //            Directory.Delete(raiz, true);
            //        }
            //    }

            //    var arrayDeBytesZip = File.ReadAllBytes(zip);

            //    if (File.Exists(zip))
            //    {
            //        File.Delete(zip);
            //    }

            //    return arrayDeBytesZip;
            //}
            //catch(Exception e)
            //{
            //    throw;
            //}

        }

        public static async Task<byte[]> ObtenerReportes(int anio, int mesEscogido, string[] partidas, Boolean macro, Boolean audit)
        {
            string raiz = @"C:\Reporte\";
            string carpetaDescargas;

            carpetaDescargas = Path.Combine(raiz, "Reportes");
            Directory.CreateDirectory(carpetaDescargas);

            if (macro)
            {
                Task<byte[]> macroT = crearReporteMacro(anio, mesEscogido, partidas);
                byte[] macorReporte = await macroT;
            }

            if (audit)
            {
                Task<byte[]> auditoriaT = crearReporteAuditoria(anio, mesEscogido, partidas);
                byte[] auditoria = await auditoriaT;
            }

            string zip = carpetaDescargas + ".zip";

            if (Directory.Exists(carpetaDescargas))
            {
                if (!Directory.Exists(zip))
                {
                    ZipFile.CreateFromDirectory(carpetaDescargas, zip);
                    Directory.Delete(carpetaDescargas, true);

                }
            }
            var arrayDeBytesZip = File.ReadAllBytes(zip);

            if (File.Exists(zip))
            {
                File.Delete(zip);
            }

            return arrayDeBytesZip;
        }

        public static List<MacroDTO> rellenarDatosXml(List<MacroDTO> xlss)
        {
            foreach (MacroDTO row in xlss)
            {
                XmlDocument documento = new XmlDocument();
                XmlNamespaceManager nm = new XmlNamespaceManager(documento.NameTable);
                nm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                nm.AddNamespace("nomina12", "http://www.sat.gob.mx/nomina12");

                byte[] bytes = Encoding.Default.GetBytes(row.contenidoXml);
                string xmlValue = Encoding.UTF8.GetString(bytes);
                documento.LoadXml(xmlValue);

                XmlElement root = documento.DocumentElement;
                XmlNode node = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina", nm);
                XmlNode deducciones = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:Deducciones", nm);
                XmlNode otrosPagos = root.SelectSingleNode("/cfdi:Comprobante/cfdi:Complemento/nomina12:Nomina/nomina12:OtrosPagos", nm);
                row.tipoNomina = node.Attributes["TipoNomina"] != null ? node.Attributes["TipoNomina"].Value : "";
                if (row.tipoNomina == "O")
                {
                    row.tipoNomina = "NORMAL";
                } else if (row.tipoNomina == "E")
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

        public static async Task<byte[]> crearReporteMacro(int anio, int mesEscogido, string[] partidas)
        {
            List<MacroDTO> depIsrList = new List<MacroDTO>();

            //Si mes = 0, obtener informacion de todo el año
            if (mesEscogido == 0)
            {
                List<int> meses = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerMesesXmls(anio);

                foreach (int mes in meses)
                {
                    List<MacroDTO> info =  await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerDatosMacro(anio, mes, partidas);

                    info = rellenarDatosXml(info);
                    depIsrList.AddRange(info);
                }
            }
            else
            {
                List<MacroDTO> info = await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerDatosMacro(anio, mesEscogido, partidas);
                info = rellenarDatosXml(info);
                depIsrList.AddRange(info);
            }

            DateTimeFormatInfo dateInfo = new CultureInfo("es-ES", false).DateTimeFormat;

            var nuevo = @"C:\Reporte\Reportes\Macro.xlsx";
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
            var listaOrigen = depIsrList.GroupBy(x => new {x.tipoRegimen, x.tipoNomina, x.numQna })
                .OrderBy(g => g.Key.tipoRegimen)
                .ThenBy(g => g.Key.tipoNomina)
                .ThenBy(g => g.Key.numQna)
                .Select(g => new { 
                    Dependencia = "SAFIN",
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

            foreach(var departamento in ramos)
            {
                sheetDepIsr.Cells[rowCount, 1].Value = departamento.Key;
                sheetDepIsr.Cells[rowCount, 2, rowCount, 4].Merge = true;
                sheetDepIsr.Cells[rowCount, 2].Value = departamento.ToList().Select(x => x.nombreRamo);
                Decimal total = 0;
                foreach(var meses in departamento.ToList().Where(x => x.tipoRegimen == 2).GroupBy(x => x.fechaPago.Substring(5,2)))
                {
                    sheetDepIsr.Cells[rowCount, columnCount].Value = meses.Sum(x => x.isr);
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
                sheetDepIsr.Cells[rowCount, columnCount].Value = mes.ToList().Where(x => x.tipoRegimen == 2).Sum(x => x.isr);
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
            columnCount = 5;

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
                        if(propiedad.Name != "contenidoXml")
                        {
                            sheet.Cells[rowCount, columnCount].Value = propiedad.GetValue(registro);
                            columnCount++;
                        }
                    }
                }
            }

            excel.Save();

            var arrayDeBytes = File.ReadAllBytes(nuevo);

            //if (File.Exists(nuevo))
            //{
            //    File.Delete(nuevo);
            //}

            return arrayDeBytes;
        }

        public static async Task<byte[]> crearReporteAuditoria(int anio, int mesEscogido, string[] partidas)
        {
            List<ReporteAudiDTO> datos =  await Datos.ProductosNominaDB.DescargasXmlDatos.ObtenerDatosAuditoria(anio, mesEscogido, partidas);
            
            var nuevo = @"C:\Reporte\Reportes\Auditoria.xlsx";

            var archivoBase = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Recursos/Auditoria.xlsx");

            var rutaNuevo = new FileInfo(nuevo);
            rutaNuevo.Refresh();
            var rutaBase = new FileInfo(archivoBase);
            rutaBase.Refresh();

            ExcelPackage origen = new ExcelPackage(rutaBase);
            ExcelPackage excel = new ExcelPackage(rutaNuevo);

            var hojaLiquido = origen.Workbook.Worksheets["LIQUIDO"];

            var sheetOrigen = excel.Workbook.Worksheets.Add("LIQUIDO", hojaLiquido);

            var columnCount = 1;
            var rowCount = 2;

            PropertyInfo[] propiedades = typeof(ReporteAudiDTO).GetProperties();

            foreach (var departamento in datos)
            {
                columnCount = 1;
                foreach (var propiedad in propiedades)

                {
                    sheetOrigen.Cells[rowCount, columnCount].Value = propiedad.GetValue(departamento);
                    columnCount++;
                }
                rowCount++;
            }

            excel.Save();

            byte[] arrayDeBytes =  File.ReadAllBytes(nuevo);
            return arrayDeBytes;
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
