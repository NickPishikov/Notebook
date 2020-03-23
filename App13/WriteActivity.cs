using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Content;
using Android.Graphics;
using Android.Database;
using Android.Database.Sqlite;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Text.Method;
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
        ImageButton SettingsBut;
        ImageButton SaveBut;
        private Bundle Args;
        long NoteNumber = 0;
        public Dictionary<string, Drawable> Images { get; set; } = new Dictionary<string, Drawable>();
        MyClickableSpan clickableSpan;
        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.NoteLayout);
            SettingsBut = FindViewById<ImageButton>(Resource.Id.settingsbut);
            SaveBut = FindViewById<ImageButton>(Resource.Id.savebut);
            EditText = FindViewById<EditText>(Resource.Id.editText1);
            //setviews
             clickableSpan = new MyClickableSpan();
            clickableSpan.Click += v =>
            {
                EditText a =(EditText) v;

                a.SetSelection(0);
            };
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
                NoteNumber = Convert.ToInt32(Args.GetString(Databasehelper.COLUMN_ID));
                cursor = Db.RawQuery("Select * from " + Databasehelper.TEXTTABLE + " Where _id =="
                    + Args.GetString(Databasehelper.COLUMN_ID), null);
                cursor.MoveToFirst();
                string text = cursor.GetString(cursor.GetColumnIndex("ColumnText"));
               
                cursor = Db.RawQuery("Select *" + " from " + Databasehelper.CONTENTTABLE
                    + " Where _id==" + Args.GetString(Databasehelper.COLUMN_ID), null);
                if (cursor.MoveToFirst())
                {

                    do
                    {
                        Images.Add(cursor.GetString(1), new BitmapDrawable(this.Resources, SqlHelper.ReturnDrawableBase(cursor.GetLong(0), cursor.GetString(1))));

                    }
                    while (cursor.MoveToNext());
                    ImageGetter a = new ImageGetter(Images);
                    var spannedFromHtml = Html.FromHtml(text, a, null);
                    EditText.SetText(spannedFromHtml, EditText.BufferType.Editable);
                }
                else EditText.Text = text;
                

            }
        }
        private class MyClickableSpan : ClickableSpan
        {
            public Action<View> Click;

            public override void OnClick(View widget)
            {
                if (Click != null)
                    Click(widget);
            }
        }

        void OnSaveClick(object sender, EventArgs e) //SAVENOTES
        {
            string str = EditText.EditableText.ToString().Trim();
            

            if (str.Length == 0)
            {
                SetResult(Result.Canceled);
            }
            else
            {
                ContentValues cv = new ContentValues();
                ContentValues cv2 = new ContentValues();
                cv.Put(Databasehelper.COLUMN_TEXT, Html.ToHtml(EditText.EditableText));
                
                if (Args != null)
                {
                    Db.Update(Databasehelper.TEXTTABLE, cv, "_id == ?", new string[] { Args.GetString(Databasehelper.COLUMN_ID) });
                    Db.ExecSQL("DELETE from " + Databasehelper.CONTENTTABLE + " Where _id == " + Args.GetString(Databasehelper.COLUMN_ID)); //Delete old image
                  
                }
                else
                {
                    long id;
                    
                    
                     cursor = Db.RawQuery("Select _id from " + Databasehelper.TEXTTABLE + " ORDER BY _id DESC LIMIT 1",null);
                    if (cursor.MoveToFirst())
                    {
                        id = cursor.GetLong(cursor.GetColumnIndex("_id"));
                        cursor.Close();
                        id++;
                    }
                    else
                    {
                        id = 1;
                    }
                    NoteNumber = id;

                   
                    cv.Put(Databasehelper.COLUMN_ID, id);
                   Db.Insert(Databasehelper.TEXTTABLE, null, cv);
                   
                 
                }
                Java.Lang.Object[] span = EditText.EditableText.GetSpans(0, EditText.Length(), Java.Lang.Class.FromType(typeof(ImageSpan))); //Insert Image in Database
                if (span != null)
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        SqlHelper.SaveBitmapBase(NoteNumber, ((ImageSpan)span[i]).Source, ((ImageSpan)span[i]).Drawable);
                    }
                }
                //     Db.Update(Databasehelper.TEXTTABLE)
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
                       //Добавление уникальных элементов в dictionary
                        Android.Net.Uri selectedImage = data.Data;
                        string myHtml = "[<img src=" + selectedImage.LastPathSegment + " style=\"color: red\"/>]"; //HTML Tag
                        string s = selectedImage.Query;
                        bitmap = Multitools.decodeSampledBitmapFromUri(this, selectedImage, 2000, 2000); //Resize Bitmap
                        bitmap = Multitools.getResizedBitmap(bitmap, 1010, 1000);
                       
                        Drawable drawable = new BitmapDrawable(this.Resources, bitmap); //GET DRAWABLE
                        
                        if (!Images.ContainsKey(selectedImage.LastPathSegment))
                            Images.Add(selectedImage.LastPathSegment, drawable);
                        Images = Images.Select(item => new KeyValuePair<string, Drawable>(item.Key, item.Value)).Distinct().ToDictionary(item => item.Key, item => item.Value);
                        int selStart = EditText.SelectionEnd;
                        ImageGetter a = new ImageGetter(Images);
                       // SpannableString ss = new SpannableString("]");
                        
                        ISpannable span = SpannableFactory.Instance.NewSpannable(Html.FromHtml(myHtml, a, null));
                        span.SetSpan("]", span.Length(), span.Length(), SpanTypes.ExclusiveExclusive);
                        //ss.SetSpan(spannedFromHtml, 0, spannedFromHtml.Length(), SpanTypes.ExclusiveExclusive);
                        //ISpannable sss = ((ISpannable)spannedFromHtml);
                        span.SetSpan(clickableSpan, 0, span.Length(), SpanTypes.ExclusiveExclusive);
                        
                       

                       
                        //ClickableSpan cs = new ClickableSpan() {

                        // public void onClick(View v)
                        //{
                        //  //  Log.D("main", "textview clicked");
                        //    Toast.MakeText(this, "textview clicked", ToastLength.Long).Show();
                        //}
                        //};
                        EditText.EditableText.Insert(selStart, span);
                        EditText.EditableText.Insert(selStart + 1, "\n");
                        EditText.MovementMethod = LinkMovementMethod.Instance;//insert HTML tag
                    //    EditText.EditableText.Insert(selStart,"\n");



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
                   // EditText.SetSelection(slend + 1);
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
                            EditText.SetSelection(slend-1);
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


       

        public class ImageGetter : Java.Lang.Object, Html.IImageGetter
        {
            Dictionary<string, Drawable> images;
            Drawable image = null;
            
            public ImageGetter(Dictionary<string, Drawable> images)
            {
                this.images=images;

            }


           

            public Drawable GetDrawable(string source)
            {
                //GetBitmap from Database;

                image = images.GetValueOrDefault(source);
              //  Drawable drawable = new BitmapDrawable(image); //GET DRAWABLE
                image.SetBounds(0, 0, image.IntrinsicWidth, image.IntrinsicHeight);

                return image;
                //  Drawable d = getResources().getDrawable(id);
                //  d.setBounds(0, 0, d.getIntrinsicWidth(), d.getIntrinsicHeight());

            }

            void IDisposable.Dispose()
            {
                throw new NotImplementedException();
            }
        }

    }
   
}
