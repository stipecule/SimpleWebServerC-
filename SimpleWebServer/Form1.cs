using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleWebServer
{
    public partial class Form1 : Form
    {
        private Socket httpServer;
        private int serverPort = 80;
        private Thread thread;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            stopServerBtn.Enabled = false;
        }

        private void startServerBtn_Click(object sender, EventArgs e)
        {
            serverLogsText.Text = "";
            try
            {
                httpServer = new Socket(SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    serverPort = int.Parse(serverPortText.Text);
                    if(serverPort >65535 || serverPort <= 0)
                    {
                        throw new Exception("Port not within a valid range!");
                    }
                }
                catch (Exception ex)
                {

                    serverPort = 80;
                    serverLogsText.Text = "Server starting failed on specified port...\n";
                }
                thread = new Thread(new ThreadStart(this.connectionThreadMethod));
                thread.Start();
                startServerBtn.Enabled = false;
                stopServerBtn.Enabled = true;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error while starting the server");
                serverLogsText.Text = "Server starting failed...";
            }
            serverLogsText.Text = "Server started...\n";

        }

        private void stopServerBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //close the socket
                httpServer.Close();
                //Kill the thread
                thread.Abort();
                startServerBtn.Enabled = true;
                stopServerBtn.Enabled = false;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Stopping failed...");
            }
        }
        private void connectionThreadMethod()
        {
            try
            {
                //ipendpoint on which server is listening:
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, serverPort);
                httpServer.Bind(endpoint);
                httpServer.Listen(1);
                startListeningForConnection();
            }
            catch (Exception ex)
            {

                Console.WriteLine("It could not start");
            }
        }
        private void startListeningForConnection()
        {
            while (true)
            {
                DateTime time = DateTime.Now;
                String data = "";
                byte[] bytes = new byte[2048];
                Socket client = httpServer.Accept(); //blocking statement
                //reading inbound connection data
                while (true)
                {
                    int numBytes = client.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, numBytes);
                    if (data.IndexOf("\r\n") > -1) break;
                }
                //data read we specify delegate caus its on ui thread...
                serverLogsText.Invoke((MethodInvoker)delegate
                {
                    serverLogsText.Text += "\r\n\r\n";
                    serverLogsText.Text += data;
                    serverLogsText.Text += "\n\n---End of request---\n";
                });
                //send the response..
                String resHeader = "HTTP/1.1 200 Everything is fine\n Server:Stipin server\n Content-Type: text/html;charset:UTF-8\n\n";
                String resBody = "<!DOCTYPE html><html><title>My server</title><head></head><body><h2>Time:"+time.ToString()+"</h2></body></html>";
                String resStr = resHeader + resBody;
                byte[] resData = Encoding.ASCII.GetBytes(resStr);
                client.SendTo(resData, client.RemoteEndPoint);
                client.Close();
            }
        }

       
    }
}
