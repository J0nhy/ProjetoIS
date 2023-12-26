using ProjetoIS_D02.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web.DynamicData;
using System.Web.Http;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;
using Application = ProjetoIS_D02.Models.Application;
using Container = ProjetoIS_D02.Models.Container;
using uPLibrary.Networking.M2Mqtt;
using System.Net.Sockets;
using System.Text;

namespace ProjetoIS_D02.Controllers
{
    public class SOMIODController : ApiController
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProjetoIS_D02.Properties.Settings.connStr"].ConnectionString;

        [HttpPost]
        [Route("api/somiod")]
        public IHttpActionResult CreateApplication()
        {
            try
            {
                var xmlString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(xmlString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                // Assuming Application class structure corresponds to the XML structure
                var serializer = new XmlSerializer(typeof(Application));
                using (TextReader reader = new StringReader(xmlString))
                {
                    var app = (Application)serializer.Deserialize(reader);

                    SqlConnection conn = null;
                    string queryString = "INSERT INTO application (name, creation_dt) VALUES (@name, GETDATE());";

                    try
                    {
                        conn = new SqlConnection(connectionString);
                        conn.Open();

                        SqlCommand command = new SqlCommand(queryString, conn);
                        command.Parameters.AddWithValue("@name", app.name);
                        SqlDataReader sqlReader = command.ExecuteReader();

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error processing XML data");
            }
        }

        [Route("api/somiod/{application}")]
        [HttpPost]
        public IHttpActionResult PostContainer()
        {
            try
            {
                var application = Request.GetRouteData().Values["application"].ToString();
                var xmlString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(xmlString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new XmlSerializer(typeof(Container));

                Application parentApp = GetApplicationByName(application);

                using (TextReader reader = new StringReader(xmlString))
                {
                    var container = (Container)serializer.Deserialize(reader);

                    SqlConnection conn = null;
                    string queryString = "INSERT INTO Container (name, creation_dt,parent) VALUES (@name, GETDATE(),@parent)";

                    try
                    {
                        conn = new SqlConnection(connectionString);
                        conn.Open();

                        SqlCommand command = new SqlCommand(queryString, conn);
                        //command.Parameters.AddWithValue("@id", container.ID);
                        command.Parameters.AddWithValue("@name", container.name);
                        command.Parameters.AddWithValue("@parent", parentApp.id);
                        SqlDataReader sqlReader = command.ExecuteReader();


                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error processing XML data");
            }
        }

        private Application GetApplicationByName(string applicationName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT id, name, creation_dt FROM Application WHERE name = @name";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@name", applicationName);

                    using (SqlDataReader sqlReader = command.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            Application app = new Application
                            {
                                id = sqlReader.GetInt32(0),
                                name = sqlReader.GetString(1),
                                creation_dt = sqlReader.GetDateTime(2),
                            };
                            return app;
                        }
                    }
                }
                return null;
            }
        }
    }


}