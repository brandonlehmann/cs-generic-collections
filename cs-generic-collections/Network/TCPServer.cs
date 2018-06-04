using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Generic.Collections;

namespace Generic.Network
{
    public class TCPServer
    {
        #region Variables
        private int intMaxClients = 1;
        private TcpListener tcp;
        private IPEndPoint localIPEndPoint;
        private Generic.Collections.Queue<KeyValuePair<IPEndPoint, byte[]>> queueOutgoing = new Generic.Collections.Queue<KeyValuePair<IPEndPoint, byte[]>>();
        private Generic.Collections.Buffer oBuffer = new Generic.Collections.Buffer();
        private Thread threadWrite;
        private Thread threadListen;
        private Generic.Collections.Dictionary<IPEndPoint, Thread> threadClients = new Generic.Collections.Dictionary<IPEndPoint, Thread>();
        //private ArrayList<Thread> threadClients = new ArrayList<Thread>();
        private ArrayList<TcpClient> tcpClients = new ArrayList<TcpClient>();
        private Generic.Collections.Dictionary<IPEndPoint, Generic.Collections.Buffer> bufferClients = new Generic.Collections.Dictionary<IPEndPoint, Generic.Collections.Buffer>();
        private static Mutex mutex = new Mutex();
        private bool boolSwitch = false;
        #endregion

        #region Events
        public event Connection onConnection;
        public event Disconnection onDisconnection;
        public event Incoming onIncoming;
        public delegate void Connection(IPEndPoint ipEndPoint);
        public delegate void Disconnection(IPEndPoint ipEndPoint);
        public delegate void Incoming(IPEndPoint ipEndPoint, byte[] message);
        #endregion

