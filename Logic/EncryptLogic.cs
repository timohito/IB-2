using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WpfApp1.Constants;

namespace IB2
{
    public class EncryptLogic
    {
        [Dependency]
        public IUnityContainer Container { get; set; }

        public EncryptLogic()
        {

        }

        public const int sizeOfBlock = 128; //в unicode символ в два раза длинее, чем в DES, увеличим блок тоже в два раза
        public const int sizeOfChar = 16; //размер одного символа в юникоде
        public const int roundQuantity = 16; //количество раундов

        private BitArray[] getKeys(string key)
        {
            //сначала занесем ключи на 16 раундов
            //получаем ключ в битах (64) - 8 байт
            byte[] strBytesKey = Encoding.UTF8.GetBytes(key);
            //.Take(8).ToArray()
            //если ключ слишком маленький
            if (strBytesKey == null || strBytesKey.Length < 8)
            {
                byte[] newKey = new byte[8];
                for (int i = 0; i < newKey.Length; i++)
                {
                    if (i < strBytesKey.Length)
                    {
                        newKey[i] = strBytesKey[i];
                    }
                    else
                    {
                        newKey[i] = 0;
                    }
                }
                strBytesKey = newKey;
            }
            else
            {
                //если больше 8 - берем первые 8
                strBytesKey = Encoding.UTF8.GetBytes(key).Take(8).ToArray();
            }
            BitArray K = new BitArray(strBytesKey);

            //получаем Co в битах
            BitArray C = new BitArray(28);
            BitArray D = new BitArray(28);
            //преобразуем по таблице Key в Co (части C и D)
            //1ый бит станет 57 из ключа
            for (int i = 0; i < 28; i++)
            {
                C[i] = K[Matrices.CoMatrix[i]];
            }
            for (int i = 28; i < 56; i++)
            {
                D[i - 28] = K[Matrices.CoMatrix[i]];
            }
            BitArray[] keys = new BitArray[roundQuantity];
            for (int i = 0; i < roundQuantity; i++)
            {
                C = keyRoundLeft(C);
                D = keyRoundLeft(D);
                K = concatinateBytes(C, D);
                K = keyReplacement(K);
                keys[i] = K;
            }
            return keys;
        }

        public void Encode(string key, string inputFilePath, string outputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new Exception("Входной файл не найден");
            }
            try
            {
                //считываем строку и задаем правильную длину
                string text = File.ReadAllText(inputFilePath);
                text = stringToCorrectLength(text);

                //получаем байтовое представление, а затем битовое
                byte[] strBytes = Encoding.UTF8.GetBytes(text);
                BitArray bits = new BitArray(strBytes);

                //массив итоговых битов, который мы запишем в файл
                BitArray writeBits = new BitArray(bits.Length);

                //получаем ключи
                BitArray[] keys = getKeys(key);

                for (int i = 0; i < bits.Length; i += 64)
                {
                    //сначала IP подстановка по таблице
                    BitArray prev = startReplacement(bits, i);
                    BitArray prevL = copy(prev, 0, 32);
                    BitArray prevR = copy(prev, 32, 32);

                    for (int j = 0; j < roundQuantity; j++)
                    {
                        BitArray L = prevR;
                        BitArray R = prevL.Xor(func(prevR, keys[j]));

                        prev = concatinateBytes(L, R);
                        prevL = L;
                        prevR = R;
                    }
                    BitArray iterRes = endReplacement(prev, 0);
                    writeBits = add(writeBits, i, iterRes);
                }

                byte[] byteArray = new byte[(int)Math.Ceiling((double)writeBits.Length / 8)];
                writeBits.CopyTo(byteArray, 0);
                File.WriteAllBytes(outputFilePath, byteArray);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Decode(string key, string inputFilePath, string outputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new Exception("Входной файл не найден");
            }
            try
            {
                //получаем байтовое представление, а затем битовое
                byte[] strBytes = File.ReadAllBytes(inputFilePath);
                BitArray bits = new BitArray(strBytes);

                //массив итоговых битов, который мы запишем в файл
                BitArray writeBits = new BitArray(bits.Length);

                //получаем ключи
                BitArray[] keys = getKeys(key);

                for (int i = bits.Length - 64; i >= 0; i -= 64)
                {
                    //сначала IP подстановка по таблице
                    BitArray prev = startReplacement(bits, i);
                    BitArray prevL = copy(prev, 0, 32);
                    BitArray prevR = copy(prev, 32, 32);

                    for (int j = roundQuantity - 1; j >= 0; j--)
                    {
                        BitArray R = prevL;
                        BitArray L = prevR.Xor(func(prevL, keys[j]));

                        prev = concatinateBytes(L, R);
                        prevL = L;
                        prevR = R;
                    }
                    BitArray iterRes = endReplacement(prev, 0);
                    writeBits = add(writeBits, i, iterRes);
                }

                byte[] byteArray = new byte[(int)Math.Ceiling((double)writeBits.Length / 8)];
                writeBits.CopyTo(byteArray, 0);

                //если мы дописывали символы в конец файла - нужно их убрать
                File.WriteAllBytes(outputFilePath, getFilterBytes(byteArray));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private byte[] getFilterBytes(byte[] arr)
        {
            int sharpCount = 0;
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                //35 - символ решетки
                if (arr[i] == 35)
                {
                    sharpCount++;
                }
            }
            byte[] res = new byte[arr.Length - sharpCount];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = arr[i];
            }
            return res;
        }

