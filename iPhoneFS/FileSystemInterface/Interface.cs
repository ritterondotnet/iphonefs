using System;
using System.IO;
using System.Collections.Generic;

namespace Suchwerk.Interfaces
{
	/// <summary>
	/// Sammlung der Interfaces für Suchwerk und WinFUSE
	/// </summary>
	/// 
	
	public interface ISCMPortal
	{
		void Start();
		void Stop();
	}

	public interface IDBAdapter
	{
		// Startet die Datenbank, lädt dazu eine gespeicherte Datenbank
		bool Start();
		
		//Stopped die Datenbank, speichert vorher alle Änderungen
		void Stop();

		//Speichert eine Datenbank
		bool SaveDatabase();

		//Datenbank Funktionen
		//Löscht alle Einträge aus der Datenbank die mit Pfad übereinstimmen, ein % am Ende wird unterstützt zum
		//Löschen von Verzeichnisbäumen
		void DeleteDirectoryTree(string Pfad);

		//Löscht die angegeben Datei aus der Datenbank
		void DeleteFile(string Pfad, string FileName);

		//Liestes alls Tochterverzeichnisse vom angegeben Verzeichnis auf
		System.Collections.ArrayList ListDirectories(string start);

		//Liest den Inhalt der Datei in die Suchmaschine ein
		void ScanFile(FileInfo f);

		//Increments DatabaseChanged und DatabaseChangedAll
		void IncDatabaseChanged();
		
		//Increments DatabaseChangedAll
		void IncDatabaseChangedAll();

		//Liefert den Wert von DatabaseChanged
		int GetDatabaseChanged();
		
		//Liefert den Wert von DatabaseChangedAll
		int GetDatabaseChangedAll();

        //Erstellt ein neues Such-Verzeichnis
        string CreateAdhocDir(string Verzeichnisname, string Suchkriterium);
           
        //Gibt eine Liste mit der max Anzahl von Worten zurück die mit dem Kritium übereinstummen
        string ListWords(string Suchkriterium, int Anzahl);

        //Gibt eine Liste von Files zurück die dieses Wort enthalten
        string ListFiles(string Suchkriterium, int Anzahl);
    }

	public interface ILoader
	{
		//Muss als erstes aufgerufen werden, wenn Rückgabe false schwerwiegender Fehler der nicht behoben werden kann
		bool Init();

		//Starten den Loader, davor muss Init mit true erfoglt sein
		void Start(bool FullUpdate);  // Es kommen noch mehr Parameter

		// Speichert den Zustand des Loaders
		void Save();
	
		//Bricht den Vorgang bei nächster Gelegenheit ab
		void Cancel();

		//Zeigt an ob der Auftrag abgeschlossen wurde, bzw. erfolgreich abgebrochen
		bool Running();

        //Liefert einen String der angibt wie viele Verzeichnisse gefunden wurden und wie viele schon indexiert sind
        //Also: AlreadyFound,AlreadyIndexed
        string GetIndexProgress();
	}

	public interface IFileAdapter
	{
		//LanguageCode: = 0 => alle Sprachen werden gelesen, MS-Landescode => nur diese Sparche wird gelesen, 
		// z.B. 1031 für Deutsche
		//myWordBreaker: Muss eine instanzierte Klasse sein die das Interface IWordBreaker implementiert
		//myWordSink:	 Muss eine instanzierte Klasse sein die das Interface IWordSink implementiert
		void Init(int LanguageCode, object myWordBreaker, object myWordSink);
		
		//Voll qualifizerte Pfad zur Datei die gelesen werden soll, die entsprechenden Routinen von WordBreaker und WordSink werden aufgerufen
		//Funktion arbeitet synchron, d.h. wird erst beendet wenn der Inhalt komplett verarbeitet ist. 
		bool ReadFile(string FileName);

		//Dient zum Abbrechen von ReadFile, falls dort zu lange gewartet wird, oder zu viel Inhalt produziert wird, 
		//also ein Fehler im IFilter vorliegt
		void Cancle();
	}

    /// <summary>
    /// Interface for creating, reading and writing configuration values
    /// </summary>
	public interface IConfigAdapter
	{
        /// <summary>
        /// Create a new configuration entry. The type of the entry is defined by the type of the value parameter
        /// </summary>
        /// <param name="key">Name of configuration entry</param>
        /// <param name="value">Value and Type of the entry</param>
        /// <exception cref="Throws a exception if the entry already exists"></exception>
		void	Add(string key, object value);

