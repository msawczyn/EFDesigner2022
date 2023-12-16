namespace Transient
{
   internal class Program
   {
      static void Main(string[] args)
      {
         using (TransientContext context = new TransientContext())
         {
            Master master = new Master
            {
               Foo = "Foo",
               StringArray = new[] { "Foo", "Bar" },
               TransientDetailAsJson = new TransientDetailAsJson
               {
                  Zoom = "Zoom",
                  Zoom1 = "Zoom 1",
                  Zoom2 = "Zoom 2",
                  Zoom3 = "Zoom 3"
               }
            };
            context.Masters.Add(master);
            context.SaveChanges();
         }
      }
   }
}
