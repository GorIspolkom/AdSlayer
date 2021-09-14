using UnityEngine;

namespace Clicker.Models
{
    [System.Serializable]
    public struct XXLNum
    {
        static string[] prefixes;
        public static void localizePrefix()
        {
            prefixes = MyUtile.JsonWorker.JsonParser.getAllValName();
        }

        float mantice;
        int ten_power;
        public int getPower() => ten_power;
        public XXLNum(float mantice, int ten_power)
        {
            this.mantice = mantice;
            this.ten_power = ten_power;
        }
        public static XXLNum zero
        {
            get => new XXLNum(0, 0);
        }
        private float getUpperMantice(int delta_power)
        {
            return mantice * Mathf.Pow(10, delta_power);
        }
        public static XXLNum calibrate(float mantice, int ten_power = 0)
        {
            if (mantice == 0)
                return new XXLNum(0, 0);
            int delta = (ten_power % 3);
            mantice = mantice * Mathf.Pow(10, delta);
            ten_power -= delta;
            if (mantice < 1 && mantice > -1)
            {
                while (mantice < 1 && mantice > -1)
                {
                    mantice = mantice * Mathf.Pow(10, 3);
                    ten_power -= 3;
                }
            }
            else if (mantice >= 1000 || mantice <= -1000)
            {
                while (mantice >= 1000 || mantice <= -1000)
                {
                    mantice = mantice * Mathf.Pow(10, -3);
                    ten_power += 3;
                }
            }
            return new XXLNum(mantice, ten_power);
        }
        //math module
        //---------------------------------------------------------
        public static XXLNum operator ++(XXLNum num1)
        {
            if(num1.ten_power < 36)
                num1.mantice += 1f / Mathf.Pow(10, num1.ten_power);
            return num1;
        }

        public static XXLNum operator -(XXLNum num1, XXLNum num2)
        {
            return calcLinelOper(num1, num2, -1);
        }
        public static XXLNum operator +(XXLNum num1, XXLNum num2)
        {
            return calcLinelOper(num1, num2, 1);
        }
        public static XXLNum operator +(XXLNum num1, float _num2)
        {
            XXLNum num2 = calibrate(_num2, 0);
            return calcLinelOper(num1, num2, 1);
        }
        public static XXLNum operator -(XXLNum num1, float _num2)
        {
            XXLNum num2 = calibrate(_num2, 0);
            return calcLinelOper(num1, num2, -1);
        }
        private static XXLNum calcLinelOper(XXLNum num1, XXLNum num2, int sign)
        {

            int delta_power = num1.ten_power - num2.ten_power;

            if (delta_power >= 0)
            {
                if (delta_power < 7)
                    return calibrate(num1.getUpperMantice(delta_power) + num2.mantice * sign, num2.ten_power);
                else
                    return calibrate(num1.mantice, num1.ten_power);
            }
            else
            { 
                if (delta_power < 7)
                    return calibrate(num1.mantice + num2.getUpperMantice(-delta_power) * sign, num1.ten_power);
                else
                    return calibrate(num2.mantice * sign, num2.ten_power);
            }
        }
        public static XXLNum operator -(XXLNum num1)
        {
            num1.mantice *= -1;
            return num1;
        }
        public static XXLNum operator *(XXLNum num1, XXLNum num2)
        {
            float new_mantice = num1.mantice * num2.mantice;
            int new_ten_power = num1.ten_power + num2.ten_power;
            return calibrate(new_mantice, new_ten_power);
        }
        public static XXLNum operator *(XXLNum num1, float num2)
        {
            float new_mantice = num1.mantice * num2;
            return calibrate(new_mantice, num1.ten_power);
        }
        public static XXLNum operator /(XXLNum num1, XXLNum num2)
        {
            float new_mantice = num1.mantice / num2.mantice;
            int new_ten_power = num1.ten_power - num2.ten_power;
            return calibrate(new_mantice, new_ten_power);
        }
        public static XXLNum operator /(XXLNum num1, float num2)
        {
            float new_mantice = num1.mantice / num2;
            int new_ten_power = num1.ten_power;
            return calibrate(new_mantice, new_ten_power);
        }
        //bool modul
        //---------------------------------------------------------
        public static bool operator >(XXLNum num1, XXLNum num2)
        {
            if (num1.ten_power != num2.ten_power)
                return num1.ten_power > num2.ten_power;
            else
                return num1.mantice > num2.mantice;
        }
        public static bool operator <(XXLNum num1, XXLNum num2)
        {
            if (num1.ten_power != num2.ten_power)
                return num1.ten_power < num2.ten_power;

            else
                return num1.mantice < num2.mantice;
        }
        public static bool operator >(XXLNum num1, float num2)
        {
            return num1.getUpperMantice(num1.ten_power) > num2;
        }
        public static bool operator <(XXLNum num1, float num2)
        {
            return num1.getUpperMantice(num1.ten_power) < num2;
        }
        public static bool operator <=(XXLNum num1, XXLNum num2)
        {
            if (num1.ten_power != num2.ten_power)
                return num1.ten_power < num2.ten_power;

            else
                return num1.mantice <= num2.mantice;
        }
        public static bool operator >=(XXLNum num1, XXLNum num2)
        {
            if (num1.ten_power != num2.ten_power)
                return num1.ten_power > num2.ten_power;

            else
                return num1.mantice >= num2.mantice;
        }

        public static bool operator ==(XXLNum num1, XXLNum num2) => num1.Equals(num2);
        public static bool operator !=(XXLNum num1, XXLNum num2) => !num1.Equals(num2);


        public bool isZero() => mantice == 0;
        //string module
        //---------------------------------------------------------
        public override string ToString()
        {
            if (ten_power == 0)
                return mantice.ToString("0");
            else
            {
                int power = ten_power < 0 ? 0 : ten_power / 3 - 1;
                return mantice.ToString("0.0") + " " + prefixes[power];
            }
        }
        public string ToString(string name)
        {
            return $"{name}: {ToString()}";
        }
        public string ValueTest()
        {
            return $"Mantice: {mantice}; Power: {ten_power}; Value: {ToString()}";
        }

        public string ValueTest(string name)
        {
            return $"{name} <=> Mantice: {mantice}; Power: {ten_power}; Value: {ToString()}";
        }
        //transform module
        //---------------------------------------------------------
        public int ToInt()
        {
            return (int)getUpperMantice(ten_power);
        }
        public long ToLong()
        {
            return (long)getUpperMantice(ten_power);
        }
        public double ToDouble()
        {
            return getUpperMantice(ten_power);
        }

        public override bool Equals(object obj)
        {
            var num = (XXLNum)obj;
            return mantice == num.mantice &&
                   ten_power == num.ten_power;
        }
    }
}