        /// <summary>
        /// Reads an existing configuration value
        /// </summary>
        /// <param name="key">Name of configuration entry</param>
        /// <returns>Value and Type of the existing entry</returns>
        /// <exception cref="Throws a execption of the entry is not existing"></exception>
		object	Get(string key);
        /// <summary>
        /// Reads an existing configration value
        /// </summary>
        /// <param name="key">Name of configuration entry</param>
        /// <returns>String value of the existing entry</returns>
        /// <exception cref="Throws a execption of the entry is not existing or is not of type string"></exception>
		string	GetString(string key);				
		int		GetInt(string key);					// Liest angegebenen Wert, wenn nicht vorhanden oder falscher Typ Exception 
		void	Update(string key, object value);	// Überschreibt vorhandenen Wert, wenn nicht vorhanden oder falscher Typ Exception 
		void	Save(string key, object value);		// Update und Save in einem
		void	Save(string key);					// Speichert Wert dauerhaft
		object	Load(string key, object Default);	// Lädt gespeicherten Wert, wenn nicht vorhanden wird Wert neu angelegt
		int		Load(string key, int Default, int Min, int Max);
	}

    /// <summary>
    /// Flags to control the CIFS server, set in the START method of the SMBPortal
    /// </summary>
	[FlagsAttribute]
	public enum NetFlag: uint
	{

        None            =  0x00,
        /// <summary>
        /// If set the server accept request from the LAN
        /// If not set the server only accept request from the local host.
        /// </summary>
		RemoteAccess	=	0x01,
        
        /// <summary>
        /// If set the server is listed in the network browser, to accept request from outside the RemoteAccess flag must be set
        /// If not set the server is not listed in the network browser, but still can be connected if the name is known
        /// </summary>
		Announce		=	0x02,
        
        /// <summary>
        /// If set all OpLock operations request are denid. 
        /// OpLock are used to tell client that they can use a local cache, they have nothing to do with Locks.
        /// Sometime necessary as the OpLock protocol is not stable on very slow network connections
        /// </summary>
        DeactivedOpLocks =   0x04,

        /// <summary>
        /// If set the server works in ASCCI mode, otherwise all strings are UNICODE
        /// Should normally not be set, but must be set to be compatible with very old machines, like WIN95 and WIN98.
        /// </summary>
        AscciCode       =   0x08,

        /// <summary>
        /// If set the locking is deactived and all lock request will be acctepted. Has nothing to do with OpLocks.
        /// Should normally not be set
        /// </summary>
        DeactivedLocking =  0x10,

        /// <summary>
        /// If set no ChangeNotify messages will be sent out by the server to a client.
        /// The ChangeNotify messages are used by Explorer (and FileSystemWatcher) to receive a message when a directory or file has been changed.
        /// </summary>
        DeactivedChangeNotify = 0x20,

        /// <summary>
        /// If set Named stream will be suppported
        /// If not set all request of named streams will be answers with INVALID_NAME
        /// </summary>
        ActivedNamedStreams = 0x40
	}

    /// <summary>
    /// Interface zum Starten und Stoppen des CIFS-Server
    /// </summary>
	public interface ISMBPortal			
	{
        /// <summary>
        /// Startet den CIFS-Server
        /// </summary>
        /// <param name="ServerName">Name des CIFS Servers der eingerichtet werden soll, muss im Netzwerk eindeutig sein"</param>
        /// <param name="BufferSize">Größe des SMB Puffers der verwendet wird, z.B. 16834, wichtig für Performance</param>
        /// <param name="NetworkFlags">Flags zum Steuern des Servers, siehe NetFlag</param>
        /// <param name="Threads">// Maximalanzahl von gleichzeitigen Anfragen, wenn 0 wird ein Standardwert verwendet</param>
        /// <param name="DomainController">Name des Domain Controllers der zur User-Validierung verwendet werden soll. Wird kein DC angegeben,
        /// wird die Klasse UserLogon verwendet.</param>
		void Start(
			string		ServerName,		
			ushort		BufferSize,
			NetFlag		NetworkFlags,
            ushort      Threads,    
            string      DomainController);  

        /// <summary>
        /// Stop den CIFS Server, kann ein paar Sekunden dauern bis alle Threads beeendet sind.
        /// </summary>
        /// <param name="UseForce">Der Wert sollte TRUE sein wenn der Computer heruntergefahren wird und so das Beenden 
        /// besonders massiv durchgeführt werden muss weil sonst ungespeicherte Daten entstehen könnten.</param>
        /// <param name="KillWaitTime">Wartezeit bis alle Threads gekilled wurden in ms, z.B. 5000. Danach wird es mit abort versucht,
        /// es kann nicht garantiert werden dass alle Threads erfolgreich beendet wurden.</param>
		void Stop(bool UseForce, int KillWaitTime);

        /// <summary>
        /// Versetzt den Server in einen Pausemodus. 
        /// Keine neuen Verbindungen werden mehr angenommen.
        /// Bestehende Verbindungen werden ......
        /// Was passiert mit offenen Handles ???
        /// </summary>
        void Pause();

        /// <summary>
        /// Der Server setzt seine Arbeit wieder normal fort. 
        /// Dort wird der Server ggf. noch als Name bei NetBios eingetragen und Announced
        /// </summary>
        void Continue();
	}

