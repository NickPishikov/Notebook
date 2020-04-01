using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Text;
using Java.Lang;
using Android.Text.Style;
using Android.Content;
using Android.Graphics;
using Android.Database;
using Android.Database.Sqlite;
using Android.Graphics.Drawables;
using Android.Support.V4.App;


namespace App13
{
    [Activity(Label = "WriteActivity")]
    public class WriteActivity : Activity
    {
   
       
        private EditText EditText;
        const int GALLERY_REQUEST = 1;
        Databasehelper SqlHelper;
        SQLiteDatabase Db;
        ICursor cursor;
        ImageButton ImgBut;
        ImageButton SaveBut;
        private Bundle Args;
        ImageButton Notification;

        long NoteNumber = 0;
        public Dictionary<string, Bitmap> Images { get; set; } = new Dictionary<string, Bitmap>();
        public TextWatcher textWatcher;


        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.NoteLayout);
            Notification = FindViewById<ImageButton>(Resource.Id.notification);
            ImgBut = FindViewById<ImageButton>(Resource.Id.setimg);
            SaveBut = FindViewById<ImageButton>(Resource.Id.savebut);
            EditText = FindViewById<EditText>(Resource.Id.editText1);
            //setviews
            


            SqlHelper = new Databasehelper(this);
            Db = SqlHelper.WritableDatabase;
            Notification.Click += sendNotify;
            ImgBut.Click += OnImageclick;
            SaveBut.Click += OnSaveClick;

            EditText.SetPadding(40, 10, 40, 10);


            textWatcher = new TextWatcher(EditText);
            EditText.AddTextChangedListener(textWatcher);

