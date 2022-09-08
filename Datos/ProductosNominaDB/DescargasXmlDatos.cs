using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos;
using System.Data.SqlClient;
using Entidades.DTOs.DescargasXmlDTOs;
using Entidades.DTOs;

namespace Datos.ProductosNominaDB
{
    public class DescargasXmlDatos
    {

        static string connectionString = CadenasDeConexion.Conexiones.conexionPrueba();

        public static List<int> ObtenerAniosXmls()
        {
            try
            {

                string query = "Select year(FechaPago) from ProductosNomina.dbo.TBL_layoutXML where year(FechaPago) > 2017 group by year(FechaPago)";

                List<int> Anios = new List<int>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        Anios.Add(Convert.ToInt32(reader[0].ToString()));
                    }
                }
                return Anios;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<int> ObtenerMesesXmls(int anio)
        {
            try
            {
                string query = $"Select MONTH(FechaPago) from ProductosNomina.dbo.TBL_layoutXML where year(FechaPago) = {anio} group by MONTH(FechaPago) order by MONTH(FechaPago)";

                List<int> meses = new List<int>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        meses.Add(int.Parse(reader[0].ToString()));
                    }

                    return meses;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static List<string> ObtenerPartidas(int anio)
        {
            try
            {
                List<string> partidas = new List<string>();

                string anioInterfaz = "";
                if (anio != DateTime.Now.Year)
                {
                    anioInterfaz = "" + anio;
                }

                string query = $"select distinct( SUBSTRING(PARTIDA,1,6)) from Interfaces{anioInterfaz}.dbo.ACUM{anio} order by SUBSTRING(PARTIDA,1,6)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        partidas.Add(reader[0].ToString());
                    }

                    return partidas;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public static List<int> ObtenerTotal(int anio, string mes, string partidas)
        {
            try
            {
                List<int> datos = new List<int>();

                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();

                    string anioInterfaz = "";
                    if (anio != DateTime.Now.Year)
                    {
                        anioInterfaz = "" + anio;
                    }

                    command.Connection = connection;
                    command.CommandText = $"SELECT  count (tlx.contenidoXML ) TotalXMl  , sum(tlx.isr) SumaISR FROM ProductosNomina.dbo.TBL_layoutXML tlx INNER JOIN Interfaces{anioInterfaz}.dbo.ACUM{anio} a  ON a.FOLIOCFDI = tlx.folio where SUBSTRING(a.PARTIDA,1,6) in ({partidas}) and YEAR(tlx.FechaPago) = {anio} AND MONTH(tlx.FechaPago) in ({mes}) AND tlx._Status = 'E' and SUBSTRING(Archivo, 32, 3) != 'VTE'";

                    SqlDataReader reader = command.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        datos.Add(Convert.ToInt32(reader[0]));
                        datos.Add(Convert.ToInt32(reader[1]));
                    }

                    return datos;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static List<XmlDTO> ObtenerXmls(int anio, int mes, string[] partidas)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();

                    string listaPartidas = "";

                    int i = 0;

                    List<XmlDTO> resultados = new List<XmlDTO>();

                    foreach (var partida in partidas)
                    {
                        if (i != 0)
                        {
                            listaPartidas += ",";
                        }
                        listaPartidas += "'" + partida + "'";
                        i++;
                    }

                    string anioInterfaz = "";
                    if (anio != DateTime.Now.Year)
                    {
                        anioInterfaz = "" + anio;
                    }

                    command.Connection = connection;
                    command.CommandText = $"SELECT a.PARTIDA , tlx.Archivo, tlx.ISR, tlx.Num, tlx.Rfc, tlx.contenidoXML FROM ProductosNomina.dbo.TBL_layoutXML tlx INNER JOIN Interfaces{anioInterfaz}.dbo.ACUM{anio} a ON a.FOLIOCFDI = tlx.folio where SUBSTRING(a.PARTIDA,1,6) in ("+listaPartidas+") and YEAR(tlx.FechaPago) = "+anio+" AND MONTH(tlx.FechaPago) = "+mes+ " AND tlx._Status = 'E' and SUBSTRING(Archivo,32,3) != 'VTE' order by a.PARTIDA";

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string partida = reader[0].ToString();
                        string archivo = reader[1].ToString();
                        int isr = Convert.ToInt32(reader[2]);
                        int num = Convert.ToInt32(reader[3]);
                        string rfc = reader[4].ToString();
                        string contenidoXml = reader[5].ToString();
                        resultados.Add(new XmlDTO
                        {
                            partida = partida,
                            archivo = archivo,
                            isr = isr,
                            num = num,
                            rfc = rfc,
                            contenidoXml = contenidoXml
                        }); 
                    }
                    return resultados;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<XlsDTO> ObtenerXls(int anio, int mes, string[] partidas)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();

                    string listaPartidas = "";

                    int i = 0;

                    List<XlsDTO> resultados = new List<XlsDTO>();

                    foreach (var partida in partidas)
                    {
                        if (i != 0)
                        {
                            listaPartidas += ",";
                        }
                        listaPartidas += "'" + partida + "'";
                        i++;
                    }

                    string anioInterfaz = "";
                    if (anio != DateTime.Now.Year)
                    {
                        anioInterfaz = "" + anio;
                    }

                    command.Connection = connection;
                    command.CommandText = $"SELECT tlx.Archivo, tlx.ISR, tlx.Num, tlx.Rfc, tlx.numqna, tlx.foliofiscal, tlx.Sindicalizado, tlx.TipoContrato, tlx.NumSeguridadSocial, tlx.Curp, tlx.Departamento, tlx.Puesto, tlx.TipoJornada, tlx.TipoRegimen, tlx.contenidoXML FROM ProductosNomina.dbo.TBL_layoutXML tlx INNER JOIN Interfaces{anioInterfaz}.dbo.ACUM{anio} a ON a.FOLIOCFDI = tlx.folio where SUBSTRING(a.PARTIDA,1,6) in (" + listaPartidas + ") and YEAR(tlx.FechaPago) = " + anio + " AND MONTH(tlx.FechaPago) = " + mes + " AND tlx._Status = 'E' and SUBSTRING(Archivo,32,3) != 'VTE' order by a.PARTIDA";

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string archivo = reader[0].ToString();
                        int isr = Convert.ToInt32(reader[1]);
                        int num = Convert.ToInt32(reader[2]);
                        string rfc = reader[3].ToString();
                        int numQna = Convert.ToInt32(reader[4]);
                        string folioFiscal = reader[5].ToString();
                        string sindicalizado = reader[6].ToString();
                        int tipoContrato = Convert.ToInt32(reader[7]);
                        string numeroSeguridadSocial = reader[8] == DBNull.Value ? "" : reader[8].ToString();
                        string curp = reader[9].ToString();
                        string departamento = reader[10].ToString();
                        string puesto = reader[11].ToString();
                        int tipoJornada = Convert.ToInt32(reader[12]);
                        int tipoRegimen = Convert.ToInt32(reader[13]);
                        string contenidoXml = reader[14].ToString();
                        resultados.Add(new XlsDTO
                        {
                            archivo = archivo,
                            isr = isr,
                            num = num,
                            rfc = rfc,
                            numQna = numQna,
                            folioFiscal = folioFiscal,
                            sindicalizado = sindicalizado,
                            tipoContrato = tipoContrato,
                            numSeguridadSocial = numeroSeguridadSocial,
                            curp = curp,
                            departamento = departamento,
                            puesto = puesto,
                            tipoJornada = tipoJornada,
                            tipoRegimen = tipoRegimen,
                            contenidoXml = contenidoXml
                        });
                    }
                    return resultados;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
