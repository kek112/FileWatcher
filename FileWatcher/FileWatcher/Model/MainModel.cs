using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kley.Base.Infrastructure;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using System.Windows;

namespace FileWatcher.Model
{
    class MainModel : BaseNotifyPropertyChanged
    {
        #region Fields

        private ImageSource activeImage;
        private string sourceFilePath;
        private string targetFilePath;
        private string informationContent;
        private string statusBar;
        private string extraExtension;
        private int selectedListBoxItemIndex;
        private bool elementActive;
        private List<string> bottomList;
   
        private CheckableObservableCollection<string> items;
        private RelayCommand scanFolderCommand;
        private RelayCommand setSourceFilePathCommand;
        private RelayCommand setTargetFilePathCommand;
        private RelayCommand stopWatcherCommand;
        private RelayCommand startWatcherCommand;
        private RelayCommand addExtraExtensionCommand;
        private RelayCommand removeExtensionCommand;

        private FileSystemWatcher watcher;
        private bool running;
        
        #endregion Fields

        #region Constructor

        public MainModel()
        {
            Items = new CheckableObservableCollection<string>();
            SourceFilePath = "Sourcepath -->";
            TargetFilePath = "Targetpath -->";
            ExtraExtension = "add own extension";
            StatusBarMode(false);
            showImage("Red");
            running = false;
            ElementActive = true;

            LoadXML();
        }

   
        #endregion Constructor

        #region Properties

        public string StatusBar
        {
            get { return statusBar; }
            set
            {
                statusBar = value;
                NotifyPropertyChanged("StatusBar");
              
            }
        }

        public string SourceFilePath
        {
            get 
            {
                return sourceFilePath; 
            }
            set 
            {
                if (sourceFilePath != value)
                {
                    sourceFilePath = value;
                    NotifyPropertyChanged("SourceFilePath");
                }
                
            }
        }

        public string TargetFilePath
        {
            get { return targetFilePath; }
            set
            {
                if (targetFilePath != value)
                {
                    targetFilePath = value;
                    NotifyPropertyChanged("TargetFilePath");
                }
            }
        }

        public string InformationContent
        {
            get { return informationContent; }
            set
            {
                if (informationContent != value)
                {
                    informationContent = value;
                    NotifyPropertyChanged("InformationContent");
                }
            }
        }

        public CheckableObservableCollection<string> Items
        {
            get { return items; }
            set
            {
                items = value;
                NotifyPropertyChanged("Items");
            }
        }

        public ImageSource ActiveImage 
        {
            get { return activeImage; }
            set 
            {
                activeImage = value;
                NotifyPropertyChanged("ActiveImage");
             }
         }

        public string ExtraExtension
        {
            get { return extraExtension; }
            set 
            {
                extraExtension = value;
                NotifyPropertyChanged("ExtraExtension");               
             }
         }

        public int SelectedListBoxItemIndex
        {
            get { return selectedListBoxItemIndex; }
            set 
            {
                selectedListBoxItemIndex = value;
                NotifyPropertyChanged("SelectedListBoxItemIndex");
             }
         }

        public bool ElementActive 
        {
            get { return elementActive; }
            set
            {
                if (elementActive != value)
                {
                    elementActive = value;
                    NotifyPropertyChanged("ElementActive");
                }
            }
        }

        public List<string> BottomList
        {
            get
            {
                return bottomList;
            }
            set
            {
                if (bottomList != value)
                {
                    bottomList = value;
                    NotifyPropertyChanged("BottomList");
                }

            }
        }

        #endregion Properties

        #region Commands

        public RelayCommand AddExtraExtensionCommand
        {
            get
            {
                if (addExtraExtensionCommand == null)
                {
                    addExtraExtensionCommand = new RelayCommand(
                        (obj) =>
                        {
                            return !running;
                        },
                        (obj) =>
                        {
                            if (!string.IsNullOrEmpty(ExtraExtension) )
                            {
                                Items.Add(ExtraExtension);
                            }
                        }
                        );
                }
                return addExtraExtensionCommand;
            }
        }

