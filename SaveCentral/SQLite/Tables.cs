using SaveCentral.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveCentral.SQLite
{
    class Tables
    {
        public void CreateTables()
        {
            SQLiteHelper db = new SQLiteHelper();
            db.DeleteDB();
            string Query = " CREATE TABLE IF NOT EXISTS " + Constants.files_data + " (" +
                            " Username VARCHAR(100) NOT NULL," +
                            " FileName VARCHAR(100) NOT NULL PRIMARY KEY," +
                            " GameName VARCHAR(100) NOT NULL," +
                            " SaveType VARCHAR(1) NOT NULL," +
                            " Region VARCHAR(1) NOT NULL," +
                            " Size VARCHAR(20)," +
                            " Title VARCHAR(100)," +
                            " Description TEXT," +
                            " FilesIncluded TEXT," +
                            " HasExtData VARCHAR(1)," +
                            " DLCount INT NOT NULL DEFAULT 0," +
                            " Date_Created DATETIME," +
                            " Date_Modif DATETIME );";
            db.ExecuteNonQuery(Query);
        }
    }
}
