using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace FindFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        DateTime startTime; //Время начала поиска
        DateTime pauseTime; //время начала паузы
        TimeSpan differentInTime;  //различие во времени между продолжением и паузами
        Thread findThread;//поток поиска
        private List<string> allPath;  //Пути папок
        TreeNode NODE;
        string nameOfFile = "";

        private void checkBox1_CheckedChanged(object sender, EventArgs e) => textBox3.Enabled = !textBox3.Enabled;


        void findFiles()
        {
            try
            {

                DirectoryInfo df = new DirectoryInfo(allPath.Last());
                var files = df.GetFiles();
                var folders = df.GetDirectories();
                foreach (var folder in folders)
                {
                    allPath.Add(folder.FullName);
                    TreeNode t = new TreeNode(folder.Name);
                    NODE.Nodes.Add(t);
                    NODE = NODE.LastNode;
                    findFiles();
                    if (NODE.Nodes.Count != 0) NODE = NODE.Parent;
                    else
                    {
                        NODE = NODE.Parent;
                        NODE.Nodes.Remove(t);
                    }

                    allPath.RemoveAt(allPath.Count - 1);
                }
                foreach (var file in files)
                {
                    label6.Invoke(new Action(() => label6.Text = file.Name));

                    if(StringExtension.Contains(file.Name,nameOfFile, StringComparison.OrdinalIgnoreCase))
                    {
                        if (checkBox1.Checked)
                        {
                            string allText = File.ReadAllText(file.FullName);

                            if (StringExtension.Contains(allText,textBox3.Text,StringComparison.OrdinalIgnoreCase)) {
                                TreeNode t = new TreeNode(file.Name);

                                NODE.Nodes.Add(t);


                                listBox1.Invoke(new Action(() => listBox1.Items.Add(file.FullName)));
                            }

                        }
                        else
                        {
                            TreeNode t = new TreeNode(file.Name);
                            NODE.Nodes.Add(t);
                            listBox1.Invoke(new Action(() => listBox1.Items.Add(file.FullName)));
                        }
                    }

                }


                if (allPath.Count == 1) MessageBox.Show("Поиск завершен!", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (System.IO.DirectoryNotFoundException)
            {
                MessageBox.Show("Такого пути не существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.UnauthorizedAccessException)
            {

            }
            finally
            {
                if (allPath.Count == 1)
                {
                    resetStartButton();

                    treeView1.Invoke(new Action(() => treeView1.Nodes.Add(NODE)));

                }


            }
        }


       

        void resetStartButton()
        {
            if (InvokeRequired)
            {
                button3.Invoke(new Action(() => button3.Enabled = !button3.Enabled));
                button2.Invoke(new Action(() => { button2.Text = "Начать поиск";
                    button2.Click += startFind;
                    button2.Click -= stopFind;
                }
                ));
                if (button3.Text == "Продолжить")
                {
                    button3.Invoke(new Action(() =>
                    {
                        button3.Text = "Приостановить";
                        button3.Click += pauseFind;
                        button3.Click -= continueFind;
                    }));

                }

            }
            else
            {
                button3.Enabled = !button3.Enabled;
                button2.Text = "Начать поиск";
                button2.Click += startFind;
                button2.Click -= stopFind;
                if (button3.Text == "Продолжить")
                {
                    button3.Text = "Приостановить";
                    button3.Click += pauseFind;
                    button3.Click -= continueFind;
                }

            }
            timer1.Stop();
        }

        void startFind(object sender, EventArgs e) //начало поиска
        {
            listBox1.Items.Clear();
            treeView1.Nodes.Clear();
            textBox3.ReadOnly = true;

            allPath = new List<string>();
            NODE = new TreeNode(new DirectoryInfo(textBox1.Text).Name);

            allPath.Add(textBox1.Text);
            button3.Enabled = !button3.Enabled;
            button2.Text = "Остановить поиск";
            button2.Click -= startFind;
            button2.Click += stopFind;
            if (textBox2.TextLength > 0) if (textBox2.Text[0] == '*') nameOfFile = textBox2.Text.Substring(1);
                else nameOfFile = textBox2.Text;
            startTime = DateTime.Now;

            differentInTime = new TimeSpan();
            timer1.Start();
            findThread = new Thread(new ThreadStart(findFiles));
            findThread.IsBackground = true;
            findThread.Start();
        }

        private void stopFind(object sender, EventArgs e) //конец поиска
        {
            resetStartButton();
            textBox3.ReadOnly = false;
            label6.Text = "";
            label7.Text = "00:00:00";
            try
            {
                findThread.Abort();
            }
            catch (Exception)
            {

            }
            try
            {
                while (NODE.Parent != null) NODE = NODE.Parent;
                treeView1.Invoke(new Action(() => treeView1.Nodes.Add(NODE)));
            }
            catch (Exception)
            {

            }
        }
        private void pauseFind(object sender, EventArgs e)  //пауза поиска
        {
            button3.Text = "Продолжить";
            button3.Click -= pauseFind;
            button3.Click += continueFind;

            while (NODE.Parent != null) NODE= NODE.Parent;
            treeView1.Invoke(new Action(() => treeView1.Nodes.Add(NODE)));
            timer1.Stop();
            pauseTime = DateTime.Now;

            findThread.Suspend();
        }
        private void continueFind(object sender, EventArgs e) //продолжение поиска
        {

            button3.Text = "Приостановить";
            button3.Click += pauseFind;
            button3.Click -= continueFind;
            differentInTime += (DateTime.Now - pauseTime);
            NODE = treeView1.TopNode;
            while (NODE.Nodes.Count != 0)  NODE = NODE.LastNode;
            
            treeView1.Nodes.Clear();
            timer1.Start();
            findThread.Resume();
        }
        private TreeNode copyNode(TreeNode tn)
        {
            if (tn.Nodes == null) return null;

            foreach (var nod in tn.Nodes)
            {
                TreeNode node = nod as TreeNode;
                return copyNode(node);
            }
            return null;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK) textBox1.Text = fd.SelectedPath;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan newTime = DateTime.Now - startTime;
            newTime -= differentInTime;
            label7.Text = String.Format("{0:D2}:{1:D2}:{2:D2}", newTime.Hours, newTime.Minutes, newTime.Seconds);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader("info.dll", Encoding.UTF8))
                {
                    textBox1.Text = sr.ReadLine();
                    textBox2.Text = sr.ReadLine();
                    checkBox1.Checked = Convert.ToBoolean(sr.ReadLine());
                    textBox3.Text = sr.ReadLine();
                }
            }
            catch (Exception)
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (StreamWriter sw = new StreamWriter("info.dll", false))
            {
                sw.WriteLine(textBox1.Text);
                sw.WriteLine(textBox2.Text);
                sw.WriteLine(checkBox1.Checked);
                sw.WriteLine(textBox3.Text);
            }
        }

    }
    public static class StringExtension{
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

    }
}
