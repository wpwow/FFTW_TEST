using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Numerics;
using FFTWSharp;
using System.IO;

namespace FFTW_TEST
{
    public partial class Form_Test : Form
    {
        private double[] dataIn = null;
        private double[,] result = null;
        private string filePath = string.Empty;

        public Form_Test()
        {
            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "请选择FFTW计算数据文件";
            dlg.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filePath = dlg.FileName.ToString();

                textBoxPath.Text = filePath;

                readFile2doubleData(filePath);
            }
        }       

        private void button_Estimate_Click(object sender, EventArgs e)
        {
            double Fn = double.Parse(textBoxFn.Text);

            result = FFTW_Estimate_Calc(dataIn, Fn);

            saveData2File(filePath);
        }

        private void saveData2File(string filePath)
        {
            if (result != null)
            {
                string file = Path.GetDirectoryName(filePath) + "\\" + "out.txt";

                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                //新建文件
                FileStream fs = new FileStream(file, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);

                for (int i = 0; i < result.Length / 2; i++)
                {
                    sw.WriteLine(result[i, 0].ToString() + "," + result[i, 1].ToString());
                }

                sw.Flush();//清空缓冲区
                sw.Close();//关闭流
                fs.Close();

                richTextBoxMsg.Text = string.Format("计算生成的数据文件保存在：\n {0}", file);
            }
        }

        private void readFile2doubleData(string filePath)
        {
            int lineNum = 0;

            string strLine = string.Empty;

            if (File.Exists(filePath))
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                lineNum = sr.ReadToEnd().Split('\n').Length - 1;
                fs.Position = 0;

                dataIn = new double[lineNum];

                for (int i = 0; i < lineNum; i++ )
                {
                    strLine = sr.ReadLine();

                    if (strLine != null)
                    {
                        dataIn[i] = double.Parse(strLine);
                    }
                }

                sr.Close();//关闭流
                fs.Close();
            }
        }

        private static double[,] FFTW_Estimate_Calc(double[] din, double FN)
        {
            int n = din.Length;
            Complex[] cin = new Complex[n];
            Complex[] cout = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                cin[i] = new Complex(din[i], 0);
            }

            fftw_plan mplan;
            fftw_complexarray min, mout;

            min = new fftw_complexarray(cin);
            mout = new fftw_complexarray(cout);

            mplan = fftw_plan.dft_1d(n, min, mout, fftw_direction.Forward, fftw_flags.Estimate);
            mplan.Execute();

            double[] real = new double[n];
            double[] imag = new double[n];

            real = mout.GetData_Real();
            imag = mout.GetData_Imaginary();

            double[,] result = new double[n, 2];

            for (int i = 0; i < n; i++)
            {
                result[i, 0] = i * FN / n;
                result[i, 1] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
            }

            return result;
        }   
    }
}
