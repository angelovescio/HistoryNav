using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using FILE = System.IO.TextWriter;
using GETPROCTIMES = System.IntPtr;
using HANDLE = System.IntPtr;
using HINSTANCE = System.IntPtr;
using sqlite3_int64 = System.Int64;
using u32 = System.UInt32;
using va_list = System.Object;
using clean = CleanSqlite;
using Sqlite3 = CleanSqlite.Sqlite3;

namespace HistoryNav
{
    using dxCallback = Sqlite3.dxCallback;
    using FILETIME = Sqlite3.FILETIME;
    using sqlite3 = Sqlite3.sqlite3;
    using sqlite3_stmt = Sqlite3.Vdbe;
    using sqlite3_value = Sqlite3.Mem;
    using System.IO;
    using System.Windows.Forms;
    using System.IO.Compression;
    using System.Security.Cryptography;
    using System.Runtime.InteropServices;
    using System.Data;

    public class Database
    {
        class previous_mode_data
        {
            public bool valid;        /* Is there legit data in here? */
            public int mode;
            public bool showHeader;
            public int[] colWidth = new int[200];
        };
        /*
       ** An pointer to an instance of this structure is passed from
       ** the main program to the callback.  This is used to communicate
       ** state and mode information.
       */
        class callback_data
        {
            public Sqlite3.sqlite3 db;            /* The database */
            public bool echoOn;           /* True to echo input commands */
            public bool statsOn;          /* True to display memory stats before each finalize */
            public int cnt;               /* Number of records displayed so far */
            public FILE Out;             /* Write results here */
            public int mode;              /* An output mode setting */
            public bool writableSchema;  /* True if PRAGMA writable_schema=ON */
            public bool showHeader;      /* True to show column names in List or Column mode */
            public string zDestTable;     /* Name of destination table when MODE_Insert */
            public string separator = ""; /* Separator character for MODE_List */
            public int[] colWidth = new int[200];      /* Requested width of each column when in column mode*/
            public int[] actualWidth = new int[200];   /* Actual width of each column */
            public string nullvalue = "NULL";          /* The text to print when a null comes back from
** the database */
            public previous_mode_data explainPrev = new previous_mode_data();
            /* Holds the mode information just before
            ** .explain ON */
            public StringBuilder outfile = new StringBuilder(260); /* Filename for Out */
            public string zDbFilename;    /* name of the database file */
            public string zVfs;           /* Name of VFS to use */
            public sqlite3_stmt pStmt;   /* Current statement if any. */
            public FILE pLog;            /* Write log output here */

            internal callback_data Copy()
            {
                return (callback_data)this.MemberwiseClone();
            }
        };

