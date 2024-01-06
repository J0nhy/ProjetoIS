using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using uPLibrary.Networking.M2Mqtt;
using RestSharp;
using System.Data.SqlClient;
using Switch.Models;
using System.Reflection;
using uPLibrary.Networking.M2Mqtt.Messages;
using Switch.Models;
using System.Xml.Serialization;

namespace Switch
{
    public partial class FormSwitch : Form
    {

        string baseURI = @"http://localhost:49744/";
        string conn_string = System.Configuration.ConfigurationManager.ConnectionStrings["Switch.Properties.Settings.ConnStr"].ConnectionString.ToString();
        string path = @"C:\DevelopmentIS\";

        RestClient client = null;

        MqttClient mqttClient;

        public FormSwitch()
        {
            InitializeComponent();
            client = new RestClient(baseURI);

        }

        //Ligar/Abrir Válvula
        private void btnOn_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(conn_string);
            string ContainerName = textBoxContainerName.Text;
            string AppName = textBoxAppName.Text;

            SqlDataReader SR = null;
            con.Open();

            string sql = "SELECT Id FROM Container WHERE name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", ContainerName);

            SqlDataReader SRapp = null;
            string sqlApp = "SELECT Id FROM Application WHERE name=@nameApp";
            SqlCommand cmdApp = new SqlCommand(sqlApp, con);
            cmdApp.Parameters.AddWithValue("@nameApp", AppName);

            SqlDataReader SRcontainer = null;
            string sqlContainer = "SELECT Parent FROM Container WHERE name=@nameContainer";
            SqlCommand cmdContainer = new SqlCommand(sqlContainer, con);
            cmdContainer.Parameters.AddWithValue("@nameContainer", ContainerName);

            RestRequest request = new RestRequest("api/somiod/{application}/{container}/data", Method.Post);

            string json = File.ReadAllText(@"" + path + "\\ProjetoIS_D02\\Valvula\\bin\\Debug\\Names.txt");
            json = json.Remove(json.Length - 2);

            if (json == textBoxContainerName.Text)
            {
                SRapp = cmdApp.ExecuteReader();
                if (SRapp.Read())
                {
                    int idApp = (int)SRapp.GetValue(0);
                    SRapp.Close();

                    SRcontainer = cmdContainer.ExecuteReader();
                    if (SRcontainer.Read())
                    {
                        int parentContainer = (int)SRcontainer.GetValue(0);
                        SRcontainer.Close();

                        SR = cmd.ExecuteReader();
                        if (idApp == parentContainer && SR.Read())
                        {
                            int parent = (int)SR.GetValue(0);
                            SR.Close();

                            Data data = new Data
                            {
                                Res_type = "data",
                                Content = "On",
                                Name = "On" + DateTime.Now.ToString(),
                                Creation_dt = DateTime.Now,
                                Parent = parent,
                            };

                            // Convert the Data object to XML
                            string xml;
                            XmlSerializer serializer = new XmlSerializer(typeof(Data));
                            using (StringWriter writer = new StringWriter())
                            {
                                serializer.Serialize(writer, data);
                                xml = writer.ToString();
                            }

                            request.AddParameter("application/xml", xml, ParameterType.RequestBody);
                            request.AddUrlSegment("application", textBoxAppName.Text);
                            request.AddUrlSegment("container", textBoxContainerName.Text);

                            var response = client.Execute(request);
                            MessageBox.Show(response.StatusCode.ToString());
                            con.Close();
                        }
                        else
                        {
                            MessageBox.Show("O id da App e o parent do Container nao coincidem");
                            con.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Erro a ler o container dado");
                        con.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Erro a ler a application dada");
                    con.Close();
                }
            }
            else
            {
                MessageBox.Show("DOESN'T HAVE THE SAME TOPIC AS THE GATE");
                con.Close();
            }
        }
    

    private void panel1_Paint(object sender, PaintEventArgs e)
        {
    
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void postButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(conn_string);

            SqlDataReader SR = null;
            con.Open();
            string sql = "SELECT Id FROM Application WHERE name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", textBoxAppContainer.Text);

            SR = cmd.ExecuteReader();

            if (SR.Read())
            {
                int parent = (int)SR.GetValue(0);
                string name = textBoxCreateContainer.Text;

                // Serialize the Container object to XML
                var container = new Models.Container
                {
                    Res_type = "container",
                    Name = name,
                    Creation_dt = DateTime.Now,
                    Parent = parent
                };

                var serializer = new XmlSerializer(typeof(Models.Container));
                var stringWriter = new StringWriter();
                serializer.Serialize(stringWriter, container);
                string xmlString = stringWriter.ToString();

                RestRequest request = new RestRequest("api/somiod/{application}", Method.Post);

                // Set the request content type to XML
                request.AddHeader("Content-Type", "application/xml");

                // Set the request body to the XML string
                request.AddParameter("application/xml", xmlString, ParameterType.RequestBody);

                request.AddUrlSegment("application", textBoxAppContainer.Text);

                var response = client.Execute(request);
                MessageBox.Show(response.StatusCode.ToString());
            }
            else
            {
                MessageBox.Show("A aplicacao dada não existe!");
                con.Close();
            }
        }

