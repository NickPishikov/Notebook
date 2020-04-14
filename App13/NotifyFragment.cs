using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Views;
using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Text;
using Java.Text;
using Java.Util;
using Java.Lang;
using Android.Widget;
using Android.Systems;
using Android.Preferences;
using Android.Database.Sqlite;
using Android.Database;


namespace App13
{
   [BroadcastReceiver]
   public class  NotifyManager : BroadcastReceiver
    {
        ISharedPreferences Shared;
        ISharedPreferencesEditor PrefsEditor;
        Databasehelper databasehelper;
        ICursor cursor;
        SQLiteDatabase Db;
        public override void OnReceive(Context context, Intent intent)
        {
            databasehelper = new Databasehelper(context);
            Db = databasehelper.WritableDatabase;
            cursor = Db.Query(Databasehelper.NOTIFYTABLE, new string[] { Databasehelper.NEW_ID }, Databasehelper.START_ID + " = ?",new string[] { intent.GetLongExtra("_id", 0).ToString() }, null, null, null);
            cursor.MoveToFirst();
            long a = cursor.GetLong(0);
            Intent intent1 = new Intent(context, typeof(WriteActivity));
            intent1.PutExtra("_id",cursor.GetLong(cursor.GetColumnIndex(Databasehelper.NEW_ID)).ToString());
            Db.Delete(Databasehelper.NOTIFYTABLE, Databasehelper.START_ID + " = ?", new string[] { intent.GetLongExtra("_id", 0).ToString() });
            Android.App.PendingIntent resultPendingIntent = Android.App.PendingIntent.GetActivity(context, Convert.ToInt32(a), intent1,
               Android.App.PendingIntentFlags.UpdateCurrent);
            NotificationCompat.Builder builder =
            new NotificationCompat.Builder(context,"ID")
                    .SetSmallIcon(Android.Resource.Drawable.IcButtonSpeakNow)
                    .SetContentTitle(intent.GetStringExtra("Вам напоминание!"))
                    .SetContentText(intent.GetStringExtra("message"))
                    .SetPriority((int)Android.App.NotificationPriority.Default)
                    .SetContentIntent(resultPendingIntent)
                    .SetAutoCancel(true)
            ;
            Android.App.NotificationManager manager = (Android.App.NotificationManager)context.GetSystemService(Context.NotificationService);

           Android.App.Notification notification = builder.Build();


            Multitools.createChannelIfNeeded(manager);
            manager.Notify(Convert.ToInt32(a), notification);
        }
        public void SetAlarm(Context context,string content,Calendar calendar)
        {
           
           
        }
    }
    public class NotifyFragment : DialogFragment
    {
        ISharedPreferences Shared;
        ISharedPreferencesEditor PrefsEditor;
        public string Content { get; set; } = "Посмотрите заметку!";
        Button TimeBut;
        Button DateBut;
        Button AcceptBut;
        Button CancelBut;
        Intent intent;
        IEditable Editable;
        Android.App.AlarmManager alarm;
        ICursor cursor;
      public  Bundle Args;
        Android.App.PendingIntent pendingIntent;
        public new long Id { get; set; } = 0;
      
        public int Hour { get; set; }
       public int Minute { get; set; }
        public DateTime Date { get; set; }

        Databasehelper SqlHelper;
        SQLiteDatabase Db;
     public   NotifyFragment(IEditable text,Bundle args)
        {
            Editable = text;
            Args = args;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Shared = PreferenceManager.GetDefaultSharedPreferences(Context);

            PrefsEditor = Shared.Edit();
            SqlHelper = new Databasehelper(Context);
            Db = SqlHelper.WritableDatabase;
        }
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            
           
