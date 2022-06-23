using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Mono.Security.Cryptography;

namespace _003CPrivateImplementationDetails_003E_007BDE5600AD_002D6212_002D4D84_002D9A32_002D9D951E3289D1_007D
{
	[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
	public class Decrypt
	{
		private static byte[] publicKey = new byte[532]
		{
			6, 2, 0, 0, 0, 164, 0, 0, 82, 83,
			65, 49, 0, 16, 0, 0, 17, 0, 0, 0,
			153, 54, 153, 196, 94, 224, 24, 197, 59, 91,
			170, 73, 19, 89, 5, 119, 119, 171, 110, 19,
			58, 156, 65, 130, 156, 153, 132, 4, 113, 36,
			113, 229, 178, 192, 243, 79, 50, 188, 36, 234,
			20, 181, 62, 158, 172, 195, 129, 198, 22, 76,
			130, 137, 40, 15, 173, 191, 46, 166, 122, 54,
			207, 95, 163, 182, 66, 43, 50, 145, 122, 183,
			181, 255, 17, 79, 20, 209, 81, 4, 124, 81,
			45, 231, 14, 204, 140, 7, 68, 160, 69, 188,
			151, 142, 159, 200, 77, 240, 237, 71, 11, 85,
			236, 16, 234, 144, 13, 191, 228, 173, 92, 48,
			14, 128, 180, 251, 52, 3, 73, 118, 249, 38,
			21, 32, 225, 3, 168, 159, 198, 235, 220, 181,
			199, 89, 165, 89, 236, 142, 130, 70, 32, 243,
			120, 94, 245, 218, 44, 170, 186, 78, 46, 74,
			9, 44, 237, 192, 226, 82, 202, 54, 86, 182,
			132, 122, 204, 37, 72, 124, 117, 255, 25, 42,
			153, 181, 43, 71, 34, 48, 32, 162, 232, 243,
			21, 138, 4, 36, 235, 6, 60, 193, 198, 113,
			135, 142, 233, 34, 76, 255, 198, 118, 69, 27,
			60, 128, 225, 48, 206, 210, 210, 48, 131, 134,
			21, 235, 43, 164, 166, 201, 51, 95, 12, 209,
			153, 104, 48, 17, 29, 253, 62, 241, 75, 104,
			236, 234, 171, 212, 190, 51, 0, 64, 215, 170,
			124, 37, 234, 180, 205, 115, 63, 145, 46, 126,
			250, 56, 8, 183, 13, 138, 167, 246, 163, 10,
			91, 137, 218, 13, 39, 180, 140, 20, 85, 72,
			55, 95, 11, 30, 41, 49, 29, 78, 77, 84,
			167, 174, 14, 70, 64, 248, 7, 17, 216, 1,
			125, 128, 187, 138, 171, 220, 110, 94, 36, 45,
			86, 93, 210, 206, 87, 80, 225, 158, 103, 191,
			114, 1, 13, 198, 38, 9, 227, 82, 79, 199,
			161, 166, 189, 123, 251, 27, 56, 205, 191, 52,
			204, 19, 128, 90, 174, 237, 194, 200, 201, 72,
			231, 104, 195, 216, 209, 56, 222, 219, 53, 139,
			1, 170, 139, 65, 30, 163, 91, 141, 100, 197,
			190, 203, 54, 41, 110, 122, 57, 102, 75, 77,
			248, 68, 82, 108, 209, 188, 89, 99, 81, 172,
			16, 160, 221, 91, 22, 80, 7, 138, 170, 11,
			253, 114, 48, 139, 58, 109, 111, 149, 206, 24,
			30, 89, 244, 44, 100, 67, 232, 131, 221, 218,
			229, 8, 227, 33, 135, 102, 117, 130, 186, 157,
			26, 231, 32, 1, 104, 23, 241, 72, 36, 8,
			144, 122, 155, 168, 19, 223, 207, 149, 176, 71,
			51, 56, 99, 28, 38, 118, 110, 241, 96, 203,
			167, 237, 163, 43, 139, 109, 116, 32, 231, 90,
			120, 179, 215, 37, 72, 67, 206, 29, 107, 40,
			113, 237, 6, 170, 249, 125, 82, 199, 138, 86,
			131, 247, 1, 119, 181, 52, 224, 233, 37, 21,
			229, 17, 228, 242, 171, 250, 119, 0, 202, 171,
			229, 61, 19, 51, 219, 40, 124, 3, 140, 4,
			69, 130
		};

		private static string Translate(byte[] b)
		{
			int i;
			for (i = 0; i < b.Length && b[i] == 0; i++)
			{
			}
			if (i != b.Length)
			{
				byte[] array = new byte[b.Length - i];
				Buffer.BlockCopy(b, i, array, 0, b.Length - i);
				return Encoding.UTF8.GetString(array);
			}
			return string.Empty;
		}

		public static string DecryptLiteral(byte[] b)
		{
			int num = 4096;
			RSAManaged rSAManaged = new RSAManaged(num);
			rSAManaged.UseKeyBlinding = false;
			RSA rSA = CryptoConvert.FromCapiKeyBlob(publicKey);
			rSAManaged.ImportParameters(rSA.ExportParameters(false));
			int num2 = num / 8;
			if (b.Length == num2)
			{
				byte[] b2 = rSA.EncryptValue(b);
				string text = Translate(b2);
				return text.Substring(1, text.Length - 2);
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < b.Length / num2; i++)
			{
				byte[] array = new byte[num2];
				Buffer.BlockCopy(b, i * num2, array, 0, num2);
				byte[] b3 = rSA.EncryptValue(array);
				stringBuilder.Append(Translate(b3));
			}
			return stringBuilder.ToString(1, ((string)(object)stringBuilder).Length - 2);
		}
	}
}
