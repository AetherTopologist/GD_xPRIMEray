namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class TestAssert
{
    public static void True(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    public static void False(bool value, string message) => True(!value, message);

    public static void Equal<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message}: expected={expected}, actual={actual}");
        }
    }

    public static void NearlyEqual(float expected, float actual, float tolerance, string message)
    {
        if (!float.IsFinite(actual) || MathF.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException(
                $"{message}: expected={expected}, actual={actual}, tolerance={tolerance}");
        }
    }
}
