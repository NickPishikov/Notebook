
using Android.Content;
using Android.Database.Sqlite;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Database;
using Android.Content;
namespace App13
{
    class Databasehelper : SQLiteOpenHelper
    {
        public static int version = 1;
        public static readonly string DATABASE_NAME = "ContentBase";
        public static readonly string TEXTTABLE = "TextTable";
        public static readonly string CONTENTTABLE = "ContentTable";
        public static string COLUMN_ID = "_id";
      
        public readonly static string COLUMN_TEXT = "ColumnText";
        public readonly static string COLUMN_BITMAP = "ColumnBitmap";
        public readonly static string COLUMN_IMGPATH = "ColumnPath";
        ICursor cursor;
        public Databasehelper(Context context) : base(context, DATABASE_NAME, null, version)
        {
        } // create two tables
        public override void OnCreate(SQLiteDatabase db)
        {
            db.ExecSQL("CREATE TABLE IF NOT EXISTS Texttable (" + COLUMN_ID
                    + " INTEGER PRIMARY KEY AUTOINCREMENT," + COLUMN_TEXT //for listview
                    + " TEXT)");

            db.ExecSQL("CREATE TABLE IF NOT EXISTS Contenttable (" + COLUMN_ID + " INTEGER, "+ COLUMN_IMGPATH + " TEXT, " 
                + COLUMN_BITMAP + " BLOB)");

        }
        public void SaveBitmapBase(long id,string path,Drawable image)
        {
            Bitmap bitmap = ((BitmapDrawable)image).Bitmap;
            byte[] bytearray = Multitools.ConvertoBLob(bitmap);
            ContentValues cv = new ContentValues();
            cv.Put(COLUMN_ID, id);
            cv.Put(COLUMN_IMGPATH, path);
            cv.Put(COLUMN_BITMAP, bytearray);
            WritableDatabase.Insert(CONTENTTABLE,null, cv);
        }
        public void SaveText(string Text)
        {

        }
        public Bitmap ReturnDrawableBase(long id,string path)
        {
            cursor=WritableDatabase.RawQuery("Select " + COLUMN_BITMAP + " from " + CONTENTTABLE + " where ( _id==" + id.ToString() + " and " + COLUMN_IMGPATH + "==" + path +")",null);
            cursor.MoveToFirst();
            byte[] array = cursor.GetBlob(cursor.GetColumnIndex(COLUMN_BITMAP));
            Bitmap b = Multitools.ConvertToBitmap(array);
         //   b = Multitools.decodeSampledBitmapFromUri(this, b, 2000, 2000); //Resize Bitmap
           // b = Multitools.getResizedBitmap(b, 640, 1000);
            
          
            return b;
        }
        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
        }
    }
}