            View v = inflater.Inflate(Resource.Layout.notify_fragment,null);
            TimeBut=(Button)v.FindViewById(Resource.Id.time_but);
            DateBut = (Button)v.FindViewById(Resource.Id.date_but);
            AcceptBut = (Button)v.FindViewById(Resource.Id.accept_but);
            CancelBut = (Button)v.FindViewById(Resource.Id.cancel_but);
            AcceptBut.Click += OnClickAccept;
            CancelBut.Click += OnClickCancel;
            TimeBut.Click += OnClickTime;
            DateBut.Click += OnClickDate;
           
            SetDefaultTime();
            return v;
        }
        public void SetContent( IEditable text, Bundle args)
        {
            Editable = text;
            Args = args;
        }
        void OnClickTime(object sender,EventArgs e)
        {
            Android.App.TimePickerDialog timePicker = new Android.App.TimePickerDialog(Context,(s,args)=>
            { 
                Hour = args.HourOfDay;
               
                Minute = args.Minute;
                
                TimeBut.Text = string.Format("{0:00}",Hour) + ":" + string.Format("{0:00}",Minute);
            },DateTime.Now.Hour,DateTime.Now.Minute,true);
           
            timePicker.Show();
        }
        void OnClickDate(object sender, EventArgs e)
        {
            Android.App.DatePickerDialog datePicker = new Android.App.DatePickerDialog(Context, (s, args) => { Date = args.Date; DateBut.Text = Date.ToString("d"); },DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day);
            Date date = new Date();
            datePicker.DatePicker.MinDate = Calendar.Instance.TimeInMillis;
            datePicker.UpdateDate(DateTime.Now);
            datePicker.Show();
            
        }
        void OnClickAccept(object sender,EventArgs e)
        {
            if ((DateTime.Now.Date == Date && (DateTime.Now.Hour > Hour || DateTime.Now.Hour == Hour && DateTime.Now.Minute > Minute)) || DateTime.Now.Date > Date) //If current date, check hour is not less current hour, and if current date and hour, check minute
            {
                Toast toast = Toast.MakeText(Context, "Нельзя установить прошедшее время или дату.", ToastLength.Short);
                toast.Show();
            }
            else
            {
           Id= SqlHelper.SaveText(Editable, Args);
               
                if (Args == null)
                {
                    Args = new Bundle();
                    Args.PutString("_id", Id.ToString());
                }
                cursor = Db.RawQuery(("select " + Databasehelper.COLUMN_IMGPATH + " from " + Databasehelper.CONTENTTABLE + " where _id == " + Id.ToString()), null);
                Content = Multitools.GetNameNote(Editable.ToString().Split("\n")[0], cursor);
                Calendar calendar = Calendar.Instance;
                calendar.Set(CalendarField.Year, Date.Year);
                calendar.Set(CalendarField.Month, Date.Month-1);
                calendar.Set(CalendarField.DayOfMonth, Date.Day);
                calendar.Set(CalendarField.HourOfDay, Hour);
                calendar.Set(CalendarField.Minute, Minute);
                calendar.Set(CalendarField.Second, 0);
                NotifyManager notify = new NotifyManager();
               alarm = (Android.App.AlarmManager)Context.GetSystemService(Context.AlarmService);
                intent = new Intent(Context, typeof(NotifyManager));
                long uniqueId = SqlHelper.getNotifyId();
                intent.PutExtra("_id", uniqueId);
                intent.PutExtra("message", Content);
                //ChangeIntent(Context);
                pendingIntent = Android.App.PendingIntent.GetBroadcast(Context, Convert.ToInt32(uniqueId), intent, Android.App.PendingIntentFlags.UpdateCurrent);
               
                
               
                if (Convert.ToInt32(Build.VERSION.Sdk) >= 19)
                    alarm.SetExact(Android.App.AlarmType.RtcWakeup, calendar.TimeInMillis, pendingIntent);
                else
                    alarm.Set(Android.App.AlarmType.RtcWakeup, calendar.TimeInMillis, pendingIntent);
                
                ContentValues cv = new ContentValues();
                //cv.Put(Databasehelper.COLUMN_NOTIFY, 1);
                //cv.Put(Databasehelper.COLUMN_TIME, calendar.TimeInMillis);
                cv.Put(Databasehelper.START_ID, uniqueId);
                cv.Put(Databasehelper.NEW_ID, Id); //While id is identific

                Db.Insert(Databasehelper.NOTIFYTABLE, null, cv);
                //PrefsEditor.PutBoolean(Id.ToString(), true);
                //PrefsEditor.Apply();
               
           
                Dialog.Cancel();
              
            }
        }
        public void CancelAlarm(Context context)
        {
            //Shared = PreferenceManager.GetDefaultSharedPreferences(context);
            SqlHelper = new Databasehelper(context);
            Db = SqlHelper.WritableDatabase;
            cursor = Db.Query(Databasehelper.NOTIFYTABLE, new string[] { Databasehelper.START_ID }, Databasehelper.NEW_ID + "=?", new string[] { Id.ToString() }, null, null, null);
            cursor.MoveToFirst();
            //PrefsEditor = Shared.Edit();
            alarm = (Android.App.AlarmManager)context.GetSystemService(Context.AlarmService);
            intent = new Intent(context, typeof(NotifyManager));
            pendingIntent = Android.App.PendingIntent.GetBroadcast(context, Convert.ToInt32(cursor.GetLong(0)), intent, Android.App.PendingIntentFlags.UpdateCurrent);
            alarm.Cancel(pendingIntent);
            ContentValues cv = new ContentValues();
            //cv.Put(Databasehelper.COLUMN_NOTIFY, 0);

            //Db.Update(Databasehelper.TEXTTABLE, cv, "_id = ?", new string[] { Id.ToString() });
            //PrefsEditor.Remove(Id.ToString());
            //PrefsEditor.Apply();
        }
        public void CancelAlarm(Context context,long oldId)
        {
            SqlHelper = new Databasehelper(context);
            Db = SqlHelper.WritableDatabase;
            cursor = Db.Query(Databasehelper.NOTIFYTABLE, new string[] { Databasehelper.START_ID }, Databasehelper.NEW_ID + "=?", new string[] { oldId.ToString() }, null, null, null);
            cursor.MoveToFirst();
            if (cursor.Count != 0) {
                alarm = (Android.App.AlarmManager)context.GetSystemService(Context.AlarmService);
                intent = new Intent(context, typeof(NotifyManager));
                pendingIntent = Android.App.PendingIntent.GetBroadcast(context, Convert.ToInt32(cursor.GetLong(0)), intent, Android.App.PendingIntentFlags.UpdateCurrent);
                alarm.Cancel(pendingIntent); 
            } //cancel old alarm
        }
      public void ChangeIntent(Context context)
        {
            if (intent != null)
            {
                intent.RemoveExtra("message");
                SqlHelper = new Databasehelper(context);
                Db = SqlHelper.WritableDatabase;
                cursor = Db.Query(Databasehelper.NOTIFYTABLE, new string[] { Databasehelper.START_ID }, Databasehelper.NEW_ID + "=?", new string[] { Id.ToString() }, null, null, null);
                cursor.MoveToFirst();
                // (WriteActivity)context.ApplicationContext.
                intent.PutExtra("message", Content);
                pendingIntent = Android.App.PendingIntent.GetBroadcast(context, Convert.ToInt32(cursor.GetLong(0)), intent, Android.App.PendingIntentFlags.UpdateCurrent);
            }
        }
      

        void OnClickCancel(object sender,EventArgs e)
        {
            Dialog.Cancel();
        }
        private void SetDefaultTime()
        {


            DateBut.Text = DateTime.Now.ToString("d");
            TimeBut.Text = DateTime.Now.ToString("HH:mm");
            Hour = DateTime.Now.Hour;
            Minute = DateTime.Now.Minute;
            Date = DateTime.Now.Date;

        }
     
    }
}