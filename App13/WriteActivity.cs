using System;
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
using Android.Database;
using Android.Database.Sqlite;

namespace App13
{
    [Activity(Label = "WriteActivity")]
    public class WriteActivity : Activity
    {

        int slend;
        private bool Editing = true;
        private bool IsEnd = false;
        private EditText EditText;
        const int GALLERY_REQUEST = 1;
        Databasehelper SqlHelper;
        SQLiteDatabase Db;
        ICursor cursor;
        Button SettingsBut;
        ImageButton SaveBut;
        private Bundle Args;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.NoteLayout);
            SettingsBut = FindViewById<Button>(Resource.Id.settingsbut);
            SaveBut = FindViewById<ImageButton>(Resource.Id.savebut);
            EditText = FindViewById<EditText>(Resource.Id.editText1);
            //setviews
           
            SqlHelper = new Databasehelper(this);
            Db = SqlHelper.WritableDatabase;
              
            SettingsBut.Click += OnImageclick;
            SaveBut.Click += OnSaveClick;
           
            EditText.SetPadding(40, 10, 40, 10);

            EditText.BeforeTextChanged += BeforeTextChanged;
            EditText.AfterTextChanged += AfterTextChanged;
            Args = Intent.Extras;
            if (Args != null)
            {
                cursor= Db.RawQuery("Select * from "+ Databasehelper.TEXTTABLE + " Where _id =="
                    + Args.GetString(Databasehelper.COLUMN_ID), null);
                cursor.MoveToFirst();
                string text= cursor.GetString(cursor.GetColumnIndex("ColumnText"));
                EditText.Text = cursor.GetString(cursor.GetColumnIndex("ColumnText")); 
            }

        }
      
        void OnSaveClick(object sender, EventArgs e) //SAVENOTES
        {
            string str = EditText.EditableText.ToString().Trim();
            SaveBut.SetColorFilter(Color.ParseColor("#122343"));

            if (str.Length == 0)
            {
                SetResult(Result.Canceled);
            }
            else
            {
                ContentValues cv = new ContentValues();
                cv.Put(Databasehelper.COLUMN_TEXT, EditText.EditableText.ToString());
                
                if (Args != null)
                {
                    Db.Update(Databasehelper.TEXTTABLE, cv, "_id == ?", new string[] { Args.GetString(Databasehelper.COLUMN_ID) });
                }
                else
                {
                    long id;
                    
                    
                     cursor = Db.RawQuery("Select _id from " + Databasehelper.TEXTTABLE + " ORDER BY _id DESC LIMIT 1",null);
                    if (cursor.MoveToFirst())
                    {
                        id = cursor.GetLong(cursor.GetColumnIndex("_id"));
                        id++;
                    }
                    else
                    {
                        id = 1;
                    }
                    
                 
                 
                    cv.Put(Databasehelper.COLUMN_ID, id);
                   Db.Insert(Databasehelper.TEXTTABLE, null, cv);
                    cursor.Close();
                }
                SetResult(Result.Ok);
            }
            Finish();
            
        }
        void OnImageclick(object sender, EventArgs e) //open explorer 
        {
            Intent photoPickerIntent = new Intent(Intent.ActionPick);
            photoPickerIntent.SetType("image/*");
            StartActivityForResult(photoPickerIntent, GALLERY_REQUEST);
        }
       
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data) //setimage on text view
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Bitmap bitmap = null;
            switch (requestCode)
            {
                case GALLERY_REQUEST:
                    if (resultCode == Result.Ok)
                    {
                        string Tag = "[";
                        Android.Net.Uri selectedImage = data.Data;
                        bitmap = Multitools.decodeSampledBitmapFromUri(this, selectedImage, 2000, 2000);
                        bitmap = Multitools.getResizedBitmap(bitmap, 640, 1000);
                        var imageSpan = new ImageSpan(this, bitmap); //Find your drawable.
                        int selStart = EditText.SelectionEnd;
                        var builder = new SpannableStringBuilder();
                        builder.Append(Tag); //Set text of SpannableString from TextView
                        builder.SetSpan(imageSpan, builder.Length() - Tag.Length, builder.Length(), SpanTypes.ExclusiveExclusive);
                        EditText.EditableText.Insert(selStart, "\n");
                        EditText.EditableText.Insert(selStart + 1, builder);
                        EditText.EditableText.Insert(selStart + 2, "\n");
                        var span = EditText.EditableText.GetSpans(0, builder.Length(), Java.Lang.Class.FromType(typeof(ImageSpan)));
                    }
                    break;
            }

        }
        public void AfterTextChanged(Object Sender, EventArgs a)
        {
            if (IsEnd)
            {
                IsEnd = false;
                Editing = false;

                try
                {
                    EditText.EditableText.Insert(slend, "\n");
                }
                catch
                {
                    EditText.EditableText.Insert(slend - 1, "\n");
                }
            }
        }
        public void BeforeTextChanged(Object Sender, EventArgs a)
        {
            try
            {
                var span = EditText.EditableText.GetSpans(0, EditText.Length(), Java.Lang.Class.FromType(typeof(ImageSpan)));
                slend = EditText.SelectionEnd;
                int g;
                if (span != null && Editing)
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        int b = EditText.EditableText.GetSpanStart(span[i]);
                        g = EditText.EditableText.GetSpanEnd(span[i]);
                        if (b == slend)
                        {
                            Editing = false;
                            EditText.EditableText.Insert(slend, "\n");
                        }
                        if (g == slend)
                        {
                            Editing = false;
                            IsEnd = true;
                        }
                    }
                }
                else
                    return;
            }
            finally { Editing = true; }
        }

    }
}