        //Desligar/Fechar Valvula
        private void btnOff_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(conn_string);

            string ContainerName = textBoxContainerName.Text;
            string AppName = textBoxAppName.Text;

            SqlDataReader SR = null;
            con.Open();

            // Selecionar container com o nome dado
            string sql = "SELECT Id FROM Container WHERE name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", ContainerName);

            // Selecionar app com o nome dado
            SqlDataReader SRapp = null;
            string sqlApp = "SELECT Id FROM Application WHERE Name=@nameApp";
            SqlCommand cmdApp = new SqlCommand(sqlApp, con);
            cmdApp.Parameters.AddWithValue("@nameApp", AppName);

            SqlDataReader SRcontainer = null;

            // Selecionar parent do container
            string sqlModule = "SELECT Parent FROM Container WHERE name=@nameContainer";
            SqlCommand cmdContainer = new SqlCommand(sqlModule, con);
            cmdContainer.Parameters.AddWithValue("@nameContainer", ContainerName);

            RestRequest request = new RestRequest("api/somiod/{application}/{container}/data", Method.Post);

            // Caminho com nomes
            string json = File.ReadAllText(@"" + path + "\\ProjetoIS_D02\\Valvula\\bin\\Debug\\Names.txt");
            json = json.Remove(json.Length - 2);

            if (json == textBoxContainerName.Text)
            {
                SRapp = cmdApp.ExecuteReader();
                if (SRapp.Read())
                {
                    int idApp = (int)SRapp.GetValue(0);
                    SRapp.Close();

                    SRcontainer = cmdContainer.ExecuteReader();
                    if (SRcontainer.Read())
                    {
                        int parentContainer = (int)SRcontainer.GetValue(0);
                        SRcontainer.Close();

                        SR = cmd.ExecuteReader();
                        if (idApp == parentContainer && SR.Read())
                        {
                            int parent = (int)SR.GetValue(0);
                            SR.Close();

                            Data data = new Data
                            {
                                Res_type = "data",
                                Content = "Off",
                                Name = "Off" + DateTime.Now.ToString(),
                                Creation_dt = DateTime.Now,
                                Parent = parent,
                            };

                            // Convert the Data object to XML
                            string xml;
                            XmlSerializer serializer = new XmlSerializer(typeof(Data));
                            using (StringWriter writer = new StringWriter())
                            {
                                serializer.Serialize(writer, data);
                                xml = writer.ToString();
                            }

                            request.AddParameter("application/xml", xml, ParameterType.RequestBody);
                            request.AddUrlSegment("application", textBoxAppName.Text);
                            request.AddUrlSegment("container", textBoxContainerName.Text);

                            var response = client.Execute(request);
                            MessageBox.Show(response.StatusCode.ToString());
                            con.Close();
                        }
                        else
                        {
                            MessageBox.Show("THE ID OF THE APP DOESN'T MATCH THE CONTAINER PARENT");
                        }
                    }
                    else
                    {
                        MessageBox.Show("CONTAINER DOESN'T EXIST");
                    }
                }
                else
                {
                    MessageBox.Show("APPLICATION DOESN'T EXIST");
                }
            }
            else
            {
                MessageBox.Show("DOESN'T HAVE THE SAME TOPIC AS THE GATE");
            }

            con.Close();
        }


        private void Comando_Load(object sender, EventArgs e)
        {
            mqttClient = new MqttClient("127.0.0.1");
            mqttClient.MqttMsgPublishReceived += mqttClient_MqttMsgPublishReceivedStatus;
            mqttClient.Subscribe(new string[] { "StatusReceived" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            mqttClient.Connect("Status1");
        }
        private void mqttClient_MqttMsgPublishReceivedStatus(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
        }
    }


}
