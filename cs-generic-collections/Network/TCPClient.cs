using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Generic.Collections;

namespace Generic.Network
{
    public class TCPClient
    {
        #region Variables
        private TcpClient tcp = new TcpClient();
        private IPEndPoint remoteIPEP;
        private Queue<byte[]> gqOutgoing = new Queue<byte[]>();
        private Generic.Collections.Buffer oBuffer = new Generic.Collections.Buffer();
        private Thread threadWrite;
        private Thread threadRead;
        private Thread threadMonitor;
        private Mutex mutex = new Mutex();
        private bool _sentOnConnect;
        private bool _sentOnDisconnect;
        private bool run;
        #endregion
        #region Events (Delegates)
        public event CommunicationError onError;
        public delegate void CommunicationError(Exception exception);
        public event ConnectionEstablished onConnect;
        public delegate void ConnectionEstablished();
        public event ConnectionDisconnected onDisconnect;
        public delegate void ConnectionDisconnected();
        public event IncomingMessage onReceive;
        public delegate void IncomingMessage(byte[] message);
        public event OutgoingMessage onTransmit;
        public delegate void OutgoingMessage(byte[] message);
        #endregion
        #region Properties
        public bool Connected
        {
            get
            {
                return tcp.Connected;
            }
        }
        public int Count
        {
            get
            {
                return gqOutgoing.Count;
            }
        }
        public int Length
        {
            get
            {
                return oBuffer.Length;
            }
        }
        public bool Available
        {
            get
            {
                if (!tcp.Connected)
                    return false;
                if (oBuffer.Length > 0)
                    return true;
                return false;
            }
        }
        public bool isAlive
        {
            get
            {
                if (threadRead.IsAlive && threadWrite.IsAlive && threadMonitor.IsAlive && Connected)
                    return true;
                return false;
            }
        }
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return remoteIPEP;
            }
            set
            {
                remoteIPEP = value;
            }
        }
        #endregion
        #region Contructor Methods
        public TCPClient()
        {
            Constr();
        }
        public TCPClient(IPAddress ipAddress, int port)
        {
            remoteIPEP = new IPEndPoint(ipAddress, port);
            Constr();
        }
        public TCPClient(string hostName, int port)
        {
            try
            {
                remoteIPEP = new IPEndPoint(Dns.GetHostAddresses(hostName)[0], port);
            }
            catch { }
            Constr();
        }
        public TCPClient(IPEndPoint ipEndPoint)
        {
            remoteIPEP = ipEndPoint;
            Constr();
        }

        public void FlushQueues()
        {
            oBuffer.Clear();
            gqOutgoing.Clear();
        }
        private void Constr()
        {
            threadWrite = new Thread(new ThreadStart(WriteThread));
            threadRead = new Thread(new ThreadStart(ReadThread));
            threadMonitor = new Thread(new ThreadStart(MonitorThread));
        }
        #endregion
        #region Event Methods
        private void RunError(Exception exception)
        {
            if (onError != null)
                onError(exception);
        }
        private void RunConnect()
        {
            mutex.WaitOne();
            try
            {
                _sentOnConnect = true;
                _sentOnDisconnect = false;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            if (onConnect != null)
                onConnect();    
        }
        private void RunDisconnect()
        {
            mutex.WaitOne();
            try
            {
                _sentOnDisconnect = true;
                _sentOnConnect = false;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            FlushQueues();
            if (onDisconnect != null)
                onDisconnect();
        }
        private void ReInit()
        {
            tcp = new TcpClient();
        }
        private void RunTransmit(byte[] message)
        {
            if (onTransmit != null)
                onTransmit(message);
        }
        private void RunReceive(byte[] message)
        {
            if (onReceive != null)
                onReceive(message);
        }
        #endregion
        #region Connect Methods
        public void Connect(IPAddress ipAddress, int port)
        {
            remoteIPEP = new IPEndPoint(ipAddress, port);
            Connect();
        }
        public void Connect(string hostName, int port)
        {
            try
            {
                remoteIPEP = new IPEndPoint(Dns.GetHostAddresses(hostName)[0], port);
            }
            catch (Exception e)
            {
                RunError(e);
                return;
            }
            Connect();
        }
        public void Connect(IPEndPoint ipEndPoint)
        {
            remoteIPEP = ipEndPoint;
            Connect();
        }
        public void Connect()
        {
            _sentOnConnect = false;
            try
            {
                tcp.Connect(remoteIPEP);
            }
            catch (Exception e)
            {
                RunError(e);
            }
            Start();
        }
        #endregion
        #region Disconnect Method
        public void Disconnect()
        {
            if (tcp.Connected)
                tcp.Close();
        }
        public void Disconnect(bool exitThreads)
        {
            Stop();
            Disconnect();
        }
        #endregion
        #region Communication Methods
        public void Write(byte[] message)
        {
            gqOutgoing.Enqueue(message);
        }
        public void Write(string message)
        {
            byte[] result = ASCIIEncoding.ASCII.GetBytes(message);
            gqOutgoing.Enqueue(result);
        }
        public byte[] Read()
        {
            return Read(1);
        }
        public byte[] Read(int bytes)
        {
            return oBuffer.Next(bytes);
        }
        public void Read(ref string message)
        {
            message = ASCIIEncoding.ASCII.GetString(Read());
        }
        public void Read(ref string message, int bytes)
        {
            message = ASCIIEncoding.ASCII.GetString(Read(bytes));
        }
        #endregion
        #region Threading Methods
        private void MonitorThread()
        {
            while (run)
            {
                if (Connected)
                {
                    mutex.WaitOne();
                    try
                    {
                        if (!_sentOnConnect)
                        {
                            RunConnect();
                        }
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                else
                {
                    mutex.WaitOne();
                    try
                    {
                        if (!_sentOnDisconnect && _sentOnConnect)
                        {
                            RunDisconnect();
                        }
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                Thread.Sleep(1000);
            }
        }
        private void WriteThread()
        {
            NetworkStream ns;
            try
            {
                ns = tcp.GetStream();
            }
            catch (Exception e)
            {
                RunError(e);
                return;
            }
            while (run)
            {
                if (gqOutgoing.Count > 0)
                {
                    if (tcp.Connected)
                    {
                        byte[] message = gqOutgoing.Dequeue();
                        try
                        {
                            ns.Write(message, 0, message.Length);
                            RunTransmit(message);
                        }
                        catch (Exception e)
                        {
                            RunError(e);
                        }
                    }

                }
                Thread.Sleep(1000);
            }
        }
        private void ReadThread()
        {
            NetworkStream ns;
            try
            {
                ns = tcp.GetStream();
            }
            catch (Exception e)
            {
                RunError(e);
                return;
            }
            while (run)
            {
                try
                {
                    if (ns.DataAvailable)
                    {
                        byte[] buffer = new byte[1024];
                        if (tcp.Connected)
                        {
                            int bytesRead = 0;
                            try
                            {
                                bytesRead = ns.Read(buffer, 0, 1024);
                                byte[] temp = new byte[bytesRead];
                                for (int n = 0; n < bytesRead; n++)
                                {
                                    temp[n] = buffer[n];
                                }
                                // byte[] message = ASCIIEncoding.ASCII.GetBytes(ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead));
                                oBuffer.Add(temp);
                                RunReceive(temp);
                            }
                            catch (Exception e)
                            {
                                RunError(e);
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }
                Thread.Sleep(1000);
            }
        }
        #endregion
        #region Start/Stop Methods
        private void Start()
        {
            if (!tcp.Connected)
                return;

            mutex.WaitOne();
            try
            {
                run = true;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            try
            {
                threadWrite.Name = String.Format("BitRadius.Net.TCPClient: Thread Writer {0}", remoteIPEP.Address);
                threadRead.Name = String.Format("BitRadius.Net.TCPClient: Thread Reader {0}", remoteIPEP.Address);
                threadMonitor.Name = String.Format("BitRadius.Net.TCPClient: Thread Monitor {0}", remoteIPEP.Address);
            }
            catch { }
            try
            {
                threadWrite.Start();
            }
            catch { }

            try
            {
                threadRead.Start();
            }
            catch { }

            try
            {
                threadMonitor.Start();
            }
            catch { }
        }
        private void Stop()
        {
            run = false;
        }
        #endregion
    }
}
