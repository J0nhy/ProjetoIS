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

namespace Switch
{
    public partial class Form1 : Form
    {

        string baseURI = @"http://localhost:55398/";
        string conn_string = System.Configuration.ConfigurationManager.ConnectionStrings["connection_string"].ConnectionString.ToString();
        string ProjectPath = @"C:\DevelopmentIS\";

        RestClient client = null;

        MqttClient mqttClient;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOn_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(conn_string);

            string ModuleName = textBoxModuleName.Text;
            string AppName = textBoxAppName.Text;


            SqlDataReader SR = null;
            con.Open();
            string sql = "SELECT Id FROM Module WHERE name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", ModuleName);

            SqlDataReader SRapp = null;
            string sqlApp = "SELECT Id FROM Application WHERE Name=@nameApp";
            SqlCommand cmdApp = new SqlCommand(sqlApp, con);
            cmdApp.Parameters.AddWithValue("@nameApp", AppName);

            SqlDataReader SRmodule = null;
            //con.Open();
            string sqlModule = "SELECT Parent FROM Module WHERE name=@nameModule";
            SqlCommand cmdModule = new SqlCommand(sqlModule, con);
            cmdModule.Parameters.AddWithValue("@nameModule", ModuleName);

            RestRequest request = new RestRequest("api/somiod/{application}/{module}", Method.Post);

            string json = File.ReadAllText(@"" + ProjectPath + "\\SOMIOD\\ComandoREST\\PortaoInterface\\bin\\Debug\\Names.txt");
            json = json.Remove(json.Length - 2);

            if (json == textBoxModuleName.Text)
            {
                SRapp = cmdApp.ExecuteReader();
                if (SRapp.Read())
                {
                    int idApp = (int)SRapp.GetValue(0);
                    SRapp.Close();


                    SRmodule = cmdModule.ExecuteReader();
                    if (SRmodule.Read())
                    {
                        int parentModule = (int)SRmodule.GetValue(0);
                        SRmodule.Close();

                        SR = cmd.ExecuteReader();
                        if (idApp == parentModule && SR.Read())
                        {
                            int parent = (int)SR.GetValue(0);
                            SR.Close();
                            Data data = new Data
                            {
                                Res_type = "data",
                                content = "On",
                                creation_dt = DateTime.Now.ToString("hh:mm:ss tt"),
                                parent = parent,

                            };

                            request.AddBody(data);
                            request.AddUrlSegment("application", textBoxAppName.Text);
                            request.AddUrlSegment("module", textBoxModuleName.Text);


                            var response = client.Execute(request);
                            MessageBox.Show(response.StatusCode.ToString());
                            con.Close();

                        }
                        else
                        {
                            MessageBox.Show("O id da App e o parent do Module nao coincidem");
                            con.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("ERRO AO LER O MODULE");
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
    }
}
