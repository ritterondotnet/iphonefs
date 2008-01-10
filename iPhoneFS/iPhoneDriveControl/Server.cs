//#define SERVICE

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Configuration.Install;
using System.IO;
using Manzana;

using NeoGeo.Library.SMB;
using NeoGeo.Library.SMB.Provider;
using NeoGeo.Library.SMB.Utilities;

#if(!SERVICE)
using System.Drawing;
using System.Windows.Forms;
#endif

namespace Suchwerk
{
#if (SERVICE)
	public class SuchwerkService : ServiceBase
	{
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private IConfigAdapter Config;			// Muss global sein damit Config-Klasse nie entladen wird !!
        private MyTextWriterTraceListener LogFileListner;

		public SuchwerkService()
		{
			// Dieser Aufruf ist für den Windows Komponenten-Designer erforderlich.
			InitializeComponent();

			// TODO: Initialisierungen nach dem Aufruf von InitComponent hinzufügen
		}

		// Der Haupteinstiegspunkt für den Vorgang
		[MTAThread]
		static void Main(string[] args)
		{
        	if (args.Length==0)
			{	// Service Case, so start normal
				ServiceBase ToRun;
				ToRun = new SuchwerkService();

				ServiceBase.Run(ToRun);
                return;
			}

            if (args.Length==1)
			{
				switch (args[0].ToUpper())
				{
					case "I":
					case "-I":
					case "-INSTALL":
                        System.Windows.Forms.MessageBox.Show("Install ist noch nicht implementiert.");
                        break;
					case "U":
					case "-U":
					case "-UNINSTALL":
                        System.Windows.Forms.MessageBox.Show("Un-Install ist noch nicht implementiert.");
						break;
					default:
    					System.Windows.Forms.MessageBox.Show("Nur die Schalter -I für Install und -U für Un-Install werden unterstützt.");
						break;
				}
			}

            
    	}

		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "Suchwerk";
			this.AutoLog = true;
			this.CanHandlePowerEvent = false;
			this.CanPauseAndContinue = false;
			this.CanShutdown = true;
			this.CanStop = true;
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Führen Sie die Vorgänge aus, um den Dienst zu starten.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			Config = (IConfigAdapter)ObjectFactory.CreateInstance("Suchwerk.Interface.IConfigAdapter");
            string DataPath = (string)Config.Load("DataPath", "NO");
            if (System.IO.Directory.Exists(DataPath))
            {
                System.IO.File.Delete(DataPath + "log.txt");
                LogFileListner = new MyTextWriterTraceListener(DataPath + "log.txt");
                Trace.Listeners.Add(LogFileListner);
            }
            Trace.AutoFlush = true;
            Debug.AutoFlush = true;

            Thread InitThread = new Thread(new ThreadStart(StartServer));
			InitThread.Start();
		}
 
		/// <summary>
		/// Beenden Sie den Dienst.
		/// </summary>
		protected override void OnStop()
		{
			// TODO: Hier Code zum Ausführen erforderlicher Löschvorgänge zum Anhalten des Dienstes einfügen.
			StopServer();
		}

#else
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button button1;
        internal System.Windows.Forms.TextBox TraceTextBox;
        private System.ComponentModel.Container components = null;
        public System.Windows.Forms.CheckBox CBFatal;
        public System.Windows.Forms.CheckBox CBWarnung;
        public System.Windows.Forms.CheckBox CBInfo;
        public Label Status;

        private MyTextWriterTraceListener LogFileListner;
        private delegate void SetTextCallback(string Text);
        
        public Form1()
        {
            InitializeComponent();

            //Wird für VS2005 benötigt, dort wird die Thread-Safeheit überprüft
            TextBox.CheckForIllegalCrossThreadCalls = false;

            string DataPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			if (!DataPath.EndsWith("\\"))
				DataPath += "\\";
			
            System.IO.File.Delete(DataPath + "log.txt");
            LogFileListner = new MyTextWriterTraceListener(DataPath + "log.txt", this);
            Trace.Listeners.Add(LogFileListner);
            
            Trace.AutoFlush = true;
            Debug.AutoFlush = true;

            this.Show();
            Application.DoEvents();

            StartServer();
        }

