using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace bottleDetection.Tool
{

    //这个类不一定能用，但是起码复习了事件与委托
    //下一步就是学习. net内存管理

    class OmronFINS
    {
        /// <summary>
        /// 客户端连接Socket
        /// </summary>
        private Socket clientSocket;
        /// <summary>
        /// 连接状态
        /// </summary>
        public Boolean connected = false;
        /// <summary>
        /// 发送数据
        /// </summary>
        private Byte[] SendMess;
        /// <summary>
        /// 连接点
        /// </summary>
        private IPEndPoint hostEndPoint;
        /// <summary>
        /// 连接信号量
        /// </summary>
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false);
        /// <summary>
        /// 接受到数据时的委托
        /// 委托本质上是一种类型约束，可以约束事件
        /// </summary>
        /// <param name="info"></param>
        public delegate void ReceiveMsgHandler(Byte[] info);
        /// <summary>
        /// 接收到数据时调用的事件
        /// 这里委托和事件定义的名字是相同的，也就是说用委托定义了事件类型
        /// </summary>
        public event ReceiveMsgHandler OnMsgReceived;
        /// <summary>
        /// 开始监听数据的委托
        /// </summary>
        public delegate void StartListenHandler();
        /// <summary>
        /// 开始监听数据的事件
        /// </summary>
        public event StartListenHandler StartListenThread;
        /// <summary>
        /// 发送信息完成的委托
        /// </summary>
        /// <param name="successorfalse"></param>
        public delegate void SendCompleted(bool successorfalse);
        /// <summary>
        /// 发送信息完成的事件
        /// </summary>
        public event SendCompleted OnSended;
        /// <summary>
        /// 监听接收的SocketAsyncEventArgs
        /// </summary>
        private SocketAsyncEventArgs listenerSocketAsyncEventArgs;
        int Plcport;
        public OmronFINS(String hostName, Int32 port, Int32 PLCStaion)
        {
            Plcport = PLCStaion;
            IPAddress[] addressList = Dns.GetHostAddresses(hostName);
            this.hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
        /// <summary>
        /// 连接服务端
        /// </summary>
        private bool Connect()
        {
            using (SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs())
            {
                connectArgs.UserToken = this.clientSocket;
                connectArgs.RemoteEndPoint = this.hostEndPoint;
                connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
                clientSocket.ConnectAsync(connectArgs);
                //等待连接结果
                bool autores = autoConnectEvent.WaitOne(1000);
                if (this.connected)
                {
                    listenerSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    byte[] receiveBuffer = new byte[1024];//设置接收buffer区大小
                    listenerSocketAsyncEventArgs.UserToken = clientSocket;
                    listenerSocketAsyncEventArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                    listenerSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                    //这里是执行StartListenThread事件
                    StartListenThread();
                    // SocketExtensions.SetKeepAlive(clientSocket, 3000, 1000);
                    return true;
                }
                else
                    return false;
            }
        }
        /// <summary>
        /// 开始监听线程的入口函数
        /// </summary>
        private void Listen()
        {
            (listenerSocketAsyncEventArgs.UserToken as Socket).ReceiveAsync(listenerSocketAsyncEventArgs);
        }
        public static List<SocketAsyncEventArgs> s_lst = new List<SocketAsyncEventArgs>();
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="message"></param>
        private void Send(Byte[] message)
        {
            if (this.connected)
            {
                Byte[] sendBuffer = message;
                SocketAsyncEventArgs senderSocketAsyncEventArgs = null;// new SocketAsyncEventArgs();
                lock (s_lst)
                {
                    if (s_lst.Count > 0)
                    {
                        senderSocketAsyncEventArgs = s_lst[s_lst.Count - 1];
                        s_lst.RemoveAt(s_lst.Count - 1);
                    }
                }
                if (senderSocketAsyncEventArgs == null)
                {
                    senderSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    senderSocketAsyncEventArgs.UserToken = this.clientSocket;
                    senderSocketAsyncEventArgs.RemoteEndPoint = this.hostEndPoint;
                    senderSocketAsyncEventArgs.Completed += (object sender, SocketAsyncEventArgs _e) =>
                    {
                        lock (s_lst)
                        {
                            s_lst.Add(senderSocketAsyncEventArgs);
                        }
                    };
                }
                senderSocketAsyncEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                clientSocket.SendAsync(senderSocketAsyncEventArgs);
            }
            else
            {
                this.connected = false;
            }
            SendMess = message;
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        private bool Disconnect()
        {
            bool returnDis = true;
            try
            {
                this.clientSocket.Shutdown(SocketShutdown.Both);
                this.clientSocket.Close();
                //this.clientSocket.Dispose();
                //clientSocket.Disconnect(true);
                //clientSocket.Disconnect(false);
            }
            catch (Exception)
            {
                returnDis = false;
            }
            finally
            {
            }
            this.connected = false;
            return returnDis;
        }
        /// <summary>
        /// 连接的完成方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            this.connected = (e.SocketError == SocketError.Success);
            autoConnectEvent.Set();
        }
        /// <summary>
        /// 接收的完成方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                //Console.WriteLine("Socket is closed", Socket.Handle);
                if (clientSocket.Connected)
                {
                    try
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception)
                    {
                        //client already closed
                    }
                    finally
                    {
                        if (clientSocket.Connected)
                        {
                            clientSocket.Close();
                        }
                    }
                }
                Byte[] rs = new Byte[0];
                OnMsgReceived(rs);
                this.connected = false;
            }
            else
            {
                byte[] buffer = new byte[e.BytesTransferred];
                for (int i = 0; i < e.BytesTransferred; i++)
                {
                    buffer[i] = e.Buffer[i];
                }
                this.OnMsgReceived(buffer);
                Listen();
            }
        }
        /// <summary>
        /// 发送的完成方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                OnSended(true);
            }
            else
            {
                OnSended(false);
                this.ProcessError(e);
            }
        }
        /// <summary>
        /// 处理错误
        /// </summary>
        /// <param name="e"></param>
        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    //client already closed
                }
                finally
                {
                    if (s.Connected)
                    {
                        s.Close();
                    }
                }
                this.connected = false;
            }
            //throw new SocketException((Int32)e.SocketError);
        }
        #region IDisposable Members
        public void Dispose()
        {
            autoConnectEvent.Close();
            if (this.clientSocket.Connected)
            {
                this.clientSocket.Close();
            }
        }
        #endregion
        #region  欧姆龙通讯协议
        /// <summary>
        /// 发送命令反馈
        /// </summary>
        /// <param name="successorfalse"></param>
        void OmronFINS_OnSended(bool successorfalse)
        {
            if (!successorfalse)
            {
            }
            else
            {
            }
        }
        Byte PCS;
        Byte PLCS;
        int SendOrRev;
        int SendOrRev2;
        Byte[] SendBack;
        Byte[] RecvBack;
        /// <summary>
        /// 接受命令反馈
        /// </summary>
        /// <param name="info"></param>
        void OmronFINS_OnMsgReceived(byte[] info)
        {
            if (SendmessHas)
            {
                if (info.Length >= 24)
                {
                    if (int.Parse(info[23].ToString("X").Trim(), System.Globalization.NumberStyles.HexNumber) == Plcport)
                    {
                        PCS = info[19];
                        PLCS = info[23];
                        SendmessHas = false;
                    }
                }
            }
            else
            {
                if (info.Length > 0)//PLC连接错误NO
                {
                    if (SendOrRev2 == 1)//发送命令
                    {
                        SendBack = info;
                        SendOrRev2 = 0;
                    }
                    else if (SendOrRev == 2)//接受命令
                    {
                        RecvBack = info;
                        SendOrRev = 0;
                    }
                }
                else
                {
                    if (SendOrRev2 == 1)//发送命令
                    {
                        SendBack = new Byte[1];
                        SendBack[0] = 0x00;
                        SendOrRev2 = 0;
                    }
                    else if (SendOrRev == 2)//接受命令
                    {
                        RecvBack = new Byte[1];
                        RecvBack[0] = 0x00;
                        SendOrRev = 0;
                    }
                }
            }
        }
        void OmronFINS_StartListenThread()
        {
            this.Listen();
        }
        bool SendmessHas = false;
        /// <summary>
        /// 打开连接
        /// </summary>
        /// <returns></returns>
        public bool OpenLinkPLC()
        {
            bool Ret = false;
            //这里的+=就是为StartListenThread绑定事件处理程序
            //这里要注意 StartListenHandler是一个委托类型，
            this.StartListenThread += new StartListenHandler(OmronFINS_StartListenThread);
            this.OnMsgReceived += new ReceiveMsgHandler(OmronFINS_OnMsgReceived);
            this.OnSended += new SendCompleted(OmronFINS_OnSended);
            Ret = this.Connect();
            if (Ret)
            {
                SendmessHas = true;
                this.Send(GetPCAdress());
                int addNs = 0;
                while (SendmessHas && addNs < 500)
                {
                    Thread.Sleep(2);
                    addNs++;
                }
                if (addNs < 500)
                {
                    Ret = true;
                }
                else
                {
                    Ret = false;
                }
            }
            return Ret;
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        public bool CloseLinkPLC()
        {
            return this.Disconnect();
        }
        /// <summary>
        /// 写入值(Word)
        /// </summary>
        /// <param name="TypeInt">起始寄存器地址</param>
        /// <param name="NumData">个数</param>
        /// <param name="IntValue">值</param>
        /// <returns>true,成功;false,失败</returns>
        public bool WritePlcData(string TypeInt, int NumData, ref int[] IntValue)
        {
            bool sendRerun = false;
            if (NumData > 0)
            {
                Byte[] numData = BitConverter.GetBytes(NumData);//Use numData[0] and numData[1]
                Byte[] SendmessAge = new Byte[34 + NumData * 2];
                if (GetType(TypeInt, 0) != 0x00 && Address != null && Address.Length > 1)
                {
                    Byte[] getSendFunc = this.SetPLCvalue(PLCS, PCS, GetType(TypeInt, 0), Address[1], Address[0], 0x00, numData[1], numData[0], NumData);
                    for (int i = 0; i < 34 + NumData * 2; i++)
                    {
                        if (i < 34)
                        {
                            SendmessAge[i] = getSendFunc[i];
                        }
                        else
                        {
                            if ((i - 33) % 2 != 0)
                            {
                                Byte[] BuffInvalue = BitConverter.GetBytes(IntValue[(i - 33) / 2]);
                                SendmessAge[i] = BuffInvalue[1];
                            }
                            else
                            {
                                Byte[] BuffInvalue = BitConverter.GetBytes(IntValue[(i - 33) / 2 - 1]);
                                SendmessAge[i] = BuffInvalue[0];
                            }
                            //SendmessAge[i]
                        }
                    }
                    SendOrRev2 = 1;
                    this.Send(SendmessAge);
                    int Outtime = 0;
                    while (SendOrRev2 != 0 && this.connected && Outtime < 600)
                    {
                        Thread.Sleep(2);
                        Outtime++;
                    }
                    if (Outtime < 600 && SendBack != null)
                    {
                        try
                        {
                            //if (SendBack.Length > 1 && SendBack[26] == 0x01 && SendBack[27] == 0x02 && SendBack[28] == 0x00 && SendBack[29] == 0x00)
                            //{
                            if (SendBack.Length > 1 && SendBack[26] == 0x01 && SendBack[27] == 0x02 && SendBack[28] == 0x00)
                            {
                                sendRerun = true;
                            }
                            else
                            {
                                sendRerun = false;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        SendOrRev2 = 0;
                    }
                }
            }
            return sendRerun;
        }
        /// <summary>
        /// 读取值(Word)
        /// </summary>
        /// <param name="TypeInt">起始寄存器地址</param>
        /// <param name="NumData">个数</param>
        /// <param name="IntValue">值</param>
        /// <returns></returns>
        public bool ReadPlcData(string TypeInt, int NumData, out int[] IntValue)
        {
            bool GetPlcRet = false;
            IntValue = new int[1];
            if (NumData > 0)
            {
                IntValue = new int[NumData];
                Byte[] numData = BitConverter.GetBytes(NumData);//Use numData[0] and numData[1]
                Byte[] RecivemessAge = new Byte[34];
                if (GetType(TypeInt, 0) != 0x00 && Address != null && Address.Length > 1)
                {
                    Byte[] getReciveFunc = this.GetPLCvalue(PLCS, PCS, GetType(TypeInt, 0), Address[1], Address[0], 0x00, numData[1], numData[0]);
                    this.Send(getReciveFunc);
                    SendOrRev = 2;
                    int Outtime = 0;
                    while (SendOrRev != 0 && this.connected && Outtime < 600)
                    {
                        Thread.Sleep(2);
                        Outtime++;
                    }
                    if (Outtime < 600 && RecvBack != null)
                    {
                        try
                        {
                            //if (RecvBack.Length > 1 && RecvBack[26] == 0x01 && RecvBack[27] == 0x01 && RecvBack[28] == 0x00 && RecvBack[29] == 0x00)
                            if (RecvBack.Length == 30 + NumData * 2 && RecvBack[26] == 0x01 && RecvBack[27] == 0x01 && RecvBack[28] == 0x00)
                            {
                                GetPlcRet = true;
                                for (int i = 0; i < NumData; i++)
                                {
                                    IntValue[i] = int.Parse(RecvBack[30 + i * 2].ToString("X").PadLeft(2, '0') + RecvBack[30 + i * 2 + 1].ToString("X").PadLeft(2, '0'), System.Globalization.NumberStyles.HexNumber);
                                }
                            }
                            else
                            {
                                GetPlcRet = false;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        SendOrRev = 0;
                    }
                }
            }
            return GetPlcRet;
        }
        /// <summary>
        /// 读取值(Word)
        /// </summary>
        /// <param name="TypeInt">起始寄存器地址</param>
        /// <param name="NumData">个数</param>
        /// <param name="IntValue">值</param>
        /// <returns></returns>
        public bool ReadPlcDataCIO(string TypeInt, int NumData, out int[] IntValue)
        {
            bool GetPlcRet = false;
            IntValue = new int[1];
            if (NumData > 0)
            {
                IntValue = new int[NumData];
                Byte[] numData = BitConverter.GetBytes(NumData);//Use numData[0] and numData[1]
                Byte[] RecivemessAge = new Byte[34];
                if (GetType(TypeInt, 0) != 0x00 && Address != null && Address.Length > 1)
                {
                    Byte[] getReciveFunc = this.GetPLCvalue(PLCS, PCS, GetType(TypeInt, 0), Address[1], Address[0], 0x00, numData[1], numData[0]);
                    this.Send(getReciveFunc);
                    SendOrRev = 2;
                    int Outtime = 0;
                    while (SendOrRev != 0 & this.connected & Outtime < 800)
                    {
                        Thread.Sleep(2);
                        Outtime++;
                    }
                    if (Outtime < 800 && RecvBack != null)
                    {
                        //try
                        //{
                        //if (RecvBack.Length > 1 && RecvBack[26] == 0x01 && RecvBack[27] == 0x01 && RecvBack[28] == 0x00 && RecvBack[29] == 0x00)
                        if (RecvBack.Length > 30 && RecvBack[26] == 0x01 && RecvBack[27] == 0x01 && RecvBack[28] == 0x00)
                        {
                            GetPlcRet = true;
                            for (int i = 0; i < NumData; i++)
                            {
                                IntValue[i] = int.Parse(RecvBack[30 + i * 2].ToString("X").PadLeft(2, '0') + RecvBack[30 + i * 2 + 1].ToString("X").PadLeft(2, '0'), System.Globalization.NumberStyles.HexNumber);
                            }
                        }
                        else
                        {
                            GetPlcRet = false;
                        }
                        //}
                        //catch (Exception)
                        //{
                        //}
                    }
                    else
                    {
                        SendOrRev = 0;
                    }
                }
            }
            return GetPlcRet;
        }
        /// <summary>
        /// 读取值(Bit)
        /// </summary>
        /// <param name="TypeInt">起始寄存器地址</param>
        /// <param name="IntValue">值</param>
        /// <returns></returns>
        public bool ReadPlcBitData(string TypeInt, out int IntValue)
        {
            bool GetPlcRet = false;
            IntValue = 0;
            Byte[] RecivemessAge = new Byte[34];
            if (GetType(TypeInt, 1) != 0x00 && Address != null && Address.Length > 1)
            {
                Byte[] getReciveFunc = this.GetPLCvalue(PLCS, PCS, GetType(TypeInt, 1), Address[1], Address[0], AddressBit[0]);
                this.Send(getReciveFunc);
                SendOrRev = 2;
                int Outtime = 0;
                while (SendOrRev != 0 & Outtime < 600)
                {
                    Thread.Sleep(2);
                    Outtime++;
                }
                if (Outtime < 600)
                {
                    if (RecvBack.Length > 1 && RecvBack[26] == 0x01 && RecvBack[27] == 0x01 && RecvBack[28] == 0x00 && RecvBack[29] == 0x00)
                    {
                        GetPlcRet = true;
                        IntValue = int.Parse(RecvBack[30].ToString("X").PadLeft(2, '0'), System.Globalization.NumberStyles.HexNumber);
                    }
                }
                else
                {
                    SendOrRev = 0;
                }
            }
            return GetPlcRet;
        }
        /// <summary>
        /// Write值(bit)
        /// </summary>
        /// <param name="TypeInt">寄存器地址(CIOX.X)</param>
        /// <returns></returns>
        public bool WriteBitPlcData(string TypeInt, int ItemValue)
        {
            bool sendRerun = false;
            Byte[] SendmessAge = new Byte[35];
            if (GetType(TypeInt, 1) != 0x00 && Address != null && Address.Length > 1)
            {
                Byte[] getSendFunc = this.WritePLCBitvalue(PLCS, PCS, GetType(TypeInt, 1), Address[1], Address[0], AddressBit[0]);
                for (int i = 0; i < 35; i++)
                {
                    if (i < 32)
                    {
                        SendmessAge[i] = getSendFunc[i];
                    }
                    else if (i == 32)
                    {
                        SendmessAge[i] = 0x00;
                    }
                    else if (i == 33)
                    {
                        SendmessAge[i] = 0x01;
                    }
                    else
                    {
                        if (ItemValue == 1)
                            SendmessAge[i] = 0x01;
                        else
                            SendmessAge[i] = 0x00;
                    }
                }
                this.Send(SendmessAge);
                SendOrRev2 = 1;
                int Outtime = 0;
                while (SendOrRev2 != 0 & Outtime < 600)
                {
                    Thread.Sleep(2);
                    Outtime++;
                }
                if (Outtime < 600)
                {
                    if (SendBack.Length > 1 && SendBack[26] == 0x01 && SendBack[27] == 0x02 && SendBack[28] == 0x00 && SendBack[29] == 0x00)
                    {
                        sendRerun = true;
                    }
                }
                else
                {
                    SendOrRev2 = 0;
                }
            }
            return sendRerun;
        }
        /// <summary>
        /// Set 值(bit)
        /// </summary>
        /// <param name="TypeInt">寄存器地址(CIOX.X)</param>
        /// <returns></returns>
        public bool SetPlcData(string TypeInt)
        {
            bool sendRerun = false;
            Byte[] SendmessAge = new Byte[35];
            if (GetType(TypeInt, 1) != 0x00 && Address != null && Address.Length > 1)
            {
                Byte[] getSendFunc = this.SetPLCBitvalue(PLCS, PCS, GetType(TypeInt, 1), Address[1], Address[0], AddressBit[0]);
                for (int i = 0; i < 35; i++)
                {
                    if (i < 34)
                    {
                        SendmessAge[i] = getSendFunc[i];
                    }
                    else
                    {
                        SendmessAge[i] = 0x00;
                    }
                }
                this.Send(SendmessAge);
                SendOrRev2 = 1;
                int Outtime = 0;
                while (SendOrRev2 != 0 & Outtime < 600)
                {
                    Thread.Sleep(2);
                    Outtime++;
                }
                if (Outtime < 600)
                {
                    if (SendBack.Length > 1 && SendBack[26] == 0x01 && SendBack[27] == 0x02 && SendBack[28] == 0x00 && SendBack[29] == 0x00)
                    {
                        sendRerun = true;
                    }
                }
            }
            return sendRerun;
        }
        /// <summary>
        /// Rest 值(bit)
        /// </summary>
        /// <param name="TypeInt">寄存器地址(CIOX.X)</param>
        /// <returns></returns>
        public bool RestPlcData(string TypeInt)
        {
            bool sendRerun = false;
            Byte[] SendmessAge = new Byte[35];
            if (GetType(TypeInt, 1) != 0x00 && Address != null && Address.Length > 1)
            {
                Byte[] getSendFunc = this.RstPLCBitvalue(PLCS, PCS, GetType(TypeInt, 1), Address[1], Address[0], AddressBit[0]);
                for (int i = 0; i < 35; i++)
                {
                    if (i < 34)
                    {
                        SendmessAge[i] = getSendFunc[i];
                    }
                    else
                    {
                        SendmessAge[i] = 0x00;
                    }
                }
                this.Send(SendmessAge);
                SendOrRev2 = 1;
                int Outtime = 0;
                while (SendOrRev2 != 0 & Outtime < 600)
                {
                    Thread.Sleep(2);
                    Outtime++;
                }
                if (Outtime < 600)
                {
                    if (SendBack.Length > 1 && SendBack[26] == 0x01 && SendBack[27] == 0x02 && SendBack[28] == 0x00 && SendBack[29] == 0x00)
                    {
                        sendRerun = true;
                    }
                }
            }
            return sendRerun;
        }
        Byte[] Address;
        Byte[] AddressBit;
        private Byte GetType(string TypeInt, int SizeofInt)
        {
            Byte ms = 0x00;
            if (TypeInt.IndexOf("D") == 0 | TypeInt.IndexOf("d") == 0)
            {
                if (SizeofInt == 0)
                {
                    ms = 0x82;
                    if (TypeInt.IndexOf(".") == -1)
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.Length - 1)));
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
                else
                {
                    ms = 0x02;
                    if (TypeInt.IndexOf(".") > 1)
                    {
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.IndexOf(".") - 1)));
                        AddressBit = BitConverter.GetBytes(int.Parse(TypeInt.Substring(TypeInt.IndexOf(".") + 1, TypeInt.Length - TypeInt.IndexOf(".") - 1)));
                    }
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
            }
            else if (TypeInt.IndexOf("CIO") == 0 | TypeInt.IndexOf("cio") == 0)
            {
                if (SizeofInt == 0)
                {
                    ms = 0xB0;
                    if (TypeInt.IndexOf(".") == -1)
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(3, TypeInt.Length - 3)));
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
                else
                {
                    ms = 0x30;
                    if (TypeInt.IndexOf(".") > 3)
                    {
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(3, TypeInt.IndexOf(".") - 3)));
                        AddressBit = BitConverter.GetBytes(int.Parse(TypeInt.Substring(TypeInt.IndexOf(".") + 1, TypeInt.Length - TypeInt.IndexOf(".") - 1)));
                    }
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
            }
            else if (TypeInt.IndexOf("W") == 0 | TypeInt.IndexOf("w") == 0)
            {
                if (SizeofInt == 0)
                {
                    ms = 0xB1;
                    if (TypeInt.IndexOf(".") == -1)
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.Length - 1)));
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
                else
                {
                    ms = 0x31;
                    if (TypeInt.IndexOf(".") > 1)
                    {
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.IndexOf(".") - 1)));
                        AddressBit = BitConverter.GetBytes(int.Parse(TypeInt.Substring(TypeInt.IndexOf(".") + 1, TypeInt.Length - TypeInt.IndexOf(".") - 1)));
                    }
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
            }
            else if (TypeInt.IndexOf("H") == 0 | TypeInt.IndexOf("h") == 0)
            {
                if (SizeofInt == 0)
                {
                    ms = 0xB2;
                    if (TypeInt.IndexOf(".") == -1)
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.Length - 1)));
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
                else
                {
                    ms = 0x32;
                    if (TypeInt.IndexOf(".") > 1)
                    {
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.IndexOf(".") - 1)));
                        AddressBit = BitConverter.GetBytes(int.Parse(TypeInt.Substring(TypeInt.IndexOf(".") + 1, TypeInt.Length - TypeInt.IndexOf(".") - 1)));
                    }
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
            }
            else if (TypeInt.IndexOf("A") == 0 | TypeInt.IndexOf("a") == 0)
            {
                if (SizeofInt == 0)
                {
                    ms = 0xB3;
                    if (TypeInt.IndexOf(".") == -1)
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.Length - 1)));
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
                else
                {
                    ms = 0x33;
                    if (TypeInt.IndexOf(".") > 1)
                    {
                        Address = BitConverter.GetBytes(int.Parse(TypeInt.Substring(1, TypeInt.IndexOf(".") - 1)));
                        AddressBit = BitConverter.GetBytes(int.Parse(TypeInt.Substring(TypeInt.IndexOf(".") + 1, TypeInt.Length - TypeInt.IndexOf(".") - 1)));
                    }
                    else
                    {
                        Address = new Byte[1];
                        AddressBit = new Byte[1];
                    }
                }
            }
            return ms;
        }
        /// <summary>
        /// Get PLC DA1 SNA1
        /// </summary>
        /// <returns></returns>
        private Byte[] GetPCAdress()
        {
            Byte[] ReturnGetPC = new Byte[20];
            ReturnGetPC[0] = 0x46;//f
            ReturnGetPC[1] = 0x49;//i
            ReturnGetPC[2] = 0x4E;//n
            ReturnGetPC[3] = 0x53;//s
            ReturnGetPC[4] = 0x00;
            ReturnGetPC[5] = 0x00;
            ReturnGetPC[6] = 0x00;
            ReturnGetPC[7] = 0x0C;
            ReturnGetPC[8] = 0x00;
            ReturnGetPC[9] = 0x00;
            ReturnGetPC[10] = 0x00;
            ReturnGetPC[11] = 0x00;
            ReturnGetPC[12] = 0x00;
            ReturnGetPC[13] = 0x00;
            ReturnGetPC[14] = 0x00;
            ReturnGetPC[15] = 0x00;
            ReturnGetPC[16] = 0x00;
            ReturnGetPC[17] = 0x00;
            ReturnGetPC[18] = 0x00;
            ReturnGetPC[19] = 0x00;
            return ReturnGetPC;
        }
        /// <summary>
        /// Read Word
        /// </summary>
        /// <param name="DA1"></param>
        /// <param name="SA1"></param>
        /// <param name="DataValue"></param>
        /// <param name="Address1"></param>
        /// <param name="Address2"></param>
        /// <param name="Address3"></param>
        /// <param name="DataNum1"></param>
        /// <param name="DataNum2"></param>
        /// <returns></returns>
        private Byte[] GetPLCvalue(Byte DA1, Byte SA1, Byte DataValue, Byte Address1, Byte Address2, Byte Address3, Byte DataNum1, Byte DataNum2)
        {
            Byte[] SendFun = new Byte[34];
            SendFun[0] = 0x46;
            SendFun[1] = 0x49;
            SendFun[2] = 0x4E;
            SendFun[3] = 0x53;
            SendFun[4] = 0x00;
            SendFun[5] = 0x00;
            SendFun[6] = 0x00;
            SendFun[7] = 0x1A;
            SendFun[8] = 0x00;
            SendFun[9] = 0x00;
            SendFun[10] = 0x00;
            SendFun[11] = 0x02;
            SendFun[12] = 0x00;
            SendFun[13] = 0x00;
            SendFun[14] = 0x00;
            SendFun[15] = 0x00;
            SendFun[16] = 0x80;
            SendFun[17] = 0x00;
            SendFun[18] = 0x02;
            SendFun[19] = 0x00;
            SendFun[20] = DA1;
            SendFun[21] = 0x00;
            SendFun[22] = 0x00;
            SendFun[23] = SA1;
            SendFun[24] = 0x00;
            SendFun[25] = 0x01;
            SendFun[26] = 0x01;
            SendFun[27] = 0x01;
            SendFun[28] = DataValue;
            SendFun[29] = Address1;
            SendFun[30] = Address2;
            SendFun[31] = Address3;
            SendFun[32] = DataNum1;
            SendFun[33] = DataNum2;
            return SendFun;
        }
        /// <summary>
        /// Write Word
        /// </summary>
        /// <param name="DA1"></param>
        /// <param name="SA1"></param>
        /// <param name="DataValue"></param>
        /// <param name="Address1"></param>
        /// <param name="Address2"></param>
        /// <param name="Address3"></param>
        /// <param name="DataNum1"></param>
        /// <param name="DataNum2"></param>
        /// <returns></returns>
        private Byte[] SetPLCvalue(Byte DA1, Byte SA1, Byte DataValue, Byte Address1, Byte Address2, Byte Address3, Byte DataNum1, Byte DataNum2, int num)
        {
            Byte[] SendFun = new Byte[34];
            SendFun[0] = 0x46;
            SendFun[1] = 0x49;
            SendFun[2] = 0x4E;
            SendFun[3] = 0x53;
            SendFun[4] = 0x00;
            SendFun[5] = 0x00;
            SendFun[6] = 0x00;
            SendFun[7] = (byte)(26 + 2 * num);
            SendFun[8] = 0x00;
            SendFun[9] = 0x00;
            SendFun[10] = 0x00;
            SendFun[11] = 0x02;
            SendFun[12] = 0x00;
            SendFun[13] = 0x00;
            SendFun[14] = 0x00;
            SendFun[15] = 0x00;
            SendFun[16] = 0x80;
            SendFun[17] = 0x00;
            SendFun[18] = 0x02;
            SendFun[19] = 0x00;
            SendFun[20] = DA1;
            SendFun[21] = 0x00;
            SendFun[22] = 0x00;
            SendFun[23] = SA1;
            SendFun[24] = 0x00;
            SendFun[25] = 0x01;
            SendFun[26] = 0x01;
            SendFun[27] = 0x02;
            SendFun[28] = DataValue;
            SendFun[29] = Address1;
            SendFun[30] = Address2;
            SendFun[31] = Address3;
            SendFun[32] = DataNum1;
            SendFun[33] = DataNum2;
            return SendFun;
        }
        /// <summary>
        /// Read Bit
        /// /// </summary>
        /// <param name="DA1"></param>
        /// <param name="SA1"></param>
        /// <param name="DataValue"></param>
        /// <param name="Address1"></param>
        /// <param name="Address2"></param>
        /// <param name="Address3"></param>
        /// <param name="DataNum1"></param>
        /// <param name="DataNum2"></param>
        /// <returns></returns>
        private Byte[] GetPLCvalue(Byte DA1, Byte SA1, Byte DataValue, Byte Address1, Byte Address2, Byte Address3)
        {
            Byte[] SendFun = new Byte[34];
            SendFun[0] = 0x46;
            SendFun[1] = 0x49;
            SendFun[2] = 0x4E;
            SendFun[3] = 0x53;
            SendFun[4] = 0x00;
            SendFun[5] = 0x00;
            SendFun[6] = 0x00;
            SendFun[7] = 0x1A;
            SendFun[8] = 0x00;
            SendFun[9] = 0x00;
            SendFun[10] = 0x00;
            SendFun[11] = 0x02;
            SendFun[12] = 0x00;
            SendFun[13] = 0x00;
            SendFun[14] = 0x00;
            SendFun[15] = 0x00;
            SendFun[16] = 0x80;
            SendFun[17] = 0x00;
            SendFun[18] = 0x02;
            SendFun[19] = 0x00;
            SendFun[20] = DA1;
            SendFun[21] = 0x00;
            SendFun[22] = 0x00;
            SendFun[23] = SA1;
            SendFun[24] = 0x00;
            SendFun[25] = 0x01;
            SendFun[26] = 0x01;
            SendFun[27] = 0x01;
            SendFun[28] = DataValue;
            SendFun[29] = Address1;
            SendFun[30] = Address2;
            SendFun[31] = Address3;
            SendFun[32] = 0x00;
            SendFun[33] = 0x01;
            return SendFun;
        }
        /// <summary>
        /// Write Bit
        /// </summary>
        /// <param name="DA1"></param>
        /// <param name="SA1"></param>
        /// <param name="DataValue"></param>
        /// <param name="Address1"></param>
        /// <param name="Address2"></param>
        /// <param name="Address3"></param>
        /// <returns></returns>
        private Byte[] WritePLCBitvalue(Byte DA1, Byte SA1, Byte DataValue, Byte Address1, Byte Address2, Byte Address3)
        {
            Byte[] SendFun = new Byte[32];
            SendFun[0] = 0x46;
            SendFun[1] = 0x49;
            SendFun[2] = 0x4E;
            SendFun[3] = 0x53;
            SendFun[4] = 0x00;
            SendFun[5] = 0x00;
            SendFun[6] = 0x00;
            SendFun[7] = 0x1B;
            SendFun[8] = 0x00;
            SendFun[9] = 0x00;
            SendFun[10] = 0x00;
            SendFun[11] = 0x02;
            SendFun[12] = 0x00;
            SendFun[13] = 0x00;
            SendFun[14] = 0x00;
            SendFun[15] = 0x00;
            SendFun[16] = 0x80;
            SendFun[17] = 0x00;
            SendFun[18] = 0x02;
            SendFun[19] = 0x00;
            SendFun[20] = DA1;
            SendFun[21] = 0x00;
            SendFun[22] = 0x00;
            SendFun[23] = SA1;
            SendFun[24] = 0x00;
            SendFun[25] = 0x01;
            SendFun[26] = 0x01;
            SendFun[27] = 0x02;
            SendFun[28] = DataValue;
            SendFun[29] = Address1;
            SendFun[30] = Address2;
            SendFun[31] = Address3;
            return SendFun;
        }
        /// <summary>
        /// Set Bit
        /// </summary>
        /// <param name="DA1"></param>
        /// <param name="SA1"></param>
        /// <param name="DataValue"></param>
        /// <param name="Address1"></param>
        /// <param name="Address2"></param>
        /// <param name="Address3"></param>
        /// <returns></returns>
        private Byte[] SetPLCBitvalue(Byte DA1, Byte SA1, Byte DataValue, Byte Address1, Byte Address2, Byte Address3)
        {
            Byte[] SendFun = new Byte[35];
            SendFun[0] = 0x46;
            SendFun[1] = 0x49;
            SendFun[2] = 0x4E;
            SendFun[3] = 0x53;
            SendFun[4] = 0x00;
            SendFun[5] = 0x00;
            SendFun[6] = 0x00;
            SendFun[7] = 0x1B;
            SendFun[8] = 0x00;
            SendFun[9] = 0x00;
            SendFun[10] = 0x00;
            SendFun[11] = 0x02;
            SendFun[12] = 0x00;
            SendFun[13] = 0x00;
            SendFun[14] = 0x00;
            SendFun[15] = 0x00;
            SendFun[16] = 0x80;
            SendFun[17] = 0x00;
            SendFun[18] = 0x02;
            SendFun[19] = 0x00;
            SendFun[20] = DA1;
            SendFun[21] = 0x00;
            SendFun[22] = 0x00;
            SendFun[23] = SA1;
            SendFun[24] = 0x00;
            SendFun[25] = 0x01;
            SendFun[26] = 0x23;
            SendFun[27] = 0x01;
            SendFun[28] = 0x01;
            SendFun[29] = 0x00;
            SendFun[30] = 0x01;
            SendFun[31] = DataValue;
            SendFun[32] = Address1;
            SendFun[33] = Address2;
            SendFun[34] = Address3;
            return SendFun;
        }
        /// <summary>
        /// RST Bit
        /// </summary>
        /// <param name="DA1"></param>
        /// <param name="SA1"></param>
        /// <param name="DataValue"></param>
        /// <param name="Address1"></param>
        /// <param name="Address2"></param>
        /// <param name="Address3"></param>
        /// <returns></returns>
        private Byte[] RstPLCBitvalue(Byte DA1, Byte SA1, Byte DataValue, Byte Address1, Byte Address2, Byte Address3)
        {
            Byte[] SendFun = new Byte[35];
            SendFun[0] = 0x46;
            SendFun[1] = 0x49;
            SendFun[2] = 0x4E;
            SendFun[3] = 0x53;
            SendFun[4] = 0x00;
            SendFun[5] = 0x00;
            SendFun[6] = 0x00;
            SendFun[7] = 0x1B;
            SendFun[8] = 0x00;
            SendFun[9] = 0x00;
            SendFun[10] = 0x00;
            SendFun[11] = 0x02;
            SendFun[12] = 0x00;
            SendFun[13] = 0x00;
            SendFun[14] = 0x00;
            SendFun[15] = 0x00;
            SendFun[16] = 0x80;
            SendFun[17] = 0x00;
            SendFun[18] = 0x02;
            SendFun[19] = 0x00;
            SendFun[20] = DA1;
            SendFun[21] = 0x00;
            SendFun[22] = 0x00;
            SendFun[23] = SA1;
            SendFun[24] = 0x00;
            SendFun[25] = 0x01;
            SendFun[26] = 0x23;
            SendFun[27] = 0x01;
            SendFun[28] = 0x01;
            SendFun[29] = 0x00;
            SendFun[30] = 0x00;
            SendFun[31] = DataValue;
            SendFun[32] = Address1;
            SendFun[33] = Address2;
            SendFun[34] = Address3;
            return SendFun;
        }
        #endregion
    }
}
