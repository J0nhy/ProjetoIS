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
using System.Diagnostics;
using System.CodeDom;
using System.Xml.Linq;

namespace ProjetoIS_D02.Controllers
{
    public class SOMIODController : ApiController
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProjetoIS_D02.Properties.Settings.connStr"].ConnectionString;
        MqttClient mClient;
        string path = @"C:\DevelopmentIS\";

        //teste
        #region CRUD GET GERAL
        // PARA APAGAR
        [HttpGet] // é preciso o somiod discover
        [Route("api/somiod/{application}/")]

        public IHttpActionResult GetContainerSubscriptionDataByApplicationName(string application)
        {
            string somiodDiscoverHeaderValue = null;

            try
            {
                somiodDiscoverHeaderValue = Request.Headers.GetValues("somiod-discover")?.FirstOrDefault();
            }
            catch
            { }

            //quando não há somiod discover
            if (somiodDiscoverHeaderValue == null)
            {
                try
                {
                    List<string> NamesList = new List<string>();

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT id,name,creation_dt from Application";

                        conn.Open();

                        using (SqlCommand command = new SqlCommand(query, conn))
                        {
                            using (SqlDataReader sqlReader = command.ExecuteReader())
                            {
                                while (sqlReader.Read())
                                {
                                    string id = sqlReader.GetString(0);
                                    string name = sqlReader.GetString(1);
                                    string creation_dt = sqlReader.GetString(2);
                                    NamesList.Add(name);
                                }
                            }
                        }
                    }

                    // Serialize the list of names to XML
                    var serializer = new XmlSerializer(typeof(List<string>));
                    var stringWriter = new StringWriter();
                    serializer.Serialize(stringWriter, NamesList);
                    string xmlString = stringWriter.ToString();

                    return Ok(xmlString);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return BadRequest("Error retrieving application names");
                }
            }
            //qd há somiod-discover
            else { 
                try
                {


                    List<string> NamesList = new List<string>();

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = null;
                        if (somiodDiscoverHeaderValue == "container")
                        {
                            query = "SELECT Container.name FROM Container JOIN Application ON Container.parent = Application.id WHERE Application.name = @application";
                        }
                        if (somiodDiscoverHeaderValue == "subscription")
                        {
                            query = "SELECT Subscription.name FROM Subscription JOIN container ON Subscription.parent = container.id JOIN application ON container.parent = application.id WHERE application.name = @application";
                        }
                        if (somiodDiscoverHeaderValue == "data")
                        {
                            query = "SELECT Data.name FROM Data JOIN container ON Data.parent = container.id JOIN application ON container.parent = application.id WHERE application.name = @application";
                        }

                        conn.Open();

                        using (SqlCommand command = new SqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("@application", application);

                            using (SqlDataReader sqlReader = command.ExecuteReader())
                            {
                                while (sqlReader.Read())
                                {
                                    string name = sqlReader.GetString(0);
                                    NamesList.Add(name);
                                }
                            }
                        }
                    }

                    // Serialize the list of names to XML
                    var serializer = new XmlSerializer(typeof(List<string>));
                    var stringWriter = new StringWriter();
                    serializer.Serialize(stringWriter, NamesList);
                    string xmlString = stringWriter.ToString();

                    return Ok(xmlString);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return BadRequest("Error retrieving names");
                }
        }
        }
        #endregion

        //
        //FEITO
        #region CRUD APPLICATION
        //FEITO
        [HttpPost]
        [Route("api/somiod")]
        public IHttpActionResult CreateApplication()
        {
            try
            {
                var xmlContent = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(xmlContent))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new XmlSerializer(typeof(Application));
                using (var xmlReader = XmlReader.Create(new StringReader(xmlContent)))
                {
                    var app = (Application)serializer.Deserialize(xmlReader);

                    SqlConnection conn = null;
                    string queryString = "INSERT INTO application (name, creation_dt) VALUES (@name, GETDATE());";

                    try
                    {
                        conn = new SqlConnection(connectionString);
                        conn.Open();

                        SqlCommand command = new SqlCommand(queryString, conn);
                        command.Parameters.AddWithValue("@name", app.Name);
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

        //FEITO  
        [HttpGet]
        [Route("api/somiod/")]// Precisa do somiod discover
        //RETURN DE TODAS AS APPS
        public IHttpActionResult GetAllApplicationNames()
        {
            try
            {
                string somiodDiscoverHeaderValue = Request.Headers.GetValues("somiod-discover")?.FirstOrDefault();
                if (somiodDiscoverHeaderValue != "application") { 
                    return BadRequest("Somiod Discover needed"); 
                }
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

                // Serialize the list of names to XML
                var serializer = new XmlSerializer(typeof(List<string>));
                var stringWriter = new StringWriter();
                serializer.Serialize(stringWriter, applicationNames);
                string xmlString = stringWriter.ToString();

                return Ok(xmlString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving application names");
            }
        }

        //FEITO
        [HttpPut]
        [Route("api/somiod/{id}")]
        public IHttpActionResult UpdateApplication(int id)
        {
            try
            {
                var xmlString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(xmlString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new XmlSerializer(typeof(Application));
                using (var xmlReader = XmlReader.Create(new StringReader(xmlString)))
                {
                    var updatedApp = (Application)serializer.Deserialize(xmlReader);

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string query = "UPDATE Application SET name = @name WHERE id = @id";

                        using (SqlCommand command = new SqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            command.Parameters.AddWithValue("@name", updatedApp.Name);

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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error updating application");
            }
        }

        //FEITO
        [HttpDelete]
        [Route("api/somiod/{id}")]
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

        #endregion


        #region CRUD CONTAINER

        //FEITO
        [Route("api/somiod/{application}")]
        [HttpPost] //ADD
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
                    command.Parameters.AddWithValue("@name", container.Name);
                    command.Parameters.AddWithValue("@parent", parentApp.Id);
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

        //Feito
        [HttpGet] //search 1
        [Route("api/somiod/{application}/{name}")]
        public IHttpActionResult GetContainerByNames(string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id, name, creation_dt, parent FROM Container WHERE name = @name";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                Container container = new Container
                                {
                                    Id = sqlReader.GetInt32(0),
                                    Name = sqlReader.GetString(1),
                                    Creation_dt = sqlReader.GetDateTime(2),
                                    Parent = sqlReader.GetInt32(3),
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


        //FEITO
        [HttpPut]
        [Route("api/somiod/{application}/{id}")]
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
                        command.Parameters.AddWithValue("@name", updatedContainer.Name);
                        command.Parameters.AddWithValue("@parent", updatedContainer.Parent);

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

        //FEITO
        [HttpDelete]
        [Route("api/somiod/{application}/{id}")]
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

        #endregion


        #region CRUD DATA

        //FEITO
        [Route("api/somiod/{application}/{container}/data")]
        [HttpPost] // add
        public IHttpActionResult PostData()
        {
            try
            {
                //var application = Request.GetRouteData().Values["application"].ToString();
                var container = Request.GetRouteData().Values["container"].ToString();

                var contentType = Request.Content.Headers.ContentType?.MediaType;

                var dataString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(dataString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                Data data;


                // Deserialize XML if content type is XML
                var serializer = new XmlSerializer(typeof(Data));
                data = (Data)serializer.Deserialize(new StringReader(dataString));


                Container parentApp = GetContainerByName(container);

                SqlConnection conn = null;
                string queryString = "INSERT INTO Data (content,name, creation_dt, parent) VALUES (@content,@name, GETDATE(), @parent)";

                try
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    SqlCommand command = new SqlCommand(queryString, conn);
                    command.Parameters.AddWithValue("@content", data.Content);
                    command.Parameters.AddWithValue("@name", data.Name);

                    //command.Parameters.AddWithValue("@content", "pastilhas");
                    command.Parameters.AddWithValue("@parent", parentApp.Id);

                    string names = @"" + path + "\\ProjetoIS_D02\\Valvula\\bin\\Debug\\Names.txt";


                    string lastLine = File.ReadLines(names).LastOrDefault(); // if the file is empty

                    Char lastChar = '\0';
                    if (lastLine != null) lastChar = lastLine.LastOrDefault();

                    Trace.WriteLine(lastLine);

                    int numRegistos = command.ExecuteNonQuery();
                    conn.Close();

                    mClient = new MqttClient(IPAddress.Parse("127.0.0.1"));
                    mClient.Connect(Guid.NewGuid().ToString());

                    mClient.Publish(lastLine, Encoding.UTF8.GetBytes(data.Content));

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

        //FEITO
        [HttpPut] // edit
        [Route("api/somiod/{application}/{container}/data/{name}")]
        public IHttpActionResult UpdateData(string name)
        {
            try
            {
                var xmlString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(xmlString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new XmlSerializer(typeof(Data));

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    TextReader reader = new StringReader(xmlString);

                    var updatedData = (Data)serializer.Deserialize(reader);

                    string query = "UPDATE Data SET Content = @content WHERE name = @name";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        //command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@content", updatedData.Content);

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

        //DONE
        [HttpDelete] //delete
        [Route("api/somiod/{application}/{container}/data/{name}")]
        public IHttpActionResult DeleteData(string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Data WHERE name = @name";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@name", name);

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


        [HttpGet] //get 1
        [Route("api/somiod/{application}/{container}/data/{name}")]
        public IHttpActionResult GetDataByName(string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id,name,content,creation_dt,parent FROM data WHERE name = @name";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                Data data = new Data
                                {
                                    Id = sqlReader.GetInt32(0),
                                    Name = sqlReader.GetString(1),
                                    Content = sqlReader.GetString(2),
                                    Creation_dt = sqlReader.GetDateTime(3),
                                    Parent = sqlReader.GetInt32(4),
                                };
                                return Ok(data);
                            }
                        }
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving data by name");
            }
        }


        #endregion



        //CRUDS SUBSCRIPTION
       #region CRUD subscription

        //feito
        [Route("api/somiod/{application}/{container}/sub")]
        [HttpPost] // add
        public IHttpActionResult addSubscription()
        {
            try
            {
                //var application = Request.GetRouteData().Values["application"].ToString();
                var container = Request.GetRouteData().Values["container"].ToString();

                var contentType = Request.Content.Headers.ContentType?.MediaType;

                var dataString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(dataString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                Subscription subscription;


                // Deserialize XML if content type is XML
                var serializer = new XmlSerializer(typeof(Subscription));
                subscription = (Subscription)serializer.Deserialize(new StringReader(dataString));


                Container parentApp = GetContainerByName(container);

                SqlConnection conn = null;
                string queryString = "INSERT INTO Subscription (name,event,endpoint, creation_dt, parent) VALUES (@name,@event,@endpoint, GETDATE(), @parent)";

                try
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    SqlCommand command = new SqlCommand(queryString, conn);
                    command.Parameters.AddWithValue("@name", subscription.Name);
                    command.Parameters.AddWithValue("@event", subscription.Event);
                    command.Parameters.AddWithValue("@endpoint", subscription.Endpoint);

                    //command.Parameters.AddWithValue("@content", "pastilhas");
                    command.Parameters.AddWithValue("@parent", parentApp.Id);
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

        //DONE
        [HttpPut] // edit
        [Route("api/somiod/{application}/{container}/sub/{name}")]
        public IHttpActionResult UpdateSubscription(string name)
        {
            try
            {
                var xmlString = Request.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(xmlString))
                {
                    return BadRequest("Invalid or empty data received");
                }

                var serializer = new XmlSerializer(typeof(Subscription));

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    TextReader reader = new StringReader(xmlString);

                    var updatedSubscription = (Subscription)serializer.Deserialize(reader);

                    string query = "UPDATE Subscription SET event=@event,endpoint=@endpoint WHERE name = @name";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        //command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@event", updatedSubscription.Event);
                        command.Parameters.AddWithValue("@endpoint", updatedSubscription.Endpoint);

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

        //DONE
        [HttpDelete] //delete
        [Route("api/somiod/{application}/{container}/sub/{name}")]
        public IHttpActionResult DeleteSubscription(string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Subscription WHERE name = @name";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@name", name);

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

        //por fazer
        [HttpGet] //get 1
        [Route("api/somiod/{application}/{container}/sub/{name}")]
        public IHttpActionResult GetSubscriptionByName(string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT id,name,event,endpoint,creation_dt,parent FROM Subscription WHERE name = @name";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        using (SqlDataReader sqlReader = command.ExecuteReader())
                        {
                            while (sqlReader.Read())
                            {
                                Subscription data = new Subscription
                                {
                                    Id = sqlReader.GetInt32(0),
                                    Name = sqlReader.GetString(1),
                                    Event = sqlReader.GetString(2),
                                    Endpoint = sqlReader.GetString(3),

                                    Creation_dt = sqlReader.GetDateTime(4),
                                    Parent = sqlReader.GetInt32(5),
                                };
                                return Ok(data);
                            }
                        }
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return BadRequest("Error retrieving subscription by name");
            }
        }


        #endregion


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
                                Id = sqlReader.GetInt32(0),
                                Name = sqlReader.GetString(1),
                                Creation_dt = sqlReader.GetDateTime(2),
                            };
                            return app;
                        }
                    }
                }
                return null;
            }
        }
        private Container GetContainerByName(string containerName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT id, name, creation_dt FROM Container WHERE name = @name";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@name", containerName);

                    using (SqlDataReader sqlReader = command.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            Container app = new Container
                            {
                                Id = sqlReader.GetInt32(0),
                                Name = sqlReader.GetString(1),
                                Creation_dt = sqlReader.GetDateTime(2),
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