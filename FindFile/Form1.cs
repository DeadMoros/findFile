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
        int amountOfFiles = 0;
        int amountOfFilesFound = 0;
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
                    TreeNode node = new TreeNode(folder.Name);
                    NODE.Nodes.Add(node);
                    NODE = NODE.LastNode;
                    findFiles();
                    if (NODE.Nodes.Count == 0)
                    {
                        NODE = NODE.Parent;
                        NODE.Nodes.Remove(node);
                    }
                    else
                    {
                        if(NODE.Parent!=null)NODE = NODE.Parent;
                    }

                    allPath.RemoveAt(allPath.Count - 1);
                }
                foreach (var file in files)
                {
                    label6.Invoke(new Action(() => label6.Text = file.Name));
                    label10.Invoke(new Action(() => { label10.Text = (++amountOfFiles).ToString(); }));
                    if(StringExtension.Contains(file.Name,nameOfFile, StringComparison.OrdinalIgnoreCase))
                    {
                        if (checkBox1.Checked)
                        {
                            StreamReader SR = File.OpenText(file.FullName);
                            string oneLine;
                            while ((oneLine = SR.ReadLine()) != null)
                            {

                                if (StringExtension.Contains(oneLine, textBox3.Text, StringComparison.OrdinalIgnoreCase))
                                {                                                                                                            ////// Добавление в результат если нашли по содержимому
                                    label12.Invoke(new Action(() => label12.Text = (++amountOfFilesFound).ToString()));
                                    TreeNode node = new TreeNode(file.Name);                                                       
                                    NODE.Nodes.Add(node);
                                    TreeNode mainNode = treeView1.Nodes[0];
                                    try
                                    {
                                        while (NODE.Parent.Text != textBox1.Text) NODE = NODE.Parent;
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    findPlaceToInsertNode(ref mainNode, ref NODE);

                                    insertNode(mainNode, NODE);
                                    try
                                    {
                                        while (NODE.LastNode.Nodes.Count != 0) NODE = NODE.LastNode;
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    if (NODE.Nodes.Count == 0) NODE = NODE.Parent;

                                    listBox1.Invoke(new Action(() => listBox1.Items.Add(file.FullName)));
                                    break;
                                }
                            }

                        }
                        else
                        {
                            label12.Invoke(new Action(() => label12.Text = (++amountOfFilesFound).ToString()));
                            TreeNode node = new TreeNode(file.Name);                                                       ////добавление в результат если нашли по имени
                            NODE.Nodes.Add(node);
                            TreeNode mainNode = treeView1.Nodes[0];
                            try
                            {
                                while (NODE.Parent.Text != textBox1.Text) NODE = NODE.Parent;
                            }
                            catch (Exception)
                            {

                            }
                            findPlaceToInsertNode(ref  mainNode,   ref NODE);

                            insertNode( mainNode, NODE);
                            try
                            {
                                while (NODE.LastNode.Nodes.Count != 0) NODE = NODE.LastNode;
                            }
                            catch (Exception)
                            {

                            }
                            if (NODE.Nodes.Count == 0) NODE = NODE.Parent;   

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
                    switchEnableUI();
                }

            }
        }

        void findPlaceToInsertNode(ref TreeNode mainTree,ref TreeNode NODE)
        {
            TreeNode CurrentNode = null;
            foreach (TreeNode TN in mainTree.Nodes)
            {
                if (TN.Text == NODE.Text) CurrentNode = TN;
            }
            if (CurrentNode!= null)
            {
                mainTree = CurrentNode;
                NODE = NODE.LastNode;
                findPlaceToInsertNode(ref mainTree,ref NODE);
            } 

        }

        void insertNode(TreeNode mainTree, TreeNode NODE)
        {
            try
            {
                while (NODE.Nodes.Count != 0)
                {
                    TreeNode newFolder = new TreeNode(NODE.Text);
                    Invoke(new Action(() => mainTree.Nodes.Add(newFolder)));
                    NODE = NODE.LastNode;
                    mainTree = mainTree.LastNode;
                }
            }
            catch(Exception)
            {

            }
            Invoke(new Action(()=>mainTree.Nodes.Add(new TreeNode(NODE.Text))));

        }
       

        void resetStartButton()
        {
            if (InvokeRequired)
            {
                button2.Invoke(new Action(() => { 
                    button2.Text = "Начать поиск";
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
                button3.Invoke(new Action(() => { button3.Enabled = false; }));

            }
            else
            {
                button2.Text = "Начать поиск";
                button2.Click += startFind;
                button2.Click -= stopFind;
                if (button3.Text == "Продолжить")
                {

                    button3.Text = "Приостановить";
                    button3.Click += pauseFind;
                    button3.Click -= continueFind;

                }
                button3.Enabled = false;
            }
            timer1.Stop();
        }


        void startFind(object sender, EventArgs e) //начало поиска
        {
            switchEnableUI();
            amountOfFilesFound = 0;
            amountOfFiles = 0;
            label12.Text = "0";
            listBox1.Items.Clear();
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(new DirectoryInfo(textBox1.Text).Name);
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
            switchEnableUI();
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
            
            timer1.Start();
            findThread.Resume();
        }

        void switchEnableUI()
        {
            textBox1.Invoke(new Action(() => { textBox1.ReadOnly = !textBox1.ReadOnly; }));
            textBox2.Invoke(new Action(() => { textBox2.ReadOnly = !textBox2.ReadOnly; }));
            checkBox1.Invoke(new Action(() => { checkBox1.Enabled = !checkBox1.Enabled; }));
            textBox3.Invoke(new Action(() => { textBox3.ReadOnly = !textBox3.ReadOnly; }));
            button1.Invoke(new Action(() => { button1.Enabled = !button1.Enabled; }));
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
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
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
            try
            {
                findThread.Suspend();
            }
            catch (Exception) { }
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