        public RelayCommand RemoveExtensionCommand
        {
            get
            {
                if (removeExtensionCommand == null)
                {
                    removeExtensionCommand = new RelayCommand(
                        (obj) =>
                        {
                            return !running;
                        },
                        (obj) =>
                        {
                            if (SelectedListBoxItemIndex >= 0)
                            {
                                Items.RemoveAt(SelectedListBoxItemIndex);
                            }
                            //CreateListBox();
                        }
                        );
                }
                return removeExtensionCommand;
            }
        }

        public RelayCommand SetSourceFilePathCommand
        {
            get
            {
                if (setSourceFilePathCommand == null)
                {
                    setSourceFilePathCommand = new RelayCommand(
                        (obj) =>
                        {
                            return !running;
                        },
                        (obj) =>
                        {
                            OpenFileDialog ofd = new OpenFileDialog();
                            if (Directory.Exists(SourceFilePath))
                                ofd.InitialDirectory = SourceFilePath;

                            ofd.CheckFileExists = false;
                            ofd.FileName = "Select folder";

                            if (ofd.ShowDialog().Value)
                            {
                                SourceFilePath = Path.GetDirectoryName(ofd.FileName);
                                ScanFolderCommand.InformCanExecuteChanged();
                                StartWatcherCommand.InformCanExecuteChanged();
                                StopWatcherCommand.InformCanExecuteChanged();
                            }
                        }
                        );
                }
                return setSourceFilePathCommand;
            }
        }

        public RelayCommand SetTargetFilePathCommand
        {
            get
            {
                if (setTargetFilePathCommand == null)
                {
                    setTargetFilePathCommand = new RelayCommand(
                        (obj) =>
                        {
                            return !running;
                        },
                        (obj) =>
                        {
                            OpenFileDialog ofd = new OpenFileDialog();
                            if (Directory.Exists(TargetFilePath))
                                ofd.InitialDirectory = TargetFilePath;

                            ofd.CheckFileExists = false;
                            ofd.FileName = "Select folder";

                            if (ofd.ShowDialog().Value)
                            {
                                TargetFilePath = Path.GetDirectoryName(ofd.FileName);
                                ScanFolderCommand.InformCanExecuteChanged();
                                StartWatcherCommand.InformCanExecuteChanged();
                                StopWatcherCommand.InformCanExecuteChanged();
                            }
                        }
                        );
                }
                return setTargetFilePathCommand;
            }
        }

        public RelayCommand ScanFolderCommand 
        {
            get
            {
                if (scanFolderCommand == null)
                {
                    scanFolderCommand = new RelayCommand(
                        (obj) =>
                        {
                            return Directory.Exists(SourceFilePath)&&
                                Directory.Exists(TargetFilePath)&&!running;
                        },
                        (obj) =>
                        {
                            RunFolder();
                            SetFolderInformation();
                        }
                        );
                }
                return scanFolderCommand;
            }
        }

        public RelayCommand StartWatcherCommand
        {
            get
            {
                if (startWatcherCommand == null)
                {
                    startWatcherCommand = new RelayCommand(
                        (obj) =>
                        {
                            return Directory.Exists(SourceFilePath) &&
                                Directory.Exists(TargetFilePath) && !running;
                        },
                        (obj) =>
                        {
                            running = true;
                            ScanFolderCommand.InformCanExecuteChanged();
                            AddExtraExtensionCommand.InformCanExecuteChanged();
                            RemoveExtensionCommand.InformCanExecuteChanged();
                            SetSourceFilePathCommand.InformCanExecuteChanged();
                            SetTargetFilePathCommand.InformCanExecuteChanged();
                            StopWatcherCommand.InformCanExecuteChanged();
                            StartWatcherCommand.InformCanExecuteChanged();
                            Watch();
                        }
                        );
                }
                return startWatcherCommand;
            }
        }

