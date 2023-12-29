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
using Newtonsoft.Json;
using System.Web.Script.Serialization;

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
                var jsonContent = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(jsonContent))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new JsonSerializer();
                using (var jsonReader = new JsonTextReader(new StringReader(jsonContent)))
                {
                    var app = serializer.Deserialize<Application>(jsonReader);

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
                return BadRequest("Error processing JSON data");
            }
        }


        [HttpGet]
        [Route("api/somiod/applications")]
        public IHttpActionResult GetAllApplications()
        {
            try
            {
                List<Application> applications = new List<Application>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, name, creation_dt FROM Application";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
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
                                applications.Add(app);
                            }
                        }
                    }
                }

                return Ok(applications);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving applications");
            }
        }

        [HttpGet]
        [Route("api/somiod/applications/{id}")]
        public IHttpActionResult GetApplicationById(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, name, creation_dt FROM Application WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);

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
                                return Ok(app);
                            }
                        }
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving application by ID");
            }
        }


        //CRUD APPLICATION

        [HttpPut]
        [Route("api/somiod/applications/{id}")]
        public IHttpActionResult UpdateApplication(int id)
        {
            try
            {
                var jsonString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(jsonString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new JavaScriptSerializer();
                var updatedApp = serializer.Deserialize<Application>(jsonString);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "UPDATE Application SET name = @name WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", updatedApp.name);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error updating application");
            }
        }



        [HttpDelete]
        [Route("api/somiod/applications/{id}")]
        public IHttpActionResult DeleteApplication(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Application WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error deleting application");
            }
        }

        //CRUD CONTAINER

        [Route("api/somiod/{application}")]
        [HttpPost]
        public IHttpActionResult PostContainer()
        {
            try
            {
                var application = Request.GetRouteData().Values["application"].ToString();
                var jsonString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(jsonString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var container = JsonConvert.DeserializeObject<Container>(jsonString);

                Application parentApp = GetApplicationByName(application);

                SqlConnection conn = null;
                string queryString = "INSERT INTO Container (name, creation_dt, parent) VALUES (@name, GETDATE(), @parent)";

                try
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    SqlCommand command = new SqlCommand(queryString, conn);
                    command.Parameters.AddWithValue("@name", container.name);
                    command.Parameters.AddWithValue("@parent", parentApp.id);
                    SqlDataReader sqlReader = command.ExecuteReader();

                    return Ok();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return BadRequest("Error inserting data into the database");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error processing JSON data");
            }
        }

        [HttpGet]
        [Route("api/somiod/containers")]
        public IHttpActionResult GetAllContainers()
        {
            try
            {
                List<Container> containers = new List<Container>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, name, creation_dt, parent FROM Container";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                Container container = new Container
                                {
                                    id = sqlReader.GetInt32(0),
                                    name = sqlReader.GetString(1),
                                    creation_dt = sqlReader.GetDateTime(2),
                                    parent = sqlReader.GetInt32(3),
                                };
                                containers.Add(container);
                            }
                        }
                    }
                }

                return Ok(containers);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving containers");
            }
        }

        [HttpGet]
        [Route("api/somiod/containers/{id}")]
        public IHttpActionResult GetContainerById(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, name, creation_dt, parent FROM Container WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                Container container = new Container
                                {
                                    id = sqlReader.GetInt32(0),
                                    name = sqlReader.GetString(1),
                                    creation_dt = sqlReader.GetDateTime(2),
                                    parent = sqlReader.GetInt32(3),
                                };
                                return Ok(container);
                            }
                        }
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving container by ID");
            }
        }


        [HttpPut]
        [Route("api/somiod/containers/{id}")]
        public IHttpActionResult UpdateContainer(int id)
        {
            try
            {
                var xmlString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(xmlString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new XmlSerializer(typeof(Container));

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    TextReader reader = new StringReader(xmlString);

                    var updatedContainer = (Container)serializer.Deserialize(reader);

                    string query = "UPDATE Container SET name = @name, parent = @parent WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", updatedContainer.name);
                        command.Parameters.AddWithValue("@parent", updatedContainer.parent);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error updating container");
            }
        }

        [HttpDelete]
        [Route("api/somiod/containers/{id}")]
        public IHttpActionResult DeleteContainer(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Container WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error deleting container");
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