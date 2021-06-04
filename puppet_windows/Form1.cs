﻿using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace puppet_windows
{
    public partial class Form1 : Form
    {
        private BackgroundWorker backgroundWorker_client_listner = new BackgroundWorker();
        private BackgroundWorker backgroundWorker_client_recive = new BackgroundWorker();
        private BackgroundWorker backgroundWorker_client_send = new BackgroundWorker();
        private Socket client_listner;
        private Socket client_socket;

        private Cursor mouser = new Cursor(Cursor.Current.Handle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public Form1()
        {
            InitializeComponent();

            backgroundWorker_client_listner.DoWork += backgroundWorker_client_listner_DoWork;
            backgroundWorker_client_listner.RunWorkerCompleted += backgroundWorker_client_listner_RunWorkerComplete;
            backgroundWorker_client_listner.WorkerSupportsCancellation = true;

            backgroundWorker_client_recive.DoWork += backgroundWorker_client_recive_DoWork;
            backgroundWorker_client_recive.RunWorkerCompleted += BackgroundWorker_client_recive_RunWorkerCompleted;
            backgroundWorker_client_recive.WorkerSupportsCancellation = true;

            backgroundWorker_client_send.DoWork += BackgroundWorker_client_send_DoWork;
            backgroundWorker_client_send.RunWorkerCompleted += BackgroundWorker_client_send_RunWorkerCompleted;
            backgroundWorker_client_send.WorkerSupportsCancellation = true;

            update("Start");
            update_qr("Prajwal");
        }

        private void btn_main_Click(object sender, EventArgs e)
        {
            if (client_listner == null)
            {
                backgroundWorker_client_listner.RunWorkerAsync();
                update("waiting...");
                btn_main.Enabled = false;
            }
            else
            {
                if (client_listner != null) client_listner.Close();
                if (client_socket != null) client_socket.Close();
                // this.Close();
                client_listner = null;
                client_socket = null;
                backgroundWorker_client_listner.CancelAsync();
                backgroundWorker_client_recive.CancelAsync();
                backgroundWorker_client_send.CancelAsync();
                update("Start");
            }

        }

        private void backgroundWorker_client_listner_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (backgroundWorker_client_listner.CancellationPending) return;

            int port = 9999;
            String hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

            IPAddress ipAddress = Dns.GetHostByName(hostName).AddressList[1];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);


            // IPAddress[] ips = Dns.GetHostByName(hostName).AddressList;
            // Console.WriteLine("Len: " + ips.Length);
            // for (int i = 0; i < ips.Length; i++)  Console.WriteLine(ips[i].ToString());
;
            string name = text_name.Text;
            if (string.IsNullOrEmpty(name)) name = "server";
            string txdata = name + "<-->" + ipAddress.ToString() + "<-->" + port;
            update_qr(txdata);
            display(txdata);

            client_listner = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client_listner.Bind(localEndPoint);
                client_listner.Listen(1);
                display("Waiting for client...");
                client_socket = client_listner.Accept();
            }
            catch (Exception ex)
            {
                display("Exception: " + ex.ToString());
            }
            if (backgroundWorker_client_listner.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

        }

        private void backgroundWorker_client_listner_RunWorkerComplete(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            update("Stop");
            btn_main.Enabled = true;
            if (e.Cancelled) display("Process was canceled");
            else if (e.Error != null) display("Error occured");
            else
            {
                display("Client Connected!");
                backgroundWorker_client_recive.RunWorkerAsync();
                // backgroundWorker_client_send.RunWorkerAsync();
            }
        }

        private void backgroundWorker_client_recive_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (backgroundWorker_client_recive.CancellationPending) return;
                if (client_socket == null) e.Cancel = true;
                // Receiving
                byte[] rcvLenBytes = new byte[4];
                client_socket.Receive(rcvLenBytes);
                int rcvLen = System.BitConverter.ToInt32(rcvLenBytes, 0);
                if (rcvLen > 0)
                {
                    byte[] rcvBytes = new byte[rcvLen];
                    client_socket.Receive(rcvBytes);
                    string recived_cmd = System.Text.Encoding.ASCII.GetString(rcvBytes);
                    display(recived_cmd);
                    if (string.Equals(recived_cmd, "exit")) e.Cancel = true;
                    else executeCommands(recived_cmd);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excp: " + ex.Message);
            }
        }

        private void executeCommands(String cmds)
        {

            String[] crsr = cmds.Split(':');
            if (crsr.Length == 3)
            {
                try
                {
                    int x = Int32.Parse(crsr[0]);
                    int y = Int32.Parse(crsr[1]);
                    int c = Int32.Parse(crsr[2]);
                    Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
                    if (c == 1)
                    {
                        uint X = (uint)Cursor.Position.X;
                        uint Y = (uint)Cursor.Position.Y;
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                    }
                    else if (c == 2)
                    {
                        uint X = (uint)Cursor.Position.X;
                        uint Y = (uint)Cursor.Position.Y;
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                        Thread.Sleep(50);
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private void BackgroundWorker_client_recive_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null) display("Connection terminated! start again");
            else backgroundWorker_client_recive.RunWorkerAsync();
        }

        private void BackgroundWorker_client_send_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundWorker_client_send.CancellationPending) return;

            int num = new Random().Next(100);
            Thread.Sleep(1700);
            display("Send " + num.ToString());
        }

        private void BackgroundWorker_client_send_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null) display("Somthing went wrong");
            else backgroundWorker_client_send.RunWorkerAsync();
            // throw new NotImplementedException();
        }

        delegate void SetTextCallBack(string text);

        private void display(string message)
        {

            if (textLogs.InvokeRequired)
            {
                SetTextCallBack d = new SetTextCallBack(display);
                Invoke(d, new object[] { message });
            }
            else
            {
                // textLogs.AppendText(message + "\r\n");
                string[] msgs = Regex.Split(message, @"<-->");

                if (msgs.Length == 3)
                {
                    message = "\r\n" +
                        "  Status: " + msgs[0] + "\r\n" +
                        "  Mouse: " + msgs[1] + "\r\n" +
                        "  Keyboard: " + msgs[2];
                }
                else message = "\r\n  " + message;

                textLogs.Text = message;
            }
                
        }

        private void update(string text)
        {
            if (btn_main.InvokeRequired)
            {
                SetTextCallBack d = new SetTextCallBack(update);
                Invoke(d, new object[] { text });
            }
            else
            {
                btn_main.Text = text;
            }
        }

        private void update_qr(String text)
        {
            QRCodeGenerator qr = new QRCodeGenerator();
            QRCodeData data = qr.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode code = new QRCode(data);
            picture_qr.Image = code.GetGraphic(5);
        }
    }
}