            Args = Intent.Extras;
            if (Args != null)
            {
                NoteNumber = Convert.ToInt32(Args.GetString(Databasehelper.COLUMN_ID));
                cursor = Db.RawQuery("Select * from " + Databasehelper.TEXTTABLE + " Where _id =="
                    + Args.GetString(Databasehelper.COLUMN_ID), null);
                cursor.MoveToFirst();
                string text = cursor.GetString(cursor.GetColumnIndex("ColumnText"));
                EditText.Text = text;
                cursor = Db.RawQuery("Select *" + " from " + Databasehelper.CONTENTTABLE
                    + " Where _id==" + Args.GetString(Databasehelper.COLUMN_ID), null);
                setImages(cursor);
            }
           
                

        }
        
        void sendNotify(object sender, EventArgs e) //Create ntoification
        {
            NotificationCompat.Builder builder =
          new NotificationCompat.Builder(this,"ID")
                  .SetSmallIcon(Android.Resource.Drawable.IcButtonSpeakNow)
                  .SetContentTitle("Напоминание")
                  .SetContentText(EditText.Text);

            Notification notification = builder.Build();

            NotificationManager notificationManager =
                   (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(1, notification);
        }
        void setImages(ICursor cursor)
        {
            
            if (cursor.MoveToFirst())
            {
                string path;
                long id;
                long start;
                long end;
                Bitmap bitmap = null;
                do
                {
                    id = cursor.GetLong(0);
                    path = cursor.GetString(1);
                    start = cursor.GetLong(2);
                    end = cursor.GetLong(3);
                    bitmap = SqlHelper.ReturnDrawableBase(id, path);
                    path = '[' + path + ']';
                    var imageSpan = new ImageSpan(this, bitmap);
                    ISpannable spann = SpannableFactory.Instance.NewSpannable(path);
                    spann.SetSpan(imageSpan, 0, path.Length, SpanTypes.ExclusiveExclusive);
                    textWatcher.Editing = false;
                    EditText.EditableText.Replace((int)start, (int)end, spann);
                }
                while (cursor.MoveToNext());
               
            }
           
        } //SetImages in Text
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
                cv.Put(Databasehelper.COLUMN_TEXT,EditText.Text);
                
                if (Args != null)
                {
                    Db.Update(Databasehelper.TEXTTABLE, cv, "_id == ?", new string[] { Args.GetString(Databasehelper.COLUMN_ID) });
                    Db.ExecSQL("DELETE from " + Databasehelper.CONTENTTABLE + " Where _id == " + Args.GetString(Databasehelper.COLUMN_ID)); //Delete old image
                  
                }
                else
                {


                    long id = 1;
                     cursor = Db.RawQuery("Select _id from " + Databasehelper.TEXTTABLE + " ORDER BY _id DESC LIMIT 1",null);
                    if (cursor.MoveToFirst())
                    {
                        id = cursor.GetLong(cursor.GetColumnIndex("_id"));
                        cursor.Close();
                        id++;
                    }
                    cv.Put(Databasehelper.COLUMN_ID, id);
                   Db.Insert(Databasehelper.TEXTTABLE, null, cv);
                    NoteNumber = id;
                 
                }
                Java.Lang.Object[] span = EditText.EditableText.GetSpans(0, EditText.Length(), Java.Lang.Class.FromType(typeof(ImageSpan)));
              //Insert Image in Database
                if (span != null)
                {
                    for (int i = 0; i < span.Length; i++)
                    {
                        int start = EditText.EditableText.GetSpanStart(span[i]);
                        int end = EditText.EditableText.GetSpanEnd(span[i]);
                        string source;
                       
                             source = EditText.Text.Substring(start + 1, end -start- 2);
                        
                       
                        SqlHelper.SaveBitmapBase(NoteNumber, source, start, end, ((BitmapDrawable)((ImageSpan)span[i]).Drawable).Bitmap);
                    }
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
 
                        Android.Net.Uri selectedImage = data.Data;
                        string Tag ='['+ selectedImage.LastPathSegment+']';
                        bitmap = Multitools.decodeSampledBitmapFromUri(this, selectedImage, 2000, 2000);
                        bitmap = Multitools.getResizedBitmap(bitmap, 1000, 1000);
                       
                        var imageSpan = new ImageSpan(this, bitmap); //Find your drawable.
                       
                        int selStart = EditText.SelectionEnd;
                        var span = EditText.EditableText.GetSpans(0, EditText.Length(), Java.Lang.Class.FromType(typeof(ImageSpan)));
                        //for (int i = 0; i < span.Length; i++)  
                        //{
                        //    int end = EditText.EditableText.GetSpanEnd(span[i]);
                        //    if (selStart == end)
                        //    {
                        //        EditText.EditableText.Insert(selStart, "\n");
                        //        selStart=EditText.SelectionEnd;
                        //    }


                        //}//if image add in end other image

                        ISpannable spann = SpannableFactory.Instance.NewSpannable(Tag);
                        spann.SetSpan(imageSpan, 0, Tag.Length, SpanTypes.ExclusiveExclusive);
                        if (selStart!=0)
                        EditText.EditableText.Insert(selStart, "\n");
                        selStart = EditText.SelectionEnd;
                        
                        EditText.EditableText.Insert(selStart,spann);
                        textWatcher.Editing = false;
                        EditText.EditableText.Insert(selStart + Tag.Length, "\n");
                        textWatcher.Editing = true;


                    }
                    break;
            }

        }


       
     public   class TextWatcher : Java.Lang.Object, ITextWatcher
        {
            int slend;
            int spanStart;
            int spanEnd;
            public bool Editing = true;
            EditText EditText;
          
            private bool IsEnd = false;
         public   TextWatcher(EditText text)
            {
                EditText = text;
            }
            public void AfterTextChanged(IEditable a)
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
                        EditText.EditableText.Insert(slend-1  , "\n");
                    }
                }
            } 

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {


              
                if (count<before && spanStart!=-1&&spanEnd>-1 )
                {
                    int startSpan = spanStart;
                    int endSpan = spanEnd;
                    if (startSpan < 0 || endSpan > EditText.EditableText.Length())
                    {
                        endSpan = EditText.EditableText.Length();
                    }
                    spanStart = -1;
                    spanEnd = -1;
                    EditText.EditableText.Replace(startSpan, endSpan, "");
                }

            }  //replace deleted spans
            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)  //delete spans and insert spaces
             {
                try
                {
                    var span = EditText.EditableText.GetSpans(0, EditText.Length(), Java.Lang.Class.FromType(typeof(ImageSpan)));
                    spanStart = -1;
                    spanEnd = -1;
                    int end;
                    slend = EditText.SelectionEnd;

                    
                    if (span != null && Editing)
                    {
                       
                        if (count >after)
                        {

                            for (int i = 0; i < span.Length; i++)
                            {
                                end =EditText.EditableText.GetSpanEnd(span[i]);
                                if (slend != end) continue;
                                spanStart = EditText.EditableText.GetSpanStart(span[i]);
                                spanEnd = EditText.EditableText.GetSpanEnd(span[i]);
                                EditText.EditableText.RemoveSpan(span[i]);


                            }

                            return;
                        }
                        for (int i = 0; i < span.Length; i++)
                        {
                            spanStart = EditText.EditableText.GetSpanStart(span[i]);
                            spanEnd = EditText.EditableText.GetSpanEnd(span[i]);
                            if (spanStart == slend)
                            {
                                Editing = false;
                                EditText.EditableText.Insert(slend, "\n");
                            }
                            if (spanEnd == slend)
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
        } //TextWatcher EditText
      
    }

}
   

