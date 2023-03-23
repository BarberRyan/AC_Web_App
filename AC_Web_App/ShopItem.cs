namespace AC_Web_App
{
    public class ShopItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal Rating { get; set; }
        public List<string> ImageNames { get; set; }

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