    /// <summary>
    /// Type of Response to the Challenge in Authenticiation
    /// </summary>
    public enum AuthenticaionType : int
    {
        /// <summary>
        /// 1. The simplest way, hasing the ASCCI password with the MagicKey, used by Win95
        /// </summary>
        LM = 0,         // Acsii password first 14 byte, MagicHash
        /// <summary>
        /// 2. Hashing the UNICODE password with MD4, used by NT4
        /// </summary>
        NTLM = 1,       // Unicode password, MD4
        /// <summary>
        /// 3. Two round of exchange with MD5
        /// </summary>
        LMv2 = 2,       // Unicode password, MD5
        /// <summary>
        /// 4. ???
        /// </summary>
        NTLMv2 = 3,     // with blob
        /// <summary>
        /// 5. ???
        /// </summary>
        NTLM2 = 4,       // with blob and more
        /// <summary>
        /// ?. Guess login
        /// </summary>
        Anonymous = 5   // Response for anonymous logon in, just 16 null bytes
    }

    public interface IAuth
    {
        /// <summary>
        /// Holt für einen UserNamen das Hash codierte Password
        /// </summary>
        /// <param name="UserName">Username der Anmeldung versucht hat</param>
        /// <param name="PasswordHash">HashCode des Password des Users, entsprechend des AuthenticationTypes</param>
        /// <param name="UserContext">Beschreibt den Context des Users, völlig frei wie dieser ausgestaltet ist.</param>
        /// <returns>Wenn TRUE soll der User ohne weitere Prüfung akzeptiert werden, nur für DEBUG Zwecke wenn keine User-Verwaltung eingebaut ist.</returns>
        bool Logon(string UserName, AuthenticaionType Type, out byte[] PasswordHash, out object UserContext);
    }

    /// <summary>
    /// Basisklasse für den FileContext, wird von jeder IFilesystem Implementierung entsprechend erweitert.
    /// </summary>
    public class FileContext
    {
        /// <summary>
        /// Legt fest ob es sich um eine Datei oder ein Verzeichnis handelt
        /// </summary>
        private bool _IsDirectory = false;
        /// <summary>
        /// Name der Datei oder Verzeichnis
        /// </summary>
        private string _Name = string.Empty;
        
        /// <summary>
        /// Enthält die Unterverzeichnisse und Dateien des aktuellen Verzeichnisses.
        /// Wird durch ReadDirectory gefüllt, ist sonst leer
        /// </summary>
        public List<DirData> Items = null;
        /// <summary>
        /// Enthält die Datei-Filter Maske bei dem ReadDirectory Befehl, 
        /// ist normalerweise leer und oft mit "*" gefüllt
        /// </summary>
        public string Filter = null;

        /// <summary>
        /// Creates a new entry
        /// </summary>
        /// <param name="Name">Name of the file or directory</param>
        /// <param name="IsDirectory">If TRUE we deal with a directroy</param>
        public FileContext(string Name, bool IsDirectory)
        {
            _Name = Name;
            _IsDirectory = IsDirectory;
        }

        public bool IsDirectory
        {
            get { return _IsDirectory; }
        }

        public string Name
        {
            get { return _Name; }
        }

    }

    /// <summary>
    /// Schnittstelle die ein virtuelles Filesystem implementieren muss damit es vom CIFS-Server eingebunden werden kann.
    /// Einbindung erfolgt über die AppConfig.
    /// </summary>
    public interface IFilesystem : IDisposable
	{	
		// Der Rückgabewert ist als DOS-Fehler codiert, d.h. 0 ist OK. Sollte ein Funktion nicht implementiert worden sein
		// muss als Rückgabewert:  0x020040 für Kommando nicht verstanden oder 0x020001 für allgemeiner Fehler zurück
		// gegeben werden

        // UserContext: Wenn null dann wurde beim Logon kein User/Passwort angegeben, also UID=0
        // Es steht der IFilesystem frei wie damit verfahren wird, vermutlich "SMBMessage.ERROR.SERVER_ACCESS"
               
		/// <summary>
        /// Erstellt einen FileContext für die angegebene Datei oder Verzeichnis. SearchFlag legt fest was erwartet wird.
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="Name">Name der Datei oder des Verzeichnisses. Ist der Name leer wird das Root-Verzeichnis des Shares geöffnet</param>
        /// <param name="SearchFlags">Legt fest ob eine Datei, ein Verzechnis geöffnet werden soll.</param>
        /// <param name="Mode">Bestimmt ob eine bestehende Datei oder neue Datei geöffnet werden soll</param>
        /// <param name="Access"></param>
        /// <param name="Share"></param>
        /// <param name="FileObject">Object das auf FileObject basiert und für weitere Dateioperationen verwendet werden kann</param>
        /// <returns>SMBErrorCode</returns>
        /// <remarks>ErrorCodes:
        /// OBJECT_NAME_NOT_FOUND: if the file is not existing
        /// ACCESS_DENIED: if the file e.g. read_only and the FileAccess Write
        /// FILE_IS_A_DIRECORY: if the Name is a directory, but the create is on a file
        /// ...
        /// </remarks>
        NT_STATUS Create(object UserContext, string Name, SearchFlag SearchFlags, FileMode Mode, FileAccess Access, FileShare Share, out FileContext FileObject);
	
