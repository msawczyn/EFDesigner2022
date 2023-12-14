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
               TransientDetailAsJson = new TransientDetailAsJson
               {
                  Zoom = "Zoom"
               }
            };
            context.Masters.Add(master);
            context.SaveChanges();
         }
      }
   }
}
