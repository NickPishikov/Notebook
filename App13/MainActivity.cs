

using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Android.Preferences;

namespace App13
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Databasehelper sqlHelper;
        SQLiteDatabase db;
        ImageButton addToList;
        ImageButton CancelBut;
        ImageButton DeleteBut;
        Listadapter cursorAdapter;
        ICursor cursor;
        ListView list;
        CheckBox checknotes;
        ISharedPreferences Shared;
        ISharedPreferencesEditor Editor;
        
        private const int REQUEST_RETURN_NOTE = 1; //Возвращаемое значение текста
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
           
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            addToList = FindViewById<ImageButton>(Resource.Id.notebut);
            CancelBut =(ImageButton)FindViewById(Resource.Id.CancelBut);
            DeleteBut = (ImageButton)FindViewById(Resource.Id.DeleteBut);
            list = (ListView)FindViewById(Resource.Id.values);
            list.RequestFocus();
            
            //views
            sqlHelper = new Databasehelper(this);
            db = sqlHelper.ReadableDatabase;
            //   db.ExecSQL("DROP Table " + Databasehelper.CONTENTTABLE);
            //  db.ExecSQL("DROP Table " + Databasehelper.TEXTTABLE);
            //sql
            //getcursor
           
            cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + " from " + Databasehelper.TEXTTABLE, null);
            Shared = PreferenceManager.GetDefaultSharedPreferences(this);
            Editor = Shared.Edit();


            string[] headers = new string[] {Databasehelper.COLUMN_TEXT}; //used columns in note
            
           
            cursorAdapter = new Listadapter(this, Resource.Layout.activity_rows,
                             cursor,headers, new int[] { Resource.Id.namenote}, 0);
           
            list.Adapter = cursorAdapter;
            CancelBut.Click += (sender, e) =>
            {
                cursorAdapter.IsShowCheckbox(false);
                CancelBut.Visibility = ViewStates.Invisible;
                DeleteBut.Visibility = ViewStates.Invisible;
                addToList.Visibility = ViewStates.Visible;
            };
            DeleteBut.Click += (sender, e) =>
              {
                  cursor = db.Query(Databasehelper.TEXTTABLE, new string[] { Databasehelper.COLUMN_ID }, Databasehelper.COLUMN_NOTIFY + "= ?", new string[] { "1" }, null, null, null);
                  List<int> checkedpos = cursorAdapter.GetChecked();
                  for (int i = 0; i < checkedpos.Count; i++)
                  {
                      NotifyFragment a = new NotifyFragment(null, null);
                      cursor = db.Query(Databasehelper.TEXTTABLE, new string[] { Databasehelper.COLUMN_NOTIFY }, "_id = ?", new string[] { cursorAdapter.GetItemId(checkedpos[i]).ToString() }, null, null,null);
                      cursor.MoveToFirst();
                      db.ExecSQL("Delete from " + Databasehelper.TEXTTABLE + " Where _id == " +cursorAdapter.GetItemId(checkedpos[i]).ToString());
                      db.ExecSQL("Delete from " + Databasehelper.CONTENTTABLE + " Where _id == " + cursorAdapter.GetItemId(checkedpos[i]).ToString());
                      if(cursor.GetInt(0)==1)
                      a.CancelAlarm(this, cursorAdapter.GetItemId(checkedpos[i]));       
                  }

                  cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + " from " + Databasehelper.TEXTTABLE, null);
                  cursorAdapter.ChangeCursor(cursor);
                  cursorAdapter.IsShowCheckbox(false);
                  DeleteBut.Visibility = ViewStates.Invisible;
                  CancelBut.Visibility = ViewStates.Invisible;
                  addToList.Visibility = ViewStates.Visible;
              };
            addToList.Click += addToListClick;
            list.ItemClick += (sender, e) =>
             {

                 if (cursorAdapter.IsShow)
                 {
                     cursorAdapter.ChangeChecked(e.Position);
                     
                 }
                 else
                 {
                     Intent intent = new Intent(this, typeof(WriteActivity));
                     intent.PutExtra("_id", cursorAdapter.GetItemId(e.Position).ToString());
                     StartActivityForResult(intent, REQUEST_RETURN_NOTE);
                 }
             };
            list.ItemLongClick += (sender, e) =>
             {
                 addToList.Visibility = ViewStates.Invisible;
                 CancelBut.Visibility = ViewStates.Visible;
                 DeleteBut.Visibility = ViewStates.Visible; 
                 cursorAdapter.IsShowCheckbox(true);
             };
            //listeners
        }
        protected  override void OnResume()
        {
            base.OnResume();
            cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY +","+Databasehelper.COLUMN_EDITINGTIME + " from " + Databasehelper.TEXTTABLE, null);
            cursorAdapter.ChangeCursor(cursor);
        }

        void addToListClick(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(WriteActivity));
        
            StartActivityForResult(intent,REQUEST_RETURN_NOTE);
          
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        
            if (requestCode == REQUEST_RETURN_NOTE)
            {
                if (resultCode == Result.Ok)
                {
                    // cursor = db.RawQuery("select trim(ltrim(ColumnText),'\n') as ColumnText, _id from " + Databasehelper.TEXTTABLE, null);
                    cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + " from " + Databasehelper.TEXTTABLE, null);



                    cursorAdapter.ChangeCursor(cursor);

                }

            }
            cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + " from " + Databasehelper.TEXTTABLE, null);
            cursorAdapter.ChangeCursor(cursor);
            cursorAdapter.NotifyDataSetChanged();
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