        private BitArray keyReplacement(BitArray key)
        {
            BitArray res = new BitArray(48);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = key[Matrices.KeyMatrix[i]];
            }
            return res;
        }

        private BitArray startReplacement(BitArray array, int startIndex)
        {
            BitArray res = new BitArray(64);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = array[startIndex + Matrices.StartMatrix[i] - 1];
            }
            return res;
        }

        private BitArray endReplacement(BitArray array, int startIndex)
        {
            BitArray res = new BitArray(64);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = array[startIndex + Matrices.EndMatrix[i] - 1];
            }
            return res;
        }
        private BitArray func(BitArray R, BitArray K)
        {
            //сначала расширяем R до 48 битов
            BitArray newR48 = new BitArray(48);

            //расширяем
            for (int i = 0; i < R.Length; i += 4)
            {
                int newI = (int)(i * 1.5);

                newR48[newI] = i - 1 < 0 ? R[R.Length - 1] : R[i - 1];

                newR48[newI + 1] = R[i];
                newR48[newI + 2] = R[i + 1];
                newR48[newI + 3] = R[i + 2];
                newR48[newI + 4] = R[i + 3];
                //заполняем следующим первым значением блока или первым, если блок последний
                newR48[newI + 5] = i + 4 >= R.Length ? R[0] : R[i + 4];

            }
            BitArray xor = newR48.Xor(K);
            //инициализируем s-боксы
            BitArray sBoxes = new BitArray(32);
            //проходимся по результату XOR и выносим данные из таблиц S
            for (int i = 0; i < sBoxes.Length; i += 4)
            {
                int resI = (int)(i * 1.5);
                BitArray sBox = getSValue(xor, resI, i / 4);
                sBoxes[i] = sBox[0];
                sBox[i + 1] = sBox[1];
                sBox[i + 2] = sBox[2];
                sBox[i + 3] = sBox[3];
            }

            BitArray res = new BitArray(32);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = sBoxes[Matrices.P[i]];
            }
            return res;
        }

        private BitArray getSValue(BitArray array, int startIndex, int boxCount)
        {
            int row = getNum(array[startIndex]) * 2 + getNum(array[startIndex + 5]);
            int column = getNum(array[startIndex + 1]) * 8 + getNum(array[startIndex + 2]) * 4
                + getNum(array[startIndex + 4]) * 2 + getNum(array[startIndex + 4]);
            int num = Matrices.S[boxCount][row][column];

            byte[] numBytes = BitConverter.GetBytes(num);
            BitArray bits = new BitArray(numBytes);
            return bits;
        }

        private int getNum(bool bit)
        {
            return bit ? 1 : 0;
        }

        private BitArray add(BitArray array, int index, BitArray addArr)
        {
            for (int i = index; i < addArr.Length + index; i++)
            {
                array[i] = addArr[i - index];
            }
            return array;
        }

        private BitArray copy(BitArray array, int index, int lenth)
        {
            BitArray res = new BitArray(lenth);

            for (int i = index; i < lenth + index; i++)
            {
                res[i - index] = array[i];
            }
            return res;
        }

        private BitArray keyRoundLeft(BitArray key)
        {
            bool firstVal = key[0];
            for (int i = 0; i < key.Length - 1; i++)
            {
                key[i] = key[i + 1];
            }
            key[key.Length - 1] = firstVal;

            return key;
        }

        private BitArray concatinateBytes(BitArray first, BitArray second)
        {
            BitArray res = new BitArray(first.Length + second.Length);
            for (int i = 0; i < first.Length; i++)
            {
                res[i] = first[i];
            }
            for (int i = first.Length; i < first.Length + second.Length; i++)
            {
                res[i] = second[i - first.Length];
            }
            return res;
        }

        private string stringToCorrectLength(string input)
        {
            while (((input.Length * sizeOfChar) % sizeOfBlock) != 0)
            {
                input += "#";
            }
               
            return input;
        }

        public string readFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
