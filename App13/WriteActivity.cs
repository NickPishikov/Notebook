
//Исправить баг при удалении
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
using Android.Support.V7.App;
using Android.Support.V4.App;
using AndroidX;
using Android.Preferences;

namespace App13
{
    [Activity(Label = "WriteActivity", Theme = "@style/AppTheme.NoActionBar")]
    public class WriteActivity : AppCompatActivity
    {

        NotifyFragment notifyFragment;
        private EditText EditText;
        const int GALLERY_REQUEST = 1;
        Databasehelper SqlHelper;
        SQLiteDatabase Db;
        ICursor cursor;
        ImageButton ImgBut;
        ImageButton SaveBut;
        ImageButton ShareBut;
        private Bundle Args;
        ImageButton Notification;
        ISharedPreferences Shared;
        ISharedPreferencesEditor PrefsEditor;
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

            ShareBut = FindViewById<ImageButton>(Resource.Id.share_but);

            SqlHelper = new Databasehelper(this);
            Db = SqlHelper.WritableDatabase;
            Notification.Click += SendNotify;
            ImgBut.Click += OnImageclick;
            SaveBut.Click += OnSaveClick;
            ShareBut.Click += ShareClick;
            Shared = PreferenceManager.GetDefaultSharedPreferences(this);
            PrefsEditor = Shared.Edit();
            EditText.SetPadding(40, 10, 40, 10);


            textWatcher = new TextWatcher(EditText);
           
            EditText.AddTextChangedListener(textWatcher);
            notifyFragment = new NotifyFragment(EditText.EditableText, Args);
          
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
                notifyFragment.Id=NoteNumber;
            }
           
           
                

        }
       public void ShareClick(object sender,EventArgs e)
        {
            string a;
            a = Html.ToHtml(EditText.EditableText,ToHtmlOptions.ParagraphLinesConsecutive);
            Intent sharingIntent = new Intent(Android.Content.Intent.ActionSend);
            sharingIntent.SetType("text/plain");
            
            string shareBody = EditText.Text;
            sharingIntent.PutExtra(Android.Content.Intent.ExtraText, shareBody);
            StartActivity(Intent.CreateChooser(sharingIntent, "Поделиться"));
        }
        public void SendNotify(object sender, EventArgs e) //Create ntoification
        {

         
           
            cursor = Db.Query(Databasehelper.TEXTTABLE, new string[] { Databasehelper.COLUMN_NOTIFY },"_id = ?", new string[] { notifyFragment.Id.ToString() }, null, null, null);
            cursor.MoveToFirst();
                if (cursor.Count!=0&&cursor.GetInt(0)==1)
            {
               
               Dialog dialog = CreateAlertAlarm(notifyFragment.GetTime(this, notifyFragment.Id));
                dialog.Show();

            }
            else
            {
                Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
             
                if (notifyFragment.Args != null)
                {
                    Args = notifyFragment.Args;
                    NoteNumber = notifyFragment.Id;
                }

                notifyFragment.SetContent(EditText.EditableText, Args);
                notifyFragment.Show(fragmentManager, "MydDialog");
            }
           
            
         

         
          
          
           
        }

        public Android.Support.V7.App.AlertDialog CreateAlertAlarm(string[] timeAndDate)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Напоминание уже установлено.");
            builder.SetMessage("Напоминание произойдет:" + "\n" + timeAndDate[0] + "\n" + "в " + timeAndDate[1]);
            builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
            builder.SetNegativeButton("Выйти", (sender, args) => { });
            builder.SetPositiveButton("Удалить", (sender, args) => { notifyFragment.CancelAlarm(this);ContentValues cv = new ContentValues(); cv.Put(Databasehelper.COLUMN_NOTIFY, 0); Db.Update(Databasehelper.TEXTTABLE,cv, "_id= ?", new string[] { notifyFragment.Id.ToString() }); Toast s = Toast.MakeText(this, "Напоминане удалено", ToastLength.Long); s.Show(); ;
                s.Show();
            });
            return builder.Create();

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

            if (notifyFragment.Args!=null)
            Args = notifyFragment.Args;
            if (str.Length == 0)
            {
                SetResult(Result.Canceled);
            }
            else
            {
              NoteNumber=SqlHelper.SaveText(EditText.EditableText, Args);

            
                if (notifyFragment != null)
                {
                    Thread thread = new Thread(ChangeNotifyContent);
                    thread.Start();
                }


                SetResult(Result.Ok);
            }
            Finish();
            
        }
        void ChangeNotifyContent()
        {
            cursor = Db.RawQuery(("select " + Databasehelper.COLUMN_IMGPATH + " from " + Databasehelper.CONTENTTABLE + " where _id == " + NoteNumber.ToString()), null);
            notifyFragment.Content = Multitools.GetNameNote(EditText.Text.Split("\n")[0], cursor);
            notifyFragment.ChangeIntent(this);

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
   