        /// <summary>
        /// Schließt das im FileObject angegebene Object. Es kann sich um eine Datei oder um ein Verzeichnis handeln. 
        /// Ein FileContext wird durch die Methode Create erstellt. 
        /// <see cref="Create"/>
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileContext">FileContext, beschreibt das Objekt</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS Close(object UserContext, FileContext FileObject);
		
		/// <summary>
        /// Schließt das im FileObject angegebene Object und setzt die LastWriteTime auf den angegenen Wert,
        /// entspricht dem UNIX touch Kommando. 
        /// Bei dem FileObject Es kann sich um eine Datei oder um ein Verzeichnis handeln. 
        /// Ein FileContext wird durch die Methode Create erstellt. 
        /// <see cref="Create"/>
		/// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileContext">FileContext, beschreibt das Objekt</param>
		/// <param name="LastWriteTime">Datumwert auf den die Dateieigenschaft "LastWriteTime gesetzt wird</param>
		/// <returns>SMBErrorCode</returns>
        NT_STATUS Close(object UserContext, FileContext FileObject, DateTime LastWriteTime);
        				 
        /// <summary>
        /// Deletes a file, directories can not be deleted with this method
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugrtiff ausgeführt wird</param>
        /// <param name="Name">File to delete</param>
        /// <returns>SMBErrorCode</returns>
        /// <remarks>ErrorCodes
        /// ACCESS_DENID: if the file is read only
        /// OBJECT_NAME_NOT_FOUND: if the file is not existing
        /// ...
        /// </remarks>
        NT_STATUS Delete(object UserContext, string Name);
		
        /// <summary>
        /// Liefert die Attribute der angegebenen Datei oder Verzeichnis. 
        /// Konnten Datumswerte nicht ermittelt werden ist, muss DirData.DateTimeFileStart geliefert werden.
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="Name">Datei oder Verzeichnisname</param>
        /// <param name="Attr">Attrbibute der Datei oder des Verzeichnisses</param>
        /// <param name="Flag">Legt fest ob es sich bei Name um eine Dateien, ein Verzeichnis oder beides handeln kann.</param>
        /// <returns>NT_STATUS.OK, OBJECT_NAME_NOT_FOUND, OBJECT_PATH_NOT_FOUND</returns>
        NT_STATUS GetAttributes(object UserContext, string Name, out DirData Attr);

        /// <summary>
        /// Liefert die Attribute des angegebenen FileObjectes. 
        /// Die FileSize-Eigenschaft muss gesetzt werden, die FileAllocationSize kann gesetzt werden wenn diese sich von der FileSize unterscheidet
        /// <remarks>Können Datumswerte nicht ermittelt werden, muss DirData.DateTimeFileStart geliefert werden.</remarks>
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">FileContext der das Object beschreibt</param>
        /// <param name="Attr">Attribute des FileObjectes</param>
        /// <returns>NT_STATUS.OK</returns>
        NT_STATUS GetAttributes(object UserContext, FileContext FileObject, out DirData Attr);
		
        /// <summary>
        /// Setzt die Attribute für das angegebene FileObject. 
        /// Es werden nur die Attributewerte gesetzt die in DirData vorgelegt sind, siehe DirData Definition.
        /// <see cref="DirData Definition"/>
        /// Durch den Parameter DeleteFlag kann eine Datei auch gelöscht werden, ober besser, zum Löschen vorgemerkt werden
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">FileContext der das Object beschreibt</param>
        /// <param name="Attr">Attribute die gesetzt werden sollen</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS SetAttributes(object UserContext, FileContext FileObject, DirData Attr);

        /// <summary>
        /// Gibt die Informationen über die in der Datei enthaltenen "Alternative Data Streams" zurück. Pro vorhandenen Stream wird 
        /// ein DirData Element ausgefüllt, dabei werden nur die Fehler Name, FileSize, AllocationSize verwendet.
        /// Im einfachsten Fall kann einfach NT_STATUS.NOT_IMPLMENTED zurückgegeben werden.
        /// Es wird nur der Stream Name zurückgegeben, ohne Dateiname. Ein Stream Name hat das Format ":xzy:$DATA", 
        /// wobei xzy für den Main Stream leer ist. 
        /// </summary>
        /// <param name="UserContext"></param>
        /// <param name="Name"></param>
        /// <param name="SI"></param>
        /// <returns></returns>
        NT_STATUS GetStreamInfo(object UserContext, string Name, out List<DirData>StreamInfo);

