namespace Nez
{
    public static class FloatExt
    {
        public static bool Approximately(this float self, float other) => Mathf.Approximately(self, other);
    }
}