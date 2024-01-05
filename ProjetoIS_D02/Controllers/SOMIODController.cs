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
using Data = ProjetoIS_D02.Models.Data;
using Subscription = ProjetoIS_D02.Models.Subscription;
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

        [HttpGet]
        [Route("api/somiod")]
        public IHttpActionResult GetApplicationsName()
        {
            try
            {
                List<string> applicationNames = new List<string>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT name FROM Application";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                string appName = sqlReader.GetString(0);
                                applicationNames.Add(appName);
                            }
                        }
                    }
                }

                var serializer = new XmlSerializer(typeof(List<string>));
                StringWriter xmlString = new StringWriter();

                serializer.Serialize(xmlString, applicationNames);

                return Ok(xmlString.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving application names in XML format");
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
                var contentType = Request.Content.Headers.ContentType?.MediaType;

                var dataString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(dataString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                Container container;

         
                    // Deserialize XML if content type is XML
                    var serializer = new XmlSerializer(typeof(Container));
                    container = (Container)serializer.Deserialize(new StringReader(dataString));
         

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
                return BadRequest("Error processing data");
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

        [HttpGet]
        [Route("api/somiod/application/{parentId}/containers")]
        public IHttpActionResult GetContainersByParentId(int parentId)
        {
            try
            {
                List<string> containerNames = new List<string>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT name FROM Container WHERE parent = @parentId";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@parentId", parentId);

                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                string containerName = sqlReader.GetString(0);
                                containerNames.Add(containerName);
                            }
                        }
                    }
                }

                // Use XmlSerializer without a wrapper class
                var serializer = new XmlSerializer(typeof(List<string>));
                StringWriter xmlString = new StringWriter();

                // Serialize the list of strings directly
                serializer.Serialize(xmlString, containerNames);

                return Ok(xmlString.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving container names by parent ID in XML format");
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


        //DATA STUFF
        [HttpGet]
        [Route("api/somiod/data")]
        public IHttpActionResult GetAllData()
        {
            try
            {
                List<Data> dataList = new List<Data>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, content, parent FROM Data";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                Data data = new Data
                                {
                                    id = sqlReader.GetInt32(0),
                                    content = sqlReader.GetString(1),
                                    parent = sqlReader.GetInt32(2)
                                };
                                dataList.Add(data);
                            }
                        }
                    }
                }

                return Ok(dataList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving data");
            }
        }

        [HttpGet]
        [Route("api/somiod/data/{id}")]
        public IHttpActionResult GetDataById(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, content, parent FROM Data WHERE id = @Id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            if (sqlReader.Read())
                            {
                                Data data = new Data
                                {
                                    id = sqlReader.GetInt32(0),
                                    content = sqlReader.GetString(1),
                                    parent = sqlReader.GetInt32(2)
                                };
                                return Ok(data);
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving data");
            }
        }

        [HttpPost]
        [Route("api/somiod/data")]
        public IHttpActionResult CreateData([FromBody] Data data)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "INSERT INTO Data (content, creation_dt, parent) VALUES (@Content, GETDATE(), @Parent); SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@Content", data.content);
                        command.Parameters.AddWithValue("@Parent", data.parent);

                        int newId = Convert.ToInt32(command.ExecuteScalar());

                        data.id = newId;
                    }
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error creating data");
            }
        }

        [HttpPut]
        [Route("api/somiod/data/{id}")]
        public IHttpActionResult UpdateData(int id, [FromBody] Data data)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "UPDATE Data SET content = @Content, parent = @Parent WHERE id = @Id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Content", data.content);
                        command.Parameters.AddWithValue("@Parent", data.parent);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(data);
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
                return BadRequest("Error updating data");
            }
        }

        [HttpDelete]
        [Route("api/somiod/data/{id}")]
        public IHttpActionResult DeleteData(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Data WHERE id = @Id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@Id", id);

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
                return BadRequest("Error deleting data");
            }
        }

        //subscription stuff

    
        [HttpGet]
        [Route("api/somiod/subscriptions")]
        public IHttpActionResult GetAllSubscriptions()
        {
            try
            {
                List<Subscription> subscriptionList = new List<Subscription>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT Res_type, id, name, creation_dt FROM Subscription";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                Subscription subscription = new Subscription
                                {
                                    Res_type = sqlReader.GetString(0),
                                    id = sqlReader.GetInt32(1),
                                    name = sqlReader.GetString(2),
                                    creation_dt = sqlReader.GetDateTime(3)
                                };
                                subscriptionList.Add(subscription);
                            }
                        }
                    }
                }

                return Ok(subscriptionList);
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine(sqlEx.ToString());
                return BadRequest("Error retrieving subscriptions from the database.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("An unexpected error occurred while processing the request.");
            }
        }

        [HttpGet]
        [Route("api/somiod/subscriptions/{id}")]
        public IHttpActionResult GetSubscriptionById(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT Res_type, id, name, creation_dt FROM Subscription WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            if (sqlReader.Read())
                            {
                                Subscription subscription = new Subscription
                                {
                                    Res_type = sqlReader.GetString(0),
                                    id = sqlReader.GetInt32(1),
                                    name = sqlReader.GetString(2),
                                    creation_dt = sqlReader.GetDateTime(3)
                                };

                                return Ok(subscription);
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine(sqlEx.ToString());
                return BadRequest("Error retrieving subscription from the database.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("An unexpected error occurred while processing the request.");
            }
        }




        [HttpPost]
        [Route("api/somiod/subscriptions")]
        public IHttpActionResult CreateSubscription(Subscription newSubscription)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "INSERT INTO Subscription (Res_type, name, creation_dt) VALUES (@Res_type, @name, @creation_dt)";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@Res_type", newSubscription.Res_type);
                        command.Parameters.AddWithValue("@name", newSubscription.name);
                        command.Parameters.AddWithValue("@creation_dt", newSubscription.creation_dt);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok("Subscription created successfully");
                        }
                        else
                        {
                            return BadRequest("Failed to create subscription");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine(sqlEx.ToString());
                return BadRequest("Error creating subscription in the database.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("An unexpected error occurred while processing the request.");
            }
        }

        [HttpPut]
        [Route("api/somiod/subscriptions/{id}")]
        public IHttpActionResult UpdateSubscription(int id, Subscription updatedSubscription)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "UPDATE Subscription SET Res_type = @Res_type, name = @name, creation_dt = @creation_dt WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@Res_type", updatedSubscription.Res_type);
                        command.Parameters.AddWithValue("@name", updatedSubscription.name);
                        command.Parameters.AddWithValue("@creation_dt", updatedSubscription.creation_dt);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok($"Subscription with ID {id} updated successfully");
                        }
                        else
                        {
                            return BadRequest($"Failed to update subscription with ID {id}");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine(sqlEx.ToString());
                return BadRequest("Error updating subscription in the database.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("An unexpected error occurred while processing the request.");
            }
        }

        [HttpDelete]
        [Route("api/somiod/subscriptions/{id}")]
        public IHttpActionResult DeleteSubscription(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Subscription WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok($"Subscription with ID {id} deleted successfully");
                        }
                        else
                        {
                            return BadRequest($"Failed to delete subscription with ID {id}");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine(sqlEx.ToString());
                return BadRequest("Error deleting subscription from the database.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("An unexpected error occurred while processing the request.");
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