        #region Properties
        /// <summary>
        /// Very important
        /// If True - Sent items will be sent to ALL connected clients EXCEPT the one specified (if not specified then to all)
        /// If False - Sent items will be sent ONLY to the one specified (if not specified then all)
        /// </summary>
        public bool Switching
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return boolSwitch;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            set
            {
                mutex.WaitOne();
                try
                {
                    boolSwitch = value;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public int MaxClients
        {
            get
            {
                mutex.WaitOne();
                try
                {
                    return intMaxClients;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            set
            {
                mutex.WaitOne();
                try
                {
                    intMaxClients = value;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        public int Count
        {
            get
            {
                return threadClients.Count;
            }
        }
        public int Port
        {
            get
            {
                return localIPEndPoint.Port;
            }
        }
        public bool isAlive
        {
            get
            {
                if (threadListen.IsAlive && threadWrite.IsAlive)
                    return true;
                return false;
            }
        }
        protected IPEndPoint LocalEndPoint
        {
            get
            {
                return localIPEndPoint;
            }
            set
            {
                localIPEndPoint = value;
            }
        }
        #endregion

        #region Construct Methods
        protected TCPServer()
        {
        }
        protected TCPServer(int port)
        {
            localIPEndPoint = new IPEndPoint(IPAddress.Any, port);
        }
        protected TCPServer(int port, int maxClients)
        {
            localIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            intMaxClients = maxClients;
        }
        #endregion

        #region Core Methods
        protected int Length(IPEndPoint ipEndPoint)
        {
            try
            {
                return bufferClients[ipEndPoint].Length;
            }
            catch
            {
                return 0;
            }
        }
        protected byte[] Read(IPEndPoint ipEndPoint)
        {
            return Read(ipEndPoint, 1);
        }
        protected byte[] Read(IPEndPoint ipEndPoint, int bytes)
        {
            return bufferClients[ipEndPoint].Next(bytes);
        }
        protected void Read(IPEndPoint ipEndPoint, ref string message)
        {
            message = ASCIIEncoding.ASCII.GetString(Read(ipEndPoint, 1));
        }
        protected void Read(IPEndPoint ipEndPoint, ref string message, int bytes)
        {
            message = ASCIIEncoding.ASCII.GetString(Read(ipEndPoint, bytes));
        }
        protected void Write(byte[] message)
        {
            queueOutgoing.Enqueue(new KeyValuePair<IPEndPoint, byte[]>(null, message));
        }
        protected void Write(byte[] message, IPEndPoint target)
        {
            queueOutgoing.Enqueue(new KeyValuePair<IPEndPoint, byte[]>(target, message));
        }
        protected void Write(byte[] message, IPAddress ipAddress, int port)
        {
            IPEndPoint endpoint = new IPEndPoint(ipAddress, port);
            queueOutgoing.Enqueue(new KeyValuePair<IPEndPoint, byte[]>(endpoint, message));
        }
        protected void Write(string message)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(message);
            queueOutgoing.Enqueue(new KeyValuePair<IPEndPoint, byte[]>(null, bytes));
        }
        protected void Write(string message, IPEndPoint target)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(message);
            queueOutgoing.Enqueue(new KeyValuePair<IPEndPoint, byte[]>(target, bytes));
        }
        protected void Write(string message, IPAddress ipAddress, int port)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(message);
            IPEndPoint endpoint = new IPEndPoint(ipAddress, port);
            queueOutgoing.Enqueue(new KeyValuePair<IPEndPoint, byte[]>(endpoint, bytes));
        }
        #endregion

        #region Threading Methods
        protected void Start()
        {
            tcp = new TcpListener(localIPEndPoint);

            threadWrite = new Thread(new ThreadStart(WriteThread));
            threadWrite.Start();

            threadListen = new Thread(new ThreadStart(ListenThread));
            threadListen.Start();
        }
        protected void Stop()
        {
            foreach (TcpClient client in tcpClients)
            {
                if (client != null && client.Connected)
                    client.Close();
            }

            if (threadWrite != null && threadWrite.IsAlive)
                threadWrite.Abort();

            if (threadListen != null && threadListen.IsAlive)
                threadListen.Abort();
        }
        private void ListenThread()
        {
            tcp.Start();
            while (true)
            {
                TcpClient client = tcp.AcceptTcpClient();
                if (Count < MaxClients)
                {
                    Thread clientThread = new Thread(new ParameterizedThreadStart(ClientThread));
                    clientThread.Start(client);
                    threadClients.Add( (IPEndPoint)client.Client.RemoteEndPoint, clientThread);
                }
                else
                {
                    client.Close();
                }
            }
        }
        private void ClientThread(object obj)
        {
            TcpClient client = (TcpClient)obj;
            IPEndPoint ipEndPoint;
            try
            {
                ipEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            }
            catch { return; }
            if (onConnection != null)
                onConnection(ipEndPoint);
            tcpClients.Add(client);
            bufferClients.Add(ipEndPoint, new Generic.Collections.Buffer());
            NetworkStream ns = client.GetStream();
            int timeout = 0;
            while (timeout < 600)
            {
                if (!client.Connected)
                    break;

                if (ns.DataAvailable)
                {
                    timeout = 0;
                    byte[] buffer = new byte[8192];
                    if (client.Connected)
                    {
                        int bytesRead = 0;
                        try
                        {
                            bytesRead = ns.Read(buffer, 0, 8192);
                            byte[] message = ASCIIEncoding.ASCII.GetBytes(ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead));
                            bufferClients[ipEndPoint].Add(message);
                            if (onIncoming != null)
                                onIncoming(ipEndPoint, message);
                        }
                        catch { }
                    }
                    else
                    {
                        break;
                    }
                }
                Thread.Sleep(100);
                timeout++;
            }
            DeadClient(client);
        }
        private void DeadClient(TcpClient client)
        {
            IPEndPoint ipEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            if (onDisconnection != null)
                onDisconnection(ipEndPoint);
            bufferClients.Remove(ipEndPoint);
            tcpClients.Remove(client);
            threadClients.Remove(ipEndPoint);
            client.Close();
        }
        private void WriteThread()
        {
            while (true)
            {
                if (queueOutgoing.Count > 0)
                {
                    KeyValuePair<IPEndPoint, byte[]> message = queueOutgoing.Dequeue();
                    if (tcpClients.Count > 0)
                        try
                        {
                            foreach (TcpClient client in tcpClients)
                            {
                                try
                                {
                                    if (!client.Connected)
                                    {
                                        DeadClient(client);
                                        continue;
                                    }
                                    if (Switching)
                                    {
                                        if ((IPEndPoint)client.Client.RemoteEndPoint != message.Key)
                                        {
                                            try
                                            {
                                                NetworkStream ns = client.GetStream();
                                                ns.Write(message.Value, 0, message.Value.Length);
                                            }
                                            catch
                                            {
                                                DeadClient(client);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (message.Key == null)
                                            {
                                                NetworkStream ns = client.GetStream();
                                                ns.Write(message.Value, 0, message.Value.Length);
                                            }
                                            else if ((IPEndPoint)client.Client.RemoteEndPoint == message.Key)
                                            {
                                                NetworkStream ns = client.GetStream();
                                                ns.Write(message.Value, 0, message.Value.Length);
                                            }
                                        }
                                        catch
                                        {
                                            DeadClient(client);
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                }
                Thread.Sleep(5);
            }
        }
        #endregion
    }
}
