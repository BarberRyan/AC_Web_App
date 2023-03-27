﻿namespace AC_Web_App
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public List<string> ImageNames { get; set; }

        public void AddImage(string filename)
        {
            if (ImageNames == null)
            {
                ImageNames = new List<string>();
            }
            ImageNames.Add(filename);
        }
    }
}