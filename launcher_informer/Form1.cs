using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO.Compression;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace launcher_informer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            update();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
          //  update();

        }
        public void update()
        {
            try
            {
                string fullPath = Application.StartupPath.ToString();
                INIManager manager = new INIManager(fullPath + "\\my.ini");
                string v = manager.GetPrivateString("main", "version");
                string pack = new WebClient().DownloadString("http://allminer.ru/api/?method=version");
                Movie m = JsonConvert.DeserializeObject<Movie>(pack);
                string ver = m.version;
                GlobalVars.version = m.version;
                GlobalVars.link = m.source;
               
                if (v == ver)
                {
                    //MessageBox.Show("У Вас старая версия");
                    label1.Text = "У Вас актуальная версия";
                    Process psiw;
                    psiw = Process.Start("cmd", @"/c taskkill /f /im informer.exe");
                    psiw.Close();
                    System.Threading.Thread.Sleep(2000);
                    Process.Start("informer.exe");
                }
                else
                {
                    label1.Text = "Вышла новая версия! Обновление!";
                    //MessageBox.Show("Новая версия!!!" + v + " " + ver);
                    Process psiw;
                    psiw = Process.Start("cmd", @"/c taskkill /f /im informer.exe");
                    psiw.Close();
                    download(GlobalVars.link);

                    


                }

            }
            catch (Exception ex)
            {
                LogFile Log = new LogFile("error_launcher");
                Log.writeLogLine(ex.Message + " fun update");

            }
        }


        
        public void download(string link)
        {
            try
            {

                Uri uri3 = new Uri(link);
                string filename2 = "bin.zip";
               // MessageBox.Show(link);
                if (File.Exists(filename2))
                {
                    File.Delete(filename2);
                    label1.Text = "Файл удален";
                    
                  
                   
                    WebClient wc = new WebClient();
                 
                       wc.DownloadFileAsync(uri3, filename2);
                       wc.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    label1.Text = "Файл скачан";
                }
                else
                {
                  
                    WebClient wc = new WebClient();
                    wc.DownloadFileAsync(uri3, filename2);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    label1.Text = "Файл скачан2";
                }
            }
            catch (Exception ex)
            {
                LogFile Log = new LogFile("error_launcher");
                Log.writeLogLine(ex.Message + " download");
            }

        }
        public void unzip()
        {
            try
            {
                Process psiw;
                psiw = Process.Start("cmd", @"/c taskkill /f /im informer.exe");
                psiw.Close();
                System.Threading.Thread.Sleep(2000);
                //string startPath = @"D:\YandexDisk\dev.sanekxxx4.ru\c#\launcher_informer\launcher_informer\bin\Debug\";
                string zipPath = @"bin.zip";
                string extractPath = @"bin\" + GlobalVars.version;
                //ZipFile.CreateFromDirectory(startPath, zipPath);
                DirectoryInfo directoryinfo = new DirectoryInfo(extractPath);
                if (directoryinfo.Exists) directoryinfo.Delete(true);
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                label1.Text = "Разархивация!";
                copy();
                string fullPath = Application.StartupPath.ToString();
                INIManager manager = new INIManager(fullPath + "\\my.ini");
                manager.WritePrivateString("main", "version", GlobalVars.version);
                Process.Start("informer.exe");
            }
            catch (Exception ex)
            {
                LogFile Log = new LogFile("error_launcher");
                Log.writeLogLine(ex.Message + " unzip");
            }
        }
        
        public void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                label1.Text = e.Error.Message;
                LogFile Log = new LogFile("error_launcher");
                Log.writeLogLine(e.Error.Message);
                Process.Start("informer.exe");
                Process psiw;
                psiw = Process.Start("cmd", @"/c taskkill /f /im launcher_informer.exe");
                psiw.Close();
            }
            else
            {
                label1.Text = "Загружен!";
                unzip();
            }
        }
        
        public void copy()
        {
            try
            {
                string sourceDir = @"bin\" + GlobalVars.version;
                string backupDir = GlobalVars.root_dir;
                string[] picList = Directory.GetFiles(sourceDir, "*");

                // Copy picture files.
                foreach (string f in picList)
                {
                    // Remove path from the file name.
                    string fName = f.Substring(sourceDir.Length + 1);

                    // Use the Path.Combine method to safely append the file name to the path.
                    // Will overwrite if the destination file already exists.
                    File.Copy(Path.Combine(sourceDir, fName), Path.Combine(backupDir, fName), true);
                }
            }
            catch (Exception ex)
            {
                LogFile Log = new LogFile("error_launcher");
                Log.writeLogLine(ex.Message + " copy");
            }
        }

        public class Movie
        {
            public string version { get; set; }
            public string source { get; set; }
        }
        //Класс для чтения/записи INI-файлов
        public class INIManager
        {
            //Конструктор, принимающий путь к INI-файлу
            public INIManager(string aPath)
            {
                path = aPath;
            }

            //Конструктор без аргументов (путь к INI-файлу нужно будет задать отдельно)
            public INIManager() : this("") { }

            //Возвращает значение из INI-файла (по указанным секции и ключу) 
            public string GetPrivateString(string aSection, string aKey)
            {
                //Для получения значения
                StringBuilder buffer = new StringBuilder(SIZE);

                //Получить значение в buffer
                GetPrivateString(aSection, aKey, null, buffer, SIZE, path);

                //Вернуть полученное значение
                return buffer.ToString();
            }

            //Пишет значение в INI-файл (по указанным секции и ключу) 
            public void WritePrivateString(string aSection, string aKey, string aValue)
            {
                //Записать значение в INI-файл
                WritePrivateString(aSection, aKey, aValue, path);
            }

            //Возвращает или устанавливает путь к INI файлу
            public string Path { get { return path; } set { path = value; } }

            //Поля класса
            private const int SIZE = 1024; //Максимальный размер (для чтения значения из файла)
            private string path = null; //Для хранения пути к INI-файлу

            //Импорт функции GetPrivateProfileString (для чтения значений) из библиотеки kernel32.dll
            [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
            private static extern int GetPrivateString(string section, string key, string def, StringBuilder buffer, int size, string path);

            //Импорт функции WritePrivateProfileString (для записи значений) из библиотеки kernel32.dll
            [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
            private static extern int WritePrivateString(string section, string key, string str, string path);
        }
        class LogFile
        {
            private System.IO.StreamWriter sw;

            public LogFile(string path)
            {
                try
                {
                    sw = new System.IO.StreamWriter(path + ".log", true, Encoding.UTF8);
                }
                catch (System.IO.IOException e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
            }
            ~LogFile()
            {
                sw.Close();
            }

            public void writeLogLine(string line)
            {
                DateTime presently = DateTime.Now;
                line = presently.ToString() + " - " + line;
                sw.WriteLine(line);
                sw.Flush();
                sw.Close();
            }
        }
        static class GlobalVars
        {
            public static string link;
            public static string version;
            public static string root_dir = Environment.CurrentDirectory;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            update();
        }
    }
}