using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Valvula
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        string conn_string = System.Configuration.ConfigurationManager.ConnectionStrings["connection_string"].ConnectionString.ToString();



        string baseURI = @"http://localhost:55398/"; //TODO: needs to be updated!

        RestClient client = null;

        MqttClient mClient;

        string PortaoPath = Environment.CurrentDirectory;



        public Portao()
        {
            InitializeComponent();
            client = new RestClient(baseURI);
        }



        private void button2_Click(object sender, EventArgs e) //post app
        {

            string name = textBoxAppName.Text;

            RestRequest request = new RestRequest("api/somiod/", Method.Post);

            Application app = new Application
            {
                Res_type = "application",
                Name = name,
                Creation_dt = DateTime.Now.ToString("hh:mm:ss tt")
            };

            request.AddBody(app);

            var response = client.Execute(request);
            MessageBox.Show(response.StatusCode.ToString());

        }

        private void button3_Click(object sender, EventArgs e)//post module
        {

            //SqlConnection con = new SqlConnection(connectionString.ConnectionString);

            SqlConnection con = new SqlConnection(conn_string);

            SqlDataReader SR = null;
            con.Open();
            string sql = "SELECT Id FROM Application WHERE Name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", textBoxAppName.Text);

            SR = cmd.ExecuteReader();

            if (SR.Read())
            {
                int parent = (int)SR.GetValue(0);
                string name = textBoxModuleName.Text;
                RestRequest request = new RestRequest("api/somiod/{application}", Method.Post);

                Module module = new Module
                {
                    Res_type = "module",
                    Name = name,
                    Creation_dt = DateTime.Now.ToString("hh:mm:ss tt"),
                    Parent = parent
                };

                request.AddBody(module);
                request.AddUrlSegment("application", textBoxAppName.Text);

                var response = client.Execute(request);
                MessageBox.Show(response.StatusCode.ToString());

            }
            else
            {
                MessageBox.Show("THE APPLICATION DOESNT EXIST");
                con.Close();
            }


        }

        private void button4_Click(object sender, EventArgs e)//post Subscription  
        {
            SqlConnection con = new SqlConnection(conn_string);

            string AppName = textBoxAppName.Text;
            string ModuleName = textBoxModuleName.Text;
            string name = textBoxSubName.Text;
            string subEvent = textBoxSubEvent.Text;
            string endpoint = textBoxSubEndpoint.Text;

            RestRequest request = new RestRequest("api/somiod/{application}/{module}", Method.Post);


            SqlDataReader SR = null;
            con.Open();
            string sql = "SELECT Id FROM Module WHERE name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", ModuleName);

            SqlDataReader SRapp = null;
            //con.Open();
            string sqlApp = "SELECT Id FROM Application WHERE Name=@nameApp";
            SqlCommand cmdApp = new SqlCommand(sqlApp, con);
            cmdApp.Parameters.AddWithValue("@nameApp", AppName);

            SqlDataReader SRmodule = null;
            //con.Open();
            string sqlModule = "SELECT Parent FROM Module WHERE name=@nameModule";
            SqlCommand cmdModule = new SqlCommand(sqlModule, con);
            cmdModule.Parameters.AddWithValue("@nameModule", ModuleName);


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
                        Subscription subscription = new Subscription
                        {
                            Res_type = "subscription",
                            Name = name,
                            Creation_dt = DateTime.Now.ToString("hh:mm:ss tt"),
                            Parent = parent,
                            Event = subEvent,
                            Endpoint = endpoint
                        };


                        request.AddBody(subscription);
                        request.AddUrlSegment("application", textBoxAppName.Text);
                        request.AddUrlSegment("module", textBoxModuleName.Text);


                        var response = client.Execute(request);
                        MessageBox.Show(response.StatusCode.ToString());
                        con.Close();

                    }
                    else
                    {
                        MessageBox.Show("THE ID OF THE APP DOESNT MATCH THE MODULE PARENT");
                        con.Close();
                    }
                }
                else
                {
                    MessageBox.Show("MODULE DOESNT EXIST");
                    con.Close();
                }

            }
            else
            {
                MessageBox.Show("APPLICATION DOESNT EXIT");
                con.Close();
            }




        }

        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                listBox1.Items.Add(Encoding.UTF8.GetString(e.Message));

                string openImage = @"" + PortaoPath + "\\garage_open.png";
                string closeImage = @"" + PortaoPath + "\\garage_close.png";

                if (listBox1.Items.Count > 0)
                {
                    label4.Text = listBox1.Items[listBox1.Items.Count - 1].ToString();
                }

                if (label4.Text == "On")
                {

                    Image newImage = Image.FromFile(openImage);
                    pictureBox1.Image = newImage;

                }
                else if (label4.Text == "Off")
                {

                    Image newImage = Image.FromFile(closeImage);
                    pictureBox1.Image = newImage;

                }
            });
        }
        void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            MessageBox.Show("SUBSCRIBED WITH SUCCESS");
        }

        private void button1_Click_1(object sender, EventArgs e) //SUBCRIBE CHANNEL
        {
            string[] mStrTopicsInfo = { textBox1.Text };
            SqlConnection con = new SqlConnection(conn_string);

            con.Open();

            SqlDataReader SRmodule = null;
            string sqlModule = "SELECT Parent FROM Module WHERE name=@nameModule";
            SqlCommand cmdModule = new SqlCommand(sqlModule, con);
            cmdModule.Parameters.AddWithValue("@nameModule", textBox1.Text);

            SRmodule = cmdModule.ExecuteReader();
            if (SRmodule.Read())
            {

                mClient = new MqttClient(IPAddress.Parse("127.0.0.1"));
                mClient.Connect(Guid.NewGuid().ToString());
                if (!mClient.IsConnected)
                {
                    MessageBox.Show("CONNECTION FAILED");
                    return;
                }
                //Specify events we are interest on
                //New Msg Arrived
                mClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                //Subscribe to topics
                byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };//QoS
                mClient.Subscribe(mStrTopicsInfo, qosLevels);
                mClient.MqttMsgSubscribed += client_MqttMsgSubscribed;

                string names = @"" + PortaoPath + "\\Names.txt";

                File.WriteAllText(names, String.Empty);
                StreamWriter sw = new StreamWriter(names, true);
                sw.WriteLine(textBox1.Text);
                sw.Dispose();
            }
            else
            {
                MessageBox.Show("THIS MODULE DOESNT EXIST");
                con.Close();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(conn_string);

            string name = textBoxAppName.Text;
            string newName = textBox2.Text;

            SqlDataReader SR = null;
            con.Open();
            string sql = "SELECT Id FROM Application WHERE Name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", name);

            RestRequest request = new RestRequest("api/somiod/{id}", Method.Put);

            SR = cmd.ExecuteReader();
            if (SR.Read())
            {
                int id = (int)SR.GetValue(0);
                SR.Close();
                Application application = new Application
                {
                    Name = newName,
                    Creation_dt = DateTime.Now.ToString("hh:mm:ss tt")
                };

                request.AddBody(application);
                request.AddUrlSegment("id", id);

                var response = client.Execute(request);
                MessageBox.Show(response.StatusCode.ToString());
                con.Close();
            }
            else
            {
                MessageBox.Show("APPLICATION DOESNT EXIT");
                con.Close();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(conn_string);

            string name = textBoxAppName.Text;

            SqlDataReader SR = null;
            con.Open();
            string sql = "SELECT Id FROM Application WHERE Name=@name";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", name);

            RestRequest request = new RestRequest("api/somiod/{id}", Method.Delete);

            SR = cmd.ExecuteReader();
            if (SR.Read())
            {
                int id = (int)SR.GetValue(0);
                SR.Close();

                request.AddUrlSegment("id", id);

                var response = client.Execute(request);
                MessageBox.Show(response.StatusCode.ToString());
                con.Close();
            }
            else
            {
                MessageBox.Show("APPLICATION DOESNT EXIT");
                con.Close();
            }
        }
    }
}
}