		/// <summary>
		/// Liest den Inhalt eines Verzeichnisses, also Unterverzeichnisse und Dateien und speichert
		/// </summary>
		/// <param name="UserContext"></param>
		/// <param name="FileObject"></param>
		/// <returns></returns>
        NT_STATUS ReadDirectory(object UserContext, FileContext FileObject);

		/// <summary>
		/// Creates a new directory with the given attributes
		/// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
		/// <param name="DirName">Name of the directory</param>
		/// <param name="Attributes">Attrbutes of the new directory</param>
		/// <returns>SMBErrorCode</returns>
        /// <remarks>Error codes
        /// NT_Status.OBJECT_NAME_COLLISION: If the Name already exists as File or Directory
        /// ...
        /// </remarks>
        NT_STATUS CreateDirectory(object UserContext, string DirName, FileAttributes Attributes);

        /// <summary>
        /// Löscht das angegebene Verzeichnis (Fehler 5 wenn Verzeichnis ReadOnly oder nicht Leer)
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="DirName">Name des Verzeichnisses das gelöscht wird</param>
        /// <returns>SMBErrorCode
        /// 3:      If the Directory is not existing
        /// 5:      If the Directory is read-only or if the root directory is deleted
        /// 145:    If the Directory is not empty
        NT_STATUS DeleteDirectory(object UserContext, string DirName);

		/// <summary>
        /// Gibt beschreibende des Filesystem wie Sektoren, Größe und freien Platz zurück
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="data">Sammlung von Filesystem Attributen</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS FSInfo(object UserContext, out FileSystemAttributes data);
		
		/// <summary>
        /// Liest Bytes aus einem FileObject in einen ByteBuffer
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">FileObject aus dem gelesen werden soll</param>
        /// <param name="Offset">Position ab der aus der Datei gelesen werden soll</param>
        /// <param name="Count">[in]Anzahl von Bytes die gelesen werden sollen, [out]Anzahl der Bytes die gelesen wurden</param>
        /// <param name="Buffer">Buffer in den die gelesenen Bytes geschrieben werden</param>
        /// <param name="BufferStart">Position im Buffer an der das erste gelesene Byte geschrieben werden muss</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS Read(object UserContext, FileContext FileObject, long Offset, ref int Count, ref byte[] Buffer, int BufferStart);

        /// <summary>
        /// Schreibt Bytes einem ByteBuffer in das angegebene FileObject
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">FileObject in das geschrieben werden soll</param>
        /// <param name="Offset">Position ab der in die Datei geschrieben werden soll</param>
        /// <param name="Count">[in]Anzahl von Bytes die geschrieben werden sollen, [out]Anzahl der Bytes die geschrieben wurden</param>
        /// <param name="Buffer">Buffer der die zu schreibeneden Bytes enthält</param>
        /// <param name="BufferStart">Position im Buffer an der das erste zu schreibene Byte steht</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS Write(object UserContext, FileContext FileObject, long Offset, ref int Count, ref byte[] Buffer, int BufferStart);

        /// <summary>
        /// Sperrt einen Bereich in einer Datei vor dem Zugriff von anderen Anwendungen, muss also nur implementiert 
        /// werden verschiedene Programme gleichzeitig auf die Dateien zugreifen können. Gemeinsame Zugriffe über ein 
        /// IFilesystem werden intern verarbeitet.
        /// Eine Minimal-Implementierung gibt NT_STATUS.OK oder NT_STATUS.NOT_IMPLEMENTED zurück, alle anderen Fehlercodes werden an den Client 
        /// weitergereicht.
        /// Hinweis: Es können auch Bereiche außerhalb der Dateigrenze gesperrt werden, das ist mit C# Mittel nicht möglich
        /// sollte aber keine Fehler zurückgeben.
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">FileObject</param>
        /// <param name="Offset">Position ab der Bytes gesperrt werden sollen, kann größer als die Dateiänge sein</param>
        /// <param name="Length">Länge des Blocks der gesperrt werden soll</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS Lock(object UserContext, FileContext FileObject, long Offset, long Length);
		
        /// <summary>
        /// Entsperrt einen Bereich in einer Datei. Auch ein nicht gesperrter Bereich kann entsperrt werden.
        /// Siehe Lock für mehr Details.
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">FileObject</param>
        /// <param name="Offset">Position ab der Bytes entsperrt werden sollen, kann größer als die Dateiänge sein</param>
        /// <param name="Length">Länge des Blocks der entsperrt werden soll</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS Unlock(object UserContext, FileContext FileObject, long Offset, long Length);

