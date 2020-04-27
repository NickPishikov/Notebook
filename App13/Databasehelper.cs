using System;
using Android.Content;
using Android.Database.Sqlite;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Database;
using Android.OS;
using Android.Text;
using Android.Text.Style;
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
        public readonly static string COLUMN_START = "ColumnStart";
        public readonly static string COLUMN_END = "ColumnEnd";
        public readonly static string COLUMN_NOTIFY = "ColumnNotify";
        public readonly static string COLUMN_TIME = "ColumnTime";
        public readonly static string COLUMN_COLOR = "ColumnColor";
        public readonly static string NOTIFYTABLE = "NotifyTable";
        public readonly static string COLUMN_EDITINGTIME = "EditingTime";
        ICursor cursor;
        public Databasehelper(Context context) : base(context, DATABASE_NAME, null, version)
        {
        } // create two tables
        public override void OnCreate(SQLiteDatabase db)
        {
            db.ExecSQL("CREATE TABLE IF NOT EXISTS TextTable (" + COLUMN_ID
                    + " INTEGER PRIMARY KEY AUTOINCREMENT," + COLUMN_TEXT //for listview
                    + " TEXT," + COLUMN_NOTIFY +" BOOLEAN, "+ COLUMN_TIME + " INTEGER DEFAULT 0, " +COLUMN_EDITINGTIME+" TEXT, " + COLUMN_COLOR+" INTEGER DEFAULT -1)");

            db.ExecSQL("CREATE TABLE IF NOT EXISTS ContentTable (" + COLUMN_ID + " INTEGER, "+ COLUMN_IMGPATH + " TEXT, " +
                COLUMN_START +" INTEGER, " +
                COLUMN_END+ " INTEGER, "
                + COLUMN_BITMAP + " BLOB)");
           

        }
      
        public void SaveBitmapBase(long id,string path,int start,int end,Bitmap image) //SAVE IMAGE IN DB
        {
           
            byte[] bytearray = Multitools.ConvertoBLob(image);
            ContentValues cv = new ContentValues();
            cv.Put(COLUMN_ID, id);
            cv.Put(COLUMN_IMGPATH, path);
            cv.Put(COLUMN_START, start);
            cv.Put(COLUMN_END, end);
            cv.Put(COLUMN_BITMAP, bytearray);
            WritableDatabase.Insert(CONTENTTABLE,null, cv);
         }
        public  long SaveText(IEditable Text,Bundle args)
        {
           
            long NoteNumber = 0;
            string date = DateTime.Now.ToString("dd MMM yyyyг. HH:mm");
       
            ContentValues cv = new ContentValues();
            SpannableFactory a = new SpannableFactory();
           ISpannable saveText= a.NewSpannable(Text);
           
           
            
              
            Java.Lang.Object[] span = saveText.GetSpans(0, saveText.Length(), Java.Lang.Class.FromType(typeof(ImageSpan)));
            if (span != null)
                for (int i = 0; i < span.Length; i++)
                {
                    saveText.RemoveSpan(span[i]);
                }

            cv.Put(Databasehelper.COLUMN_TEXT, Html.ToHtml(saveText,ToHtmlOptions.ParagraphLinesIndividual));
            cv.Put(Databasehelper.COLUMN_EDITINGTIME, date);
            if (args != null)
            {
                WritableDatabase.Update(Databasehelper.TEXTTABLE, cv, "_id == ?", new string[] { args.GetString(Databasehelper.COLUMN_ID) });
                WritableDatabase.ExecSQL("DELETE from " + Databasehelper.CONTENTTABLE + " Where _id == " + args.GetString(Databasehelper.COLUMN_ID)); //Delete old image
                NoteNumber = Convert.ToInt32(args.GetString(Databasehelper.COLUMN_ID));
            }
            else
            {


                long id = 1;
                cursor = WritableDatabase.RawQuery("Select _id from " + Databasehelper.TEXTTABLE + " ORDER BY _id DESC LIMIT 1", null);
                if (cursor.MoveToFirst())
                {
                    id = cursor.GetLong(cursor.GetColumnIndex("_id"));
                    cursor.Close();
                    id++;
                }
                cv.Put(Databasehelper.COLUMN_ID, id);
                WritableDatabase.Insert(Databasehelper.TEXTTABLE, null, cv);
                NoteNumber = id;
              
            }
           Java.Lang.Object[] spans = Text.GetSpans(0, Text.Length(), Java.Lang.Class.FromType(typeof(ImageSpan)));
            //Insert Image in Database
            if (spans != null)
            {
                for (int i = 0; i < spans.Length; i++)
                {
                    int start =Text.GetSpanStart(spans[i]);
                    int end = Text.GetSpanEnd(spans[i]);
                    string source;

                    source = Text.ToString().Substring(start + 1, end - start - 2);
                  

                   SaveBitmapBase(NoteNumber, source, start, end, ((BitmapDrawable)((ImageSpan)spans[i]).Drawable).Bitmap);
                }
            }
            return NoteNumber;
        }
      
        public Bitmap ReturnDrawableBase(long id,string path)
        {
            string[] col = { "ColumnBitmap" };
            string[] param = { path };
            cursor = WritableDatabase.Query("ContentTable", col, "ColumnPath = ?", param,null,null,null);
            cursor.MoveToFirst();
            byte[] array = cursor.GetBlob(cursor.GetColumnIndex(COLUMN_BITMAP));
            Bitmap b = Multitools.ConvertToBitmap(array);
            return b;
        }
        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
        }
    }
}