using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Valvula.Properties;
using RestSharp;
using Aplicacao = Valvula.Models.Aplicacao;
using Container = Valvula.Models.Container;
using Subscricao = Valvula.Models.Subscricao;
using Dados = Valvula.Models.Dados;
using System.IO;
using System.Net;
using Image = System.Drawing.Image;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using static System.Net.Mime.MediaTypeNames;
using System.Web.UI.WebControls;
using AxWMPLib;
using System.Web.Security;
using System.Xml.Serialization;

namespace Valvula
{
    public partial class Form1 : Form
    {
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Stop any currently playing video
            axWindowsMediaPlayer.Ctlcontrols.stop();

            // Play the video for "Off"
            axWindowsMediaPlayer.URL = videoOff;
            axWindowsMediaPlayer.Ctlcontrols.play();
            label4.Text = "Off";
        }

        string conn_string = Settings.Default.conn_str;

        string baseURI = @"http://localhost:49744/"; 

        RestClient client = null;

        MqttClient mClient;

        string ValvPath = Environment.CurrentDirectory;
        string imgOn, imgOff, videoOn, videoOff;

        public Form1()
        {
            InitializeComponent();
            client = new RestClient(baseURI);

            // get images and videos path from resources
            imgOn = ValvPath + @"\Resources\ValveOn.png";
            imgOff = ValvPath + @"\Resources\ValveOff.png";
            videoOn = ValvPath + @"\Resources\VideoOn.mp3";
            videoOff = ValvPath + @"\Resources\VideoOff.mp3";


            // Inicialize o controle AxWindowsMediaPlayer
            axWindowsMediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            axWindowsMediaPlayer.Dock = DockStyle.Fill;

            // Adicione AxWindowsMediaPlayer ao formulário
            Controls.Add(axWindowsMediaPlayer);

        }

        private void button2_Click(object sender, EventArgs e) //post app
        {

            string name = txtNome.Text;

            RestRequest request = new RestRequest("api/somiod/", Method.Post);

            Aplicacao app = new Aplicacao
            {
                Res_type = "Aplicacao",
                Name = name,
                Creation_dt = DateTime.Now
            };

            request.AddBody(app);

            var response = client.Execute(request);

            MessageBox.Show("Done: " + response.StatusCode.ToString());

        }

