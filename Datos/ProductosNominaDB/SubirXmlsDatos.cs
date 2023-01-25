using Entidades.DTOs.SubirXmlsDTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.ProductosNominaDB
{
    public class SubirXmlsDatos
    {
        static string connectionString = CadenasDeConexion.Conexiones.conexionPrueba();

        public static List<string> obtenerPartidas()
        {
            try
            {
                List<string> partidas = new List<string>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        command.CommandText = @"SELECT distinct part, descrip
                                            FROM [Nomina].[dbo].[nom_cat_a_part_2022]";
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            partidas.Add(reader[0] + " " + reader[1]);
                        }

                        return partidas;
                    }
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public static List<string> buscarRepetido(string folios)
        {
            try
            {
                List<string> repetidos = new List<string>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        command.CommandText = @"SELECT folio FROM ProductosNomina.dbo.TBL_layoutXML_Externos
                                            WHERE folio in (" + folios + ")";
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            repetidos.Add(reader[0].ToString());
                        }

                        return repetidos;
                    }
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public static bool InsertarXmls(List<Externo> rows)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = connection;
                            command.Transaction = transaction;
                            command.CommandTimeout = 300;
                            command.CommandType = CommandType.Text;
                            command.CommandText = @"INSERT INTO ProductosNomina.dbo.TBL_layoutXML_Externos values
                                            (@archivo, @contXml,@folioFiscal, @fechaTimbrado, @selloCFD, @certSat,
                                            @selloSat, @isr, @subsidio, @ajusteISR, @devISR, @nominaVer, @num, @rfc,
                                            @tipoContrato, @nsc, @sindicalizado, @curp, @tipoNomina, @fechaPago,
                                            @fechaIPago, @fechaFPago, @departamento, @puesto, @tipoJornada, @tipoRegimen,
                                            @totalPercep, @totalDeducc, @totalOtros, @subtotal, @total, @descuento,
                                            @origenRecurso, @numqna, @status, @valida2016, @cadenaTemp, @folio, @numche,
                                            @idRegistro, @fechaCancel, @referencia, @subsidioE, @subsidioC, @fechaRetimb, @fechaRegistro)";
                            command.Parameters.Add("@archivo", SqlDbType.VarChar, 100);
                            command.Parameters.Add("@contXml", SqlDbType.VarChar, -1);
                            command.Parameters.Add("@folioFiscal", SqlDbType.VarChar, 50).IsNullable = true;
                            command.Parameters.Add("@fechaTimbrado", SqlDbType.SmallDateTime).IsNullable = true;
                            command.Parameters.Add("@selloCFD", SqlDbType.VarChar, -1).IsNullable = true;
                            command.Parameters.Add("@certSat", SqlDbType.VarChar, -1).IsNullable = true;
                            command.Parameters.Add("@selloSat", SqlDbType.VarChar, -1).IsNullable = true;
                            command.Parameters.Add("@isr", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@subsidio", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@ajusteISR", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@devISR", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@nominaVer", SqlDbType.VarChar, 5).IsNullable = true;
                            command.Parameters.Add("@num", SqlDbType.VarChar, 5).IsNullable = true;
                            command.Parameters.Add("@rfc", SqlDbType.VarChar, 13).IsNullable = true;
                            command.Parameters.Add("@tipoContrato", SqlDbType.VarChar, 2).IsNullable = true;
                            command.Parameters.Add("@nsc", SqlDbType.VarChar, 13).IsNullable = true;
                            command.Parameters.Add("@sindicalizado", SqlDbType.VarChar, 2).IsNullable = true;
                            command.Parameters.Add("@curp", SqlDbType.VarChar, 18).IsNullable = true;
                            command.Parameters.Add("@tipoNomina", SqlDbType.Char, 1).IsNullable = true;
                            command.Parameters.Add("@fechaPago", SqlDbType.SmallDateTime).IsNullable = true;
                            command.Parameters.Add("@fechaIPago", SqlDbType.SmallDateTime).IsNullable = true;
                            command.Parameters.Add("@fechaFPago", SqlDbType.SmallDateTime).IsNullable = true;
                            command.Parameters.Add("@departamento", SqlDbType.VarChar, 200).IsNullable = true;
                            command.Parameters.Add("@puesto", SqlDbType.VarChar, 100).IsNullable = true;
                            command.Parameters.Add("@tipoJornada", SqlDbType.Char, 2).IsNullable = true;
                            command.Parameters.Add("@tipoRegimen", SqlDbType.Char, 2).IsNullable = true;
                            command.Parameters.Add("@totalPercep", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@totalDeducc", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@totalOtros", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@subtotal", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@total", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@descuento", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@origenRecurso", SqlDbType.Char, 2).IsNullable = true;
                            command.Parameters.Add("@numqna", SqlDbType.Char, 4).IsNullable = true;
                            command.Parameters.Add("@status", SqlDbType.Char, 1).IsNullable = true;
                            command.Parameters.Add("@valida2016", SqlDbType.Bit).IsNullable = true;
                            command.Parameters.Add("@cadenaTemp", SqlDbType.VarChar, 50).IsNullable = true;
                            command.Parameters.Add("@folio", SqlDbType.VarChar, 50).IsNullable = true;
                            command.Parameters.Add("@numche", SqlDbType.VarChar, 10).IsNullable = true;
                            command.Parameters.Add("@idRegistro", SqlDbType.BigInt).IsNullable = true;
                            command.Parameters.Add("@fechaCancel", SqlDbType.SmallDateTime).IsNullable = true;
                            command.Parameters.Add("@referencia", SqlDbType.VarChar, 5).IsNullable = true;
                            command.Parameters.Add("@subsidioE", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@subsidioC", SqlDbType.Decimal).IsNullable = true;
                            command.Parameters.Add("@fechaRetimb", SqlDbType.SmallDateTime).IsNullable = true;
                            command.Parameters.Add("@fechaRegistro", SqlDbType.DateTime);
                            try
                            {
                                foreach (Externo row in rows)
                                {
                                    command.Parameters["@archivo"].Value = row.Archivo;
                                    command.Parameters["@contXml"].Value = row.contenidoXML;
                                    command.Parameters["@folioFiscal"].Value = row.foliofiscal;
                                    command.Parameters["@fechaTimbrado"].Value = row.FechaTimbrado;
                                    command.Parameters["@selloCFD"].Value = row.SelloCFD;
                                    command.Parameters["@certSat"].Value = row.CertificadoSat;
                                    command.Parameters["@selloSat"].Value = row.SelloSat;
                                    command.Parameters["@isr"].Value = row.ISR == null ? 0 : row.ISR;
                                    command.Parameters["@subsidio"].Value = row.subsidio == null ? 0 : row.subsidio;
                                    command.Parameters["@ajusteISR"].Value = row.ajusteISR == null ? 0 : row.ajusteISR;
                                    command.Parameters["@devISR"].Value = row.DevISR == null ? 0 : row.DevISR;
                                    command.Parameters["@nominaVer"].Value = row.nominaversion == null ? (object)DBNull.Value : row.nominaversion;
                                    command.Parameters["@num"].Value = row.Num == null ? (object)DBNull.Value : row.Num;
                                    command.Parameters["@rfc"].Value = row.Rfc == null ? (object)DBNull.Value : row.Rfc;
                                    command.Parameters["@tipoContrato"].Value = row.TipoContrato == null ? (object)DBNull.Value : row.TipoContrato;
                                    command.Parameters["@nsc"].Value = row.NumSeguridadSocial == null ? (object)DBNull.Value : row.NumSeguridadSocial;
                                    command.Parameters["@sindicalizado"].Value = row.Sindicalizado == null ? (object)DBNull.Value : row.Sindicalizado;
                                    command.Parameters["@curp"].Value = row.Curp == null ? (object)DBNull.Value : row.Curp;
                                    command.Parameters["@tipoNomina"].Value = row.TipoNomina == null ? (object)DBNull.Value : row.TipoNomina;
                                    command.Parameters["@fechaPago"].Value = row.FechaPago == null ? (object)DBNull.Value : row.FechaPago;
                                    command.Parameters["@fechaIPago"].Value = row.FechaInicialPago == null ? (object)DBNull.Value : row.FechaInicialPago;
                                    command.Parameters["@fechaFPago"].Value = row.FechaFinalPago == null ? (object)DBNull.Value : row.FechaFinalPago;
                                    command.Parameters["@departamento"].Value = row.Departamento == null ? (object)DBNull.Value : row.Departamento;
                                    command.Parameters["@puesto"].Value = row.Puesto == null ? (object)DBNull.Value : row.Puesto;
                                    command.Parameters["@tipoJornada"].Value = row.TipoJornada == null ? (object)DBNull.Value : row.TipoJornada;
                                    command.Parameters["@tipoRegimen"].Value = row.TipoRegimen == null ? (object)DBNull.Value : row.TipoRegimen;
                                    command.Parameters["@totalPercep"].Value = row.TotalPercepciones == null ? (object)DBNull.Value : row.TotalPercepciones;
                                    command.Parameters["@totalDeducc"].Value = row.TotalDeducciones == null ? (object)DBNull.Value : row.TotalDeducciones;
                                    command.Parameters["@totalOtros"].Value = row.TotalOtrosPagos == null ? (object)DBNull.Value : row.TotalOtrosPagos;
                                    command.Parameters["@subtotal"].Value = row.subTotal == null ? (object)DBNull.Value : row.subTotal;
                                    command.Parameters["@total"].Value = row.Total == null ? (object)DBNull.Value : row.Total;
                                    command.Parameters["@descuento"].Value = row.descuento == null ? (object)DBNull.Value : row.descuento;
                                    command.Parameters["@origenRecurso"].Value = row.OrigenRecurso == null ? (object)DBNull.Value : row.OrigenRecurso;
                                    command.Parameters["@numqna"].Value = row.numqna == null ? (object)DBNull.Value : row.numqna;
                                    command.Parameters["@status"].Value = row._Status == null ? (object)DBNull.Value : row._Status;
                                    command.Parameters["@valida2016"].Value = row.valida2016 == null ? (object)DBNull.Value : row.valida2016;
                                    command.Parameters["@cadenaTemp"].Value = row.CadenaTemp == null ? (object)DBNull.Value : row.CadenaTemp;
                                    command.Parameters["@folio"].Value = row.folio;
                                    command.Parameters["@numche"].Value = row.num_che == null ? (object)DBNull.Value : row.num_che;
                                    command.Parameters["@idRegistro"].Value = row.idregistro == null ? (object)DBNull.Value : row.idregistro;
                                    command.Parameters["@fechaCancel"].Value = row.fechacancel == null ? (object)DBNull.Value : row.fechacancel;
                                    command.Parameters["@referencia"].Value = row.referencia == null ? (object)DBNull.Value : row.referencia;
                                    command.Parameters["@subsidioE"].Value = row.subsidioE == null ? (object)DBNull.Value : row.subsidioE;
                                    command.Parameters["@subsidioC"].Value = row.subsidiocausado == null ? (object)DBNull.Value : row.subsidiocausado;
                                    command.Parameters["@fechaRetimb"].Value = row.fecha_retimb == null ? (object)DBNull.Value : row.fecha_retimb;
                                    command.Parameters["@fechaRegistro"].Value = row.fechaRegistro;
                                    command.ExecuteNonQuery();
                                }
                                transaction.Commit();
                                return true;
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                                return false;
                                throw;
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public static void GuardarRegistro(LoteRegistroDTO lote)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandTimeout = 300;
                        command.CommandType = CommandType.Text;
                        command.CommandText = @"Insert into ProductosNomina.dbo.TBL_HistoryExternos values
                                            (@RegistroId, @NoRegistros, @Dependencia, @FechaRegistro)";

                        command.Parameters.Add("@RegistroId", SqlDbType.VarChar, 20);
                        command.Parameters.Add("@NoRegistros", SqlDbType.Int);
                        command.Parameters.Add("@Dependencia", SqlDbType.VarChar, 200);
                        command.Parameters.Add("@FechaRegistro", SqlDbType.DateTime);

                        command.Parameters["@RegistroId"].Value = lote.registroId;
                        command.Parameters["@NoRegistros"].Value = lote.noRegistros;
                        command.Parameters["@Dependencia"].Value = lote.Dependencia;
                        command.Parameters["@FechaRegistro"].Value = lote.FechaRegistro;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<LoteRegistroDTO> ObtenerRegistros()
        {
            List<LoteRegistroDTO> registros = new List<LoteRegistroDTO>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        command.CommandText = @"SELECT RegistroID, NoRegistros, Dependencia, FechaRegistro
                                            FROM ProductosNomina.dbo.TBL_HistoryExternos";
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            registros.Add(new LoteRegistroDTO
                            {
                                registroId = reader[0].ToString(),
                                noRegistros = Convert.ToInt32(reader[1]),
                                Dependencia = reader[2].ToString(),
                                FechaRegistro = reader[3].ToString()
                            });
                        }

                        return registros;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool EliminarRegistro(DateTime fecha)
        {
            int affected;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 120;
                        command.CommandText = @"Delete from TBL_historyExternos where FechaRegistro = @fecha";
                        command.Parameters.Add("@fecha", SqlDbType.DateTime);
                        command.Parameters["@fecha"].Value = fecha;
                        affected = command.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
