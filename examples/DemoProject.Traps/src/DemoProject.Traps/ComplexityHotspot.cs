namespace DemoProject.Traps;

public static class ComplexityHotspot
{
    // TRAP: Агент написал метод с множеством вложенных if/switch/loops.
    // GUARDRAIL: SonarAnalyzer S3776/S1541 + ComplexityRatchetTest ловит превышение порогов.
    // NOTE: Этот метод специально нарушает thresholds 5/3, чтобы показать падение guardrail.
    public static int Calculate(int input)
    {
        if (input < 0)
        {
            if (input < -10)
            {
                if (input < -20)
                {
                    return -3;
                }
                return -2;
            }
            return -1;
        }

        if (input == 0)
        {
            return 0;
        }

        if (input > 0)
        {
            if (input > 10)
            {
                if (input > 20)
                {
                    return 3;
                }
                return 2;
            }
            return 1;
        }

        return int.MaxValue;
    }
}
