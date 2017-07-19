using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;
using MsgManager;

namespace sqlReport
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }
        ~FormMain()
        {
            Dispose_SerialPort();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Init_SerialPort();
        }


        #region 串口初始化设置
        ComManager comManager = new ComManager();

        SerialPort serial = new SerialPort();
        
        void Dispose_SerialPort()
        {
            if (serial.IsOpen)
                serial.Close();
            serial.Dispose();
        }
        void Init_SerialPort()
        {
            string[] ports = SerialPort.GetPortNames();
            this.comboBox_port.Items.AddRange(ports);
            this.comboBox_port.SelectedIndex = 0;

            string[] bauds = { "9600", "115200" };
            this.comboBox_baudrate.Items.AddRange(bauds);
            this.comboBox_baudrate.SelectedIndex = 0;

            string[] paritys = { "None", "Even", "Odd" };
            this.comboBox_parity.Items.AddRange(paritys);
            this.comboBox_parity.SelectedIndex = 0;

            string[] bits = { "8", "7" };
            this.comboBox_bits.Items.AddRange(bits);
            this.comboBox_bits.SelectedIndex = 0;

            string[] stops = { "1", "1.5", "2" };
            this.comboBox_stops.Items.AddRange(stops);
            this.comboBox_stops.SelectedIndex = 0;

            serial.DataReceived+=comManager.com_DataReceived;
        }
        private void comboBox_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            serial.PortName = cb.Text;
        }

        private void comboBox_baudrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            serial.BaudRate = Convert.ToInt32(cb.Text);
        }

        private void comboBox_parity_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            switch(cb.Text)
            {
                case "None":
                    serial.Parity = Parity.None;
                    break;
                case "Even":
                    serial.Parity = Parity.Even;
                    break;
                case "Odd":
                    serial.Parity = Parity.Odd;
                    break;
                default:
                    serial.Parity = Parity.None;
                    break;
            }
        }

        private void comboBox_bits_SelectedIndexChanged(object sender, EventArgs e)
        {

            ComboBox cb = sender as ComboBox;
            serial.DataBits = Convert.ToInt32(cb.Text);
        }

        private void comboBox_stops_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            switch(cb.Text)
            {
                case "1":
                    serial.StopBits = StopBits.One;
                    break;
                case "1.5":
                    serial.StopBits = StopBits.OnePointFive;
                    break;
                case "2":
                    serial.StopBits = StopBits.Two;
                    break;
                default:
                    serial.StopBits = StopBits.One;
                    break;
            }
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            try
            {
                serial.Open();
                this.comboBox_baudrate.Enabled = false;
                this.comboBox_bits.Enabled = false;
                this.comboBox_parity.Enabled = false;
                this.comboBox_port.Enabled = false;
                this.comboBox_stops.Enabled = false;
            }
            catch
            {

            }
        }
        #endregion

    }
}