        public void SetText(string Text)
        {
            // Sollte den Eintrag, ThreadSave machen, funktioniert aber nicht, deshalb auskommentiert
            //	if (this.TraceTextBox.InvokeRequired)
            //	{
            //		SetTextCallback call = new SetTextCallback(SetText);
            //		this.Invoke(call, new object[] { Text });
            //	}
            //	else
            {
                this.TraceTextBox.Text += Text + Environment.NewLine;
                this.TraceTextBox.Select(TraceTextBox.Text.Length, 0);
                this.TraceTextBox.ScrollToCaret();
            }
            Application.DoEvents();
        }


        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code
        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.CBFatal = new System.Windows.Forms.CheckBox();
            this.TraceTextBox = new System.Windows.Forms.TextBox();
            this.CBWarnung = new System.Windows.Forms.CheckBox();
            this.CBInfo = new System.Windows.Forms.CheckBox();
            this.Status = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(459, 301);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 24);
            this.button1.TabIndex = 4;
            this.button1.Text = "&Shutdown";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // CBFatal
            // 
            this.CBFatal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CBFatal.Checked = true;
            this.CBFatal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBFatal.Location = new System.Drawing.Point(18, 305);
            this.CBFatal.Name = "CBFatal";
            this.CBFatal.Size = new System.Drawing.Size(72, 16);
            this.CBFatal.TabIndex = 1;
            this.CBFatal.Text = "Fatal";
            // 
            // TraceTextBox
            // 
            this.TraceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TraceTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TraceTextBox.Location = new System.Drawing.Point(8, 8);
            this.TraceTextBox.MaxLength = 327670000;
            this.TraceTextBox.Multiline = true;
            this.TraceTextBox.Name = "TraceTextBox";
            this.TraceTextBox.ReadOnly = true;
            this.TraceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TraceTextBox.Size = new System.Drawing.Size(557, 285);
            this.TraceTextBox.TabIndex = 0;
            this.TraceTextBox.WordWrap = false;
            // 
            // CBWarnung
            // 
            this.CBWarnung.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CBWarnung.Location = new System.Drawing.Point(96, 301);
            this.CBWarnung.Name = "CBWarnung";
            this.CBWarnung.Size = new System.Drawing.Size(88, 24);
            this.CBWarnung.TabIndex = 2;
            this.CBWarnung.Text = "Warning";
            // 
            // CBInfo
            // 
            this.CBInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CBInfo.Location = new System.Drawing.Point(190, 305);
            this.CBInfo.Name = "CBInfo";
            this.CBInfo.Size = new System.Drawing.Size(96, 16);
            this.CBInfo.TabIndex = 3;
            this.CBInfo.Text = "Info";
            // 
            // Status
            // 
            this.Status.AutoSize = true;
            this.Status.BackColor = System.Drawing.Color.Red;
            this.Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Status.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Status.Location = new System.Drawing.Point(261, 304);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(45, 15);
            this.Status.TabIndex = 5;
            this.Status.Text = "Status";
            this.Status.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(573, 331);
            this.ControlBox = false;
            this.Controls.Add(this.Status);
            this.Controls.Add(this.CBInfo);
            this.Controls.Add(this.CBWarnung);
            this.Controls.Add(this.TraceTextBox);
            this.Controls.Add(this.CBFatal);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "iPhoneExplorer - Copyright 2007 Lokkju Inc - !!!ALPHA!!!";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        [STAThread]
        static void Main()
        {
            ensureDllExists("iTunesMobileDevice.dll");
            Application.Run(new Form1());
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            StopServer();
            this.Close();
        }
