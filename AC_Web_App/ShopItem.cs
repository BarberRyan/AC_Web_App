namespace AC_Web_App
{
    public class ShopItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal? Rating { get; set; }
        public int Qty { get; set; }

        public List<string>? ImageNames { get; set; }


        public ShopItem(int id, string name, string desc, decimal price, decimal oldPrice, decimal rating, int qty)
        {
            ID = id;
            Name = name;
            Desc = desc;
            Price = price;
            OldPrice = oldPrice;
            Rating = rating;
            Qty = qty;
        }


        public void AddImage(string filename)
        {
            if(ImageNames == null)
            {
                ImageNames = new List<string>();
            }
            ImageNames.Add(filename);
        }

    }
}
