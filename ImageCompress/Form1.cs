using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageCompress
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 打开文件夹（多选）组件
        /// </summary>
        private CommonOpenFileDialog folderBrowserDialog1;
        private IEnumerable<string> userSelectFolderPaths;
        public Form1()
        {
            InitializeComponent();
            folderBrowserDialog1 = new CommonOpenFileDialog();
            //设置为选择文件夹
            this.folderBrowserDialog1.IsFolderPicker = true;
            //设置为多选
            this.folderBrowserDialog1.Multiselect = true;
            //设置标题
            this.folderBrowserDialog1.Title = "选择要处理的文件夹(支持多选)";
            //设置
            this.folderBrowserDialog1.RestoreDirectory = true;
            this.textBox1.Text = AppDomain.CurrentDomain.BaseDirectory;
            this.folderBrowserDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == CommonFileDialogResult.Ok)
            {
                //获得用户选择文件夹(多个)
                userSelectFolderPaths = this.folderBrowserDialog1.FileNames;
                this.textBox1.Text = string.Join(";", userSelectFolderPaths);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.progressBar1.Maximum = 0;
            CheckForIllegalCrossThreadCalls = false;
            this.button2.Enabled = false;
            //创建临时文件夹
            var tempPath = AppDomain.CurrentDomain.BaseDirectory + "\\.temp\\";
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            this.label2.Text = "0/" + this.progressBar1.Maximum;
            this.progressBar1.Value = 0;
                List<KeyValuePair<string,string>> files = new List<KeyValuePair<string, string>>();
            foreach (var currentPath in userSelectFolderPaths)
            {
                var soursePath = currentPath + "\\";
                var currentPaths = currentPath.Split('\\');
                var dir = currentPaths[currentPaths.Length - 1];
                if (!Directory.Exists(tempPath + dir))
                {
                    Directory.CreateDirectory(tempPath + dir);
                }
                //读取图片
                foreach (var f in Directory.GetFiles(soursePath, ".", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".gif")))
                {
                    this.progressBar1.Maximum += 1;
                    var dic = new KeyValuePair<string, string>(dir, f);
                    files.Add(dic);
                }
               
            }
            int percent;
            int threadCount;
            int index = 0;
            if(!Int32.TryParse(this.txtThreadCount.Text,out threadCount))
            {
                threadCount = 1;
            }
            for(var i=0;i< threadCount;i++)
            {
                var fileCount = files.Count;
                var count = fileCount / threadCount;
                if(i==0&& fileCount%threadCount>0)
                {
                    count = count + (fileCount % threadCount);
                }
                if (Int32.TryParse(this.textBox2.Text, out percent))
                {
                    if (percent < 100 && percent > 0)
                    {
                        new System.Threading.Thread(() =>
                        {
                            new Action(() =>
                            {
                                for(var j= index; j< count;j++)
                                {
                                    var fileDic = files[j];
                                    var file = fileDic.Value;
                                    var dir = fileDic.Key;
                                    var filePaths = file.Split('\\');
                                    var tempFile = tempPath + dir + "\\" + filePaths[filePaths.Length - 1];
                                    ImageHelper.Compress(file, tempFile, 100, percent);
                                    if (this.checkBox1.Checked)
                                    {
                                        //替换原始文件
                                        new FileInfo(tempFile).CopyTo(file, true);
                                        File.Delete(tempFile);
                                    }
                                    this.progressBar1.Value += 1;
                                    this.label2.Text = this.progressBar1.Value + "/" + this.progressBar1.Maximum;
                                }
                                if (this.progressBar1.Value == this.progressBar1.Maximum)
                                    this.button2.Enabled = true;
                            }).Invoke();
                        }).Start();
                        index += count;

                    }

                }

            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