        /// <summary>
        /// Executes a DeviceIO operation on a device or file
        /// See the sample implementation in DummyFS
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">The FileObject that is used if IsFsctl is TRUE. If IsFsctl is false it should be the device handle. Not implmented yet!!??</param>
        /// <param name="Command">The command that should be executed</param>
        /// <param name="IsFsctl">If TRUE the command is executed on a file base, 
        /// If else the command is executed on the device</param>
        /// <param name="Input">Contains the parameters of the IOCTL function</param>
        /// <param name="Output">Contains the result of the function call, the array is pre-allocated with the maximum length.</param>
        /// <param name="ValidLength">Number of bytes that are valid in the Output Array</param>
        /// <returns></returns>
        NT_STATUS DeviceIO(object UserContext, FileContext FileObject, int Command, bool IsFsctl, ref byte[] Input, ref byte[] Output, ref int ValidLength);		

        /// <summary>
        /// Stellt sicher dass alle Daten auf das permanente Medium geschrieben werden und 
        /// sollte erst dann einen Wert zurückgeben, also nicht asynchron implementieren!
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="FileObject">FileObject das auf persistiert werden soll</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS Flush(object UserContext, FileContext FileObject);

        /// <summary>
        /// Tells the Filesystem that the server is stopped now. The Filesystem must close/release all open resources.
        /// This must be done quite fast, otherwise the service controller from the operating system kills the process.
        /// </summary>
        /// ehemals: void ShutDown();

        /// <summary>
        /// Renames/Moves a file or diectory, OldName and NewName must be of the same type. A Move is done when the path of 
        /// OldName and NewName are diffenent
        /// </summary>
        /// <param name="UserContext">UserContext in dem der Zugriff ausgeführt wird</param>
        /// <param name="OldName">Orginal name of the file or directory, must be existing</param>
        /// <param name="NewName">New name of the file or directory, must nor exist</param>
        /// <returns>SMBErrorCode</returns>
        /// <remarks>Errors:
        /// NT_STATUS_OBJECT_NAME_COLLISION: if NewName is already existing
        /// 
        /// </remarks>
        NT_STATUS Rename(object UserContext, string OldName, string NewName);
        		
        /// <summary>
        /// Liefert Information über den FileShare, muss implementiert werden
        /// <remarks>Der NET VIEW Befehl holt sich die Information über diese Methode</remarks>
        /// </summary>
        /// <param name="Service">Type des Services, "A:" für FileShare oder "IPC" für Named Pipe</param>
        /// <param name="NativeFileSystem">Namen des Filesystems auf dem der Share arbeitet, beliebig, z.B. FAT, NTFS, ...</param>
        /// <param name="Comment">Beschreibung des Shares, beliebig</param>
        /// <returns>SMBErrorCode</returns>
        NT_STATUS GetService(out string Service, out string NativeFileSystem, out string Comment);
	}
	
    /// <summary>
    /// Klasse zum Austausch von Verzeichnis/Datei Attributen.
    /// Können Datumswerte nicht ermittelt werden oder sollen sie nicht verändert werden,
    /// wird das Datum DateTimeFileStart verwendet.
    /// </summary>
    [Serializable()]
	public class DirData
	{
        /// <summary>
        /// Konstante die das kleinste mögliche Datum für eine Datei darstellt
        /// </summary>
        [NonSerialized()]
        public static DateTime DateTimeFileStart = new DateTime(1601, 1, 1, 0, 0, 0);

        public static DateTime DataToDateTime(long Convert)
        {
            if (Convert == -1 || Convert == 0)
                return DirData.DateTimeFileStart;
            else
                return DateTime.FromFileTimeUtc(Convert);
        }

        /// <summary>
        /// Is the amount of space that is reserved on the storage media. 
        /// Will be filled by GetAttributes, ReadDirectory, ... and not used in SetAttributes
        /// </summary>
        public long AllocationSize = -1;            
        
        /// <summary>
        /// Internal store for the FileSize
        /// </summary>
		private long EndOfFile = -1;

        /// <summary>
        /// Is the size of the file. Is filled by GetAttributes and written by SetAttributes to the media if != -1
        /// </summary>
        public long FileSize
        {
            get { return EndOfFile; }
            set
            {
                EndOfFile = value;
                if (value > AllocationSize)
                    AllocationSize = value;
            }
        }
		
        /// <summary>
        /// Is the file attributes, will be filled by GetAttributes and written to the media if value is !- -1
        /// </summary>
		public FileAttributes Attrib = (FileAttributes)(-1);

        // Datumswerte werden im Regelfall gelesen. Geschreiben nur wenn != SMBMessage.FileTimeStart
        public DateTime LastAccessTime = DateTimeFileStart;		
        public DateTime LastWriteTime = DateTimeFileStart;		
        public DateTime CreationTime =  DateTimeFileStart;		
        /// <summary>
        /// Contains the file or directory name
        /// </summary>
		public string Name;
        /// <summary>
        /// Contains the short (8.3) file or directory name
        /// </summary>
        public string ShortName = null;