        public RelayCommand StopWatcherCommand
        {
            get
            {
                if (stopWatcherCommand == null)
                {
                    stopWatcherCommand = new RelayCommand(
                        (obj) =>
                        {
                            return Directory.Exists(SourceFilePath) &&
                                Directory.Exists(TargetFilePath) && running;
                        },
                        (obj) =>
                        {
                            running = false;
                            StartWatcherCommand.InformCanExecuteChanged();
                            ScanFolderCommand.InformCanExecuteChanged();
                            AddExtraExtensionCommand.InformCanExecuteChanged();
                            RemoveExtensionCommand.InformCanExecuteChanged();
                            SetSourceFilePathCommand.InformCanExecuteChanged();
                            SetTargetFilePathCommand.InformCanExecuteChanged();
                            StopWatcherCommand.InformCanExecuteChanged();
                            
                            Unwatch();
                        }
                        );
                }
                return stopWatcherCommand;
            }
        }

        #endregion Commands

        #region Handler

        //void test()
        //{
        //    SourceFilePath = @"D:\Development\TestSorcueFile";
        //    TargetFilePath = @"D:\Development\TestSorcueFile";
        //    StatusBarMode(true);
        //    //CheckExtensionsInFolder();
            
        //    RunFolder();
        //}

        void showImage(string ImgName)
        {
               if(!string.IsNullOrEmpty(ImgName))
            {
                var yourImage = new BitmapImage(new Uri(String.Format("Images/{0}.jpg", ImgName), UriKind.Relative));
                //yourImage.Freeze(); // -> to prevent error: "Must create DependencySource on same Thread as the DependencyObject"
                ActiveImage = yourImage;
            }
            else
            {
                ActiveImage = null;   
            }
        }

        private void RunFolder() 
        {
            DirectoryInfo di = new DirectoryInfo(@SourceFilePath);
            List<string> extensionList = new List<string>(CheckExtensionsInFolder(di));
            extensionList = removeDoubleEntries(extensionList);
            Items.Clear();

            //Add the extension to the Listbox
            foreach(string str in extensionList)
            {
                Items.Add(str);
            }
            SaveUrlToXml();
        }


        public void Watch()
        {
            if (Directory.Exists(SourceFilePath) && Directory.Exists(TargetFilePath))
            {

                watcher = new FileSystemWatcher();

                watcher.Path = SourceFilePath;

                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = false;

                watcher.Changed += OnWeightFilesDirectoryChanged;
                watcher.Created += OnWeightFilesDirectoryChanged;

                watcher.EnableRaisingEvents = true;

                StatusBarMode(true);
                showImage("Green");

                ElementActive = false;

                InitialSort();
                //BottomList.Add( "Watcher started");
                
            }            
        }

