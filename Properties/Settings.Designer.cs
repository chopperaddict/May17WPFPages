﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WPFPages.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False")]
        public string ConnectionString {
            get {
                return ((string)(this["ConnectionString"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False")]
        public string BankSysConnectionString {
            get {
                return ((string)(this["BankSysConnectionString"]));
            }
            set {
                this["BankSysConnectionString"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"Data Source=DINO-PC; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF';  Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")]
        public string backupbanksysstring {
            get {
                return ((string)(this["backupbanksysstring"]));
            }
            set {
                this["backupbanksysstring"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\ian1.mdf;Inte" +
            "grated Security=True;Connect Timeout=30")]
        public string LocalDataConnectionString {
            get {
                return ((string)(this["LocalDataConnectionString"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"Data Source=DINO-PC; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF';  Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")]
        public string Previousconnstring {
            get {
                return ((string)(this["Previousconnstring"]));
            }
            set {
                this["Previousconnstring"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=\"C:\\USERS\\IANCH\\APPDATA\\LOCAL\\" +
            "MICROSOFT\\MICROSOFT SQL SERVER LOCAL DB\\INSTANCES\\MSSQLLOCALDB\\IAN1.MDF\";Integra" +
            "ted Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False")]
        public string C__USERS_IANCH_APPDATA_LOCAL_MICROSOFT_MICROSOFT_SQL_SERVER_LOCAL_DB_INSTANCES_MSSQLLOCALDB_IAN1_MDFConnectionString {
            get {
                return ((string)(this["C__USERS_IANCH_APPDATA_LOCAL_MICROSOFT_MICROSOFT_SQL_SERVER_LOCAL_DB_INSTANCES_MS" +
                    "SQLLOCALDB_IAN1_MDFConnectionString"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string DetailsDbView_dindex {
            get {
                return ((string)(this["DetailsDbView_dindex"]));
            }
            set {
                this["DetailsDbView_dindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string Multi_bindex {
            get {
                return ((string)(this["Multi_bindex"]));
            }
            set {
                this["Multi_bindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string Multi_cindex {
            get {
                return ((string)(this["Multi_cindex"]));
            }
            set {
                this["Multi_cindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string Multi_dindex {
            get {
                return ((string)(this["Multi_dindex"]));
            }
            set {
                this["Multi_dindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string BankDbView_bindex {
            get {
                return ((string)(this["BankDbView_bindex"]));
            }
            set {
                this["BankDbView_bindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string CustDbView_cindex {
            get {
                return ((string)(this["CustDbView_cindex"]));
            }
            set {
                this["CustDbView_cindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string SqlDbViewer_bindex {
            get {
                return ((string)(this["SqlDbViewer_bindex"]));
            }
            set {
                this["SqlDbViewer_bindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string SqlDbViewer_cindex {
            get {
                return ((string)(this["SqlDbViewer_cindex"]));
            }
            set {
                this["SqlDbViewer_cindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string SqlDbViewer_dindex {
            get {
                return ((string)(this["SqlDbViewer_dindex"]));
            }
            set {
                this["SqlDbViewer_dindex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\ianch\\Documents\\searchpaths.dat")]
        public string SearchPathFile {
            get {
                return ((string)(this["SearchPathFile"]));
            }
            set {
                this["SearchPathFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DocumentsPath {
            get {
                return ((string)(this["DocumentsPath"]));
            }
            set {
                this["DocumentsPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Notepad++.exe")]
        public string DefaultTextviewer {
            get {
                return ((string)(this["DefaultTextviewer"]));
            }
            set {
                this["DefaultTextviewer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Bank Db Viewer")]
        public string StartupWindow {
            get {
                return ((string)(this["StartupWindow"]));
            }
            set {
                this["StartupWindow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=C:\\WPFPAGES2-RECOVERED\\BIN\\DEB" +
            "UG\\NORTHWND.MDF;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustS" +
            "erverCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")]
        public string Setting {
            get {
                return ((string)(this["Setting"]));
            }
            set {
                this["Setting"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\VISUALSTUDIO\16.0_0827B8C0\DESIGNER\CACHE\1928924530X86DD\NWND.MDF;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")]
        public string NorthwindConnectionStringold {
            get {
                return ((string)(this["NorthwindConnectionStringold"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Northwind;Integrated Security=" +
            "True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIn" +
            "tent=ReadWrite;MultiSubnetFailover=False")]
        public string NorthwindConnectionString {
            get {
                return ((string)(this["NorthwindConnectionString"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\WPFPages2-Recovered")]
        public string AppRoot {
            get {
                return ((string)(this["AppRoot"]));
            }
            set {
                this["AppRoot"] = value;
            }
        }
    }
}
