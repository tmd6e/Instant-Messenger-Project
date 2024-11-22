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

        // This button starts a server
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
                // Retrieve local machine IP address
                IPAddress address = GetLocalIPAddress();
                int port = int.Parse(chatroomPort.Text);
                IPEndPoint ipEndPoint = new IPEndPoint(address, port);  // Listening on port 8000
                // Prepare the server socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(maxPeers); // Allow up to 10 pending connections

                // Lock the server information boxes once the server is established
                if (chatroomInfo.InvokeRequired)
                {
                    chatroomInfo.Invoke(new MethodInvoker(
                        delegate
                        {
                            chatroomIP.Text = ipEndPoint.Address.ToString();
                            chatroomPort.Text = ipEndPoint.Port.ToString();
                            LockUI();
                        }));
                }
                

                // Start the communication thread for any incoming clients
                while (!token.IsCancellationRequested)
                {
                    if (serverSocket.Poll(100, SelectMode.SelectRead))
                    { // Check for incoming connections
                        // Accept incoming peer connection
                        Socket incomingPeerSocket = serverSocket.Accept();
                        peerConnections.Add(incomingPeerSocket); // Add the new peer to the connection list

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

        // Handle communication connected peers
        private void HandlePeerCommunication(Socket peerSocket, CancellationToken token)
        {
            try
            {
                byte[] buffer = new byte[1024];
                byte[] historyBuffer = Encoding.UTF8.GetBytes(chatHistory);
                int bytesRead;

                // If the application is the server, update incoming peers with chat history
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

                        // Take any peer message and update the chat history with it
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        chatHistory += receivedMessage + Environment.NewLine;
                        chatHistory = ManageLines(chatHistory, maxLines);
                        chatText.Text = chatHistory;

                        historyBuffer = Encoding.UTF8.GetBytes(chatHistory);

                        foreach (Socket socket in peerConnections)
                        {
                            if (socket.Connected)
                            {
                                socket.Send(historyBuffer); // Update history for all connected clients
                            }
                        }
                    }
                    else
                    {
                        // Read the message from the peer
                        bytesRead = peerSocket.Receive(buffer);
                        if (bytesRead == 0) // Handle server disconnection
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

                        // Update the chatbox with server chat history
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

        // This button starts a client and connects with a given IP address and port
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

                // Timeout duration in milliseconds
                int timeoutMilliseconds = 5000; // 5 seconds timeout

                // Start the connection attempt on a separate thread
                clientCommunicationThread = new Thread(() => TryConnectWithTimeout(peerIP, peerPort, timeoutMilliseconds));
                clientCommunicationThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to peer: " + ex.Message);
            }
        }

        // Send a message to all connected peers in the chatroom
        private void sendButton_Click(object sender, EventArgs e)
        {
            // Make sure the application is an active client or server
            if (!isServerRunning && !isClient) {
                return;
            }

            // Build the chatbox text to send
            string message = userName.Text + ": " + messageBox.Text;

            // If the application is the server
            if (isServerRunning)
            {
                // Append your message to the chat history
                chatHistory += message + Environment.NewLine;
                // Ensure the chat history does not extend too long
                chatHistory = ManageLines(chatHistory, maxLines);
                // The realtime chat history will be the message sent for clients to update
                message = chatHistory;
                chatText.Text = message;
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

        // Clean up resources when the app closes
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        // Disconnects and cleans resources
        private void Disconnect() {
            // Clear the local chat history and empty the text box
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

            // Stop any threads and sockets running to deallocate resources

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

        // Uses the cancellation token from connection to close the server thread
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

        // Uses the cancellation token from connection to close the client thread
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

        // This retrieves the IP address from the local client 
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
        // This retrieves the internet IP address (unused for now)
        public static IPAddress GetPublicIPAddress()
        {
            // This code retrieves the internet IP address and returns it as an IPAddress object 
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string publicIpString = client.GetStringAsync("https://api.ipify.org").GetAwaiter().GetResult();

                    IPAddress publicIp = IPAddress.Parse(publicIpString);
                    return publicIp;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving public IP: " + ex.Message);
                    return null;
                }
            }
        }

        // Maintains the chatroom length to prevent excessive scrolling
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

        // Disconnects the server/client
        private void button3_Click(object sender, EventArgs e)
        {
            ResetUI();
            Disconnect();
        }

        // Restores the UI's ability to take inputs
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
        // Forces the UI to become read-only
        private void LockUI() {
            chatroomIP.ReadOnly = true;
            chatroomIP.BackColor = Control.DefaultBackColor;
            chatroomIP.BorderStyle = BorderStyle.None;
            chatroomPort.ReadOnly = true;
            chatroomPort.BackColor = Control.DefaultBackColor;
            chatroomPort.BorderStyle = BorderStyle.None;
            userName.ReadOnly = true;
            userName.BackColor = Control.DefaultBackColor;
            userName.BorderStyle = BorderStyle.None;
        }
        // Attempts connection given an IP and port
        private void TryConnectWithTimeout(string peerIP, int peerPort, int timeoutMilliseconds)
        {
            bool connected = false;
            DateTime startTime = DateTime.Now;

            try
            {
                // Start the connection attempt
                Thread connectionThread = new Thread(() =>
                {
                    try
                    {
                        peerClientSocket.Connect(new IPEndPoint(IPAddress.Parse(peerIP), peerPort));
                        connected = true; // Successfully connected
                    }
                    catch (Exception ex)
                    {
                        // Handle connection failure
                        Invoke(new Action(() => MessageBox.Show("A connection error has occurred.")));
                    }
                });

                // Start the connection thread
                connectionThread.Start();

                // Check for timeout
                while (connectionThread.IsAlive)
                {
                    // Wait until the connection attempt is complete or the timeout is reached
                    if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                    {
                        // If the timeout is reached, abort the attempt
                        if (peerClientSocket.Connected)
                        {
                            peerClientSocket.Close();
                        }
                        connectionThread.Abort(); // Abort the connection thread and display the error message
                        Invoke(new Action(() => MessageBox.Show("Connection attempt timed out.")));
                        return;
                    }

                    Thread.Sleep(50); // Sleep for a short period to prevent busy-waiting
                }

                // If connected successfully, handle the successful connection
                if (connected)
                {
                    Invoke(new Action(() =>
                    {
                        // Register the peer connection
                        peerConnections.Add(peerClientSocket);

                        LockUI();

                        isClient = true;

                        // Prepare token for disconnection
                        cancellationTokenSource = new CancellationTokenSource();
                        CancellationToken token = cancellationTokenSource.Token;

                        // Start the client thread
                        clientCommunicationThread = new Thread(() => HandlePeerCommunication(peerClientSocket, token));
                        clientCommunicationThread.Start();
                        MessageBox.Show("Connected successfully!");
                    }));
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Invoke(new Action(() => MessageBox.Show("Error during connection attempt: " + ex.Message)));
            }
        }
    }
}
