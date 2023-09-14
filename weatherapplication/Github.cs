namespace weatherapplication
{
    public class Github

        {
           public string Action { get; set; }
          /*  public string refs { get; set; }
            public string after { get; set; }*/


/*         public commits[] commits { get; set; }*/



        }



        public class commits
        {
            public string id { get; set; }
            public string tree_id { get; set; }
            public bool distinct { get; set; }
            public string message { get; set; }
        }
  
}
