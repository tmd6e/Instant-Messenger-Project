namespace InstantMessenger
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.messageBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.chatroomInfo = new System.Windows.Forms.Label();
            this.chatText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chatroomIP = new System.Windows.Forms.TextBox();
            this.userName = new System.Windows.Forms.TextBox();
            this.chatroomPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start Server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(93, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Start Client";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // messageBox
            // 
            this.messageBox.Location = new System.Drawing.Point(12, 255);
            this.messageBox.Multiline = true;
            this.messageBox.Name = "messageBox";
            this.messageBox.Size = new System.Drawing.Size(262, 41);
            this.messageBox.TabIndex = 3;
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(280, 264);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.TabIndex = 4;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // chatroomInfo
            // 
            this.chatroomInfo.AutoSize = true;
            this.chatroomInfo.Location = new System.Drawing.Point(380, 41);
            this.chatroomInfo.Name = "chatroomInfo";
            this.chatroomInfo.Size = new System.Drawing.Size(109, 13);
            this.chatroomInfo.TabIndex = 5;
            this.chatroomInfo.Text = "Chatroom IP Address:";
            // 
            // chatText
            // 
            this.chatText.Location = new System.Drawing.Point(12, 41);
            this.chatText.MaximumSize = new System.Drawing.Size(343, 200);
            this.chatText.MinimumSize = new System.Drawing.Size(343, 200);
            this.chatText.Multiline = true;
            this.chatText.Name = "chatText";
            this.chatText.ReadOnly = true;
            this.chatText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.chatText.Size = new System.Drawing.Size(343, 200);
            this.chatText.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(380, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Username:";
            // 
            // chatroomIP
            // 
            this.chatroomIP.BackColor = System.Drawing.SystemColors.Window;
            this.chatroomIP.Location = new System.Drawing.Point(383, 57);
            this.chatroomIP.MaximumSize = new System.Drawing.Size(100, 20);
            this.chatroomIP.MinimumSize = new System.Drawing.Size(100, 20);
            this.chatroomIP.Name = "chatroomIP";
            this.chatroomIP.Size = new System.Drawing.Size(100, 20);
            this.chatroomIP.TabIndex = 8;
            // 
            // userName
            // 
            this.userName.BackColor = System.Drawing.SystemColors.Window;
            this.userName.Location = new System.Drawing.Point(383, 152);
            this.userName.MaximumSize = new System.Drawing.Size(100, 20);
            this.userName.MinimumSize = new System.Drawing.Size(100, 20);
            this.userName.Name = "userName";
            this.userName.Size = new System.Drawing.Size(100, 20);
            this.userName.TabIndex = 9;
            // 
            // chatroomPort
            // 
            this.chatroomPort.BackColor = System.Drawing.SystemColors.Window;
            this.chatroomPort.Location = new System.Drawing.Point(383, 96);
            this.chatroomPort.MaximumSize = new System.Drawing.Size(55, 20);
            this.chatroomPort.MinimumSize = new System.Drawing.Size(55, 20);
            this.chatroomPort.Name = "chatroomPort";
            this.chatroomPort.Size = new System.Drawing.Size(55, 20);
            this.chatroomPort.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(380, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Chatroom IP Port:";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(174, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 12;
            this.button3.Text = "Disconnect";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(521, 308);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chatroomPort);
            this.Controls.Add(this.userName);
            this.Controls.Add(this.chatroomIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chatText);
            this.Controls.Add(this.chatroomInfo);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Instant Messenger";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox messageBox;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Label chatroomInfo;
        private System.Windows.Forms.TextBox chatText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox chatroomIP;
        private System.Windows.Forms.TextBox userName;
        private System.Windows.Forms.TextBox chatroomPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button3;
    }
}