        clean.Sqlite3.sqlite3 pDb;
        public static int NumberOfThreads = 0;
        static readonly object Locker = new object();
        int rc;
        static List<string> tables = new List<string>();
        string dbToMergeWith;
        callback_data data = null;
        public Database(string outputDb)
        {
            if (pDb == null)
            {
                rc = 0;
                dbToMergeWith = outputDb;
                NumberOfThreads = 0;
            }
            else
            {
                NumberOfThreads++;
            }
        }
        /*
        ** Initialize the state information in data
        */
        public void main_init()
        {

            data = new callback_data();//memset(data, 0, sizeof(*data));
            //data.mode = MODE_List;
            data.separator = "|";//memcpy(data.separator, "|", 2);
            data.showHeader = false;
            Sqlite3.sqlite3_initialize();
            Sqlite3.sqlite3_config(Sqlite3.SQLITE_CONFIG_URI, 1);
            Sqlite3.sqlite3_config(Sqlite3.SQLITE_CONFIG_LOG, new object[] { (Sqlite3.dxLog)shellLog, data, null });
            Sqlite3.sqlite3_config(Sqlite3.SQLITE_CONFIG_SERIALIZED);
            AttachDB(dbToMergeWith);
        }
        /*
      ** A callback for the Sqlite3.SQLITE_log() interface.
      */
        static void shellLog(object pArg, int iErrCode, string zMsg)
        {
            callback_data p = (callback_data)pArg;
            if (p.pLog == null)
                return;
            Debug.Print("(%d) %s\n", iErrCode, zMsg);
            //fflush(p.pLog);
        }
        public void AttachDB(string filename)
        {
            int rc = 0;
            pDb = null;
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists)
                {
                    //fi.Delete();
                }
            }
            catch (Exception ex)
            {

            }
            rc = Sqlite3.sqlite3_open(filename, out pDb);
        }
        public DataSet GetDBTables()
        {
            DataSet fredDs = new DataSet("FRED");
            List<string> tables = run_table_names_query(pDb);
            int rc = 0;
            foreach (string table in tables)
            {
                bool firstRun = true;
                List<string> names = new List<string>();
                List<string> arrRows = new List<string>();
                List<string> colNames = run_column_name_query(pDb, table);
                string zSql = "SELECT * from " + table + ";";
                sqlite3_stmt pSelect = new sqlite3_stmt();
                string sDummy = null;
                rc = Sqlite3.sqlite3_prepare(pDb, zSql, -1, ref pSelect, ref sDummy);
                if (rc != Sqlite3.SQLITE_OK || null == pSelect)
                {
                    continue;
                }
                rc = Sqlite3.sqlite3_step(pSelect);
                DataTable dt = new DataTable(table);
                while (rc == Sqlite3.SQLITE_ROW)
                {
                    //Get the data in the cell
                    int total = Sqlite3.sqlite3_column_count(pSelect);
                    DataRow dr = dt.NewRow();

                    List<string> values = new List<string>();
                    for (int i = 0; i < total; i++)
                    {
                        int valInt = 0;
                        string value = "";
                        sqlite3_value val = Sqlite3.sqlite3_column_value(pSelect, i);
                        if (val.type == 1)
                        {
                            valInt = Sqlite3.sqlite3_column_int(pSelect, i);
                            value = valInt.ToString();
                        }
                        else
                        {
                            value = val.z;

                        }
                        values.Add(value);
                        if (firstRun)
                        {
                            string name = Sqlite3.sqlite3_column_name(pSelect, i);
                            if (name == null)
                            {
                                continue;
                            }
                            names.Add(name);
                            dt.Columns.Add(name);

                        }
                    }
                    rc = Sqlite3.sqlite3_step(pSelect);
                    firstRun = false;
                    for (int i = 0; i < names.Count && i < values.Count; i++)
                    {
                        dr[names[i]] = values[i];
                    }
                    dt.Rows.Add(dr);
                }
                Sqlite3.sqlite3_finalize(pSelect);
                fredDs.Tables.Add(dt);
            }
            Sqlite3.sqlite3_close(pDb);
            return fredDs;
        }
        /// <summary>
        /// Retrieves the name of all the tables within the DB
        /// </summary>
        /// <param name="db">pointer to database</param>
        /// <param name="zSelect"></param>
        /// <returns>List of tablenames in database</returns>
        List<string> run_table_names_query(sqlite3 db)
        {
            List<string> tableNames = new List<string>();
            string getTables = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";

            sqlite3_stmt pSelect = new sqlite3_stmt();
            int rc;
            string sDummy = null;
            rc = Sqlite3.sqlite3_prepare(db, getTables, -1, ref pSelect, ref sDummy);
            if (rc != Sqlite3.SQLITE_OK || null == pSelect)
            {
                return tableNames;
            }
            rc = Sqlite3.sqlite3_step(pSelect);
            while (rc == Sqlite3.SQLITE_ROW)
            {
                tableNames.Add(Sqlite3.sqlite3_column_text(pSelect, 0));

                rc = Sqlite3.sqlite3_step(pSelect);
            }
            Sqlite3.sqlite3_finalize(pSelect);
            return tableNames;
        }
        List<string> run_column_name_query(sqlite3 db, string table)
        {
            List<string> columnNames = new List<string>();
            string sql = "PRAGMA table_info(" + table + ");";
            sqlite3_stmt pSelect = new sqlite3_stmt();
            int rc;
            string sDummy = null;
            rc = Sqlite3.sqlite3_prepare(db, sql, -1, ref pSelect, ref sDummy);
            if (rc != Sqlite3.SQLITE_OK || null == pSelect)
            {
                return columnNames;
            }
            rc = Sqlite3.sqlite3_step(pSelect);
            while (rc == Sqlite3.SQLITE_ROW)
            {
                sqlite3_value val = Sqlite3.sqlite3_column_value(pSelect, 1);
                columnNames.Add(val.z);
                rc = Sqlite3.sqlite3_step(pSelect);
            }
            Sqlite3.sqlite3_finalize(pSelect);
            return columnNames;
        }

        void ExecSql(string statement, string table, bool isCreate)
        {
            if (NumberOfThreads > 1)
            {
                Monitor.Enter(Locker);
                try
                {
                    if (isCreate)
                    {
                        tables.Add(table);
                    }
                    Sqlite3.sqlite3_exec(pDb, statement, 0, 0, 0);
                    rc = Sqlite3.sqlite3_errcode(pDb);
                    //Monitor.Pulse(Locker);
                }
                finally
                {
                    Monitor.Exit(Locker);
                    //Debug.Print(String.Format("Section {0} Thread {1} released Locker", table, Thread.CurrentThread.ManagedThreadId));
                }
            }
            else
            {
                if (isCreate)
                {
                    tables.Add(table);
                }
                Sqlite3.sqlite3_exec(pDb, statement, 0, 0, 0);
                rc = Sqlite3.sqlite3_errcode(pDb);
            }
        }
        public void RemoveThreadNumber()
        {
            NumberOfThreads--;
        }
        public void DiskMerge()
        {
            string strQueryFront = "ATTACH database '" + dbToMergeWith + "' AS disk;";
            Sqlite3.sqlite3_exec(pDb, strQueryFront, 0, 0, 0);
            rc = Sqlite3.sqlite3_errcode(pDb);
            if (rc == 0)
            {
                foreach (string table in tables)
                {
                    Sqlite3.sqlite3_exec(pDb, "CREATE TABLE disk." + table + " AS SELECT * FROM " + table + ";", 0, 0, 0);
                    rc = Sqlite3.sqlite3_errcode(pDb);
                    if (rc != 0)
                    {
                        Sqlite3.sqlite3_close(pDb);
                        return;
                    }

                }
                rc = Sqlite3.exec(pDb, "DETACH disk;", 0, 0, 0);
            }
            Sqlite3.sqlite3_close(pDb);
        }
        #region sqlite creation and insertion
        string insertStatement;
        /// <summary>
        /// Creates a table, string format should be "field type primary key, nextfield type"
        /// </summary>
        /// <returns>void</returns>
        public void Create(string table, string[] cols)
        {
            string strQueryFront = "CREATE TABLE " + table + " (";
            if (insertStatement == "")
            {
                insertStatement = "INSERT INTO " + table + " (";
                for (int i = 0; i < cols.Length; i++)
                {
                    if (i < cols.Length - 1)
                    {
                        strQueryFront += cols[i] + ", ";
                        cols[i] = cols[i].Replace(" TEXT", "");
                        cols[i] = cols[i].Replace(" INTEGER PRIMARY KEY ASC", "");
                        cols[i] = cols[i].Replace(" INTEGER PRIMARY KEY", "");
                        cols[i] = cols[i].Replace(" INTEGER", "");
                        cols[i] = cols[i].Replace(" NONE", "");
                        insertStatement += cols[i] + ", ";
                    }
                    else
                    {
                        strQueryFront += cols[i] + ");";
                        cols[i] = cols[i].Replace(" TEXT", "");
                        cols[i] = cols[i].Replace(" INTEGER PRIMARY KEY ASC", "");
                        cols[i] = cols[i].Replace(" INTEGER PRIMARY KEY", "");
                        cols[i] = cols[i].Replace(" INTEGER", "");
                        cols[i] = cols[i].Replace(" NONE", "");
                        insertStatement += cols[i] + ") VALUES (";
                    }
                }
            }
            else
            {
                insertStatement = "";
                Create(table, cols);
                return;
            }
            ExecSql(strQueryFront, table, true);
        }
        public void Insert(string table, string[] values)
        {
            string strQueryFront = insertStatement;
            string strQueryEnd = "";
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == "NULL")
                {
                    if (i < values.Length - 1)
                    {
                        strQueryEnd += "NULL,";
                    }
                    else
                    {
                        strQueryEnd += "NULL);";
                    }
                }
                else
                {
                    if (i < values.Length - 1)
                    {
                        strQueryEnd += "'" + values[i] + "', ";
                    }
                    else
                    {
                        strQueryEnd += "'" + values[i] + "');";
                    }
                }
            }
            strQueryFront += strQueryEnd;
            ExecSql(strQueryFront, table, false);

        }
        #endregion
    }
}
