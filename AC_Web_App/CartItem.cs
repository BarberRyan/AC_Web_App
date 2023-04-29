namespace AC_Web_App
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CartQty { get; set; }
        public List<string>? ImageNames { get; set; }

        public CartItem(int ID, string name, int qty)
        {
            Id = ID;
            Name = name;
            CartQty = qty;
        }

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
