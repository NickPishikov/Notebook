//savestext
//deleting notes
//savesbitmaps


using System;
using System.Collections;
using System.Collections.Generic;
using Android.App;
using Android.OS;

using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Graphics.Drawables;
using Android.Content;
using Android.Provider;
using Android.Graphics;
using Android.Media;
using Android.Text.Method;
using Android;
using Android.Database;
using Android.Database.Sqlite;
using Android.Util;
namespace App13
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Databasehelper sqlHelper;
        SQLiteDatabase db;
        Button addToList;
        SimpleCursorAdapter cursorAdapter;
        ICursor cursor;
        ListView list;
        
        private const int REQUEST_RETURN_NOTE = 1; //Возвращаемое значение текста
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
           
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            addToList = FindViewById<Button>(Resource.Id.notebut);
            list = (ListView)FindViewById(Resource.Id.values);
            //views
            sqlHelper = new Databasehelper(this);
            db = sqlHelper.ReadableDatabase;
            //sql
            //getcursor

            cursor = db.RawQuery("select trim(ltrim(ColumnText),'\n') as ColumnText, _id from " + Databasehelper.TEXTTABLE, null);


            string[] headers = new string[] {Databasehelper.COLUMN_TEXT}; //used columns in note
            
           
            cursorAdapter = new SimpleCursorAdapter(this, Resource.Layout.activity_rows,
                             cursor,headers, new int[] { Resource.Id.namenote }, 0);
            
            list.Adapter = cursorAdapter;
            addToList.Click += addToListClick;
            list.ItemClick += (sender, e) =>
             {
                 Intent intent = new Intent(this, typeof(WriteActivity));
                 intent.PutExtra("_id", cursorAdapter.GetItemId(e.Position).ToString());
                 StartActivity(intent);
             };
            //listeners
        }

        void addToListClick(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(WriteActivity));
        
            StartActivityForResult(intent,REQUEST_RETURN_NOTE);
            //ВАЖНО!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //db = sqlHelper.WritableDatabase;
            //ContentValues cv = new ContentValues();
            //cv.Put(Databasehelper.COLUMN_TEXT, "dwadwa");
            //db.Insert(Databasehelper.TEXTTABLE, null, cv);
            //cursor = db.RawQuery("select * from " + Databasehelper.TEXTTABLE, null);
            //cursorAdapter.ChangeCursor(cursor);
            //cursorAdapter.NotifyDataSetChanged();
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == REQUEST_RETURN_NOTE)
            {
                if (resultCode == Result.Ok)
                {
                    cursor = db.RawQuery("select trim(ltrim(ColumnText),'\n') as ColumnText, _id from " + Databasehelper.TEXTTABLE, null);
                    
                    //cursor = db.RawQuery("select * from " + Databasehelper.TEXTTABLE, null);
                    cursorAdapter.ChangeCursor(cursor);
                   //ursorAdapter.NotifyDataSetChanged();
                    //START ACTIVITY
                }
            }
           
          
        }
        protected  override void OnDestroy()
        {
            base.OnDestroy();
            //db = sqlHelper.WritableDatabase;
            //db.ExecSQL("DROP TABLE IF EXISTS " + Databasehelper.TEXTTABLE);
            //db.Close();
        }
    }
}

