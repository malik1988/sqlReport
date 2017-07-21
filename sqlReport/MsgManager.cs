using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using System.IO.Ports;

namespace MsgManager
{
    /// <summary>
    /// 消息格式
    /// </summary>
    class MsgStruct
    {
        //|包头     |长度   |messageID|设备ID   |功能   |消息内容   |校验|
        //|2字节    |1字节  |2字节    |2字节        |1字节  |           |1字节|
        //|0xAA55   |总长度 |         |             |       |可能为空   |
        //


        byte[] data = null;
        int len = 0;


        public byte[] msgId = new byte[2];
        public byte[] devId = new byte[2];
        public byte[] code = new byte[1];
        public byte[] msg = null;

        public MsgStruct(List<byte> buf, int len)
        {
            this.data = new byte[len];
            buf.CopyTo(0, this.data, 0, len);
            this.len = len;
        }


        public MsgStruct(byte[] msgId, byte[] devId, byte[] code, byte[] msg)
        {
            Array.Copy(msgId, this.msgId, msgId.Length);
            Array.Copy(devId, this.devId, devId.Length);
            Array.Copy(code, this.code, code.Length);
            if (null != msg)
            {
                this.msg = new byte[msg.Length];
                Array.Copy(msg, this.msg, msg.Length);
            }
        }

        /// <summary>
        /// 消息解析，分离出msgId,devId,msg等
        /// </summary>
        public void decode()
        {
            if (data == null || len < 3)
                return;

            int index = 0;
            index += 3; //直接跳过包头和长度，共3个字节

            //消息ID
            Array.Copy(data, index, msgId, 0, msgId.Length);
            index += msgId.Length;



            //设备ID
            Array.Copy(data, index, devId, 0, devId.Length);
            index += devId.Length;



            //功能码 1字节
            Array.Copy(data, index, code, 0, code.Length);
            index += code.Length;

            //可能没有消息内容
            if (len - index - 1 > 1) //最后一字节为校验
            {
                //消息内容 
                msg = new byte[len - index - 1];
                Array.Copy(data, index, msg, 0, msg.Length);
                index += msg.Length;
            }

        }
        /// <summary>
        /// 消息打包
        /// </summary>
        /// <returns></returns>
        public byte[] encode()
        {
            int len = 2 + 1 + 1; //包头+长度+校验，4字节

            len += msgId.Length;
            len += devId.Length;
            len += code.Length;
            if (null != msg)
                len += msg.Length;

            byte[] buf = new byte[len];
            int index = 0;
            buf[0] = 0xaa;
            buf[1] = 0x55;
            buf[2] = (byte)len;
            index += 3;
            Array.Copy(msgId, 0, buf, index, msgId.Length);
            index += msgId.Length;
            Array.Copy(devId, 0, buf, index, devId.Length);
            index += devId.Length;
            Array.Copy(code, 0, buf, index, code.Length);
            index += code.Length;
            if (null != msg)
            {
                Array.Copy(msg, 0, buf, index, msg.Length);
                index += msg.Length;
            }

            buf[index] = getCheckByte(buf, len);

            return buf;
        }
        /// <summary>
        /// 计算校验
        /// </summary>
        /// <param name="buf">数据</param>
        /// <param name="len">数据长度</param>
        /// <returns></returns>
        byte getCheckByte(byte[] buf, int len)
        {
            byte sum = 0;
            for (int i = 0; i < len - 1; i++)
            {
                sum ^= buf[i];
            }
            return sum;
        }
        /// <summary>
        /// 检查校验是否成功
        /// </summary>
        /// <returns>成功/失败</returns>
        public bool check()
        {
            if (data == null)
                return false;

            byte sum = getCheckByte(data, len);
            if (data[len - 1] == sum)
                return true;
            else
                return false;
        }

    }//class MsgStruct
    class ComManager
    {
        List<byte> buffer = new List<byte>(1024); //定义一块缓冲区

        const int MIN_LEN = 3;//最小数据长度，只有包头和数据长度
        const int MAX_LEN = 50; //最大长度

        //自身设备ID
        byte[] deviceId = new byte[2] { 0x00, 0x01 };



        SqlManager.SqlHelper sql = new SqlManager.SqlHelper();
        string sql_tableName = "Message";

        object objectLock = new object();
        public delegate void MsgStructEventHandler(object sender, MsgStructEventArgs e);
        event  MsgStructEventHandler PostDataUpdate;  //数据更新后事件处理
        public event MsgStructEventHandler OnPostDataUpdate
        {
            add
            {
                lock (objectLock)
                {
                    PostDataUpdate += value;
                }
            }
            remove
            {
                lock (objectLock) { PostDataUpdate -= value; }
            }
        }

