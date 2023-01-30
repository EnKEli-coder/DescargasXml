using Entidades.DTOs;
using Entidades.DTOs.DescargasXmlDTOs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
namespace Datos.ProductosNominaDB
{
    public class DescargasExternosDatos
    {

        static string connectionString = CadenasDeConexion.Conexiones.conexionPrueba();

        public static List<int> ObtenerAniosXmls()
        {
            List<int> Anios = new List<int>();

            try
            {
                string query = @"Select distinct year(FechaPago) from ProductosNomina.dbo.TBL_layoutXML_Externos order by year(FechaPago)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(query, connection);

                    command.CommandTimeout = 120;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Anios.Add(Convert.ToInt32(reader[0].ToString()));
                    }
                }
                return Anios;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<int>> ObtenerMesesXmls(int anio)
        {
            List<int> meses = new List<int>();

            try
            {
                string query = $"Select distinct MONTH(FechaPago) from ProductosNomina.dbo.TBL_layoutXML_Externos " +
                               $" where year(FechaPago) = {anio} order by MONTH(FechaPago)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand command = new SqlCommand(query, connection);

                    command.CommandTimeout = 120;

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        meses.Add(int.Parse(reader[0].ToString()));
                    }

                    return meses;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<PartidaDTO>> ObtenerPartidas(int anio)
        {
            List<PartidaDTO> partidas = new List<PartidaDTO>();

            try
            {
                string query = $"select distinct substring(b.Departamento,1,2) as ramo, LEFT(b.Departamento,CHARINDEX(' ', b.Departamento) -1) as partida, c.descrip as nombrePartida, a.descrip as nombreRamo " +
                                $"from nomina.dbo.nom_cat_a_part_{anio} a right join ProductosNomina.dbo.TBL_layoutXML_Externos b on SUBSTRING(b.Departamento, 1, 2) COLLATE MODERN_SPANISH_CI_AS = a.part " +
                                $"left join nomina.dbo.nom_cat_a_part_{anio} c on LEFT(b.Departamento,CHARINDEX(' ', b.Departamento) -1) COLLATE MODERN_SPANISH_CI_AS = c.PART " +
                                $"order by LEFT(b.Departamento,CHARINDEX(' ', b.Departamento) -1)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand command = new SqlCommand(query, connection);

                    command.CommandTimeout = 300;

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        string ramo = reader[0].ToString();
                        string partida = reader[1].ToString();
                        string nombrePartida = reader[2].ToString();
                        string nombreRamo = reader[3].ToString();
                        partidas.Add(new PartidaDTO
                        {
                            ramo = ramo,
                            partida = partida,
                            nombrePartida = nombrePartida,
                            nombreRamo = nombreRamo
                        });
                    }

                    return partidas;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static List<Decimal> ObtenerTotal(int anio, string mes, string partidas)
        {
            List<Decimal> datos = new List<Decimal>();

            try
            {
                

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();

                    string anioInterfaz = "";
                    if (anio != DateTime.Now.Year)
                    {
                        anioInterfaz = "" + anio;
                    }

                    command.Connection = connection;
                    command.CommandText = $"SELECT  count (tlx.contenidoXML ) TotalXMl  , sum(tlx.isr) SumaISR " +
                        $"FROM ProductosNomina.dbo.TBL_layoutXML_Externos tlx " +
                        $"where LEFT(tlx.Departamento,CHARINDEX(' ', tlx.Departamento) -1) in ({partidas}) " +
                        $"and YEAR(tlx.FechaPago) = {anio} " +
                        $"AND MONTH(tlx.FechaPago) in ({mes}) " +
                        $"AND tlx._Status = 'E' " +
                        $"and SUBSTRING(Archivo, 32, 3) != 'VTE'";

                    command.CommandTimeout = 120;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        datos.Add(Convert.ToInt32(reader[0]));
                        datos.Add(Convert.ToDecimal(reader[1]));
                    }

                    return datos;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<XmlDTO>> ObtenerXmls(int anio, int mes, string[] partidas)
        {
            List<XmlDTO> resultados = new List<XmlDTO>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand();

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

                    string anioInterfaz = "";
                    if (anio != DateTime.Now.Year)
                    {
                        anioInterfaz = "" + anio;
                    }

                    command.Connection = connection;
                    command.CommandText = $"SELECT LEFT(tlx.Departamento,CHARINDEX(' ', tlx.Departamento) -1) , tlx.Archivo, tlx.ISR, tlx.Num, tlx.Rfc, tlx.contenidoXML " +
                        $"FROM ProductosNomina.dbo.TBL_layoutXML_Externos tlx " +
                        $"where LEFT(tlx.Departamento,CHARINDEX(' ', tlx.Departamento) -1) in (" + listaPartidas + ") " +
                        "and YEAR(tlx.FechaPago) = " + anio + " " +
                        "AND MONTH(tlx.FechaPago) = " + mes + " " +
                        "AND tlx._Status = 'E' " +
                        "and SUBSTRING(Archivo,32,3) != 'VTE' " +
                        "order by LEFT(tlx.Departamento,CHARINDEX(' ', tlx.Departamento) -1)";

                    command.CommandTimeout = 300;

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        string partida = reader[0].ToString();
                        string archivo = reader[1].ToString();
                        Decimal isr = Convert.ToDecimal(reader[2]);
                        string num = reader[3].ToString();
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
            catch (Exception)
            {
                throw;
            }
        }

        public static List<MacroDTO> ObtenerXls(int anio, int mes, string[] partidas)
        {
            List<MacroDTO> resultados = new List<MacroDTO>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();

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

                    string anioInterfaz = "";
                    if (anio != DateTime.Now.Year)
                    {
                        anioInterfaz = "" + anio;
                    }

                    command.CommandTimeout = 300;
                    command.Connection = connection;
                    command.CommandText = $"SELECT CONCAT(n.cve_poder, SUBSTRING(p.departamento, 1, 2) COLLATE MODERN_SPANISH_CI_AS ) AS ramo, n.descrip, p.Departamento , p.numqna, p.foliofiscal, p.Rfc, p.TipoRegimen, p.ISR, p.contenidoXML " +
                        $"FROM ProductosNomina.dbo.TBL_layoutXML_Externos p " +
                        $"LEFT JOIN nomina.dbo.nom_cat_a_part_{anio} n ON SUBSTRING(p.Departamento, 1, 2) COLLATE MODERN_SPANISH_CI_AS = n.PART " +
                        $"where LEFT(p.Departamento,CHARINDEX(' ', p.Departamento) -1) in (" + listaPartidas + ") " +
                        $"AND YEAR(p.FechaPago) = " + anio + " " +
                        $"AND MONTH(p.FechaPago) = " + mes + " " +
                        $"AND p._Status = 'E' " +
                        $"AND SUBSTRING(Archivo,32,3) != 'VTE' " +
                        $"ORDER BY LEFT(p.Departamento,CHARINDEX(' ', p.Departamento) -1)";

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string ramo = reader[0].ToString();
                        string nombreRamo = reader[1].ToString();
                        string departamento = reader[2].ToString();
                        int numQna = Convert.ToInt32(reader[3]);
                        string folioFiscal = reader[4].ToString();
                        string rfc = reader[5].ToString();
                        int tipoRegimen = Convert.ToInt32(reader[6]);
                        decimal isr = Convert.ToDecimal(reader[7]);
                        string contenidoXml = reader[8].ToString();

                        resultados.Add(new MacroDTO
                        {
                            ramo = "00" + ramo.Substring(0, 1) + "-" + ramo.Substring(1, 2),
                            nombreRamo = nombreRamo,
                            Dependencia = departamento,
                            numQna = numQna,
                            folioFiscal = folioFiscal,
                            rfc = rfc,
                            tipoRegimen = tipoRegimen,
                            isr = isr,
                            contenidoXml = contenidoXml
                        });
                    }
                    return resultados;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<MacroDTO>> ObtenerDatosMacro(int anio, int mes, string[] partidas)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand();

                    string listaPartidas = "";

                    int i = 0;

                    List<MacroDTO> resultados = new List<MacroDTO>();

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

                    command.CommandTimeout = 300;
                    command.Connection = connection;
                    command.CommandText = $"SELECT SUBSTRING(a.PARTIDA,1,3) AS ramo, n.descrip, p.Departamento , p.numqna, p.foliofiscal, p.Rfc, p.TipoRegimen, p.ISR, p.contenidoXML " +
                        $"FROM ProductosNomina.dbo.TBL_layoutXML p INNER JOIN Interfaces{anioInterfaz}.dbo.ACUM{anio} a " +
                        $"ON p.folio = a.FOLIOCFDI " +
                        $"LEFT JOIN nomina.dbo.nom_cat_a_part_{anio} n ON SUBSTRING(p.departamento, 1, 2) COLLATE MODERN_SPANISH_CI_AS = n.PART " +
                        $"WHERE SUBSTRING(a.PARTIDA,1,6) IN (" + listaPartidas + ") " +
                        $"AND YEAR(p.FechaPago) = " + anio + " " +
                        $"AND MONTH(p.FechaPago) = " + mes + " " +
                        $"AND a.NOMINA_F <> '03' " +
                        $"AND p._Status = 'E' " +
                        $"AND SUBSTRING(Archivo,32,3) != 'VTE' " +
                        $"ORDER BY a.PARTIDA";

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        string ramo = reader[0].ToString();
                        string nombreRamo = reader[1].ToString();
                        string departamento = reader[2].ToString();
                        int numQna = Convert.ToInt32(reader[3]);
                        string folioFiscal = reader[4].ToString();
                        string rfc = reader[5].ToString();
                        int tipoRegimen = Convert.ToInt32(reader[6]);
                        decimal isr = Convert.ToDecimal(reader[7]);
                        string contenidoXml = reader[8].ToString();

                        resultados.Add(new MacroDTO
                        {
                            ramo = "00" + ramo.Substring(0, 1) + "-" + ramo.Substring(1, 2),
                            nombreRamo = nombreRamo,
                            Dependencia = departamento,
                            numQna = numQna,
                            folioFiscal = folioFiscal,
                            rfc = rfc,
                            tipoRegimen = tipoRegimen,
                            isr = isr,
                            contenidoXml = contenidoXml
                        });
                    }
                    return resultados;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