#endif
        static bool ensureDllExists(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                DialogResult res = MessageBox.Show("You are missing iTunesMobileDevice.dll!\nDo you want to locate a copy?", "Fatal Error!", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                if (res == DialogResult.No)
                {
                    System.Environment.Exit(0);
                }
                else
                {
                    OpenFileDialog fd = new OpenFileDialog();
                    fd.CheckFileExists = true;
                    fd.FileName = filename;
                    fd.Filter = filename + "|" + filename;
                    fd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + @"\Apple\Mobile Device Support\bin\";
                    fd.Title = "Select a " + filename + " File";
                    res = fd.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        try
                        {
                            if (Path.GetFileName(fd.FileName) == filename)
                            {
                                File.Copy(fd.FileName, Path.GetDirectoryName(Application.ExecutablePath) + @"\" + filename);
                                return true;
                            }
                        }
                        catch { }
                    }
                }
                return false;
            }
        }
        SMB CIFS = null;
        iPhone phone;
        string drive;
        //Eigentliche Funktionalität des Programms !
        private void startCIFSServer()
        {
            // Test of Suchwerk schon läuft
            if (RunAlready())
            {
                Trace.WriteLine("Fatal->Program läuft schon, diese Instanz wird beendet.");
                throw new ApplicationException("Program kann nicht zwei mal gestartet werden.");
            }

            const int SMBNameLength = 15;
            // SMB Server starten
            try
            {
                //Servernamen von Suchwerk erstellen
                string HostName = System.Windows.Forms.SystemInformation.ComputerName.ToUpper();

                if (HostName.Length > SMBNameLength)
                    HostName = HostName.Substring(0, SMBNameLength);

                //string ServerName = "%X-SW";		// Vorschrift zum Bauen des ServerNamens, %X wird durch Rechnernamen ersetzt
                string ServerName = "iPhoneDrive";	// oder fester Name
                string DomainController = string.Empty; //  "Master"; // Name es Domaincontrollers der zur Validierung von User-Anfragen verwendet wird. 

                if (ServerName.IndexOf("%X") == -1)
                {
                    if (ServerName.Length > SMBNameLength)
                        ServerName = ServerName.Substring(0, SMBNameLength);
                }
                else
                {	// We have someting to replace
                    int copy = 15 - (ServerName.Length - 2); // 14 ist maximallänge - anhang - length("%x")
                    if (HostName.Length <= copy)
                        ServerName = ServerName.Replace("%X", HostName);
                    else
                        ServerName = ServerName.Replace("%X", HostName.Substring(0, copy));
                }
                CIFS = new SMB();
                FileSystemProviderCollection fspc = new FileSystemProviderCollection();
                FileSystemProvider fsp = new lokkju.iPx.iPhoneDrive.iPhoneFS();
                fspc.Add(new lokkju.iPx.iPhoneDrive.iPhoneFS());
                CIFS.SetFileSystems(fspc);
                CIFS.Start(
                    ServerName,
                    (ushort)16384,
                    0,
                    5, // Number of concurrent queries
                    DomainController);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("CIFS server could not be established.", ex);
            }
        }
        public void StartServer()
        {
            System.Threading.Thread.CurrentThread.Name = "Main-Thread";
            Trace.WriteLine("-------------- Start of Trace --------------");
            Trace.WriteLine("Waiting for iPhone to connect");
            phone = new iPhone();
            phone.Connect += new ConnectEventHandler(phone_Connect);
            phone.Disconnect += new ConnectEventHandler(phone_Disconnect);
        }

        void phone_Disconnect(object sender, ConnectEventArgs args)
        {
            Trace.WriteLine("iPhone disconnected, stopping CIFS Server");
            StopServer();
        }

        void phone_Connect(object sender, ConnectEventArgs args)
        {
            Trace.WriteLine("iPhone connected, starting CIFS Server");
            startCIFSServer();
            mapDrive();
        }
        private void mapDrive()
        {
            drive = iPhoneDriveControl.NetworkDriveManagement.NextDrive();
            Trace.WriteLine("Found drive '" + drive + "', mapping it...");
            int ret = iPhoneDriveControl.NetworkDriveManagement.MapDrive(drive, @"\\iphonedrive\iPhoneFS");
            Trace.WriteLine("Ok,'" + drive + "' is pointing at your iPhone!");
            
        }
        private void unmapDrive()
        {
            int ret = iPhoneDriveControl.NetworkDriveManagement.UnmapDrive(drive);
            Trace.WriteLine("Drive '" + drive + "'is unmapped...");
            
        }

        public void StopServer()
        {
	       	CIFS.Stop(false, 1000);
            unmapDrive();
            Trace.WriteLine("-------------- End of Trace --------------");
			Trace.Flush();
		    Debug.Flush();
		}

        private static bool RunAlready()
        {
            bool CreateNew;
            string MutexGuid = "{91FD0A7F-0E9A-4FE2-A4DC-5E20316F9BEF}";
            System.Threading.Mutex mtx = new System.Threading.Mutex(false, MutexGuid, out CreateNew);
            return (!CreateNew);
        }	// RunAlready


    }  // class


    // Helper-Funktionen für Logging
