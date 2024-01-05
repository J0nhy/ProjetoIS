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
            //Definir Conexão à BD
            SqlConnection con = new SqlConnection(conn_string);

            string ContainerName = textBoxContainerName.Text;
            string AppName = textBoxAppName.Text;

            SqlDataReader SR = null;
            con.Open();

            //Selecionar id do container com o nome a ser usado
            string sql = "SELECT Id FROM Container WHERE name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", ContainerName);

            //Selecionar id da App com o nome a ser usada
            SqlDataReader SRapp = null;
            string sqlApp = "SELECT Id FROM Application WHERE name=@nameApp";
            SqlCommand cmdApp = new SqlCommand(sqlApp, con);
            cmdApp.Parameters.AddWithValue("@nameApp", AppName);


            //Selecionar Parent do container com o nome a ser usado
            SqlDataReader SRcontainer = null;
            //con.Open();
            string sqlContainer = "SELECT Parent FROM Container WHERE name=@nameContainer";
            SqlCommand cmdContainer = new SqlCommand(sqlContainer, con);
            cmdContainer.Parameters.AddWithValue("@nameContainer", ContainerName);

            RestRequest request = new RestRequest("api/somiod/data", Method.Post);


            //Caminho com nomes
            string json = File.ReadAllText(@"" + path + "\\ProjetoIS_D02\\Valvula\\bin\\Debug\\Names.txt");
            json = json.Remove(json.Length - 2);

            //verificar se o nome da App do ficheiro é igual ao inserido
            if (json == textBoxContainerName.Text)
            {
                //Leitura da App
                SRapp = cmdApp.ExecuteReader();
                if (SRapp.Read())
                {
                    int idApp = (int)SRapp.GetValue(0);
                    SRapp.Close();

                    //Leitura do Container
                    SRcontainer = cmdContainer.ExecuteReader();
                    if (SRcontainer.Read())
                    {
                        int parentContainer = (int)SRcontainer.GetValue(0);
                        SRcontainer.Close();

                        //Comparar id da app com o id do parent do Container
                        SR = cmd.ExecuteReader();
                        if (idApp == parentContainer && SR.Read())
                        {
                            int parent = (int)SR.GetValue(0);
                            SR.Close();

                            Data data = new Data
                            {
                                Res_type = "data",
                                content = "On",
                                creation_dt = DateTime.Now,
                                parent = parent,

                            };

                            request.AddBody(data);
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
                    name = name,
                    creation_dt = DateTime.Now,
                    parent = parent
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

            string ContainerName = textBoxAppContainer.Text;
            string AppName = textBoxAppName.Text;


            SqlDataReader SR = null;
            con.Open();

            //Selecionar container com o nome dado
            string sql = "SELECT Id FROM Container WHERE name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", ContainerName);

            //Selecionar app com o nome dado
            SqlDataReader SRapp = null;
            string sqlApp = "SELECT Id FROM Application WHERE Name=@nameApp";
            SqlCommand cmdApp = new SqlCommand(sqlApp, con);
            cmdApp.Parameters.AddWithValue("@nameApp", AppName);

            SqlDataReader SRcontainer = null;
            
            //Selecionar parent do container
            string sqlModule = "SELECT Parent FROM Container WHERE name=@nameContainer";
            SqlCommand cmdModule = new SqlCommand(sqlModule, con);
            cmdModule.Parameters.AddWithValue("@nameContainer", ContainerName);

            RestRequest request = new RestRequest("api/somiod/{application}/{container}", Method.Post);

            string json = File.ReadAllText(@"" + path + "\\SOMIOD\\ComandoREST\\PortaoInterface\\bin\\Debug\\Names.txt");
            json = json.Remove(json.Length - 2);

            if (json == textBoxContainerName.Text)
            {
                SRapp = cmdApp.ExecuteReader();
                if (SRapp.Read())
                {
                    int idApp = (int)SRapp.GetValue(0);
                    SRapp.Close();


                    SRcontainer = cmdModule.ExecuteReader();
                    if (SRcontainer.Read())
                    {
                        int parentModule = (int)SRcontainer.GetValue(0);
                        SRcontainer.Close();

                        SR = cmd.ExecuteReader();
                        if (idApp == parentModule && SR.Read())
                        {
                            int parent = (int)SR.GetValue(0);
                            SR.Close();
                            Data data = new Data
                            {
                                Res_type = "data",
                                creation_dt = DateTime.Now,
                                parent = parent,

                            };

                            request.AddBody(data);
                            request.AddUrlSegment("application", textBoxAppName.Text);
                            request.AddUrlSegment("container", textBoxContainerName.Text);


                            var response = client.Execute(request);
                            MessageBox.Show(response.StatusCode.ToString());
                            con.Close();


                        }
                        else
                        {
                            MessageBox.Show("THE ID OF THE APP DOESNT MATCH THE CONTAINER PARENT");
                            con.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("CONTAINER DOESNT EXIST");
                        con.Close();
                    }
                }
                else
                {
                    MessageBox.Show("APPLICATION DOESNT EXIST");
                    con.Close();
                }
            }
            else
            {
                MessageBox.Show("DOESN'T HAVE THE SAME TOPIC AS THE GATE");
                con.Close();
            }
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
