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
using Net.ArcanaStudio.ColorPicker;
using Android.Graphics;

namespace App13
{
    [Activity(Label = "SimpleNote", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity,IColorPickerDialogListener
    {
       
        Databasehelper sqlHelper;
        SQLiteDatabase db;
        ImageButton addToList;
       
        ImageButton CancelBut;
        ImageButton DeleteBut;
        ImageButton ColorBut;
        Listadapter cursorAdapter;
        ICursor cursor;
        ListView list;
        private long LastClickTime = 0;
        private const int REQUEST_RETURN_NOTE = 1; //Возвращаемое значение текста
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
           
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            
            addToList = FindViewById<ImageButton>(Resource.Id.notebut);
            CancelBut =(ImageButton)FindViewById(Resource.Id.CancelBut);
            DeleteBut = (ImageButton)FindViewById(Resource.Id.DeleteBut);
            ColorBut = (ImageButton)FindViewById(Resource.Id.ColorBut);
            
           
            list = (ListView)FindViewById(Resource.Id.values);
            list.RequestFocus();
         
            //views
            sqlHelper = new Databasehelper(this);
            db = sqlHelper.ReadableDatabase;
      

            cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + "," + Databasehelper.COLUMN_COLOR + " from " + Databasehelper.TEXTTABLE, null);
           
            string[] headers = new string[] {Databasehelper.COLUMN_TEXT}; //used columns in note
            
           
            cursorAdapter = new Listadapter(this, Resource.Layout.activity_rows,
                             cursor,headers, new int[] { Resource.Id.namenote}, 0);
    
            list.Adapter = cursorAdapter;
            ColorBut.Click += ColorClick;
            CancelBut.Click += (sender, e) =>
            {
                if (SystemClock.ElapsedRealtime() - LastClickTime < 1000) return;
                LastClickTime = SystemClock.ElapsedRealtime();
                cursorAdapter.IsShowCheckbox(false);
                CancelBut.Visibility = ViewStates.Invisible;
                DeleteBut.Visibility = ViewStates.Invisible;
                addToList.Visibility = ViewStates.Visible;
                ColorBut.Visibility = ViewStates.Gone;

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
                  DeleteBut.Visibility = ViewStates.Gone;
                  CancelBut.Visibility = ViewStates.Gone;
                  ColorBut.Visibility = ViewStates.Gone;
                  addToList.Visibility = ViewStates.Visible;
                  cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + "," + Databasehelper.COLUMN_COLOR + " from " + Databasehelper.TEXTTABLE, null);
                  cursorAdapter.ChangeCursor(cursor);
                  cursorAdapter.IsShowCheckbox(false);
                
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
                     if (SystemClock.ElapsedRealtime() - LastClickTime < 800) return;
                     LastClickTime = SystemClock.ElapsedRealtime();
                     Intent intent = new Intent(this, typeof(WriteActivity));
                     intent.PutExtra("_id", cursorAdapter.GetItemId(e.Position).ToString());
                     StartActivityForResult(intent, REQUEST_RETURN_NOTE);
                 }
              
             };
            list.ItemLongClick += (sender, e) =>
             {
               
                 addToList.Visibility = ViewStates.Invisible;
                 ColorBut.Visibility = ViewStates.Visible;
                 CancelBut.Visibility = ViewStates.Visible;
                 DeleteBut.Visibility = ViewStates.Visible; 
                 cursorAdapter.IsShowCheckbox(true);
                 cursorAdapter.ChangeChecked(e.Position);
             };
            //listeners
        }
        protected  override void OnResume()
        {
            base.OnResume();
            cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + "," + Databasehelper.COLUMN_COLOR + " from " + Databasehelper.TEXTTABLE, null);
            cursorAdapter.ChangeCursor(cursor);
        }
        void ColorClick(object sender,EventArgs e)
        {
            if (SystemClock.ElapsedRealtime() - LastClickTime < 1000) return;
            LastClickTime = SystemClock.ElapsedRealtime();
            ColorPickerDialog.NewBuilder().SetColor(Color.Red).SetDialogType(ColorPickerDialog.DialogType.Preset).SetAllowCustom(false).SetColorShape(ColorShape.Square).SetShowAlphaSlider(false).SetDialogId(1).Show(this);
        }
        void addToListClick(object sender, EventArgs e)
        {
            if (SystemClock.ElapsedRealtime() - LastClickTime < 1000) return;
            LastClickTime = SystemClock.ElapsedRealtime();

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
                    cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME +","+Databasehelper.COLUMN_COLOR+" from " + Databasehelper.TEXTTABLE, null);



                    cursorAdapter.ChangeCursor(cursor);

                }

            }
            cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + "," + Databasehelper.COLUMN_COLOR + " from " + Databasehelper.TEXTTABLE, null);
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

        public void OnColorSelected(int dialogId, Color color)
        {
            Color a = Color.White;
            int b= a.ToArgb();
            Color x = Color.Red;
            int z = x.ToArgb();
            List<int> checkedId = cursorAdapter.GetChecked();
            int colorValue =color.ToArgb();
            ContentValues cv = new ContentValues();
            cv.Put(Databasehelper.COLUMN_COLOR, colorValue);
           
            foreach (int number in checkedId) {
             db.Update(Databasehelper.TEXTTABLE, cv, "_id = ?",new string[] { (cursorAdapter.GetItemId(number)).ToString() });  
            }
            cursor = db.RawQuery("select ColumnText,_id," + Databasehelper.COLUMN_NOTIFY + "," + Databasehelper.COLUMN_EDITINGTIME + "," + Databasehelper.COLUMN_COLOR + " from " + Databasehelper.TEXTTABLE, null);
            cursorAdapter.ChangeCursor(cursor);
            DeleteBut.Visibility = ViewStates.Gone;
            CancelBut.Visibility = ViewStates.Gone;
            ColorBut.Visibility = ViewStates.Gone;
            addToList.Visibility = ViewStates.Visible;
            cursorAdapter.IsShowCheckbox(false);
        }

        public void OnDialogDismissed(int dialogId)
        {
            DeleteBut.Visibility = ViewStates.Gone;
            CancelBut.Visibility = ViewStates.Gone;
            ColorBut.Visibility = ViewStates.Gone;
            addToList.Visibility = ViewStates.Visible;
            cursorAdapter.IsShowCheckbox(false);
        }
    }
}



