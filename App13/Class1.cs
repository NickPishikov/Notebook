using System.IO;
using System.Collections.Generic;
using Android.Graphics;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Database;
using Android.Database.Sqlite;
using Android.App;
using Android.OS;
using System;

namespace App13
{
    static class Multitools
    {
        public static void createChannelIfNeeded(NotificationManager manager)
        {
            if (Convert.ToInt32(Build.VERSION.Sdk) >= 26)
            {
                NotificationChannel notificationChannel = new NotificationChannel("ID", "ID", NotificationImportance.High);
                manager.CreateNotificationChannel(notificationChannel);
            }
        }
        private static int calculateInSampleSize(
             BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) >= reqHeight
                        && (halfWidth / inSampleSize) >= reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }
        public static Bitmap decodeSampledBitmapFromUri(Context context, Android.Net.Uri imageUri, int reqWidth, int reqHeight)
        {
            Bitmap bitmap = null;

            // Get input stream of the image
            BitmapFactory.Options options = new BitmapFactory.Options();
            var iStream = context.ContentResolver.OpenInputStream(imageUri);

            // First decode with inJustDecodeBounds=true to check dimensions
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeStream(iStream, null, options);
            if (iStream != null)
            {
                iStream.Close();
            }
            iStream = context.ContentResolver.OpenInputStream(imageUri);

            // Calculate inSampleSize
            options.InSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            bitmap = BitmapFactory.DecodeStream(iStream, null, options);
            if (iStream != null)
            {
                iStream.Close();
            }

            return bitmap;
        }
        public static Bitmap getResizedBitmap(Bitmap bm, int newWidth, int newHeight)
        {
            int width = bm.Width;
            int height = bm.Height;
            float scale = Math.Min(((float)newWidth) / width, ((float)newHeight) / height);

            // CREATE A MATRIX FOR THE MANIPULATION
            Matrix matrix = new Matrix();
            // RESIZE THE BIT MAP
            matrix.PostScale(scale, scale);

            // "RECREATE" THE NEW BITMAP
            Bitmap resizedBitmap = Bitmap.CreateBitmap(
                bm, 0, 0, width, height, matrix, false);
            bm.Recycle();
            return resizedBitmap;
        }
        public static byte[] ConvertoBLob( Bitmap image)
        {
            MemoryStream stream = new MemoryStream();

            image.Compress(Bitmap.CompressFormat.Jpeg, 50, stream);
            
           
            return stream.ToArray();
        }
        public static Bitmap ConvertToBitmap(byte[] bytearray)
        {

            Bitmap BitmapResult = BitmapFactory.DecodeByteArray(bytearray, 0, bytearray.Length);
            return BitmapResult;
        }
        public static string GetNameNote(string text,ICursor cursor1) //position +1
        {
            string paths;
            while (cursor1.MoveToNext())
            {
                 paths = cursor1.GetString(0);
                if (text == '[' + paths + ']')
                {
                    return "Изображение";
                }

            }
            return text;
        }
            
           
           
        }
    







    class Listadapter : SimpleCursorAdapter
    {
      public  static int id = 1;
        public bool IsShow;
        public bool[] IsChecked;
     
        Databasehelper SqlHelper;
        SQLiteDatabase Db;
        ICursor cursor1;
        Context context;
        public Listadapter(Context context, int layout, ICursor c, string[] from, int[] to, [GeneratedEnum] CursorAdapterFlags flags) : base(context, layout, c, from, to, flags)
        {
            this.context = context;
            IsChecked = new bool[Cursor.Count];
            cursor1 = c;
        }
        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            base.GetView(position, convertView, parent);
            string title;
            ViewHolder viewHolder;
            string[] nameNote = Cursor.GetString(Cursor.GetColumnIndex("ColumnText")).Split("\n");
            int IsNotify = Cursor.GetInt(Cursor.GetColumnIndex(Databasehelper.COLUMN_NOTIFY));
            
            SqlHelper = new Databasehelper(context);
            Db = SqlHelper.ReadableDatabase;
            if (convertView == null)
            {
                convertView = View.Inflate(context, Resource.Layout.activity_rows, null);
                Cursor.MoveToPosition(position);
                
                viewHolder = new ViewHolder(convertView);
             

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (ViewHolder)convertView.Tag;

            }
            try
            {
                cursor1 = Db.RawQuery("select " + Databasehelper.COLUMN_IMGPATH + " from " + Databasehelper.CONTENTTABLE + " where _id == " + GetItemId(position).ToString(), null);
                title = Multitools.GetNameNote(nameNote[0], cursor1);
            }
            catch { title = nameNote[0]; }
            viewHolder.namenotes.Text = title; //Set Title In List
            if (IsNotify == 1) viewHolder.image.Visibility = ViewStates.Visible;
            else viewHolder.image.Visibility = ViewStates.Invisible;
            viewHolder.editingTime.Text = Cursor.GetString(Cursor.GetColumnIndex(Databasehelper.COLUMN_EDITINGTIME));
            Color c = new Color(Cursor.GetInt(Cursor.GetColumnIndex(Databasehelper.COLUMN_COLOR)));
            viewHolder.layout.SetBackgroundColor(c);

            if (IsShow)
            {
                viewHolder.checkBox.Visibility = ViewStates.Visible;
            }
            else
            {
                viewHolder.checkBox.Visibility = ViewStates.Invisible;
            }

             if (IsShow) viewHolder.checkBox.Checked = IsChecked[position];
            if (!viewHolder.checkBox.HasOnClickListeners)
                viewHolder.checkBox.Click += (sender, e) =>
                {
                    IsChecked[position] = !IsChecked[position];
                   
                };
            return convertView;
        }
        
        private class ViewHolder : Java.Lang.Object
        {
        public  LinearLayout layout;
           public TextView namenotes;
           public CheckBox checkBox;
            public ImageView image;
            public TextView editingTime;
            public ViewHolder(View view)
            {
                namenotes = (TextView)view.FindViewById(Resource.Id.namenote);
                checkBox = (CheckBox)view.FindViewById(Resource.Id.checknote);
                image = (ImageView)view.FindViewById(Resource.Id.clock_notify);
                editingTime = (TextView)view.FindViewById(Resource.Id.editing_time);
                layout = (LinearLayout)view.FindViewById(Resource.Id.back_layout);
            }
        }
         

            public List<int> GetChecked()
            {
                List<int> checkedpos = new List<int>();
                for (int i = 0; i < IsChecked.Length; i++)
                {
                    if (IsChecked[i])
                    {
                        checkedpos.Add(i);
                    }
                }
            NotifyDataSetChanged();
            return checkedpos;
            
        }
            public void IsShowCheckbox(bool show)
            {

                IsShow = show;
                if (IsShow) IsChecked = new bool[Cursor.Count];
                System.Array.Fill<bool>(IsChecked, false); //сбрасываем для нового использования
                this.ChangeCursor(Cursor);
                NotifyDataSetChanged();
            }
            public void ChangeChecked(int position)
            {
                IsChecked[position] = !IsChecked[position];
                NotifyDataSetChanged();
            }
        }
    }
