using AI;
using testowanie;

namespace Program
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await HintTesting.Test();
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
        
    }

}