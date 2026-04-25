namespace WCS.Application.Services.Common
{
    public class MathHelpers
    {
        public static double CalculateFactorial(int n)
        {
            if (n < 0) throw new ArgumentException("n must be greater than 0.");

            double factorial = 1;
            for (int i = 1; i <= n; i++)
            {
                factorial *= i;
            }
            return factorial;
        }
    }
}
