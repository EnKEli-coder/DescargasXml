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

                string query = @"Select distinct year(FechaPago) from ProductosNomina.dbo.TBL_layoutXML 
                                where year(FechaPago) > 2017 order by year(FechaPago)";

                List<int> Anios = new List<int>();

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
            try
            {
                string query = $"Select distinct MONTH(FechaPago) from ProductosNomina.dbo.TBL_layoutXML" +
                               $" where year(FechaPago) = {anio} order by MONTH(FechaPago)";

                List<int> meses = new List<int>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand command = new SqlCommand(query, connection);

                    command.CommandTimeout = 120;

                    SqlDataReader reader =  await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        meses.Add(int.Parse(reader[0].ToString()));
                    }

                    return meses;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }
        public static async Task<List<PartidaDTO>> ObtenerPartidas(int anio)
        {
            try
            {
                List<PartidaDTO> partidas = new List<PartidaDTO>();

                string anioInterfaz = "";
                if (anio != DateTime.Now.Year)
                {
                    anioInterfaz = "" + anio;
                }

                string query = $"select  distinct substring(b.partida,1,3) as ramo, substring(b.partida,1,6) as partida," +
                                $"c.descrip as nombrePartida, a.descrip as nombreRamo " +
                                $"from nomina.dbo.nom_cat_a_part_{anio} a right join Interfaces{anioInterfaz}.dbo.ACUM{anio} b on SUBSTRING(b.PARTIDA, 2, 2) = a.part" +
                                $" left join nomina.dbo.nom_cat_a_part_{anio} c on SUBSTRING(b.PARTIDA, 2, 5) = SUBSTRING(c.part, 1, 5)" +
                                $" order by SUBSTRING(PARTIDA,1,6)";

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

        public static Dictionary<string, string> obtenerDescripciones()
        {
            try
            {
                Dictionary<string, string> descripciones = new Dictionary<string, string>();

                string query = @"select distinct(SUBSTRING(part, 1, 5)), descrip from nomina.dbo.nom_cat_a_part_2021 
                                order by SUBSTRING(part, 1, 5)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(query, connection);

                    command.CommandTimeout = 60;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        descripciones.Add(reader[0].ToString(), reader[1].ToString());
                    }

                    return descripciones;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public static async Task<List<Decimal>> ObtenerTotal(int anio, string mes, string partidas)
        {
            try
            {
                List<Decimal> datos = new List<Decimal>();

                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand();

                    string anioInterfaz = "";
                    if (anio != DateTime.Now.Year)
                    {
                        anioInterfaz = "" + anio;
                    }

                    command.Connection = connection;
                    command.CommandText = $"SELECT  count (tlx.contenidoXML ) TotalXMl  , sum(tlx.isr) SumaISR " +
                        $"FROM ProductosNomina.dbo.TBL_layoutXML tlx INNER JOIN Interfaces{anioInterfaz}.dbo.ACUM{anio} a  ON a.FOLIOCFDI = tlx.folio " +
                        $"where SUBSTRING(a.PARTIDA,1,6) in ({partidas}) " +
                        $"and YEAR(tlx.FechaPago) = {anio} " +
                        $"AND MONTH(tlx.FechaPago) in ({mes}) " +
                        $"AND a.NOMINA_F <> '03'" +
                        $"AND tlx._Status = 'E' " +
                        $"and SUBSTRING(Archivo, 32, 3) != 'VTE'";

                    command.CommandTimeout = 300;

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    
                    while (reader.Read())
                    {
                        datos.Add(Convert.ToInt32(reader[0]));
                        datos.Add(Convert.ToDecimal(reader[1]));
                    }

                    return datos;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public static async Task<List<XmlDTO>> ObtenerXmls(int anio, int mes, string[] partidas)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
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
                    command.CommandText = $"SELECT substring(a.PARTIDA,1,6) , tlx.Archivo, tlx.ISR, tlx.Num, tlx.Rfc, tlx.contenidoXML " +
                        $"FROM ProductosNomina.dbo.TBL_layoutXML tlx INNER JOIN Interfaces{anioInterfaz}.dbo.ACUM{anio} a ON a.FOLIOCFDI = tlx.folio " +
                        $"where SUBSTRING(a.PARTIDA,1,6) in (" + listaPartidas + ") " +
                        "and YEAR(tlx.FechaPago) = " + anio + " " +
                        "AND MONTH(tlx.FechaPago) = " + mes + " " +
                        "AND a.NOMINA_F <> '03'" +
                        "AND tlx._Status = 'E' " +
                        "and SUBSTRING(Archivo,32,3) != 'VTE' " +
                        "order by SUBSTRING(a.PARTIDA,1,6)";

                    command.CommandTimeout = 600;

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
                        $"AND YEAR(p.FechaPago) = "+ anio + " " +
                        $"AND MONTH(p.FechaPago) = "+ mes +" " +
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
                            ramo = "00"+ramo.Substring(0,1)+"-"+ramo.Substring(1,2),
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

        public static async Task<List<ReporteAudiDTO>> ObtenerDatosAuditoria(int anio, int mes, string[] partidas)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand();

                    string listaPartidas = "";
                    string meses;

                    int i = 0;

                    List<ReporteAudiDTO> resultados = new List<ReporteAudiDTO>();

                    if(mes == 0)
                    {
                        meses = "1,2,3,4,5,6,7,8,9,10,11,12";
                    }
                    else
                    {
                        meses = mes.ToString();
                    }

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
                    command.CommandText = $"SELECT SUBSTRING(a.partida,1,3) AS ramo, n.descrip, SUM(a.TOT_PER), SUM(a.TOT_DEDU), SUM(a.LIQUIDO) " +
                        $"from Interfaces{anioInterfaz}.dbo.ACUM{anio} a INNER JOIN ProductosNomina.dbo.TBL_layoutXML p " +
                        $"ON p.folio = a.FOLIOCFDI " +
                        $"inner join nomina.dbo.nom_cat_a_part_{anio} n ON SUBSTRING(a.PARTIDA, 2, 2) COLLATE MODERN_SPANISH_CI_AS = n.PART " +
                        $"WHERE SUBSTRING(a.PARTIDA,1,6) IN (" + listaPartidas + ") " +
                        $"AND YEAR(p.FechaPago) = " + anio + " " +
                        $"AND MONTH(p.FechaPago) in (" + meses + ") " +
                        $"AND a.NOMINA_F <> '03' " +
                        $"AND p._Status = 'E' " +
                        $"AND SUBSTRING(Archivo,32,3) != 'VTE' " +
                        $"group by SUBSTRING(a.partida, 1, 3), n.descrip " +
                        $"order by SUBSTRING(a.partida,1,3)";

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        string ramo = reader[0].ToString();
                        string nombreRamo = reader[1].ToString();
                        decimal percepciones = Convert.ToInt32(reader[2]);
                        decimal deducciones = Convert.ToInt32(reader[3]);
                        decimal liquido = Convert.ToInt32(reader[4]);

                        resultados.Add(new ReporteAudiDTO
                        {
                            Codigo = ramo,
                            Departamento = nombreRamo,
                            Percepciones = percepciones,
                            Deducciones = deducciones,
                            Liquido = liquido
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
