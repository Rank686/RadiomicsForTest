namespace Test
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnImg = new Button();
            tbImgPath = new TextBox();
            label1 = new Label();
            btnMask = new Button();
            tbMaskPath = new TextBox();
            label2 = new Label();
            btnStart = new Button();
            label3 = new Label();
            openFileDialog1 = new OpenFileDialog();
            dataGridView1 = new DataGridView();
            cbGLCM = new CheckBox();
            txtBinWidth = new TextBox();
            label4 = new Label();
            txtBinCount = new TextBox();
            cbFixedNumber = new CheckBox();
            cbRange = new CheckBox();
            txtRangeMin = new TextBox();
            label6 = new Label();
            label7 = new Label();
            txtRangeMax = new TextBox();
            cbNormalize = new CheckBox();
            txtScale = new TextBox();
            label5 = new Label();
            label9 = new Label();
            cbResample = new CheckBox();
            txtResample = new TextBox();
            cboFilterMode = new ComboBox();
            cbFirstOrder = new CheckBox();
            btnBatch = new Button();
            btn3D = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // btnImg
            // 
            btnImg.Location = new Point(828, 12);
            btnImg.Name = "btnImg";
            btnImg.Size = new Size(94, 29);
            btnImg.TabIndex = 0;
            btnImg.Text = "浏览...";
            btnImg.UseVisualStyleBackColor = true;
            btnImg.Click += btnImg_Click;
            // 
            // tbImgPath
            // 
            tbImgPath.Location = new Point(229, 12);
            tbImgPath.Name = "tbImgPath";
            tbImgPath.ReadOnly = true;
            tbImgPath.Size = new Size(581, 27);
            tbImgPath.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(110, 16);
            label1.Name = "label1";
            label1.Size = new Size(100, 20);
            label1.TabIndex = 2;
            label1.Text = "图像dicom：";
            // 
            // btnMask
            // 
            btnMask.Location = new Point(828, 55);
            btnMask.Name = "btnMask";
            btnMask.Size = new Size(94, 29);
            btnMask.TabIndex = 0;
            btnMask.Text = "浏览...";
            btnMask.UseVisualStyleBackColor = true;
            btnMask.Click += btnMask_Click;
            // 
            // tbMaskPath
            // 
            tbMaskPath.Location = new Point(229, 55);
            tbMaskPath.Name = "tbMaskPath";
            tbMaskPath.ReadOnly = true;
            tbMaskPath.Size = new Size(581, 27);
            tbMaskPath.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(112, 59);
            label2.Name = "label2";
            label2.Size = new Size(100, 20);
            label2.TabIndex = 2;
            label2.Text = "ROI dicom：";
            // 
            // btnStart
            // 
            btnStart.Location = new Point(452, 287);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(94, 29);
            btnStart.TabIndex = 0;
            btnStart.Text = "开始计算";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnGLCM_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(99, 330);
            label3.Name = "label3";
            label3.Size = new Size(84, 20);
            label3.TabIndex = 2;
            label3.Text = "计算结果：";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "dicom文件|*.dcm|所有文件|*.*";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(229, 330);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.RowTemplate.Height = 29;
            dataGridView1.Size = new Size(694, 468);
            dataGridView1.TabIndex = 3;
            // 
            // cbGLCM
            // 
            cbGLCM.AutoSize = true;
            cbGLCM.Checked = true;
            cbGLCM.CheckState = CheckState.Checked;
            cbGLCM.Location = new Point(98, 287);
            cbGLCM.Name = "cbGLCM";
            cbGLCM.Size = new Size(75, 24);
            cbGLCM.TabIndex = 4;
            cbGLCM.Text = "GLCM";
            cbGLCM.UseVisualStyleBackColor = true;
            // 
            // txtBinWidth
            // 
            txtBinWidth.Location = new Point(229, 101);
            txtBinWidth.Name = "txtBinWidth";
            txtBinWidth.Size = new Size(125, 27);
            txtBinWidth.TabIndex = 5;
            txtBinWidth.Text = "25";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(98, 104);
            label4.Name = "label4";
            label4.Size = new Size(129, 20);
            label4.TabIndex = 2;
            label4.Text = "直方图区间大小：";
            // 
            // txtBinCount
            // 
            txtBinCount.Enabled = false;
            txtBinCount.Location = new Point(541, 102);
            txtBinCount.Name = "txtBinCount";
            txtBinCount.Size = new Size(125, 27);
            txtBinCount.TabIndex = 5;
            txtBinCount.Text = "44";
            // 
            // cbFixedNumber
            // 
            cbFixedNumber.AutoSize = true;
            cbFixedNumber.Location = new Point(410, 104);
            cbFixedNumber.Name = "cbFixedNumber";
            cbFixedNumber.Size = new Size(136, 24);
            cbFixedNumber.TabIndex = 4;
            cbFixedNumber.Text = "固定区间个数：";
            cbFixedNumber.UseVisualStyleBackColor = true;
            cbFixedNumber.CheckedChanged += cbFixedNumber_CheckedChanged;
            // 
            // cbRange
            // 
            cbRange.AutoSize = true;
            cbRange.Location = new Point(115, 225);
            cbRange.Name = "cbRange";
            cbRange.Size = new Size(91, 24);
            cbRange.TabIndex = 4;
            cbRange.Text = "范围过滤";
            cbRange.UseVisualStyleBackColor = true;
            cbRange.CheckedChanged += cbRange_CheckedChanged;
            // 
            // txtRangeMin
            // 
            txtRangeMin.Location = new Point(452, 223);
            txtRangeMin.Name = "txtRangeMin";
            txtRangeMin.Size = new Size(125, 27);
            txtRangeMin.TabIndex = 5;
            txtRangeMin.Text = "-1000";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(377, 226);
            label6.Name = "label6";
            label6.Size = new Size(69, 20);
            label6.TabIndex = 2;
            label6.Text = "值范围：";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(582, 227);
            label7.Name = "label7";
            label7.Size = new Size(41, 20);
            label7.TabIndex = 2;
            label7.Text = "——";
            // 
            // txtRangeMax
            // 
            txtRangeMax.Location = new Point(636, 224);
            txtRangeMax.Name = "txtRangeMax";
            txtRangeMax.Size = new Size(125, 27);
            txtRangeMax.TabIndex = 5;
            txtRangeMax.Text = "1000";
            // 
            // cbNormalize
            // 
            cbNormalize.AutoSize = true;
            cbNormalize.Location = new Point(115, 142);
            cbNormalize.Name = "cbNormalize";
            cbNormalize.Size = new Size(76, 24);
            cbNormalize.TabIndex = 4;
            cbNormalize.Text = "标准化";
            cbNormalize.UseVisualStyleBackColor = true;
            cbNormalize.CheckedChanged += cbNormalize_CheckedChanged;
            // 
            // txtScale
            // 
            txtScale.Enabled = false;
            txtScale.Location = new Point(346, 143);
            txtScale.Name = "txtScale";
            txtScale.Size = new Size(125, 27);
            txtScale.TabIndex = 5;
            txtScale.Text = "1";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(228, 144);
            label5.Name = "label5";
            label5.Size = new Size(114, 20);
            label5.TabIndex = 2;
            label5.Text = "缩放范围系数：";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(229, 185);
            label9.Name = "label9";
            label9.Size = new Size(99, 20);
            label9.TabIndex = 2;
            label9.Text = "重采样间隔：";
            // 
            // cbResample
            // 
            cbResample.AutoSize = true;
            cbResample.Location = new Point(113, 183);
            cbResample.Name = "cbResample";
            cbResample.Size = new Size(106, 24);
            cbResample.TabIndex = 4;
            cbResample.Text = "数据重采样";
            cbResample.UseVisualStyleBackColor = true;
            cbResample.CheckedChanged += cResample_CheckedChanged;
            // 
            // txtResample
            // 
            txtResample.Enabled = false;
            txtResample.Location = new Point(347, 181);
            txtResample.Name = "txtResample";
            txtResample.Size = new Size(125, 27);
            txtResample.TabIndex = 5;
            txtResample.Text = "1.0\\1.0\\1.0";
            // 
            // cboFilterMode
            // 
            cboFilterMode.Enabled = false;
            cboFilterMode.FormattingEnabled = true;
            cboFilterMode.Items.AddRange(new object[] { "绝对值", "相对最大值", "标准差倍数" });
            cboFilterMode.Location = new Point(234, 221);
            cboFilterMode.Name = "cboFilterMode";
            cboFilterMode.Size = new Size(125, 28);
            cboFilterMode.TabIndex = 6;
            // 
            // cbFirstOrder
            // 
            cbFirstOrder.AutoSize = true;
            cbFirstOrder.Checked = true;
            cbFirstOrder.CheckState = CheckState.Checked;
            cbFirstOrder.Location = new Point(179, 287);
            cbFirstOrder.Name = "cbFirstOrder";
            cbFirstOrder.Size = new Size(91, 24);
            cbFirstOrder.TabIndex = 4;
            cbFirstOrder.Text = "一阶特征";
            cbFirstOrder.UseVisualStyleBackColor = true;
            // 
            // btnBatch
            // 
            btnBatch.Location = new Point(1051, 221);
            btnBatch.Name = "btnBatch";
            btnBatch.Size = new Size(94, 29);
            btnBatch.TabIndex = 0;
            btnBatch.Text = "批量计算";
            btnBatch.UseVisualStyleBackColor = true;
            btnBatch.Click += btnBatch_Click;
            // 
            // btn3D
            // 
            btn3D.Location = new Point(1051, 287);
            btn3D.Name = "btn3D";
            btn3D.Size = new Size(94, 29);
            btn3D.TabIndex = 0;
            btn3D.Text = "三维计算";
            btn3D.UseVisualStyleBackColor = true;
            btn3D.Click += btn3D_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1275, 750);
            Controls.Add(cboFilterMode);
            Controls.Add(txtResample);
            Controls.Add(txtScale);
            Controls.Add(txtBinCount);
            Controls.Add(txtRangeMax);
            Controls.Add(txtRangeMin);
            Controls.Add(cbResample);
            Controls.Add(txtBinWidth);
            Controls.Add(cbNormalize);
            Controls.Add(cbRange);
            Controls.Add(cbFixedNumber);
            Controls.Add(cbFirstOrder);
            Controls.Add(cbGLCM);
            Controls.Add(dataGridView1);
            Controls.Add(label7);
            Controls.Add(label9);
            Controls.Add(label3);
            Controls.Add(label5);
            Controls.Add(label6);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(tbMaskPath);
            Controls.Add(btn3D);
            Controls.Add(btnBatch);
            Controls.Add(btnStart);
            Controls.Add(btnMask);
            Controls.Add(tbImgPath);
            Controls.Add(btnImg);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "FrmMain";
            Text = "测试程序";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnImg;
        private TextBox tbImgPath;
        private Label label1;
        private Button btnMask;
        private TextBox tbMaskPath;
        private Label label2;
        private Button btnStart;
        private Label label3;
        private OpenFileDialog openFileDialog1;
        private DataGridView dataGridView1;
        private CheckBox cbGLCM;
        private TextBox txtBinWidth;
        private Label label4;
        private TextBox txtBinCount;
        private CheckBox cbFixedNumber;
        private CheckBox cbRange;
        private TextBox txtRangeMin;
        private Label label6;
        private Label label7;
        private TextBox txtRangeMax;
        private CheckBox cbNormalize;
        private TextBox txtScale;
        private Label label5;
        private Label label9;
        private CheckBox cbResample;
        private TextBox txtResample;
        private ComboBox cboFilterMode;
        private CheckBox cbFirstOrder;
        private Button btnBatch;
        private Button btn3D;
    }
}
