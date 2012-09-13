namespace ThomsonReuters.CodeGeneration
{
    public static class Hashing
    {
        // Jenkins32 mixing function
        public static int Mix(int input)
        {
            int output = input;

            output += (output << 12);
            output ^= (output >> 22);
            output += (output << 4);
            output ^= (output >> 9);
            output += (output << 10);
            output ^= (output >> 2);
            output += (output << 7);
            output ^= (output >> 12);

            return output;
        }
    }
}