        private void button3_Click(object sender, EventArgs e)//post container
        {

            SqlConnection conn = new SqlConnection(conn_string);

            SqlDataReader reader = null;
            conn.Open();

            string sql = "select Id from Application where Name=@name";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", txtNome.Text);

            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int parent = (int)reader.GetValue(0);
                string name = txtNovoNomeContainer.Text;

                // Serialize the Container object to XML
                var container = new Container
                {
                    Res_type = "container",
                    name = name,
                    creation_dt = DateTime.Now,
                    parent = parent
                };

                var serializer = new XmlSerializer(typeof(Container));
                var stringWriter = new StringWriter();
                serializer.Serialize(stringWriter, container);
                string xmlString = stringWriter.ToString();

                RestRequest request = new RestRequest("api/somiod/{application}", Method.Post);

                // Set the request content type to XML
                request.AddHeader("Content-Type", "application/xml");

                // Set the request body to the XML string
                request.AddParameter("application/xml", xmlString, ParameterType.RequestBody);

                request.AddUrlSegment("application", txtNome.Text);

                var response = client.Execute(request);
                MessageBox.Show(response.StatusCode.ToString());
            }
            else
            {
                MessageBox.Show("Erro no reader");
                conn.Close();
            }


        }

        private void button4_Click(object sender, EventArgs e)//post Subscription  
        {
            SqlConnection conn = new SqlConnection(conn_string);

            string NomeApp = txtNome.Text;
            string NomeContainer = txtNomeContainer.Text;
            string NomeSub = txtNomeSub.Text;
            string Event = txtEvent.Text;
            string Endpoint = txtEndpoint.Text;

            RestRequest request = new RestRequest("api/somiod/subscriptions", Method.Post);


            SqlDataReader reader = null;
            SqlDataReader readercontainer = null;
            SqlDataReader readerapp = null;
            conn.Open();

            string sql = "select Id from Container where name=@nameCont";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nameCont", NomeContainer);

            string sqlApp = "select Id from Application where name=@nameApp";
            SqlCommand cmdApp = new SqlCommand(sqlApp, conn);
            cmdApp.Parameters.AddWithValue("@nameApp", NomeApp);

            string sqlContainer = "SELECT Parent FROM Container WHERE name=@nameCont";
            SqlCommand cmdContainer = new SqlCommand(sqlContainer, conn);
            cmdContainer.Parameters.AddWithValue("@nameCont", NomeContainer);


            readerapp = cmdApp.ExecuteReader();
            if (readerapp.Read())
            {
                int idApp = (int)readerapp.GetValue(0);
                readerapp.Close();

                readercontainer = cmdContainer.ExecuteReader();
                if (readercontainer.Read())
                {
                    int parentCont = (int)readercontainer.GetValue(0);
                    readercontainer.Close();

                    reader = cmd.ExecuteReader();
                    if (idApp == parentCont && reader.Read())
                    {

                        int parent = (int)reader.GetValue(0);
                        reader.Close();
                        Subscricao subscricao = new Subscricao
                        {
                            Res_type = "subscription",
                            Name = NomeSub,
                            Creation_dt = DateTime.Now,
                            Parent = parent,
                            Event = Event,
                            Endpoint = Endpoint
                        };


                        request.AddBody(subscricao);
                        request.AddUrlSegment("Aplicacao", txtNome.Text);
                        request.AddUrlSegment("container", txtNomeContainer.Text);


                        var response = client.Execute(request);
                        MessageBox.Show("Done: " + response.StatusCode.ToString());
                        conn.Close();

                    }
                    else
                    {
                        MessageBox.Show("Error: ID não encontrado");
                        conn.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Error: Container não existe");
                    conn.Close();
                }

            }
            else
            {
                MessageBox.Show("Error: Aplicacao não existente");
                conn.Close();
            }




        }

        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                listBox1.Items.Add(Encoding.UTF8.GetString(e.Message));
                string valor = null;
                if (listBox1.Items.Count > 0)
                {
                    valor = listBox1.Items[listBox1.Items.Count - 1].ToString();
                }

                // Stop any currently playing video
                    axWindowsMediaPlayer.Ctlcontrols.stop();

                if (valor == "On")
                {

                    Image newImage = Image.FromFile(imgOn);
                    pictureBox1.Image = newImage;

                    // Play the video for "On"
                    axWindowsMediaPlayer.URL = videoOn;
                    axWindowsMediaPlayer.Ctlcontrols.play();
                    label4.Text = "On";

                }
                else if (valor == "Off")
                {

                    Image newImage = Image.FromFile(imgOff);
                    pictureBox1.Image = newImage;

                    // Play the video for "Off"
                    axWindowsMediaPlayer.URL = videoOff;
                    axWindowsMediaPlayer.Ctlcontrols.play();
                    label4.Text = "Off";

                }
            });
        }
        void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            MessageBox.Show("Subscrição efetuada com sucesso");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] str_container = { txtNovoNomeContainer.Text };
            SqlConnection conn = new SqlConnection(conn_string);

            conn.Open();
            SqlDataReader readercontainer = null;

            string sqlContainer = "select Parent from Container where name=@nomeContainer";
            SqlCommand cmdContainer = new SqlCommand(sqlContainer, conn);
            cmdContainer.Parameters.AddWithValue("@nomeContainer", txtNovoNomeContainer.Text);

            readercontainer = cmdContainer.ExecuteReader();
            if (readercontainer.Read())
            {

                mClient = new MqttClient(IPAddress.Parse("127.0.0.1"));
                mClient.Connect(Guid.NewGuid().ToString());
                if (!mClient.IsConnected)
                {
                    MessageBox.Show("Error: client nao está ligado.");
                    return;
                }

                mClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };

                // subscrever cliente
                mClient.Subscribe(str_container, qosLevels);
                mClient.MqttMsgSubscribed += client_MqttMsgSubscribed;

                string names = @"" + ValvPath + "\\Names.txt";

                File.WriteAllText(names, String.Empty);
                StreamWriter writer = new StreamWriter(names, true);
                writer.WriteLine(txtNovoNomeContainer.Text);
                writer.Dispose();
            }
            else
            {
                MessageBox.Show("Error: Este container nao existe.");
                conn.Close();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(conn_string);

            string nomeApp = txtNome.Text;
            string novoNomeApp = txtNovoNomeApp.Text;

            SqlDataReader reader = null;
            conn.Open();

            string sql = "select Id from Application where name=@name";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", nomeApp);

            RestRequest request = new RestRequest("api/somiod/applications/{id}", Method.Put);

            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int id = (int)reader.GetValue(0);
                reader.Close();

                Aplicacao updatedApp = new Aplicacao
                {
                    Name = novoNomeApp,
                    // Assuming you have a Creation_dt property in your Application class
                    Creation_dt = DateTime.Now
                };

                request.AddJsonBody(updatedApp);
                request.AddUrlSegment("id", id);

                var response = client.Execute(request);
                MessageBox.Show("Done: " + response.StatusCode.ToString());
                conn.Close();
            }
            else
            {
                MessageBox.Show("Esta aplicação nao foi encontrada ou nao existe");
                conn.Close();
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(conn_string);

            string nome = txtNome.Text;

            SqlDataReader reader = null;
            conn.Open();

            string sql = "select Id from Application where name=@name";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", nome);

            RestRequest request = new RestRequest("api/somiod/applications/{id}", Method.Delete);

            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int id = (int)reader.GetValue(0);
                reader.Close();

                request.AddUrlSegment("id", id);

                var response = client.Execute(request);
                MessageBox.Show("Done: " + response.StatusCode.ToString());
                conn.Close();
            }
            else
            {
                MessageBox.Show("Esta aplicação nao foi encontrada ou nao existe");
                conn.Close();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //listBox1.Items.Add(Encoding.UTF8.GetString(e.Message));

            if (listBox1.Items.Count > 0)
            {
                label4.Text = listBox1.Items[listBox1.Items.Count - 1].ToString();
            }

            // Stop any currently playing video
            axWindowsMediaPlayer.Ctlcontrols.stop();

            if (label4.Text == "Off")
            {

                Image newImage = Image.FromFile(imgOn);
                pictureBox1.Image = newImage;

                // Play the video for "On"
                axWindowsMediaPlayer.URL = videoOn;
                axWindowsMediaPlayer.Ctlcontrols.play();
                label4.Text = "On";

            }
            else if (label4.Text == "On")
            {

                Image newImage = Image.FromFile(imgOff);
                pictureBox1.Image = newImage;

                // Play the video for "Off"
                axWindowsMediaPlayer.URL = videoOff;
                axWindowsMediaPlayer.Ctlcontrols.play();
                label4.Text = "Off";

            }
        }
    }
}
