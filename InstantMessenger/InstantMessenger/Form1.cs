using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Http;

namespace InstantMessenger
{
    public partial class Form1 : Form
    {
        // List to store active peer connections
        private List<Socket> peerConnections = new List<Socket>();
        private Socket serverSocket; // The socket for listening to incoming connections
        private Thread serverThread; // Thread to handle server listening
        private Thread clientCommunicationThread;
        private Socket peerClientSocket;
        private bool isServerRunning = false; // To track if server is running
        private bool isClient = false;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken token;
        private int maxLines = 15;
        private int maxPeers = 10;
        // Chat history
        private string chatHistory = "";

        public Form1()
        {
            InitializeComponent();
            //Environment.Exit(0);
        }

        // Start listening for incoming peer connections (server mode)
        private void button1_Click(object sender, EventArgs e)
        {
            if (isServerRunning || isClient) {
                MessageBox.Show("You are already in a chatroom.");
                return;
            }

            if (userName.Text != "") {
                // Create a token
                cancellationTokenSource = new CancellationTokenSource();
                token = cancellationTokenSource.Token;
                // Start the server (listener) on a separate thread
                serverThread = new Thread(() => StartServer(token));
                serverThread.Start();
                //lblStatus.Text = "Listening for incoming connections...";
                isServerRunning = true;
                
            }
            else
            {
                MessageBox.Show("Please enter a name.");
                return;
            }
        }