        public DirData(string name, FileAttributes attrib)
        {
            Name = name;
            Attrib = attrib;
        }
        public DirData() { }
	}

	/// <summary>
	/// Klasse zum Austausch von Volumen-Informationen
	/// </summary>
	public struct FileSystemAttributes 
	{
        /// <summary>
        /// Name des Filesystems
        /// </summary>
		public string FSName; 
        /// <summary>
        /// Komplettanzahl der vorhandenen Bytes
        /// </summary>
		public long TotalBytes;
        /// <summary>
        /// Anzahl der freien Bytes
        /// </summary>
		public long FreeBytes;
        /// <summary>
        /// Anzahl der Bytes pro Sektor ?
        /// </summary>
		public uint SectorUnit;
        /// <summary>
        /// Anzahl der Sektoren
        /// </summary>
		public uint Sectors;
        /// <summary>
        /// Stores the unique object ID of the Volume, is also used for the old-faishend Volume ID
        /// </summary>
        public byte[] ObjectID;
        /// <summary>
        /// Attributes of the File System, is defined in the type FILE_FS_ATTRIBUTE_INFORMATION
        /// </summary>
        public FILE_FS_ATTRIBUTE_INFORMATION FSAttributes;
	};

    /// <summary>
    /// Legt fest ob eine Datei, ein Verzeichnis oder beides in Create-Befehlen gesucht, geöffnet oder erstellt werden soll.
    /// Es wird immer versucht entweder Datei oder Verzeichnis anzugeben, nur wenn es nicht ermittelt werden
    /// kann wird "Datei oder Verzeichnis" angefragt, dann muss die Create Methode entscheiden um was es sich handelt.
    /// </summary>
	public enum SearchFlag
	{
        /// <summary>
        /// Es werden nur Dateien gesucht, geöffnet
        /// </summary>
		File		=  0,
        /// <summary>
        /// Es werden nur Verzeichnisse gesucht, geöffnet
        /// </summary>
		Dir			= 16,
        /// <summary>
        /// Es werden sowohl Verzeichnisse als auch Dateien gesucht, geöffnet. 
        /// Die aufgerufene Methode muss selbst entscheiden um was es sich handelt
        /// </summary>
		FileAndDir	= -1
	}

    /// <summary>
    /// NT Error code used in SMB statements
    /// </summary>
    public enum NT_STATUS : uint
    {
        OK                      = 0,

        BUFFER_OVERFLOW         = 0x80000005,

        UNSUCCESSFUL            = 0xC0000001,
        NOT_IMPLEMENTED         = 0xC0000002,

        //INFO_LENGTH_MISMATCH    = 0xC0000004,

        INVALID_HANDLE          = 0xC0000008,

        INVALID_PARAMETER       = 0xC000000D,

        NO_SUCH_FILE            = 0xC000000F,

        INVALID_DEVICE_REQUEST  = 0xC0000010,
        ACCESS_DENIED           = 0xC0000022,
        BUFFER_TOO_SMALL        = 0xC0000023,

        OBJECT_NAME_INVALID     = 0xC0000033,
        OBJECT_NAME_NOT_FOUND   = 0xC0000034,
        OBJECT_NAME_COLLISION   = 0xC0000035,

        OBJECT_PATH_NOT_FOUND   = 0xC000003A,

        SHARING_VIOLATION       = 0xC0000043,

        FILE_LOCK_CONFLICT      = 0xC0000054,

        LOGON_FAILURE           = 0xC000006D,    

        ILLEGAL_FUNCTION        = 0xC00000AF,

        FILE_IS_A_DIRECTORY     = 0xC00000BA,
        NOT_SUPPORTED           = 0xC00000BB,

        BAD_DEVICE_TYPE         = 0xC00000CB,
        BAD_NETWORK_NAME        = 0xC00000CC,

        REQUEST_NOT_ACCEPTED    = 0xC00000D0,

        INVALID_OPLOCK_PROTOCOL = 0xC00000E3,

        DIRECTORY_NOT_EMPTY     = 0xC0000101,
        
        CANCELLED               = 0xC0000120,

        INVALID_LEVEL           = 0xC0000148,

        DOMAIN_CONTROLLER_NOT_FOUND 
                                = 0xC0000233,

        CONNECTION_INVALID      = 0xC000023A,

        ADAPTER_HARDWARE_ERROR  = 0xC00000C2,  // Read/Write error ?
        IO_DEVICE_ERROR         = 0xC0000185,  // Read/Write error ?
        NET_WRITE_FAULT         = 0xC00000D2   // Read/Write error ?
        
    }

    [Flags]
    public enum FILE_FS_ATTRIBUTE_INFORMATION : uint
    {
        // Values are from a free ntifs.h and checked with some network sniffers

