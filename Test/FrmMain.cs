using FellowOakDicom;
using Radiomics.Net;

namespace Test
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            cboFilterMode.SelectedIndex = 0;
        }

        private void btnImg_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbImgPath.Text = openFileDialog1.FileName;
            }
        }

        private void btnMask_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbMaskPath.Text = openFileDialog1.FileName;
            }
        }

        private void btnGLCM_Click(object sender, EventArgs e)
        {
            if ((tbImgPath.Text.Length == 0) || (tbMaskPath.Text.Length == 0))
            {
                MessageBox.Show("请先选择dicom文件");
                return;
            }
            DicomDataset dcmImg = DicomFile.Open(tbImgPath.Text).Dataset;
            DicomDataset dcmMask = DicomFile.Open(tbMaskPath.Text).Dataset;
            AddParams(dcmImg);
            //DateTime startTime = DateTime.Now;
            ResultMessage message = FeatureCalculator.Calclate(new List<DicomDataset>() { dcmImg, dcmMask });
            //TimeSpan timeSpan = DateTime.Now - startTime;
            //double consumeTime = timeSpan.TotalSeconds;

            List<FeatureResult> resultList = new List<FeatureResult>();
            if (message.IsSuccess)
            {
                if (cbGLCM.Checked)
                {
                    //获取glcm结果标签集合
                    List<DicomTag> glcmTags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0015);
                    foreach (DicomTag tag in glcmTags)
                    {
                        FeatureResult featureResult = new FeatureResult();
                        featureResult.FeatureName = tag.PrivateCreator.Creator;
                        featureResult.FeatureValue = dcmImg.GetValue<double>(tag, 0);
                        featureResult.FeatureGroup = "GLCM";
                        resultList.Add(featureResult);
                    }
                }
                if (cbFirstOrder.Checked)
                {
                    //获取glcm结果标签集合
                    List<DicomTag> tags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0017);
                    foreach (DicomTag tag in tags)
                    {
                        FeatureResult featureResult = new FeatureResult();
                        featureResult.FeatureName = tag.PrivateCreator.Creator;
                        featureResult.FeatureValue = dcmImg.GetValue<double>(tag, 0);
                        featureResult.FeatureGroup = "FirstOrder";
                        resultList.Add(featureResult);
                    }
                }
            }
            else
            {
                MessageBox.Show(message.ErrorMessage);
            }
            this.dataGridView1.DataSource = resultList;
            this.dataGridView1.Columns[0].Width = 130;
            this.dataGridView1.Columns[1].Width = 300;
            this.dataGridView1.Columns[2].Width = 500;

        }

        private void cbFixedNumber_CheckedChanged(object sender, EventArgs e)
        {
            this.txtBinWidth.Enabled = !cbFixedNumber.Checked;
            this.txtBinCount.Enabled = cbFixedNumber.Checked;
        }

        private void cbNormalize_CheckedChanged(object sender, EventArgs e)
        {
            this.txtScale.Enabled = cbNormalize.Checked;
        }

        private void cResample_CheckedChanged(object sender, EventArgs e)
        {
            this.txtResample.Enabled = cbResample.Checked;
        }

        private void cbRange_CheckedChanged(object sender, EventArgs e)
        {
            this.cboFilterMode.Enabled = cbRange.Checked;
        }

        private void btnBatch_Click(object sender, EventArgs e)
        {
            string imgFolder = @"D:\Image\";
            string maskFolder = @"D:\Mask\";
            try
            {
                string[] imgFiles = Directory.GetFiles(imgFolder);
                foreach (string imgFile in imgFiles)
                {
                    string fileName = imgFile.Substring(imgFile.LastIndexOf('.') + 1);
                    string maskFile = maskFolder + fileName;
                    if (!File.Exists(maskFile))
                    {
                        continue;
                    }
                    DicomDataset dcmImg = DicomFile.Open(imgFile).Dataset;
                    DicomDataset dcmMask = DicomFile.Open(maskFile).Dataset;
                    AddParams(dcmImg);
                    //DateTime startTime = DateTime.Now;
                    ResultMessage message = FeatureCalculator.Calclate(new List<DicomDataset>() { dcmImg, dcmMask });
                    //TimeSpan timeSpan = DateTime.Now - startTime;
                    //double consumeTime = timeSpan.TotalSeconds;

                    List<FeatureResult> resultList = new List<FeatureResult>();
                    if (message.IsSuccess)
                    {
                        if (cbGLCM.Checked)
                        {
                            //获取glcm结果标签集合
                            List<DicomTag> glcmTags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0015);
                            foreach (DicomTag tag in glcmTags)
                            {
                                FeatureResult featureResult = new FeatureResult();
                                featureResult.FeatureName = tag.PrivateCreator.Creator;
                                featureResult.FeatureValue = dcmImg.GetValue<double>(tag, 0);
                                featureResult.FeatureGroup = "GLCM";
                                resultList.Add(featureResult);
                            }
                        }
                        if (cbFirstOrder.Checked)
                        {
                            //获取glcm结果标签集合
                            List<DicomTag> tags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0017);
                            foreach (DicomTag tag in tags)
                            {
                                FeatureResult featureResult = new FeatureResult();
                                featureResult.FeatureName = tag.PrivateCreator.Creator;
                                featureResult.FeatureValue = dcmImg.GetValue<double>(tag, 0);
                                featureResult.FeatureGroup = "FirstOrder";
                                resultList.Add(featureResult);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(message.ErrorMessage);
                    }
                    MessageBox.Show("计算完成");
                }
            }
            catch
            {
            }
        }

        private void btn3D_Click(object sender, EventArgs e)
        {
            string imgFolder = @"F:\study\RadiomicsNet\TestData\image\";
            string maskFolder = @"F:\study\RadiomicsNet\TestData\mask\";
            List<DicomDataset> imgs = new List<DicomDataset>();
            List<DicomDataset> masks = new List<DicomDataset>();
            try
            {
                string[] imgFiles = Directory.GetFiles(imgFolder);
                foreach (string imgFile in imgFiles)
                {
                    string fileName = imgFile.Substring(imgFile.LastIndexOf('\\') + 1);
                    string maskFile = maskFolder + fileName;
                    if (!File.Exists(maskFile))
                    {
                        continue;
                    }
                    DicomDataset dcmImg1 = DicomFile.Open(imgFile).Dataset;
                    DicomDataset dcmMask = DicomFile.Open(maskFile).Dataset;
                    imgs.Add(dcmImg1);
                    masks.Add(dcmMask);
                }
                imgs.AddRange(masks);
                if (imgs.Count == 0) 
                {
                    MessageBox.Show("未找到dicom文件");
                    return;
                }
                DicomDataset dcmImg = imgs[0];
                AddParams(dcmImg);
                //DateTime startTime = DateTime.Now;
                ResultMessage message = FeatureCalculator.Calclate(imgs.ToList());
                //TimeSpan timeSpan = DateTime.Now - startTime;
                //double consumeTime = timeSpan.TotalSeconds;

                List<FeatureResult> resultList = new List<FeatureResult>();
                if (message.IsSuccess)
                {
                    if (cbGLCM.Checked)
                    {
                        //获取glcm结果标签集合
                        List<DicomTag> glcmTags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0015);
                        foreach (DicomTag tag in glcmTags)
                        {
                            FeatureResult featureResult = new FeatureResult();
                            featureResult.FeatureName = tag.PrivateCreator.Creator;
                            featureResult.FeatureValue = dcmImg.GetValue<double>(tag, 0);
                            featureResult.FeatureGroup = "GLCM";
                            resultList.Add(featureResult);
                        }
                    }
                    if (cbFirstOrder.Checked)
                    {
                        //获取glcm结果标签集合
                        List<DicomTag> tags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0017);
                        foreach (DicomTag tag in tags)
                        {
                            FeatureResult featureResult = new FeatureResult();
                            featureResult.FeatureName = tag.PrivateCreator.Creator;
                            featureResult.FeatureValue = dcmImg.GetValue<double>(tag, 0);
                            featureResult.FeatureGroup = "FirstOrder";
                            resultList.Add(featureResult);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(message.ErrorMessage);
                }
                MessageBox.Show("计算完成");
                this.dataGridView1.DataSource = resultList;
                this.dataGridView1.Columns[0].Width = 130;
                this.dataGridView1.Columns[1].Width = 300;
                this.dataGridView1.Columns[2].Width = 500;
            }
            catch
            {
            }
        }
        void AddParams(DicomDataset dcmImg) 
        {
            List<string> preProcesses = new List<string>();
            //设置参数
            if (cbRange.Checked)
            {
                preProcesses.Add("RangeFilter");
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.RangeFilterMode, cboFilterMode.SelectedIndex);
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.RangeMax, double.Parse(txtRangeMax.Text));
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.RangeMin, double.Parse(txtRangeMin.Text));
            }
            if (cbNormalize.Checked)
            {
                preProcesses.Add("Normalize");
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.NormalizeScale, double.Parse(txtScale.Text));
            }
            if (cbResample.Checked)
            {
                preProcesses.Add("Resample");
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.ResamplingFactorXYZ, txtResample.Text.Split('\\').Select(c => double.Parse(c)).ToList().ToArray());
            }
            PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.Preprocess, String.Join('\\', preProcesses));
            if (cbFixedNumber.Checked)
            {
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.UseFixedBins, 1);
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.NBins, int.Parse(txtBinCount.Text));
            }
            else
            {
                PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.BinWidth, double.Parse(txtBinWidth.Text));
            }
            if (cbGLCM.Checked)
            {
                //获取glcm结果标签集合
                List<DicomTag> glcmTags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0015);
                foreach (DicomTag tag in glcmTags)
                {
                    PrivateDicomTag.AddOrUpdate(dcmImg, tag, 0d);
                }
            }
            if (cbFirstOrder.Checked)
            {
                //获取glcm结果标签集合
                List<DicomTag> tags = PrivateDicomTag.GetPrivateDicomTagByGroupId(0x0017);
                foreach (DicomTag tag in tags)
                {
                    PrivateDicomTag.AddOrUpdate(dcmImg, tag, 0d);
                }
            }
            //计算glcm
            PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.EnableGLCM, Convert.ToInt32(cbGLCM.Checked));
            PrivateDicomTag.AddOrUpdate(dcmImg, PrivateDicomTag.EnableFirstOrder, Convert.ToInt32(cbFirstOrder.Checked));
        }
    }
}