        private void InitialSort()
        {

            List<string> CheckedExtensions = new List<string>();
            foreach(var Item in Items)
            {
                if (Item.IsChecked) 
                {
                    CheckedExtensions.Add(Item.Value);
                }
            }

            DirectoryInfo di = new DirectoryInfo(@SourceFilePath);

            foreach(var extension in CheckedExtensions)
            {
                Directory.CreateDirectory(@TargetFilePath + "/" + extension.ToString() );     
            }
            Directory.CreateDirectory(@SourceFilePath + "/Processed");

            foreach (var file in di.GetFiles()) 
            {
                foreach (var extensions in CheckedExtensions) 
                {
                    if (file.Extension == "."+extensions) 
                    {
                        // set preferences for overwrite
                        string target = @TargetFilePath + "/" + extensions +"/"+ file.Name;
                       
                        try
                        {
                            File.Copy(file.FullName.ToString(), target, true);
                            
                        }
                        catch (Exception e) 
                        {
                            MessageBox.Show(e.Message);
                        }

                        try
                        {
                            //File.MoveTo(@TargetFilePath + "/Processed/" + file.Name);
                            File.Move(file.FullName.ToString(),@SourceFilePath + "/Processed/" + file.Name);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    
                    }
                }
            }
        }

        private void Unwatch()
        {
            if (watcher != null)
            {
                watcher.Changed -= OnWeightFilesDirectoryChanged;
                watcher.Created -= OnWeightFilesDirectoryChanged;

                watcher.EnableRaisingEvents = false;
                StatusBarMode(false);
                ElementActive = true;
                showImage("Red");
                //BottomList.Add( "Watcher ended");
            }
           
        }

        void SetFolderInformation()
        {
            InformationContent = "";
            DirectoryInfo di = new DirectoryInfo(@SourceFilePath);
            InformationContent += "Name: " + di.FullName.ToString()+"\n";

            try
            {
                double Size = di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                Size /= 1024*1024 ;
                Size=Math.Round(Size, 3);
                InformationContent += "Size: " + Size.ToString()+" MB";
                NotifyPropertyChanged("InformationContent");
            }
            catch (Exception e) 
            {
              
            }

            if (SourceFilePath != TargetFilePath) 
            {
                DirectoryInfo di_t = new DirectoryInfo(@TargetFilePath);
                InformationContent += "\n" + "\n" + "Name: " + di_t.FullName.ToString() + "\n";

                try
                {
                    double Size = di_t.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                    Size /= 1024 * 1024;
                    Size = Math.Round(Size, 3);
                    InformationContent += "Size: " + Size.ToString() +" MB";
                    NotifyPropertyChanged("InformationContent");
                }
                catch (Exception e)
                {

                }
            }
            // read folder and set InfromationContent
        }

        private void OnWeightFilesDirectoryChanged(object source, FileSystemEventArgs e)
        {
            //checked extension --> check the new file and if match copy to folder if not do nothjing

            //check if file is completly written
            //problem: file to large --> smaller incoming files are processed later
            //while (!IsFileLocked(new FileInfo(e.FullPath)))
            //{
            //    InitialSort();
            //}
            InitialSort();

        }

        private List<string> CheckExtensionsInFolder(DirectoryInfo di) 
        { 
            List<string> extensionList = new List<string>();   
            
            foreach (var filename in di.GetFiles())
            {
                extensionList.Add(filename.Extension.ToString().Substring(1));
            }
            return extensionList;
           
        }

        private void StatusBarMode(bool p)
        {
            if (p)
            {
                StatusBar = "Filewatcher aktiv";
            }
            else
            {
                StatusBar = "Filewatcher nicht aktiv";
            }
        }

        private static List<string> removeDoubleEntries(List<string> stringList)
        {
            return (new HashSet<string>(stringList)).ToList();
        }

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }


        /// <summary>
        /// Create a Settings xml file to save the entered URL to load them with the start of the program
        /// </summary>
        private void SaveUrlToXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode myRoot, myNode;

            myRoot = doc.CreateElement("URL");
            doc.AppendChild(myRoot);

            myNode = doc.CreateElement("SourcePath");
            myNode.InnerText = SourceFilePath.ToString();
            myRoot.AppendChild(myNode);

            myNode = doc.CreateElement("TargetPath");
            myNode.InnerText = TargetFilePath.ToString();
            myRoot.AppendChild(myNode);

            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
                path += "\\FileWatcherSettings";
                Directory.CreateDirectory(path);
                doc.Save(path + "\\Settings.xml");
            }
            catch (Exception e)
            {

                MessageBox.Show(e.Message);
            }


        }
        /// <summary>
        /// Loads the SettingsXML to get the URL for the Watcher
        /// </summary>
        private void LoadXML()
        {
            try 
            {
                List<string> url = new List<string>();

                XmlDocument doc = new XmlDocument();
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
                path += "\\FileWatcherSettings\\Settings.xml";
                doc.Load(path);
                XmlElement root = doc.DocumentElement;
                
                foreach (XmlNode data in root) 
                {
                   url.Add(data.InnerText);
                }
                SourceFilePath = url[0];
                TargetFilePath = url[1];
            }
            catch(Exception e)
            {
            }
        }

        #endregion Handler

    }

}
