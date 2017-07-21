using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;

namespace sqlReport
{
    /// <summary>
    /// DataGridView适配器
    /// 可以根据用户数据自动调节每一列的宽度
    /// </summary>
    class DataGridViewAdapter
    {
        string name; //
        DataGridView dgv = null;


        public string text
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public DataGridViewAdapter(DataGridView dgv,DataTable dt,int[] scaler)
        {      
            this.dgv = dgv;
            InitDataGridView(dt, scaler);
        }

        void InitDataGridView(DataTable dt,int[] scaler)
        {
            if (null == dgv)
                return;
            //先绑定数据
            dgv.DataSource = dt;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            int widthBase = 0;
            int sum = 0;
            foreach (int x in scaler)
            {
                sum += x;
            }
            widthBase = dgv.Width / (sum);
            int i = 0;
            foreach (DataColumn col in dt.Columns)
            {
                dgv.Columns[col.ColumnName].Width = widthBase * scaler[i++];
            }

        }

    }//CustomDataGridView

    /// <summary>
    /// DataGrid适配器
    /// 可以根据用户自数据动设置列宽度和名称
    /// </summary>
    class DataGridAdapter
    {
        DataGrid dg = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dg">DataGrid对象</param>
        /// <param name="dt">数据表</param>
        /// <param name="scaler">数据显示列比例（例如：1:2:1）</param>
        public DataGridAdapter(DataGrid dg,DataTable dt,int[] scaler)
        {
            this.dg = dg;
            InitDataGrid(dt, scaler);
        }
        /// <summary>
        /// 初始化DataGrid
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="scaler">数据显示列比例（例如：1:2:1）</param>
        void InitDataGrid(DataTable dt,int []scaler)
        {
            if (dg == null)
                return;

            DataGridTableStyle dts = new DataGridTableStyle();
            dts.MappingName = dt.TableName;

            dg.TableStyles.Add(dts);
            dg.TableStyles[0].GridColumnStyles.Clear();
            int widthBase = 1;
            if (scaler == null)
                widthBase = dg.Width / dt.Columns.Count;
            else
            {
                int sum = 0;
                foreach (int x in scaler)
                {
                    sum += x;
                }
                widthBase = dg.Width / (sum);
            }

            int i = 0;
            foreach (DataColumn col in dt.Columns)
            {
                DataGridTextBoxColumn tc = new DataGridTextBoxColumn();
                tc.HeaderText = col.ColumnName;
                if (scaler == null)
                    tc.Width = widthBase;
                else
                    tc.Width = widthBase * scaler[i++];
                tc.MappingName = col.ColumnName;

                dg.TableStyles[0].GridColumnStyles.Add(tc);
            }
            dg.DataSource = dt;

        }



        object objectLock = new object();
        event EventHandler UpdateDataGrid;
        public event EventHandler OnUpdateDataGrid
        {
            add
            {
                lock (objectLock)
                {
                    UpdateDataGrid += value;
                }
            }
            remove
            {
                lock (objectLock)
                {
                    UpdateDataGrid -= value;
                }
            }
        }
    }//CustomDataGrid
}