        // Start the server to listen for peer connections
        private void StartServer(CancellationToken token)
        {
            try
            {

                IPAddress address = GetLocalIPAddress();
                IPEndPoint ipEndPoint = new IPEndPoint(address, 8000);  // Listening on port 8000
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(maxPeers); // Allow up to 10 pending connections

                if (chatroomInfo.InvokeRequired)
                {
                    chatroomInfo.Invoke(new MethodInvoker(
                        delegate
                        {
                            chatroomIP.Text = ipEndPoint.Address.ToString();
                            chatroomPort.Text = ipEndPoint.Port.ToString();
                            chatroomIP.ReadOnly = true;
                            chatroomIP.BackColor = Control.DefaultBackColor;
                            chatroomIP.BorderStyle = BorderStyle.None;
                            chatroomPort.ReadOnly = true;
                            chatroomPort.BackColor = Control.DefaultBackColor;
                            chatroomPort.BorderStyle = BorderStyle.None;
                            userName.ReadOnly = true;
                            userName.BackColor = Control.DefaultBackColor;
                            userName.BorderStyle = BorderStyle.None;
                        }));
                }


                while (!token.IsCancellationRequested)
                {
                    if (serverSocket.Poll(100, SelectMode.SelectRead))
                    { // Check for incoming connections
                        // Accept incoming peer connection
                        Socket incomingPeerSocket = serverSocket.Accept();
                        peerConnections.Add(incomingPeerSocket); // Add the new peer to the connection list

                        //lblStatus.Invoke(new Action(() => lblStatus.Text = "Peer connected"));

                        // Start a new thread to handle communication with the incoming peer
                        Thread peerCommunicationThread = new Thread(() => HandlePeerCommunication(incomingPeerSocket, token));
                        peerCommunicationThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in server: " + ex.Message);
            }
            finally
            { 
                serverSocket.Close();
            }
        }

        // Handle communication with a connected peer
        private void HandlePeerCommunication(Socket peerSocket, CancellationToken token)
        {
            try
            {
                byte[] buffer = new byte[1024];
                byte[] historyBuffer = Encoding.UTF8.GetBytes(chatHistory);
                int bytesRead;

                if (isServerRunning)
                {
                    peerSocket.Send(historyBuffer);

                }

                while (!token.IsCancellationRequested)
                {
                    if (isServerRunning)
                    {
                        // Read the message from the peer
                        bytesRead = peerSocket.Receive(buffer);
                        if (bytesRead == 0)
                            break;

                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        chatHistory += receivedMessage + Environment.NewLine;
                        chatHistory = ManageLines(chatHistory, maxLines);
                        chatText.Text = chatHistory;

                        historyBuffer = Encoding.UTF8.GetBytes(chatHistory);

                        foreach (Socket socket in peerConnections)
                        {
                            if (socket.Connected)
                            {
                                socket.Send(historyBuffer); // Send the message to the peer
                            }
                        }
                    }
                    else
                    {
                        // Read the message from the peer
                        bytesRead = peerSocket.Receive(buffer);
                        if (bytesRead == 0)
                        {
                            MessageBox.Show("Server disconnected.");
                            Invoke(new MethodInvoker(
                                delegate
                                {
                                    chatText.Text = "";
                                    ResetUI();
                                }
                            ));
                            break;
                        }

                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        chatText.Text = receivedMessage;
                    }
                }

                // Once done, remove the peer from the list and close the connection
                peerConnections.Remove(peerSocket);
                peerSocket.Close();

                if (isClient)
                {
                    //MessageBox.Show("I am the client has been disconnected.");
                    isClient = false;
                }
                else {
                    //MessageBox.Show("I have disconnected from a client as the server. Server Status: " + isServerRunning);
                }

                
            }
            catch (Exception ex)
            {
                if (isClient && !isServerRunning)
                {
                    Disconnect();
                }
            }
        }

        // Connect to another peer (client mode)
        private void button2_Click(object sender, EventArgs e)
        {
            if (isServerRunning) {
                MessageBox.Show("You are already in a chatroom.");
                return;
            }

            if (userName.Text == "") {
                MessageBox.Show("Please enter a name.");
                return;
            }

            string peerIP = chatroomIP.Text; // Get the peer's IP address from textbox
            int peerPort;

            if (!int.TryParse(chatroomPort.Text, out peerPort)) {
                MessageBox.Show("Error connecting to peer: Port number was incorrectly provided.");
                return;
            }


            try
            {
                // Create a client socket to connect to another peer
                peerClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                peerClientSocket.Connect(new IPEndPoint(IPAddress.Parse(peerIP), peerPort));
                chatroomIP.Text = peerIP;
                chatroomPort.Text = peerPort.ToString();

                peerConnections.Add(peerClientSocket); // Add this peer to the connection list
                //lblStatus.Text = "Connected to peer.";

                // Start a thread to handle communication with this peer
                chatroomIP.ReadOnly = true;
                chatroomIP.BackColor = Control.DefaultBackColor;
                chatroomIP.BorderStyle = BorderStyle.None;
                chatroomPort.ReadOnly = true;
                chatroomPort.BackColor = Control.DefaultBackColor;
                chatroomPort.BorderStyle = BorderStyle.None;
                userName.ReadOnly = true;
                userName.BackColor = Control.DefaultBackColor;
                userName.BorderStyle = BorderStyle.None;
                isClient = true;

                cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = cancellationTokenSource.Token;
                clientCommunicationThread = new Thread(() => HandlePeerCommunication(peerClientSocket, token));
                clientCommunicationThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to peer: " + ex.Message);
            }
        }

        // Send a message to all connected peers
        private void sendButton_Click(object sender, EventArgs e)
        {
            if (!isServerRunning && !isClient) {
                return;
            }

            string message = userName.Text + ": " + messageBox.Text;
            if (isServerRunning)
            {
                chatHistory += message + Environment.NewLine; // Display sent message
                chatHistory = ManageLines(chatHistory, maxLines);
                message = chatHistory;
                chatText.Text = message;
            }
            else { 
                chatText.AppendText(message + Environment.NewLine);
            }
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            try
            {

                foreach (Socket peerSocket in peerConnections)
                {
                    if (peerSocket.Connected)
                    {
                        peerSocket.Send(buffer); // Send the message to the peer
                    }
                }
                messageBox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while sending message: " + ex.Message);
            }
        }

        // Clean up resources when the form is closed
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect() {
            // Call StopServer to gracefully stop the server before the form closes
            chatHistory = "";
            chatText.Text = chatHistory;
            // Close any remaining connections
            foreach (Socket peerSocket in peerConnections)
            {
                if (peerSocket.Connected)
                {
                    peerSocket.Shutdown(SocketShutdown.Both);
                    peerSocket.Close();
                }
            }

            if (isServerRunning)
            {
                StopServer();
            }
            if (isClient)
            {
                StopClient();
            }

            if (peerClientSocket != null && peerClientSocket.Connected)
            {
                peerClientSocket.Close();
            }

            if (serverSocket != null && serverSocket.Connected)
            {
                serverSocket.Close();
            }
        }

        private void StopServer() {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();  // Signal the cancellation
                Console.WriteLine("Status: " + cancellationTokenSource.IsCancellationRequested);
                Console.WriteLine("Cancel flag updated, closing");
                if (serverThread != null && serverThread.IsAlive)
                {
                    serverThread.Join();  // Wait for the thread to finish
                }
                Console.WriteLine("Thread has finished");
                isServerRunning = false;
            }
        }
        private void StopClient()
        {
            if (cancellationTokenSource != null)
            {
                //MessageBox.Show("Closing client connections");
                cancellationTokenSource.Cancel();  // Signal the cancellation
                Console.WriteLine("Status: " + cancellationTokenSource.IsCancellationRequested);
                Console.WriteLine("Cancel flag updated, closing");
                if (clientCommunicationThread != null && clientCommunicationThread.IsAlive)
                {
                    clientCommunicationThread.Abort();
                }
                Console.WriteLine("Thread has finished");
                isClient = false;
            }
        }

        public static IPAddress GetLocalIPAddress()
        {
            // Get all IP addresses associated with the local machine
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                // Select the first IPv4 address (not IPv6)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip;  // Return the local IPv4 address as an IPAddress variable
                }
            }

            return null;  // If no IPv4 address found
        }

        public static string ManageLines(string text, int maxLines)
        {
            // Split the text into lines
            var lines = new List<string>(text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

            // Check if the number of lines exceeds the maxLines limit
            if (lines.Count > maxLines)
            {
                // Remove the oldest line (first line in the list)
                lines.RemoveAt(0);
            }

            // Rebuild the text from the remaining lines
            return string.Join(Environment.NewLine, lines);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ResetUI();
            Disconnect();
        }
        private void ResetUI() {
            chatroomIP.Text = "";
            chatroomIP.ReadOnly = false;
            chatroomIP.BackColor = TextBox.DefaultBackColor;
            chatroomIP.BorderStyle = BorderStyle.Fixed3D;
            chatroomPort.Text = "";
            chatroomPort.ReadOnly = false;
            chatroomPort.BackColor = TextBox.DefaultBackColor;
            chatroomPort.BorderStyle = BorderStyle.Fixed3D;
            userName.Text = "";
            userName.ReadOnly = false;
            userName.BackColor = TextBox.DefaultBackColor;
            userName.BorderStyle = BorderStyle.Fixed3D;
        }
    }
}
