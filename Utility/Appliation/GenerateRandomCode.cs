namespace Utility.Appliation
{
    public static class GenerateRandomCode
    {
        public static int GenerateUserRegisterCode()
        {
            Random random = new Random();
            return random.Next(00000, 99999);
        }
    }
}
