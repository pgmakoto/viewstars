
namespace viewstars
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            ucPgmac.Cel cel1 = new ucPgmac.Cel();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ucDay_Hour = new ucPgmac.ucSwitch();
            this.tenqu1 = new ucPgmac.Tenqu();
            this.SuspendLayout();
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hScrollBar1.Location = new System.Drawing.Point(72, 9);
            this.hScrollBar1.Maximum = 720;
            this.hScrollBar1.Minimum = -720;
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(572, 17);
            this.hScrollBar1.TabIndex = 1;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.ForeColor = System.Drawing.Color.Orange;
            this.label1.Location = new System.Drawing.Point(660, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "-";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            // 
            // ucDay_Hour
            // 
            this.ucDay_Hour.Checked = false;
            this.ucDay_Hour.ColorOff = System.Drawing.SystemColors.Control;
            this.ucDay_Hour.ColorOn = System.Drawing.Color.Empty;
            this.ucDay_Hour.Location = new System.Drawing.Point(12, 2);
            this.ucDay_Hour.Name = "ucDay_Hour";
            this.ucDay_Hour.Size = new System.Drawing.Size(48, 24);
            this.ucDay_Hour.TabIndex = 3;
            this.ucDay_Hour.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucDay_Hour_CheckedChanged);
            this.ucDay_Hour.Load += new System.EventHandler(this.ucDay_Hour_Load);
            // 
            // tenqu1
            // 
            this.tenqu1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tenqu1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(48)))));
            cel1.Latitude = 35.345500117631445D;
            cel1.Longitude = -137.15796326512671D;
            this.tenqu1.cel = cel1;
            this.tenqu1.Location = new System.Drawing.Point(0, 29);
            this.tenqu1.Name = "tenqu1";
            this.tenqu1.Size = new System.Drawing.Size(916, 713);
            this.tenqu1.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(914, 741);
            this.Controls.Add(this.tenqu1);
            this.Controls.Add(this.ucDay_Hour);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.hScrollBar1);
            this.Name = "Form1";
            this.Text = "ViewStar";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Timer timer1;
        private ucPgmac.ucSwitch ucDay_Hour;
        private ucPgmac.Tenqu tenqu1;
    }
}