#if (!SERVICE)
    public class MyTextWriterTraceListener : TraceListener
    {
        private TextWriterTraceListener Output;
        private Suchwerk.Form1 Form;

        public MyTextWriterTraceListener(string Path, Suchwerk.Form1 form)
        {
            Output = new TextWriterTraceListener(Path);
            Form = form;
        }

        public override void Write(string Text)
        {
            Output.Write(Text);
            //Form.TraceTextBox.Text += Text;
            //Form.TraceTextBox.ScrollToCaret();
        }

        public override void WriteLine(string Text)
        {
            if (Text.StartsWith("Info->") && !Form.CBInfo.Checked)
                return;

            if ((Text.StartsWith("Warnung->") || Text.StartsWith("Warning->")) && !Form.CBWarnung.Checked)
                return;

            if (Text.StartsWith("Fatal->") && !Form.CBFatal.Checked)
                return;

            if (Text.StartsWith("Online->"))
            {
                Form.Status.Text = Text.Substring("Online->".Length);
                Form.Status.Visible = true;
                Application.DoEvents();
                return;
            }
            if (Text.StartsWith("Offline->"))
            {
                Form.Status.Visible = false;
                Application.DoEvents();
                return;
            }
            
            StackFrame SF = new StackFrame(3, false);
            string s = SF.GetMethod().Name.PadRight(20);
            s = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss ") + s + " : " + Text;
            Output.WriteLine(s);
            Form.SetText(s);
            //Form.TraceTextBox.Text += s + Environment.NewLine;
            //Form.TraceTextBox.Select(Form.TraceTextBox.Text.Length, 0);
            //Form.TraceTextBox.ScrollToCaret();
        }
    }
#else
	public class MyTextWriterTraceListener : TraceListener
	{
		// Ins Eventlog schreiben aufnehmen !!!!
		private TextWriterTraceListener Output;
				
		public MyTextWriterTraceListener(string Path)
		{	
			Output = new TextWriterTraceListener(Path);
		}
				
		public override void Write(string Text)
		{
			Output.Write(Text);
		}

		public override void WriteLine(string Text)
		{
			StackFrame SF = new StackFrame(3, false);
			string s = SF.GetMethod().Name.PadRight(20);
			s = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss ") + s + " : " + Text;
			Output.WriteLine(s);
		}
	}

#endif

}
[RunInstallerAttribute(true)]
public class MyProjectInstaller : Installer
{
    private ServiceInstaller serviceInstaller;
    private ServiceProcessInstaller processInstaller;

    public MyProjectInstaller()
    {
        // Instantiate installers for process and services.
        processInstaller = new ServiceProcessInstaller();
        serviceInstaller = new ServiceInstaller();

        // The services run under the system account.
        processInstaller.Account = ServiceAccount.LocalSystem;

        // The services are started manually.
        serviceInstaller.StartType = ServiceStartMode.Automatic;

        // ServiceName must equal those on ServiceBase derived classes.            
        serviceInstaller.ServiceName = "iPhoneDrive";

        // Add installers to collection. Order is not important.
        Installers.Add(serviceInstaller);
        Installers.Add(processInstaller);
    }
}
