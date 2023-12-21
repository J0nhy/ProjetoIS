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

namespace Switch
{
    public partial class FormSwitch : Form
    {

        string baseURI = @"http://localhost:55398/";
        string conn_string = System.Configuration.ConfigurationManager.ConnectionStrings["connection_string"].ConnectionString.ToString();
        string ProjectPath = @"C:\DevelopmentIS\";

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

            RestRequest request = new RestRequest("api/somiod/{application}/{module}", Method.Post);


            //Caminho com nomes
            string json = File.ReadAllText(@"" + ProjectPath + "\\ProjetoIS_02\\Valvula\\bin\\Debug\\Names.txt");
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
                        MessageBox.Show("ERRO AO LER O CONTAINER");
                        con.Close();
                    }
                }
                else
                {
                    MessageBox.Show("ERRO AO LER A APPLICATION");
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
                RestRequest request = new RestRequest("api/somiod/{application}", Method.Post);

                Models.Container module = new Models.Container
                {
                    Res_type = "module",
                    name = name,
                    creation_dt = DateTime.Now,
                    parent = parent
                };

                request.AddBody(module);
                request.AddUrlSegment("application", textBoxAppContainer.Text);

                var response = client.Execute(request);
                MessageBox.Show(response.StatusCode.ToString());

            }
            else
            {
                MessageBox.Show("A APLICACAO NAO EXISTE");
                con.Close();
            }
        }
    }
}
