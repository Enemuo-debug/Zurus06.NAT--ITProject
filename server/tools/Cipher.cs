using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.tools
{
    public class Cipher
    {
        private static readonly char[] alphabet = ")-_=+[]{qrstuvwxyzABCDEFdefghijklmn^LMNOPQRSTUV&*(WXYGHIJKabcop9!@#$%Z012345678}|;:',.<>?/`~ ".ToCharArray();
        public static void Main(string[] args)
        {
            string plainText = "This is Nigeria and everybody is a criminal___";
            string cipherText = HillCipherEncrypt(plainText);
            string decryptedText = HillCipherDecrypt(cipherText);
        }

        public static uint EuclideanAlgorithm(uint a, uint b)
        {
            if (Math.Min(a, b) == 0)
            {
                return Math.Max(a, b);
            }
            return EuclideanAlgorithm(Math.Min(a, b), Math.Max(a, b) % Math.Min(a, b));
        }

        public static int[,] MultiplyMatrices(int[,] a, int[,] b, int mode)
        {
            int aRows = a.GetLength(0);
            int aCols = a.GetLength(1);
            int bRows = b.GetLength(0);
            int bCols = b.GetLength(1);

            if (aCols != bRows)
                throw new ArgumentException("Number of columns in matrix A must match number of rows in matrix B.");

            int[,] result = new int[aRows, bCols];

            for (int i = 0; i < aRows; i++)
            {
                for (int j = 0; j < bCols; j++)
                {
                    int sum = 0;
                    for (int k = 0; k < aCols; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = ((sum % mode) + mode) % mode;
                }
            }

            return result;
        }
        public static int Determinant(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Matrix must be square.");

            // Base cases
            if (n == 1)
                return matrix[0, 0];

            if (n == 2)
                return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            int det = 0;
            for (int j = 0; j < n; j++)
            {
                int[,] minor = minorMatrix(matrix, Tuple.Create(0, j));
                int cofactor = ((j % 2 == 0) ? 1 : -1) * matrix[0, j] * Determinant(minor);
                det += cofactor;
            }

            return det;
        }

        public static int[,] minorMatrix(int[,] matrix, Tuple<int, int> coordinate)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Matrix must be square.");

            if (coordinate.Item1 < 0 || coordinate.Item1 >= n || coordinate.Item2 < 0 || coordinate.Item2 >= n)
                throw new ArgumentOutOfRangeException("Coordinate must be within matrix bounds.");

            int[,] result = new int[n - 1, n - 1];
            int rowOffset = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                if (i == coordinate.Item1)
                {
                    continue;
                }
                int colOffset = 0;
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (j == coordinate.Item2)
                    {
                        continue;
                    }
                    result[rowOffset, colOffset] = matrix[i, j];
                    colOffset++;
                }
                rowOffset++;
            }
            return result;
        }

        public static int[,] MatrixCofactors(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Matrix must be square.");

            int[,] cofactors = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int[,] minor = minorMatrix(matrix, Tuple.Create(i, j));
                    cofactors[i, j] = ((i + j) % 2 == 0 ? 1 : -1) * Determinant(minor);
                }
            }
            return cofactors;
        }

        public static int[,] Transpose(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[,] transposed = new int[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transposed[j, i] = matrix[i, j];
                }
            }
            return transposed;
        }

        public static int[,] Inverse(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Matrix must be square.");

            int det = Determinant(matrix);
            if (det == 0)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            int[,] cofactors = MatrixCofactors(matrix);
            int[,] adjugate = Transpose(cofactors);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    adjugate[i, j] /= det;
                }
            }
            return adjugate;
        }

        public static int[,] Inverse(int[,] matrix, int mod)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("Matrix must be square.");

            int det = Determinant(matrix);
            det = ((det % mod) + mod) % mod;
            int detInv = BruteForceExtendedEuclideanAlgorithm(det, mod);
            if (det == 0)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            int[,] cofactors = MatrixCofactors(matrix);
            int[,] adjugate = Transpose(cofactors);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    adjugate[i, j] = (adjugate[i, j] * detInv) % mod;
                    if (adjugate[i, j] < 0)
                        adjugate[i, j] += mod;
                }
            }
            return adjugate;
        }

        public static int BruteForceExtendedEuclideanAlgorithm(int a, int b)
        {
            a = a % b;
            for (int x = 0; x < b; x++)
            {
                if ((a * x) % b == 1)
                    return x;
            }
            throw new ArgumentException("No modular inverse exists.");
        }

        public static string HillCipherEncrypt(string plainText, string key = "y:b:Z$(u[W6*WXI3")
        {
            int alphabetSize = alphabet.Length;
            string cipherText = "";

            int digramSize = (int)Math.Ceiling(Math.Sqrt(key.Length));

            while (plainText.Length % digramSize != 0)
            {
                plainText += "_";
            }

            int[,] keyMatrix = new int[digramSize, digramSize];
            int keyIterator = 0;
            for (int i = 0; i < digramSize; i++)
            {
                for (int j = 0; j < digramSize; j++)
                {
                    if (keyIterator < key.Length)
                        keyMatrix[i, j] = Array.IndexOf(alphabet, key[keyIterator++]);
                    else
                        keyMatrix[i, j] = Array.IndexOf(alphabet, ' ');
                }
            }

            // Check if determinant is coprime with alphabet size
            // So we don't encrypt with a modularly singular matrix
            int det = Determinant(keyMatrix);
            det = (det % alphabetSize + alphabetSize) % alphabetSize;
            if (EuclideanAlgorithm((uint)det, (uint)alphabetSize) != 1)
                throw new ArgumentException("Invalid key: determinant not coprime with alphabet size.");

            int index = 0;
            while (index < plainText.Length)
            {
                int[,] blockVector = new int[digramSize, 1];
                for (int i = 0; i < digramSize; i++)
                {
                    char c = plainText[index++];
                    blockVector[i, 0] = Array.IndexOf(alphabet, c);
                }

                int[,] cipherVector = MultiplyMatrices(keyMatrix, blockVector, alphabetSize);

                for (int i = 0; i < digramSize; i++)
                {
                    cipherText += alphabet[cipherVector[i, 0]];
                }
            }

            return cipherText;
        }
        public static string HillCipherDecrypt(string cipherText, string key = "y:b:Z$(u[W6*WXI3")
        {
            int alphabetSize = alphabet.Length;
            string plainText = "";

            // Determine matrix size (digram size)
            int digramSize = (int)Math.Ceiling(Math.Sqrt(key.Length));

            // --- Build key matrix ---
            int[,] keyMatrix = new int[digramSize, digramSize];
            int keyIterator = 0;
            for (int i = 0; i < digramSize; i++)
            {
                for (int j = 0; j < digramSize; j++)
                {
                    if (keyIterator < key.Length)
                    {
                        int idx = Array.IndexOf(alphabet, key[keyIterator++]);
                        if (idx == -1)
                            throw new ArgumentException($"Key character '{key[keyIterator - 1]}' not in alphabet");
                        keyMatrix[i, j] = idx;
                    }
                    else
                    {
                        keyMatrix[i, j] = Array.IndexOf(alphabet, '_'); // padding
                    }
                }
            }

            // --- Check determinant ---
            int det = ((Determinant(keyMatrix) % alphabetSize) + alphabetSize) % alphabetSize;
            if (EuclideanAlgorithm((uint)det, (uint)alphabetSize) != 1)
                throw new ArgumentException("Invalid key: determinant not coprime with alphabet size");

            // --- Compute inverse key matrix ---
            int[,] inverseKeyMatrix = Inverse(keyMatrix, alphabetSize);

            // --- Decrypt block by block ---
            int index = 0;
            while (index < cipherText.Length)
            {
                int[,] blockVector = new int[digramSize, 1];
                for (int i = 0; i < digramSize; i++)
                {
                    char c = cipherText[index++];
                    int idx = Array.IndexOf(alphabet, c);
                    if (idx == -1)
                        throw new ArgumentException($"Cipher character '{c}' not in alphabet");
                    blockVector[i, 0] = idx;
                }

                int[,] plainVector = MultiplyMatrices(inverseKeyMatrix, blockVector, alphabetSize);

                for (int i = 0; i < digramSize; i++)
                {
                    int idx = ((plainVector[i, 0] % alphabetSize) + alphabetSize) % alphabetSize;
                    plainText += alphabet[idx];
                }
            }

            return plainText.TrimEnd('_');
        }
    }
}