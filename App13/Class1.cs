using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Database;
using Android.Support.V7.Widget;

using System;
namespace App13
{
    static class Multitools
    {
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
        public static byte[] ConvertoBLob(ref Bitmap image)
        {
            MemoryStream stream = new MemoryStream();

            image.Compress(Bitmap.CompressFormat.Png, 100, stream);
            byte[] byteArray = stream.ToArray();
            return byteArray;
        }
        public static Bitmap ConvertToBitmap(byte[] bytearray)
        {

            Bitmap BitmapResult = BitmapFactory.DecodeByteArray(bytearray, 0, bytearray.Length);
            return BitmapResult;
        }
    }
    class Listadapter : SimpleCursorAdapter
    {
      public  static int id = 1;
        public bool IsShow;
        public bool[] IsChecked;
        CheckBox checkbox;
        TextView textbox;
        int Position;
        ICursor cursor1;
        Context context;
        public Listadapter(Context context, int layout, ICursor c, string[] from, int[] to, [GeneratedEnum] CursorAdapterFlags flags) : base(context, layout, c, from, to, flags)
        {
            this.context = context;
            IsChecked = new bool[Cursor.Count];
            cursor1 = c;
        }
        //public override void BindView(View view, Context context, ICursor cursor)
        //{

        //    base.BindView(view, context, cursor);

        //    textbox = (TextView)view.FindViewById(Resource.Id.namenote);
        //    checkbox = (CheckBox)view.FindViewById(Resource.Id.checknote);

        //    int position = (int)view.GetTag(view.Id);

        //    if (IsShow)
        //    {
        //        checkbox.Visibility = ViewStates.Visible;
        //    }
        //    else
        //    {
        //        checkbox.Visibility = ViewStates.Invisible;
        //    }

        //    if (IsShow) checkbox.Checked=IsChecked[position];
        //    if(!checkbox.HasOnClickListeners)
        //    checkbox.Click += (sender, e) =>
        //    {
        //        IsChecked[position] = !IsChecked[position];

        //    };

        //}
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            base.GetView(position, convertView, parent);
            ViewHolder viewHolder;


            if (convertView == null)
            {
                convertView = View.Inflate(context, Resource.Layout.activity_rows, null);
                Cursor.MoveToPosition(position);
                
                viewHolder = new ViewHolder(convertView);
                viewHolder.namenotes.Text= Cursor.GetString(Cursor.GetColumnIndex("ColumnText"));
                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (ViewHolder)convertView.Tag;
            }
               // viewHolder.namenotes=
                




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

           public TextView namenotes;
           public CheckBox checkBox;
            public ViewHolder(View view)
            {
                namenotes = (TextView)view.FindViewById(Resource.Id.namenote);
                checkBox = (CheckBox)view.FindViewById(Resource.Id.checknote);

            }
        }
            //public override View NewView(Context context, ICursor cursor, ViewGroup parent)
            //{
            //    //base.GetView(position, convertView, parent);
            //    View v;


            //        LayoutInflater vi;
            //        vi = LayoutInflater.From(parent.Context);
            //        v = vi.Inflate(Resource.Layout.activity_rows,parent,false);



            //    v.SetTag(v.Id, Cursor.posi);
            //    // BindView(v, parent.Context, cursor1);
            //   //NotifyDataSetChanged();





            //    return v;

            //}

            public List<int> GetChecked()
            {
                List<int> checkedpos = new List<int>();
                for (int i = 0; i < IsChecked.Length; i++)
                {
                    if (IsChecked[i])
                    {
                        checkedpos.Add(i + 1);
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
