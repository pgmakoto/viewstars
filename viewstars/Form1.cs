using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ucPgmac;

namespace viewstars
{
    public partial class Form1 : Form
    {
        public double ScopeAltitude = 0;
        public double ScopeAzimuth = 0;

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // StReader reader = new StReader();

            //////////////////////
            if (true)
            {
                //https://dobon.net/vb/dotnet/system/registrykey.html
                //キー（HKEY_CURRENT_USER\Software\test\sub）を開く
                #region Registry
                /*
                Microsoft.Win32.RegistryKey regkey =
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\" + Application.ProductName + @"\sub");
                //上のコードでは、指定したキーが存在しないときは新しく作成される。
                //作成されないようにするには、次のようにする。
                //Microsoft.Win32.RegistryKey regkey =
                //    Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\test\sub", true);

                //レジストリに書き込み
                //文字列を書き込む（REG_SZで書き込まれる）
                regkey.SetValue("string", "StringValue");
                //整数（Int32）を書き込む（REG_DWORDで書き込まれる）
                regkey.SetValue("int", 100);
                //文字列配列を書き込む（REG_MULTI_SZで書き込まれる）
                string[] s = new string[] { "1", "2", "3" };
                regkey.SetValue("StringArray", s);
                //バイト配列を書き込む（REG_BINARYで書き込まれる）
                byte[] bs = new byte[] { 0, 1, 2 };
                regkey.SetValue("Bytes", bs);

                //閉じる
                regkey.Close();


                //キー（HKEY_CURRENT_USER\Software\test\sub）を読み取り専用で開く+
                Microsoft.Win32.RegistryKey regkey =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\" + Application.ProductName + @"\sub", false);
                //キーが存在しないときは null が返される
                if (regkey == null) return;

                //文字列を読み込む
                //指定した名前の値が存在しないときは null が返される
                string stringValue = (string)regkey.GetValue("string");
                //キーに値が存在しないときに指定した値を返すようにするには、次のようにする
                //（ここでは"default"を返す）
                //string stringValue = (string) regkey.GetValue("string", "default");

                //整数（REG_DWORD）を読み込む
                int intValue = (int)regkey.GetValue("int");
                //整数（REG_QWORD）を読み込む
                long longVal = (long)regkey.GetValue("QWord");
                //文字列配列を読み込む
                string[] strings = (string[])regkey.GetValue("StringArray");
                //バイト配列を読み込む
                byte[] bytes = (byte[])regkey.GetValue("Bytes");

                */

                //reg
                //          textBox1.Text = ReadSetting("DataPath");
                //          tenqu1.readDatas(textBox1.Text);
                //フォルダの存在確認
                //      if (File.Exists(textBox1.Text))
                //      {
                //          // filePathのファイルは存在する
                //      }
                //      else
                //      {
                //          // filePathのファイルは存在しない
                //      }
                #endregion

            }

            //////////////////////
        }

        #region Graphic
        /// <summary>
        /// 文字列の描画、回転、基準位置指定
        /// </summary>
        /// <param name="g">描画先のGraphicsオブジェクト</param>
        /// <param name="s">描画する文字列</param>
        /// <param name="f">文字のフォント</param>
        /// <param name="brush">描画用ブラシ</param>
        /// <param name="x">基準位置のX座標</param>
        /// <param name="y">基準位置のY座標</param>
        /// <param name="deg">回転角度（度数、時計周りが正）</param>
        /// <param name="format">基準位置をStringFormatクラスオブジェクトで指定します</param>
        public void DrawString(Graphics g, string s, Font f, Brush brush, float x, float y, float deg, StringFormat format)
        {
            using (var pathText = new System.Drawing.Drawing2D.GraphicsPath())  // パスの作成
            using (var mat = new System.Drawing.Drawing2D.Matrix())             // アフィン変換行列
            {
                // 描画用Format
                var formatTemp = (StringFormat)format.Clone();
                formatTemp.Alignment = StringAlignment.Near;        // 左寄せに修正
                formatTemp.LineAlignment = StringAlignment.Near;    // 上寄せに修正

                // 文字列の描画
                pathText.AddString(
                        s,
                        f.FontFamily,
                        (int)f.Style,
                        f.SizeInPoints,
                        new PointF(0, 0),
                        format);
                formatTemp.Dispose();

                // 文字の領域取得
                var rect = pathText.GetBounds();

                // 回転中心のX座標
                float px;
                switch (format.Alignment)
                {
                    case StringAlignment.Near:
                        px = rect.Left;
                        break;
                    case StringAlignment.Center:
                        px = rect.Left + rect.Width / 2f;
                        break;
                    case StringAlignment.Far:
                        px = rect.Right;
                        break;
                    default:
                        px = 0;
                        break;
                }
                // 回転中心のY座標
                float py;
                switch (format.LineAlignment)
                {
                    case StringAlignment.Near:
                        py = rect.Top;
                        break;
                    case StringAlignment.Center:
                        py = rect.Top + rect.Height / 2f;
                        break;
                    case StringAlignment.Far:
                        py = rect.Bottom;
                        break;
                    default:
                        py = 0;
                        break;
                }

                // 文字の回転中心座標を原点へ移動
                mat.Translate(-px, -py, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 文字の回転
                mat.Rotate(deg, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 表示位置まで移動
                mat.Translate(x, y, System.Drawing.Drawing2D.MatrixOrder.Append);

                // パスをアフィン変換
                pathText.Transform(mat);

                // 描画
                g.FillPath(brush, pathText);
            }
        }

        #endregion


        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (ucDay_Hour.Checked)
            {
                //panel1_MouseMove(sender,new MouseEventArgs (MouseButtons.Left,0, MD.X, MD.Y,0));
                tenqu1.hourOffset = hScrollBar1.Value / 60.0;
                int s = (hScrollBar1.Value < 0) ? -1 : 1;
                int m = hScrollBar1.Value * s;
                int h = (int)hScrollBar1.Value / 60;
                m -= h * 60;

                label1.Text = h.ToString() + "h" + m.ToString() + "m";
            }
            else
            {
                int s = hScrollBar1.Value / 2;
                tenqu1.dateOffset = s;

                label1.Text = s.ToString() + "day";
            }
            tenqu1.tenqu_update();
        }

        private void label1_Click(object sender, EventArgs e)
        {

            hScrollBar1.Value = 0;
            hScrollBar1_Scroll(null, null);
            tenqu1.tenqu_update();
        }

        private void ucDay_Hour_Load(object sender, EventArgs e)
        {

        }

        private void ucDay_Hour_CheckedChanged(object sender, BoolEventArgs e)
        {
            if (ucDay_Hour.Checked)
            {
                hScrollBar1.Value = (int)(tenqu1.hourOffset * 60);
            }
            else
            {
                hScrollBar1.Value = (int)(tenqu1.dateOffset);
            }

            hScrollBar1_Scroll(null, null);


        }
    }
}
