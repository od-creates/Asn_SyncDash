public static class IdGenerator
{
    private static int nextId = 1;
    public static int Next()
    {
        return nextId++;
    }
}