        /// <summary>
        /// 数据接收处理
        /// </summary>
        /// <param name="sender">串口设备对象</param>
        /// <param name="e"></param>
        public void com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort com = sender as SerialPort;

            int bLen = com.BytesToRead;
            byte[] buf = new byte[bLen]; //接收缓存

            com.Read(buf, 0, bLen);
            buffer.AddRange(buf);

            while (buffer.Count > MIN_LEN)
            {
                if (buffer[0] == 0xaa && buffer[1] == 0x55)
                {
                    int len = buffer[2];

                    //获取数据长度不够直接退出
                    if (buffer.Count < len)
                        break;


                    MsgStruct msg = new MsgStruct(buffer, len);

                    if (!msg.check())
                    {//校验失败，重新重头开始，丢弃前包头（两个字节）
                        buffer.RemoveRange(0, 2);
                        continue;
                    }

                    comDataHandler(msg, com);

                    buffer.RemoveRange(0, len);


                }
                else
                {
                    buffer.RemoveAt(0);
                }
            }


        }

        public string bytesToHexString(byte[] inBytes)
        {
            string str = "";
            foreach (byte b in inBytes)
            {
                str += string.Format("{0:X2}", b);
            }
            return str;
        }
        /// <summary>
        /// byte[]转成int（byte[]长度不能超过4字节），低字节在后
        /// </summary>
        /// <param name="inBytes">需要转换的数据</param>
        /// <returns>返回数据</returns>
        int bytesToInt(byte[] inBytes)
        {
            int ret = 0;
            for (int i =0; i<inBytes.Length; i++)
            {
                ret <<= 8;
                ret += inBytes[i];
            }
            return ret;
        }
        /// <summary>
        /// 数据详细处理
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="com">串口设备</param>
        void comDataHandler(MsgStruct msg, SerialPort com)
        {

            #region 接收到的数据处理
            //解析数据
            msg.decode();
            byte code = msg.code[0];
            int strId = bytesToInt(msg.msgId);
            string strDev = bytesToHexString(msg.devId);
            string strCode = bytesToHexString(msg.code);
            string strMsg = bytesToHexString(msg.msg);
            string strTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            #endregion

            #region 回复数据定义
            bool needSend = false; //需要回复
            byte[] retMsgId = msg.msgId; //消息ID与接收到的一致
            byte[] retDevId = this.deviceId;
            byte[] retCode = new byte[1];
            byte[] retMsg = null;
            #endregion

            sql.open();
            switch (code)
            {
                case 0x01: //添加数据

                    //将消息添加到数据库中
                    string values = string.Format("'{0}','{1}','{2}','{3}','{4}'", strId, strDev, strCode, strMsg, strTime);
                    string sqlStr_insert = sql.insertFormat(sql_tableName, "id,devId,code,msg,time", values);
                    int ret=sql.ExecuteNonQuery(sqlStr_insert);

                    if(ret>0)
                    {//数据更新成功

                        //返回确认
                        needSend = true;
                        retCode[0] = 0x11; //确认码
                                           //retMsg为空
                    }

                    break;
                case 0x02: //查询数据
                    needSend = true;
                    string sqlStr_select = string.Format("select msg,time from {0} where id={1}", sql_tableName, strId);
                    System.Data.SqlClient.SqlDataReader reader = sql.ExecuteReader(sqlStr_select);
                    if(reader.Read())
                    {
                        //回复code=0x12,msg=数据
                        retCode[0] = 0x12;

                        string msg_send = reader[0].ToString();
                        string time_send = reader[1].ToString();

                        retMsg = System.Text.Encoding.ASCII.GetBytes(msg_send + time_send);
                    }
                    else
                    {
                        //回复code=0x22,msg=null
                        retCode[0] = 0x22;
                    }

                    reader.Close();


                    break;
                case 0x03: //查询结果确认

                    //删除msgId 对应数据（数据库删除）
                    string condition = string.Format("id={0}", strId);
                    string sqlStr_del = sql.deleteFormat(sql_tableName, condition);
                    sql.ExecuteNonQuery(sqlStr_del);
                    break;
                default:
                    break;
            }

            sql.close();

            if (needSend)
            {//回复数据
                MsgStruct response = new MsgStruct(retMsgId, retDevId, retCode, retMsg);
                byte[] data = response.encode();

                com.Write(data, 0, data.Length);
            }

            if(PostDataUpdate!=null)
            {
                PostDataUpdate(this, new MsgStructEventArgs(msg));
            }
        }

    }//ComManager

    class MsgStructEventArgs:EventArgs
    {
        MsgStruct message;
        public MsgStructEventArgs(MsgStruct msg)
        {
            this.message = msg;
        }

        public MsgStruct Msg
        {
            get { return this.message; }
        }

    }
 
}