        None                        = 0,
        /// <summary>
        /// The file system supports case-sensitive file names. 
        /// </summary>
        FILE_CASE_SENSITIVE_SEARCH  = 0x00000001,
        /// <summary>
        /// The file system preserves the case of file names when it places a name on disk. 
        /// </summary>
        FILE_CASE_PRESERVED_NAMES   = 0x00000002,
        /// <summary>
        /// The file system supports Unicode in file names.  
        /// </summary>
        FILE_UNICODE_ON_DISK        = 0x00000004,
        /// <summary>
        /// The file system preserves and enforces access control lists (ACL).  
        /// </summary>
        FILE_PERSISTENT_ACLS        = 0x00000008,
        /// <summary>
        /// The file system supports file-based compression. This flag is incompatible with the FILE_VOLUME_IS_COMPRESSED flag.  
        /// </summary>
        FILE_FILE_COMPRESSION       = 0x00000010,
        /// <summary>
        /// The file system supports per-user quotas.  
        /// </summary>
        FILE_VOLUME_QUOTAS          = 0x00000020,
        /// <summary>
        /// The file system supports sparse files.  
        /// </summary>
        FILE_SUPPORTS_SPARSE_FILES  = 0x00000040,
        /// <summary>
        /// The file system supports reparse points.  
        /// </summary>
        FILE_SUPPORTS_REPARSE_POINTS = 0x00000080,
        /// <summary>
        /// The file system supports remote storage.  
        /// </summary>
        FILE_SUPPORTS_REMOTE_STORAGE = 0x00000100,
        FS_LFN_APIS = 0x00004000,
        /// <summary>
        /// The specified volume is a compressed volume. This flag is incompatible with the FILE_FILE_COMPRESSION flag.  
        /// </summary>
        FILE_VOLUME_IS_COMPRESSED   = 0x00008000,
        /// <summary>
        /// The file system supports object identifiers.  
        /// </summary>
        FILE_SUPPORTS_OBJECT_IDS    = 0x00010000,
        /// <summary>
        /// The file system supports the Encrypted File System (EFS).  
        /// </summary>
        FILE_SUPPORTS_ENCRYPTION    = 0x00020000,
        /// <summary>
        /// The file system supports named streams.  
        /// </summary>
        FILE_NAMED_STREAMS          = 0x00040000,
        /// <summary>
        /// Microsoft Windows XP and later: The specified volume is read-only.  
        /// </summary>
        FILE_READ_ONLY_VOLUME       = 0x00080000

        //Values from Leach, but seams to be wrong !
        //FILE_CASE_SENSITIVE_SEARCH   0x00000001
        //FILE_CASE_PRESERVED_NAMES    0x00000002
        //FILE_PRSISTENT_ACLS          0x00000004
        //FILE_FILE_COMPRESSION        0x00000008
        //FILE_VOLUME_QUOTAS           0x00000010
        //FILE_DEVICE_IS_MOUNTED       0x00000020
        //FILE_VOLUME_IS_COMPRESSED    0x00008000
    }

    public class BaseFileSystem
    {
        public NT_STATUS GetService(out string Service, out string NativeFileSystem, out string Comment)
        {
            Service = "A:";
            NativeFileSystem = "PalissimoFS"; //  "NTFS";
            Comment = "Filesystem based on WinFUSE from Palissimo GmbH";
            return NT_STATUS.OK;
        }

        public NT_STATUS FSInfo(out FileSystemAttributes data)
        {
            data = new FileSystemAttributes();
            data.FSName = "Virtual WinFUSE volume";
            data.TotalBytes = 0x1FFFFFFFF;				// Eigentlicht Anzahl der Sektoren, berichtet irgendwelche Dummy-Werte

            data.FreeBytes = 0x1FFFFFFFF / 1024;
            data.SectorUnit = 1;
            data.Sectors = 1024;

            // These two features should be supported by any implementation, all FAT filesystems set these flags !
            data.FSAttributes = FILE_FS_ATTRIBUTE_INFORMATION.FILE_CASE_PRESERVED_NAMES | FILE_FS_ATTRIBUTE_INFORMATION.FILE_UNICODE_ON_DISK;

            // Add some more flags in your implementation if your filesystem supports more features, like Streams, ACL, ...
            // XP SP2 reports 000700FF

            //data.FSAttributes |= FILE_FS_ATTRIBUTE_INFORMATION.FILE_PERSISTENT_ACLS;
            //if added the security options must be supported and the LSRPC pipe must be implemented in the IPC$ share

            //data.FSAttributes |= FILE_FS_ATTRIBUTE_INFORMATION.FILE_SUPPORTS_ENCRYPTION;
            //data.FSAttributes |= FILE_FS_ATTRIBUTE_INFORMATION.FILE_SUPPORTS_OBJECT_IDS;
            //data.FSAttributes |= FILE_FS_ATTRIBUTE_INFORMATION.FILE_VOLUME_QUOTAS;

            return NT_STATUS.OK;
        }
    }

}
