    public class Person
    {
        public int Id { get; set; }

        [Validate("value.Length == 11")]
        public string TcNo { get; set; }

        public string Name { get; set; }

        [Validate("value.HasValue && value.Value > 0M")]
        public decimal? Height { get; set; }

        [Validate("value.HasValue && value.Value > 0M && value.Value < 100M")]
        public decimal? Width { get; set; }

        public int Age { get; set; }

        [Validate("null != value && value.Length <= 60 && value.Length >= 3")]
        public string Title { get; set; }

        [Validate("!String.IsNullOrEmpty(value) && value.Length == 5 && Regex.IsMatch(value, @\"^[A-Z]+[a-zA-Z''-'\\s]*$\")")]
      //  [Validate("!String.IsNullOrEmpty(value) && value.Length == 5")]
        public string Genre { get; set; }
    }



	     private static void Main(string[] args)
        {

            DynamicValidator.Init(Assembly.GetExecutingAssembly().ToSingleItemList());


            Person p  = new Person();
            p.TcNo = "49936399888";
            p.Height = 10m;
            p.Width = 10M;
            p.Title = "d4a";
            p.Genre = "ABCDE";

            Stopwatch bench  =Stopwatch.StartNew();
            for (int j = 0; j < 100000; ++j)
                p.IsValid();
            bench.Stop();

            Console.WriteLine(bench.ElapsedMilliseconds);

            Console.WriteLine(p.IsValid());
        }