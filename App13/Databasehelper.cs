using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Database.Sqlite;
using Android.Database;
using Android.Graphics;
namespace App13
{
    class Databasehelper : SQLiteOpenHelper
    {
        public static int version = 1;
        public static readonly string DATABASE_NAME = "ContentBase";
        public static readonly string TEXTTABLE = "TextTable";
        public static readonly string CONTENTTABLE = "ContentTable";
        public static string COLUMN_ID = "_id";
        public static string COLUMN_NUMBERNOTE = "NumberNote";
        public readonly static string COLUMN_TEXT = "ColumnText";
        public readonly static string COLUMN_BITMAP = "ColumnBitmap";
        public Databasehelper(Context context) : base(context, DATABASE_NAME, null, version)
        {
        } // create two tables
        public override void OnCreate(SQLiteDatabase db)
        {
            db.ExecSQL("CREATE TABLE IF NOT EXISTS Texttable (" + COLUMN_ID
                    + " INTEGER PRIMARY KEY AUTOINCREMENT," + COLUMN_TEXT //for listview
                    + " TEXT)");

            db.ExecSQL("CREATE TABLE IF NOT EXISTS Contenttable (" + COLUMN_NUMBERNOTE + "INTEGER"
                + COLUMN_BITMAP + " BLOB )");

        }
        public void SaveBitmapBase(ref Bitmap image,int notenumber)
        {
            byte[] bytearray = Multitools.ConvertoBLob(ref image);
            ContentValues cv = new ContentValues();
            cv.Put(COLUMN_NUMBERNOTE, notenumber);
            cv.Put(COLUMN_BITMAP, bytearray);
            WritableDatabase.Insert(CONTENTTABLE,null, cv);
        }
        public void SaveText(string Text)
        {

        }
        public void ReturnBitmapBase()
        {

        }
        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
        }
    }
}