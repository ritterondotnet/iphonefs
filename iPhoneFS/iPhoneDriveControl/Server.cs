/*---------------------------------------------------------------------------*\
* Copyright (C) 2007-2011 Lokkju, Inc <lokkju@lokkju.com>                     *
*                                                                             *
* This program is free software; you can redistribute it and/or modify it     *
* under the terms of the GNU General Public License as published by the Free  *
* Software Foundation; either version 3 of the License, or (at your option)   *
* any later version.                                                          *
*                                                                             *
* This program is distributed in the hope that it will be useful, but WITHOUT *
* ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or       *
* FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for    *
* more details.                                                               *
* You should have received a copy of the GNU General Public License along     *
* with this program; if not, see <http://www.gnu.org/licenses>.               *
*                                                                             *
* Additional permission under GNU GPL version 3 section 7:                    *
* If you modify this Program, or any covered work, by linking or combining it *
* with the NeoGeo SMB library, or a modified version of that library,         *
* the licensors of this Program grant you additional permission to convey the *
* resulting work as long as the library is distributed without fee.           *
*-----------------------------------------------------------------------------*
* @category   iPhone                                                          *
* @package    iPhone File System for Windows                                  *
* @copyright  Copyright (c) 2010 Lokkju Inc. (http://www.lokkju.com)          *
* @license    http://www.gnu.org/licenses/gpl-3.0.txt GNU v3 Licence          *
*                                                                             *
* $Revision::                                     $:  Revision of last commit *
* $Author::                                         $:  Author of last commit *
* $Date::                                             $:  Date of last commit *
* $Id::                                                                     $ *
\*---------------------------------------------------------------------------*/

/*--------------------------------------------------------------------*\
 * Some code Copyright 2007 Richard.Heinrich@palissimo.de             *
\*--------------------------------------------------------------------*/

//Define this constant to change the project to a Windows Server application, but do not forget to register 
//the service with installutil 

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


#if(!SERVICE)
using System.Drawing;
using System.Windows.Forms;
#endif

namespace com.lokkju.iphonefs
{
#if (SERVICE)
	public class DriveControlServer : ServiceBase
	{
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private IConfigAdapter Config;			// Muss global sein damit Config-Klasse nie entladen wird !!
        private MyTextWriterTraceListener LogFileListner;

        public DriveControlServer()
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
                ToRun = new DriveControlServer();

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
    public class DriveControlServer : System.Windows.Forms.Form
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

        public DriveControlServer()
        {
            InitializeComponent();

            //Is necessary for VS2005, as it checks for Thread-safty
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
            // Should it make ThreadSave, but is not working
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
        /// Free used resources
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
            this.Text = "iPhoneFS - Copyright 2010 Lokkju Inc - !!!ALPHA!!!";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        [STAThread]
        static void Main()
        {
            //ensureDllExists("iTunesMobileDevice.dll");
            Application.Run(new DriveControlServer());
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            StopServer();
            this.Close();
        }
#endif

        
        SMB CIFS = null;

        iPhone phone;
        string drive;
        
        //That is the main part of the program
        private void startCIFSServer()
        {
            // Check if the server is already running
            if (RunAlready())
            {
                Trace.WriteLine("Fatal->The server is already running, this instance is beeing stopped.");
                throw new ApplicationException("Server must only run in one instance.");
            }

            const int SMBNameLength = 15;
            // Start SMB server
            try
            {
                string HostName = System.Windows.Forms.SystemInformation.ComputerName.ToUpper();

                if (HostName.Length > SMBNameLength)
                    HostName = HostName.Substring(0, SMBNameLength);

                string ServerName = "iPhoneDrive";	// oder fester Name
                string DomainController = string.Empty; //  Name of the domain controller, is used for validating users, set it null to accept everybody
                
                CIFS = new SMB();

                CIFS.Start(
                    ServerName,
                    (ushort)16384,
                    NetFlag.Announce | NetFlag.RemoteAccess | NetFlag.ListenAllNetworkCards,
                    5, // Anzahl der gleichzeitigen Anfragen
                    DomainController);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("CIFS server could not be started.", ex);
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
            int ret = iPhoneDriveControl.NetworkDriveManagement.MapDrive(drive, @"\\iphonedrive\phone");
            Trace.WriteLine("Ok,'" + drive + "' is pointing at your iPhone!");
            
        }
        private void unmapDrive()
        {
            int ret = iPhoneDriveControl.NetworkDriveManagement.UnmapDrive(drive);
            Trace.WriteLine("Drive '" + drive + "'is unmapped...");
            
        }

        public void StopServer()
        {
            if (CIFS != null)
            {
                CIFS.Stop(false, 1000);
                CIFS = null;
            }
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
        private com.lokkju.iphonefs.DriveControlServer Form;

        public MyTextWriterTraceListener(string Path, com.lokkju.iphonefs.DriveControlServer form)
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
